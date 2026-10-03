# Copyright (c) 2026 nkramber. Licensed under the MIT License. See LICENSE.
"""Makes the scripted content of the project in a headless editor (D-133, D-134).

`run.ps1 content-build` runs this script through the Python commandlet of the editor, with no
window. The script is the source of each asset that it makes: the input actions, the mapping
context, the movement tuning, the outline and glow materials of interact, the weapons and their
sounds, the melee attack, the player Blueprints and the HUD, and the gym map `L_Gym` with the views of the frame-time
capture. A change to one of these assets is a change to this script, then a new run. A change in
the editor alone goes away at the next run.

The script makes an asset when it is absent, and writes each value again when it is present. Each
fault raises an exception, so the commandlet gives a nonzero exit code (T-2).
"""

import configparser
import math
import os

import unreal

INPUT_FOLDER = "/Game/Input"
PLAYER_FOLDER = "/Game/Player"
WEAPON_FOLDER = "/Game/Weapons"
SOUND_FOLDER = "/Game/Weapons/Sounds"
GYM_MAP = "/Game/Maps/L_Gym"

# The first values of the movement (D-135). `docs/game/movement-metrics.md` gives each reason.
MOVEMENT_TUNING = {
    "run_speed": 900.0,
    "acceleration": 8000.0,
    "braking_deceleration": 8000.0,
    "ground_friction": 8.0,
    "jump_height": 120.0,
    "gravity_scale": 1.5,
    "air_control": 0.5,
    "step_height": 45.0,
    "walkable_slope": 45.0,
    "eye_height": 160.0,
    # The mantle (D-143) and the reach of interact (D-144, D-163).
    "mantle_min_height": 50.0,
    "mantle_max_height": 130.0,
    "mantle_time": 0.4,
    "interact_reach": 400.0,
}

# The two data assets of the weapon rule (D-151, D-153, D-157, D-159 to D-162). `docs/game/weapon-tuning.md`
# gives each reason. The range is 100 m, and each shot spends one round.
WEAPON_TUNING = {
    "DA_WeaponRifle": {
        "display_name": "Rifle",
        "automatic": True,
        "shots_per_second": 10.0,
        "ammo_capacity": 60,
        "range": 10000.0,
        "pellet_count": 1,
        "spread_angle": 0.0,
        "raise_time": 0.25,
        "recoil_kick": 0.5,
        "recoil_side_kick": 0.25,
        "recoil_recovery_time": 0.15,
        "shot_sound": "S_RifleShot",
        "shot_sound_start_time": 0.0,
        # A long thin box, low and to the right of the view.
        "view_scale": unreal.Vector(0.6, 0.06, 0.08),
        "view_offset": unreal.Vector(45.0, 18.0, -20.0),
    },
    "DA_WeaponScatter": {
        "display_name": "Scatter",
        "automatic": False,
        "shots_per_second": 1.5,
        "ammo_capacity": 12,
        "range": 10000.0,
        "pellet_count": 8,
        "spread_angle": 2.0,
        "raise_time": 0.35,
        "recoil_kick": 3.0,
        "recoil_side_kick": 1.0,
        "recoil_recovery_time": 0.35,
        "shot_sound": "S_ScatterShot",
        # The file starts with 56 ms of silence, so the shot starts the sound 50 ms in (D-165).
        "shot_sound_start_time": 0.05,
        # A short thick box, low and to the right of the view.
        "view_scale": unreal.Vector(0.45, 0.12, 0.12),
        "view_offset": unreal.Vector(40.0, 18.0, -22.0),
    },
}

# The data asset of the melee attack (D-170, D-171). `docs/game/weapon-tuning.md` gives each reason.
# The front of the sphere reaches 250 cm from the eye. The jab moves the weapon in the view 20 cm
# forward and back in 0.2 s, and the weapon fires no shot in this time (D-173).
MELEE_ASSET = "DA_MeleeAttack"
MELEE_TUNING = {
    "range": 250.0,
    "sweep_radius": 30.0,
    "attack_interval": 0.8,
    "jab_distance": 20.0,
    "jab_time": 0.2,
}

# The sounds of the weapon: the high-quality OGG previews of three CC0 files of freesound.org
# (D-155). A hit on a gym target has no sound (D-167). `docs/game/provenance.md` holds the record of
# each file. The key is the name of the asset.
SOUND_SOURCE_FOLDER = "SourceAssets/Sounds"
SOUNDS = {
    "S_RifleShot": "rifle_shot.ogg",
    "S_ScatterShot": "scatter_shot.ogg",
    "S_EmptyClick": "empty_click.ogg",
}

# The mix (D-168). The rifle fires 10 shots each second, and the tail of each shot lasts about 1 s,
# so the shots of a burst overlap. Each shot of the rifle plays about 9 dB under a shot of the
# scatter gun. Each value is the linear volume of the sound asset.
SOUND_VOLUMES = {
    "S_RifleShot": 0.35,
    "S_ScatterShot": 1.0,
    "S_EmptyClick": 1.0,
}

# The flash at the point of a hit (D-154): a small sphere that fades out. The parameter name is
# `AIronHitFlash::BrightnessParameter`.
SPHERE = "/Engine/BasicShapes/Sphere.Sphere"
FLASH_DIAMETER = 8.0
FLASH_LIFETIME = 0.1
FLASH_COLOR = unreal.LinearColor(r=1.0, g=0.85, b=0.5, a=1.0)
FLASH_BRIGHTNESS = 20.0
FLASH_PARAMETER = "Brightness"

# The HUD (D-154, D-158). Each size is in pixels.
HUD_VALUES = {
    "color": unreal.LinearColor(r=1.0, g=0.78, b=0.16, a=1.0),
    "empty_color": unreal.LinearColor(r=1.0, g=0.1, b=0.1, a=1.0),
    "crosshair_length": 8.0,
    "crosshair_gap": 4.0,
    "line_thickness": 2.0,
    "hit_marker_gap": 8.0,
    "hit_marker_length": 8.0,
    "hit_marker_seconds": 0.15,
    "text_scale": 2.0,
    "text_margin": 40.0,
}

# The project defaults of the aim settings (D-141). The views of the frame-time capture take the
# default field of view from this file, so M-3 does not depend on the setting of one player.
AIM_DEFAULTS_FILE = "DefaultGameUserSettings.ini"
AIM_DEFAULTS_SECTION = "/Script/IronAbsolution.IronGameUserSettings"

# The engine meshes of the gym. Each file of the gym is original, or a basic shape of the engine.
CUBE = "/Engine/BasicShapes/Cube.Cube"
FLOOR_MATERIAL = "/Engine/EngineMaterials/WorldGridMaterial.WorldGridMaterial"

# The cube of the engine is 100 cm on each side, with its pivot at the center.
CUBE_SIZE = 100.0

# The station rows of the gym run along +X. The player starts at the -X end and looks along +X.
PLAYER_START = unreal.Vector(-2400.0, 0.0, 100.0)
ROW_START_X = -1800.0
GAP_ROW_Y = -1500.0
LEDGE_ROW_Y = -600.0
DISTANCE_ROW_Y = 0.0
STEP_ROW_Y = 600.0
HALL_ROW_Y = 1500.0

# The views of the frame-time capture of M-3 (D-137): the place of the camera, then the roll, the
# pitch, and the yaw. The capture shows them in this order. The height of each low view is the eye
# height of the player above the floor.
FRAME_TIME_VIEWS = [
    ("the spawn, along the rows", unreal.Vector(-2400.0, 0.0, 160.0), unreal.Rotator(0.0, 0.0, 0.0)),
    ("the high corner, over each row", unreal.Vector(-3100.0, -1950.0, 900.0), unreal.Rotator(0.0, -15.0, 22.0)),
    ("the far end, back to the spawn", unreal.Vector(6600.0, 0.0, 160.0), unreal.Rotator(0.0, 0.0, 180.0)),
    ("the hall row, between the walls", unreal.Vector(-2400.0, HALL_ROW_Y, 160.0), unreal.Rotator(0.0, 0.0, 0.0)),
]

# The ledge row. The band of the mantle is 50 cm to 130 cm above the feet (D-143). A jump of
# 120 cm then climbs a ledge from 50 cm to 250 cm high. The top of the jump falls between two
# frames, so the stations near the upper limit stand 5 cm inside and 5 cm outside it.
LEDGE_HEIGHTS = (40, 50, 75, 100, 125, 150, 200, 245, 255)

# The door station: a wall along X with a door and a switch, to the right of the start (D-146).
DOOR_WALL_Y = 1000.0
DOOR_CENTER_X = -2500.0
DOOR_SIZE = unreal.Vector(200.0, 20.0, 250.0)
DOOR_WALL_HEIGHT = 300.0
# The open panel sinks into the floor, below the floor top at Z 0.
DOOR_OPEN_OFFSET = unreal.Vector(0.0, 0.0, -260.0)
SWITCH_SIZE = 20.0
SWITCH_LOCATION = unreal.Vector(-2250.0, DOOR_WALL_Y - DOOR_SIZE.y / 2.0 - SWITCH_SIZE / 2.0, 120.0)

# The targets of the weapon, between the distance row and the ledge row, at 10 m, 25 m, and 50 m
# from the start along +X. Each board is 10 cm deep, 100 cm wide, and 180 cm high.
TARGET_ROW_Y = -300.0
TARGET_DISTANCES = (10, 25, 50)
TARGET_SIZE = unreal.Vector(10.0, 100.0, 180.0)

# The ammo station, to the left of the start (D-156).
AMMO_STATION_LOCATION = unreal.Vector(-2600.0, -400.0, 50.0)
AMMO_STATION_SIZE = unreal.Vector(50.0, 50.0, 100.0)

# The cue of the target in reach (D-145, D-148), in the color of the labels. The outline is 3
# pixels wide. The step of the custom depth, in cm, marks the edge of a target. Each brightness is a
# multiple of the color above 1, so the bloom of the engine makes a glow around it.
CUE_COLOR = unreal.LinearColor(r=1.0, g=0.78, b=0.16, a=1.0)
OUTLINE_WIDTH = 3.0
OUTLINE_DEPTH_STEP = 100.0
OUTLINE_BRIGHTNESS = 12.0
# The fill glow: dim at the center of a face, bright at the edges of the target.
GLOW_CENTER_BRIGHTNESS = 0.6
GLOW_EDGE_BRIGHTNESS = 4.0

LABEL_HEIGHT = 30.0
LABEL_COLOR = unreal.Color(r=255, g=200, b=40, a=255)


class ContentError(Exception):
    """A fault of the script, with the asset or the step that failed."""


def asset_tools():
    return unreal.AssetToolsHelpers.get_asset_tools()


def asset_subsystem():
    return unreal.get_editor_subsystem(unreal.EditorAssetSubsystem)


def load_or_create(folder, name, asset_class, factory):
    """Loads the asset at folder/name, or makes it with the factory when it is absent."""
    path = f"{folder}/{name}"
    if asset_subsystem().does_asset_exist(path):
        asset = asset_subsystem().load_asset(path)
        if asset is None or not isinstance(asset, asset_class):
            raise ContentError(f"The asset {path} exists, but it is not a {asset_class.__name__}.")
        return asset

    asset = asset_tools().create_asset(name, folder, asset_class, factory)
    if asset is None:
        raise ContentError(f"The asset {path} did not come from the factory {type(factory).__name__}.")
    return asset


def save(asset):
    if not asset_subsystem().save_loaded_asset(asset, only_if_is_dirty=False):
        raise ContentError(f"The asset {asset.get_path_name()} did not save.")


def input_action(name, value_type):
    action = load_or_create(INPUT_FOLDER, name, unreal.InputAction, unreal.InputAction_Factory())
    action.set_editor_property("value_type", value_type)
    save(action)
    return action


def negate(owner, x, y):
    modifier = unreal.new_object(unreal.InputModifierNegate, outer=owner)
    modifier.set_editor_property("x", x)
    modifier.set_editor_property("y", y)
    modifier.set_editor_property("z", False)
    return modifier


def swizzle(owner):
    # The order YXZ moves the value of a 1D key to the Y axis, the axis of "forward".
    modifier = unreal.new_object(unreal.InputModifierSwizzleAxis, outer=owner)
    modifier.set_editor_property("order", unreal.InputAxisSwizzle.YXZ)
    return modifier


def key_mapping(action, key_name, modifiers):
    mapping = unreal.EnhancedActionKeyMapping()
    mapping.set_editor_property("action", action)
    key = unreal.Key()
    key.set_editor_property("key_name", key_name)
    mapping.set_editor_property("key", key)
    mapping.set_editor_property("modifiers", modifiers)
    return mapping


def scalar(owner, value):
    """Multiplies the value of a key, for example to give the key 2 the value of slot 2."""
    modifier = unreal.new_object(unreal.InputModifierScalar, outer=owner)
    modifier.set_editor_property("scalar", unreal.Vector(value, value, value))
    return modifier


def keyboard_mouse_context(move, look, jump, interact, quit_game, fire, change_weapon, select_weapon, melee):
    """The mapping context of the keyboard and the mouse (D-136). No C++ names a key (OQ-21)."""
    context = load_or_create(INPUT_FOLDER, "IMC_KeyboardMouse", unreal.InputMappingContext, unreal.InputMappingContext_Factory())
    mappings = [
        # The move action has X to the right and Y forward.
        key_mapping(move, "W", [swizzle(context)]),
        key_mapping(move, "S", [swizzle(context), negate(context, True, True)]),
        key_mapping(move, "A", [negate(context, True, True)]),
        key_mapping(move, "D", []),
        key_mapping(jump, "SpaceBar", []),
        key_mapping(interact, "E", []),
        # Escape closes the game until the menu of phase 8 (D-150).
        key_mapping(quit_game, "Escape", []),
        # The mouse gives a positive Y when it moves forward. The project turns off the input
        # scales of the engine, so a positive pitch input turns the view up with no modifier (D-139).
        key_mapping(look, "Mouse2D", []),
        # The fire key (D-151). Each step of the wheel takes the next weapon, and the number keys
        # take a slot. The value of the select action is the number of the slot (D-152).
        key_mapping(fire, "LeftMouseButton", []),
        key_mapping(change_weapon, "MouseScrollUp", []),
        key_mapping(change_weapon, "MouseScrollDown", []),
        key_mapping(select_weapon, "One", []),
        key_mapping(select_weapon, "Two", [scalar(context, 2.0)]),
        # The melee key (D-169).
        key_mapping(melee, "F", []),
    ]
    data = unreal.InputMappingContextMappingData()
    data.set_editor_property("mappings", mappings)
    context.set_editor_property("default_key_mappings", data)
    save(context)
    return context


def movement_tuning():
    factory = unreal.DataAssetFactory()
    factory.set_editor_property("data_asset_class", unreal.IronMovementTuning)
    tuning = load_or_create(PLAYER_FOLDER, "DA_PlayerMovement", unreal.IronMovementTuning, factory)
    for name, value in MOVEMENT_TUNING.items():
        tuning.set_editor_property(name, value)
    save(tuning)
    return tuning


def material_node(material, expression_class, x, y, **properties):
    """Adds one node to a material graph, with the given properties."""
    node = unreal.MaterialEditingLibrary.create_material_expression(material, expression_class, x, y)
    if node is None:
        raise ContentError(f"The node {expression_class.__name__} did not come into {material.get_path_name()}.")
    for name, value in properties.items():
        node.set_editor_property(name, value)
    return node


def material_link(source, output, target, target_input):
    """Connects an output of one node to an input of another node. An empty name is the first pin."""
    if not unreal.MaterialEditingLibrary.connect_material_expressions(source, output, target, target_input):
        raise ContentError(f"The output '{output}' of {source.get_name()} did not connect to the input '{target_input}' of {target.get_name()}.")


def custom_depth_at(material, viewport_uv, texel, dx, dy, y):
    """Reads the custom depth one outline width away from the pixel, in the direction dx, dy."""
    offset = material_node(material, unreal.MaterialExpressionConstant2Vector, -1400, y, r=dx * OUTLINE_WIDTH, g=dy * OUTLINE_WIDTH)
    scaled = material_node(material, unreal.MaterialExpressionMultiply, -1200, y)
    material_link(texel, "", scaled, "A")
    material_link(offset, "", scaled, "B")
    uv = material_node(material, unreal.MaterialExpressionAdd, -1000, y)
    material_link(viewport_uv, "ViewportUV", uv, "A")
    material_link(scaled, "", uv, "B")
    depth = material_node(material, unreal.MaterialExpressionSceneTexture, -800, y, scene_texture_id=unreal.SceneTextureId.PPI_CUSTOM_DEPTH)
    material_link(uv, "", depth, "UVs")
    red = material_node(material, unreal.MaterialExpressionComponentMask, -600, y, r=True, g=False, b=False, a=False)
    material_link(depth, "Color", red, "")
    return red


def outline_material():
    """The post-process material of the outline of the cue of interact (D-145, D-148).

    A pixel outside the target is on the outline when a pixel one outline width away holds the
    target in the custom depth. The target is much nearer than the empty custom depth, so the step
    of the depth marks the edge. The material runs before the bloom, with a color above 1, so the
    bloom of the engine makes the outline glow.
    """
    material = load_or_create(PLAYER_FOLDER, "M_InteractOutline", unreal.Material, unreal.MaterialFactoryNew())
    unreal.MaterialEditingLibrary.delete_all_material_expressions(material)
    material.set_editor_property("material_domain", unreal.MaterialDomain.MD_POST_PROCESS)
    material.set_editor_property("blendable_location", unreal.BlendableLocation.BL_SCENE_COLOR_BEFORE_BLOOM)

    viewport_uv = material_node(material, unreal.MaterialExpressionScreenPosition, -1600, -200)
    view_size = material_node(material, unreal.MaterialExpressionViewSize, -1800, 0)
    one = material_node(material, unreal.MaterialExpressionConstant, -1800, 100, r=1.0)
    texel = material_node(material, unreal.MaterialExpressionDivide, -1600, 0)
    material_link(one, "", texel, "A")
    material_link(view_size, "", texel, "B")

    right = custom_depth_at(material, viewport_uv, texel, 1.0, 0.0, 200)
    left = custom_depth_at(material, viewport_uv, texel, -1.0, 0.0, 400)
    down = custom_depth_at(material, viewport_uv, texel, 0.0, 1.0, 600)
    up = custom_depth_at(material, viewport_uv, texel, 0.0, -1.0, 800)
    near_x = material_node(material, unreal.MaterialExpressionMin, -400, 300)
    material_link(right, "", near_x, "A")
    material_link(left, "", near_x, "B")
    near_y = material_node(material, unreal.MaterialExpressionMin, -400, 700)
    material_link(down, "", near_y, "A")
    material_link(up, "", near_y, "B")
    nearest = material_node(material, unreal.MaterialExpressionMin, -250, 500)
    material_link(near_x, "", nearest, "A")
    material_link(near_y, "", nearest, "B")

    center = material_node(material, unreal.MaterialExpressionSceneTexture, -800, -400, scene_texture_id=unreal.SceneTextureId.PPI_CUSTOM_DEPTH)
    center_red = material_node(material, unreal.MaterialExpressionComponentMask, -600, -400, r=True, g=False, b=False, a=False)
    material_link(center, "Color", center_red, "")
    step = material_node(material, unreal.MaterialExpressionSubtract, -100, 0)
    material_link(center_red, "", step, "A")
    material_link(nearest, "", step, "B")

    threshold = material_node(material, unreal.MaterialExpressionConstant, -100, 150, r=OUTLINE_DEPTH_STEP)
    on = material_node(material, unreal.MaterialExpressionConstant, -100, 250, r=1.0)
    off = material_node(material, unreal.MaterialExpressionConstant, -100, 350, r=0.0)
    edge = material_node(material, unreal.MaterialExpressionIf, 100, 0)
    material_link(step, "", edge, "A")
    material_link(threshold, "", edge, "B")
    material_link(on, "", edge, "A > B")
    material_link(off, "", edge, "A == B")
    material_link(off, "", edge, "A < B")

    scene = material_node(material, unreal.MaterialExpressionSceneTexture, -100, -300, scene_texture_id=unreal.SceneTextureId.PPI_POST_PROCESS_INPUT0)
    scene_rgb = material_node(material, unreal.MaterialExpressionComponentMask, 100, -300, r=True, g=True, b=True, a=False)
    material_link(scene, "Color", scene_rgb, "")
    color = material_node(material, unreal.MaterialExpressionConstant3Vector, 100, -150, constant=bright(CUE_COLOR, OUTLINE_BRIGHTNESS))
    mix = material_node(material, unreal.MaterialExpressionLinearInterpolate, 300, 0)
    material_link(scene_rgb, "", mix, "A")
    material_link(color, "", mix, "B")
    material_link(edge, "", mix, "Alpha")
    if not unreal.MaterialEditingLibrary.connect_material_property(mix, "", unreal.MaterialProperty.MP_EMISSIVE_COLOR):
        raise ContentError(f"The outline of {material.get_path_name()} did not connect to the emissive color.")

    unreal.MaterialEditingLibrary.recompile_material(material)
    save(material)
    return material


def bright(color, brightness):
    """Gives the color with each channel times the brightness, for a glow under the bloom."""
    return unreal.LinearColor(r=color.r * brightness, g=color.g * brightness, b=color.b * brightness, a=1.0)


def glow_material():
    """The overlay material of the fill glow of the cue of interact (D-148).

    The engine draws the target a second time with this material, while the target is in reach.
    An additive unlit color adds light to the target, and the Fresnel term makes the edges bright.
    """
    material = load_or_create(PLAYER_FOLDER, "M_InteractGlow", unreal.Material, unreal.MaterialFactoryNew())
    unreal.MaterialEditingLibrary.delete_all_material_expressions(material)
    material.set_editor_property("material_domain", unreal.MaterialDomain.MD_SURFACE)
    material.set_editor_property("blend_mode", unreal.BlendMode.BLEND_ADDITIVE)
    material.set_editor_property("shading_model", unreal.MaterialShadingModel.MSM_UNLIT)

    fresnel = material_node(material, unreal.MaterialExpressionFresnel, -600, 200)
    center = material_node(material, unreal.MaterialExpressionConstant, -600, 0, r=GLOW_CENTER_BRIGHTNESS)
    edge = material_node(material, unreal.MaterialExpressionConstant, -600, 100, r=GLOW_EDGE_BRIGHTNESS)
    brightness = material_node(material, unreal.MaterialExpressionLinearInterpolate, -400, 100)
    material_link(center, "", brightness, "A")
    material_link(edge, "", brightness, "B")
    material_link(fresnel, "", brightness, "Alpha")
    color = material_node(material, unreal.MaterialExpressionConstant3Vector, -400, -100, constant=CUE_COLOR)
    glow = material_node(material, unreal.MaterialExpressionMultiply, -200, 0)
    material_link(color, "", glow, "A")
    material_link(brightness, "", glow, "B")
    if not unreal.MaterialEditingLibrary.connect_material_property(glow, "", unreal.MaterialProperty.MP_EMISSIVE_COLOR):
        raise ContentError(f"The glow of {material.get_path_name()} did not connect to the emissive color.")

    unreal.MaterialEditingLibrary.recompile_material(material)
    save(material)
    return material


def import_sounds():
    """Imports each sound of the weapon from its source file, and gives the assets by name (D-155)."""
    source = os.path.join(unreal.Paths.convert_relative_path_to_full(unreal.Paths.project_dir()), SOUND_SOURCE_FOLDER)
    tasks = []
    for name, file_name in SOUNDS.items():
        path = os.path.join(source, file_name)
        if not os.path.isfile(path):
            raise ContentError(f"The source file {path} of the sound {name} is absent.")
        task = unreal.AssetImportTask()
        task.set_editor_property("filename", path)
        task.set_editor_property("destination_path", SOUND_FOLDER)
        task.set_editor_property("destination_name", name)
        task.set_editor_property("replace_existing", True)
        task.set_editor_property("automated", True)
        task.set_editor_property("save", True)
        tasks.append(task)
    asset_tools().import_asset_tasks(tasks)

    sounds = {}
    for name in SOUNDS:
        path = f"{SOUND_FOLDER}/{name}"
        sound = unreal.load_asset(path)
        if sound is None or not isinstance(sound, unreal.SoundWave):
            raise ContentError(f"The import of {path} gave no sound wave. Read the log of the import.")
        sound.set_editor_property("volume", SOUND_VOLUMES[name])
        save(sound)
        sounds[name] = sound
    return sounds


def hit_flash_material():
    """The material of the flash at the point of a hit (D-154).

    An additive unlit color, times the scalar parameter that the flash sets from 1 down to 0.
    """
    material = load_or_create(WEAPON_FOLDER, "M_HitFlash", unreal.Material, unreal.MaterialFactoryNew())
    unreal.MaterialEditingLibrary.delete_all_material_expressions(material)
    material.set_editor_property("material_domain", unreal.MaterialDomain.MD_SURFACE)
    material.set_editor_property("blend_mode", unreal.BlendMode.BLEND_ADDITIVE)
    material.set_editor_property("shading_model", unreal.MaterialShadingModel.MSM_UNLIT)

    color = material_node(material, unreal.MaterialExpressionConstant3Vector, -400, -100, constant=bright(FLASH_COLOR, FLASH_BRIGHTNESS))
    brightness = material_node(material, unreal.MaterialExpressionScalarParameter, -400, 100, parameter_name=FLASH_PARAMETER, default_value=1.0)
    glow = material_node(material, unreal.MaterialExpressionMultiply, -200, 0)
    material_link(color, "", glow, "A")
    material_link(brightness, "", glow, "B")
    if not unreal.MaterialEditingLibrary.connect_material_property(glow, "", unreal.MaterialProperty.MP_EMISSIVE_COLOR):
        raise ContentError(f"The flash of {material.get_path_name()} did not connect to the emissive color.")

    unreal.MaterialEditingLibrary.recompile_material(material)
    save(material)
    return material


def hit_flash(material):
    """The Blueprint of the flash at the point of a hit (D-154)."""
    sphere = unreal.load_asset(SPHERE)
    if sphere is None:
        raise ContentError(f"The mesh {SPHERE} did not load.")
    flash, flash_defaults = blueprint("BP_HitFlash", unreal.IronHitFlash, WEAPON_FOLDER)
    flash_defaults.set_editor_property("flash_mesh", sphere)
    flash_defaults.set_editor_property("flash_material", material)
    flash_defaults.set_editor_property("diameter", FLASH_DIAMETER)
    flash_defaults.set_editor_property("lifetime", FLASH_LIFETIME)
    save(flash)
    return flash


def weapon_tunings(sounds, flash):
    """The two data assets of the weapon rule, in the order of the slots (D-128, D-153)."""
    mesh = unreal.load_asset(CUBE)
    if mesh is None:
        raise ContentError(f"The mesh {CUBE} did not load.")
    factory = unreal.DataAssetFactory()
    factory.set_editor_property("data_asset_class", unreal.IronWeaponTuning)
    tunings = []
    for name, values in WEAPON_TUNING.items():
        tuning = load_or_create(WEAPON_FOLDER, name, unreal.IronWeaponTuning, factory)
        for key, value in values.items():
            if key == "display_name":
                tuning.set_editor_property(key, unreal.Text(value))
            elif key == "shot_sound":
                tuning.set_editor_property(key, sounds[value])
            else:
                tuning.set_editor_property(key, value)
        tuning.set_editor_property("empty_sound", sounds["S_EmptyClick"])
        tuning.set_editor_property("hit_flash_class", flash.generated_class())
        tuning.set_editor_property("view_mesh", mesh)
        save(tuning)
        tunings.append(tuning)
    return tunings


def melee_tuning(flash):
    """The data asset of the melee attack (D-170). The flash of a hit is the flash of a shot (D-171)."""
    factory = unreal.DataAssetFactory()
    factory.set_editor_property("data_asset_class", unreal.IronMeleeTuning)
    tuning = load_or_create(WEAPON_FOLDER, MELEE_ASSET, unreal.IronMeleeTuning, factory)
    for key, value in MELEE_TUNING.items():
        tuning.set_editor_property(key, value)
    tuning.set_editor_property("hit_flash_class", flash.generated_class())
    save(tuning)
    return tuning


def blueprint(name, parent_class, folder=PLAYER_FOLDER):
    """Loads or makes a Blueprint subclass, and gives its default object."""
    factory = unreal.BlueprintFactory()
    factory.set_editor_property("parent_class", parent_class)
    asset = load_or_create(folder, name, unreal.Blueprint, factory)
    generated = asset.generated_class()
    if generated is None:
        raise ContentError(f"The Blueprint {asset.get_path_name()} has no generated class.")
    return asset, unreal.get_default_object(generated)


def player_blueprints(tuning, actions, outline, glow, context, weapons, melee):
    character, character_defaults = blueprint("BP_PlayerCharacter", unreal.IronPlayerCharacter)
    character_defaults.set_editor_property("movement_tuning", tuning)
    character_defaults.set_editor_property("move_action", actions["move"])
    character_defaults.set_editor_property("look_action", actions["look"])
    character_defaults.set_editor_property("jump_action", actions["jump"])
    character_defaults.set_editor_property("interact_action", actions["interact"])
    character_defaults.set_editor_property("fire_action", actions["fire"])
    character_defaults.set_editor_property("change_weapon_action", actions["change_weapon"])
    character_defaults.set_editor_property("select_weapon_action", actions["select_weapon"])
    character_defaults.set_editor_property("melee_action", actions["melee"])
    character_defaults.set_editor_property("interact_outline_material", outline)
    character_defaults.set_editor_property("interact_glow_material", glow)
    character_defaults.set_editor_property("weapons", weapons)
    character_defaults.set_editor_property("melee_tuning", melee)
    save(character)

    hud, hud_defaults = blueprint("BP_PlayerHUD", unreal.IronHUD)
    for name, value in HUD_VALUES.items():
        hud_defaults.set_editor_property(name, value)
    save(hud)

    controller, controller_defaults = blueprint("BP_PlayerController", unreal.IronPlayerController)
    controller_defaults.set_editor_property("mapping_contexts", [context])
    controller_defaults.set_editor_property("quit_action", actions["quit"])
    save(controller)

    game_mode, game_mode_defaults = blueprint("BP_PlayerGameMode", unreal.GameModeBase)
    game_mode_defaults.set_editor_property("default_pawn_class", character.generated_class())
    game_mode_defaults.set_editor_property("player_controller_class", controller.generated_class())
    game_mode_defaults.set_editor_property("hud_class", hud.generated_class())
    save(game_mode)
    return game_mode


def open_empty_gym():
    """Opens the gym map, or makes it, and removes each actor that an earlier run placed."""
    levels = unreal.get_editor_subsystem(unreal.LevelEditorSubsystem)
    if asset_subsystem().does_asset_exist(GYM_MAP):
        if not levels.load_level(GYM_MAP):
            raise ContentError(f"The map {GYM_MAP} did not load.")
    elif not levels.new_level(GYM_MAP, False):
        # A plain level with no World Partition, as the test map (D-84).
        raise ContentError(f"The map {GYM_MAP} did not come from new_level.")

    actors = unreal.get_editor_subsystem(unreal.EditorActorSubsystem)
    old = [actor for actor in actors.get_all_level_actors() if not isinstance(actor, (unreal.WorldSettings, unreal.Brush))]
    if old and not actors.destroy_actors(old):
        raise ContentError(f"The old actors of {GYM_MAP} did not go away.")
    return actors


def spawn(actors, actor_class, location, rotation=None, label=None):
    actor = actors.spawn_actor_from_class(actor_class, location, rotation or unreal.Rotator(0.0, 0.0, 0.0))
    if actor is None:
        raise ContentError(f"The actor {actor_class.__name__} did not spawn at {location}.")
    if label is not None:
        actor.set_actor_label(label)
    return actor


def block(actors, label, center, size, rotation=None, material=None):
    """Places a cube of the engine with its center and its size in centimeters."""
    mesh = unreal.load_asset(CUBE)
    if mesh is None:
        raise ContentError(f"The mesh {CUBE} did not load.")
    actor = spawn(actors, unreal.StaticMeshActor, center, rotation, label)
    component = actor.get_editor_property("static_mesh_component")
    component.set_static_mesh(mesh)
    if material is not None:
        component.set_material(0, material)
    actor.set_actor_scale3d(unreal.Vector(size.x / CUBE_SIZE, size.y / CUBE_SIZE, size.z / CUBE_SIZE))
    return actor


def text(actors, words, location, yaw=180.0):
    """Places a label. The default yaw faces the player at the start, who looks along +X."""
    actor = spawn(actors, unreal.TextRenderActor, location, unreal.Rotator(0.0, 0.0, yaw), f"Label {words}")
    component = actor.get_editor_property("text_render")
    component.set_text(words)
    component.set_world_size(LABEL_HEIGHT)
    component.set_horizontal_alignment(unreal.HorizTextAligment.EHTA_CENTER)
    component.set_text_render_color(LABEL_COLOR)
    return actor


def floor_and_light(actors):
    material = unreal.load_asset(FLOOR_MATERIAL)
    if material is None:
        raise ContentError(f"The material {FLOOR_MATERIAL} did not load.")
    # The top of the floor is at Z 0, so each height of a station is its height above the floor.
    # The floor runs from X -3200 to 6800, past the end of the longest row, the gap row.
    block(actors, "Floor", unreal.Vector(1800.0, 0.0, -50.0), unreal.Vector(10000.0, 4000.0, 100.0), material=material)

    sun = spawn(actors, unreal.DirectionalLight, unreal.Vector(0.0, 0.0, 1000.0), unreal.Rotator(0.0, -50.0, 30.0), "Sun")
    sun.get_editor_property("light_component").set_editor_property("atmosphere_sun_light", True)
    sky_light = spawn(actors, unreal.SkyLight, unreal.Vector(0.0, 0.0, 800.0), label="Sky light")
    sky_light.get_editor_property("light_component").set_editor_property("real_time_capture", True)
    spawn(actors, unreal.SkyAtmosphere, unreal.Vector(0.0, 0.0, 0.0), label="Sky atmosphere")
    spawn(actors, unreal.ExponentialHeightFog, unreal.Vector(0.0, 0.0, 0.0), label="Height fog")
    spawn(actors, unreal.PlayerStart, PLAYER_START, label="Player start")


def gap_row(actors):
    """Pairs of platforms 40 cm high, with a gap between them. The player jumps each gap."""
    x = ROW_START_X
    for gap in (200, 300, 400, 500, 600, 700, 800):
        size = unreal.Vector(200.0, 300.0, 40.0)
        block(actors, f"Gap {gap} take-off", unreal.Vector(x + 100.0, GAP_ROW_Y, 20.0), size)
        block(actors, f"Gap {gap} landing", unreal.Vector(x + 200.0 + gap + 100.0, GAP_ROW_Y, 20.0), size)
        text(actors, f"Gap {gap} cm", unreal.Vector(x, GAP_ROW_Y, 150.0))
        x += 400.0 + gap + 200.0


def ledge_row(actors):
    """Blocks of set heights. The player jumps up onto each one, or jumps and climbs it (D-143)."""
    x = ROW_START_X
    for height in LEDGE_HEIGHTS:
        block(actors, f"Ledge {height}", unreal.Vector(x + 100.0, LEDGE_ROW_Y, height / 2.0), unreal.Vector(200.0, 300.0, float(height)))
        text(actors, f"Ledge {height} cm", unreal.Vector(x - 10.0, LEDGE_ROW_Y, height + 40.0))
        x += 600.0


def distance_row(actors):
    """Thin lines on the floor each 500 cm along +X, to read the run speed."""
    for index in range(1, 11):
        distance = index * 500
        x = PLAYER_START.x + distance
        block(actors, f"Distance {distance}", unreal.Vector(x, DISTANCE_ROW_Y, 0.5), unreal.Vector(10.0, 300.0, 1.0))
        text(actors, f"{distance // 100} m", unreal.Vector(x - 10.0, DISTANCE_ROW_Y, 60.0))


def step_row(actors):
    """Stairs of five steps with set step heights, then ramps with set slopes."""
    x = ROW_START_X
    depth = 40.0
    for rise in (15, 30, 45, 60):
        for step in range(1, 6):
            height = float(rise * step)
            block(actors, f"Step {rise} number {step}", unreal.Vector(x + depth * (step - 0.5), STEP_ROW_Y, height / 2.0), unreal.Vector(depth, 300.0, height))
        text(actors, f"Step {rise} cm", unreal.Vector(x - 10.0, STEP_ROW_Y, rise * 5 + 60.0))
        x += 600.0

    length = 400.0
    thickness = 20.0
    for slope in (30, 40, 45, 50):
        # The ramp turns about Y, so its low end touches the floor at x and it rises along +X.
        cos = math.cos(math.radians(slope))
        sin = math.sin(math.radians(slope))
        center = unreal.Vector(x + cos * length / 2.0, STEP_ROW_Y, sin * length / 2.0 - cos * thickness / 2.0)
        block(actors, f"Ramp {slope}", center, unreal.Vector(length, 300.0, thickness), unreal.Rotator(0.0, float(slope), 0.0))
        text(actors, f"Ramp {slope} deg", unreal.Vector(x - 10.0, STEP_ROW_Y, 120.0))
        x += 600.0


def hall_row(actors):
    """Pairs of walls 300 cm high and 600 cm long, with a set width between them."""
    x = ROW_START_X
    wall = unreal.Vector(600.0, 20.0, 300.0)
    for width in (100, 150, 200, 300, 400):
        half = width / 2.0 + wall.y / 2.0
        block(actors, f"Hall {width} left", unreal.Vector(x + 300.0, HALL_ROW_Y - half, 150.0), wall)
        block(actors, f"Hall {width} right", unreal.Vector(x + 300.0, HALL_ROW_Y + half, 150.0), wall)
        text(actors, f"Hall {width} cm", unreal.Vector(x - 10.0, HALL_ROW_Y, 340.0))
        x += 900.0


def door_station(actors):
    """A wall with a test door and a test switch. Each use of the switch toggles the door (D-146)."""
    half_door = DOOR_SIZE.x / 2.0
    wall_length = 400.0
    for name, x in (("west", DOOR_CENTER_X - half_door - wall_length / 2.0), ("east", DOOR_CENTER_X + half_door + wall_length / 2.0)):
        block(actors, f"Door wall {name}", unreal.Vector(x, DOOR_WALL_Y, DOOR_WALL_HEIGHT / 2.0), unreal.Vector(wall_length, DOOR_SIZE.y, DOOR_WALL_HEIGHT))
    lintel_height = DOOR_WALL_HEIGHT - DOOR_SIZE.z
    block(actors, "Door lintel", unreal.Vector(DOOR_CENTER_X, DOOR_WALL_Y, DOOR_SIZE.z + lintel_height / 2.0), unreal.Vector(DOOR_SIZE.x, DOOR_SIZE.y, lintel_height))

    mesh = unreal.load_asset(CUBE)
    if mesh is None:
        raise ContentError(f"The mesh {CUBE} did not load.")
    # The closed panel is at the place of the door, so the door stands at the center of the panel.
    door = spawn(actors, unreal.IronDoor, unreal.Vector(DOOR_CENTER_X, DOOR_WALL_Y, DOOR_SIZE.z / 2.0), label="Test door")
    door.set_editor_property("open_offset", DOOR_OPEN_OFFSET)
    panel = door.get_editor_property("panel")
    panel.set_static_mesh(mesh)
    panel.set_relative_scale3d(unreal.Vector(DOOR_SIZE.x / CUBE_SIZE, DOOR_SIZE.y / CUBE_SIZE, DOOR_SIZE.z / CUBE_SIZE))

    switch = spawn(actors, unreal.IronSwitch, SWITCH_LOCATION, label="Test switch")
    switch.get_editor_property("button").set_static_mesh(mesh)
    switch.set_actor_scale3d(unreal.Vector(SWITCH_SIZE / CUBE_SIZE, SWITCH_SIZE / CUBE_SIZE, SWITCH_SIZE / CUBE_SIZE))
    switch.set_editor_property("door", door)

    # The labels face the player, who comes from the start at Y 0 and looks along +Y.
    text(actors, "Door", unreal.Vector(DOOR_CENTER_X, DOOR_WALL_Y - 20.0, DOOR_WALL_HEIGHT + 30.0), yaw=-90.0)
    text(actors, "Switch: E", unreal.Vector(SWITCH_LOCATION.x, SWITCH_LOCATION.y - 10.0, SWITCH_LOCATION.z + 40.0), yaw=-90.0)


def target_station(actors):
    """Three gym targets that count the hits of shots and of melee attacks, and the ammo station (D-154, D-156, D-172)."""
    mesh = unreal.load_asset(CUBE)
    if mesh is None:
        raise ContentError(f"The mesh {CUBE} did not load.")
    for distance in TARGET_DISTANCES:
        x = PLAYER_START.x + distance * 100.0
        # The root of the target is on the floor. The yaw faces the count to the player at the start.
        target = spawn(actors, unreal.IronTarget, unreal.Vector(x, TARGET_ROW_Y, 0.0), unreal.Rotator(0.0, 0.0, 180.0), f"Target {distance} m")
        board = target.get_editor_property("board")
        board.set_static_mesh(mesh)
        board.set_relative_scale3d(unreal.Vector(TARGET_SIZE.x / CUBE_SIZE, TARGET_SIZE.y / CUBE_SIZE, TARGET_SIZE.z / CUBE_SIZE))
        board.set_relative_location(unreal.Vector(0.0, 0.0, TARGET_SIZE.z / 2.0), False, False)
        count = target.get_editor_property("count_text")
        count.set_relative_location(unreal.Vector(0.0, 0.0, TARGET_SIZE.z + 20.0), False, False)
        count.set_world_size(LABEL_HEIGHT)
        count.set_text_render_color(LABEL_COLOR)
        # The two lines of the counts go up from their place, so the label is above the second line.
        text(actors, f"Target {distance} m", unreal.Vector(x, TARGET_ROW_Y, TARGET_SIZE.z + 40.0 + 2.0 * LABEL_HEIGHT))

    station = spawn(actors, unreal.IronAmmoStation, AMMO_STATION_LOCATION, label="Ammo station")
    station.get_editor_property("body").set_static_mesh(mesh)
    station.set_actor_scale3d(unreal.Vector(AMMO_STATION_SIZE.x / CUBE_SIZE, AMMO_STATION_SIZE.y / CUBE_SIZE, AMMO_STATION_SIZE.z / CUBE_SIZE))
    text(actors, "Ammo: E", unreal.Vector(AMMO_STATION_LOCATION.x, AMMO_STATION_LOCATION.y, AMMO_STATION_SIZE.z + 40.0))


def default_field_of_view():
    """Reads the default field of view of the project from its config file (D-141)."""
    path = os.path.join(unreal.Paths.convert_relative_path_to_full(unreal.Paths.project_config_dir()), AIM_DEFAULTS_FILE)
    parser = configparser.ConfigParser(interpolation=None)
    if not parser.read(path, encoding="utf-8"):
        raise ContentError(f"The config file {path} is absent, so the views have no field of view.")
    if not parser.has_option(AIM_DEFAULTS_SECTION, "FieldOfView"):
        raise ContentError(f"The config file {path} has no FieldOfView in the section [{AIM_DEFAULTS_SECTION}].")
    return parser.getfloat(AIM_DEFAULTS_SECTION, "FieldOfView")


def frame_time_views(actors):
    """Places the views of the frame-time capture, with the default field of view of the player."""
    field_of_view = default_field_of_view()
    for order, (name, location, rotation) in enumerate(FRAME_TIME_VIEWS, start=1):
        view = spawn(actors, unreal.FrameTimeView, location, rotation, f"Frame-time view {order}: {name}")
        view.set_editor_property("order", order)
        view.get_editor_property("camera_component").set_editor_property("field_of_view", field_of_view)


def gym(game_mode):
    actors = open_empty_gym()
    floor_and_light(actors)
    gap_row(actors)
    ledge_row(actors)
    distance_row(actors)
    step_row(actors)
    hall_row(actors)
    door_station(actors)
    target_station(actors)
    frame_time_views(actors)

    world = unreal.get_editor_subsystem(unreal.UnrealEditorSubsystem).get_editor_world()
    if world is None:
        raise ContentError(f"The map {GYM_MAP} has no editor world.")
    world.get_world_settings().set_editor_property("default_game_mode", game_mode.generated_class())

    if not unreal.get_editor_subsystem(unreal.LevelEditorSubsystem).save_current_level():
        raise ContentError(f"The map {GYM_MAP} did not save.")


def main():
    move = input_action("IA_Move", unreal.InputActionValueType.AXIS2D)
    look = input_action("IA_Look", unreal.InputActionValueType.AXIS2D)
    jump = input_action("IA_Jump", unreal.InputActionValueType.BOOLEAN)
    interact = input_action("IA_Interact", unreal.InputActionValueType.BOOLEAN)
    quit_game = input_action("IA_Quit", unreal.InputActionValueType.BOOLEAN)
    fire = input_action("IA_Fire", unreal.InputActionValueType.BOOLEAN)
    change_weapon = input_action("IA_ChangeWeapon", unreal.InputActionValueType.BOOLEAN)
    select_weapon = input_action("IA_SelectWeapon", unreal.InputActionValueType.AXIS1D)
    melee = input_action("IA_Melee", unreal.InputActionValueType.BOOLEAN)
    context = keyboard_mouse_context(move, look, jump, interact, quit_game, fire, change_weapon, select_weapon, melee)
    actions = {
        "move": move,
        "look": look,
        "jump": jump,
        "interact": interact,
        "quit": quit_game,
        "fire": fire,
        "change_weapon": change_weapon,
        "select_weapon": select_weapon,
        "melee": melee,
    }
    tuning = movement_tuning()
    outline = outline_material()
    glow = glow_material()
    sounds = import_sounds()
    flash = hit_flash(hit_flash_material())
    weapons = weapon_tunings(sounds, flash)
    melee_attack = melee_tuning(flash)
    game_mode = player_blueprints(tuning, actions, outline, glow, context, weapons, melee_attack)
    gym(game_mode)
    unreal.log("build_content: pass. The input, the player, the weapons, the melee attack, and the gym map saved.")


main()
