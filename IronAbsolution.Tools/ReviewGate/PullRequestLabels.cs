using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace IronAbsolution.Tools.ReviewGate;

/// <summary>One labeled event of the override label, from the PR timeline (D-65).</summary>
/// <param name="CreatedAt">The time of the event.</param>
/// <param name="Actor">The login of the account that added the label.</param>
public sealed record LabelEvent(DateTimeOffset CreatedAt, string Actor);

/// <summary>
/// The labels of a PR, and each labeled event of the override label. The workflow reads both
/// from the GitHub API and writes them to one JSON file (D-64, D-65). Every field is required.
/// An absent field is an error, never an empty list (T-2).
/// </summary>
/// <param name="Names">The name of each label on the PR.</param>
/// <param name="OverrideEvents">Each labeled event of the override label, in any order.</param>
public sealed record PullRequestLabels(IReadOnlyList<string> Names, IReadOnlyList<LabelEvent> OverrideEvents)
{
    /// <summary>The field of the label names.</summary>
    public const string LabelsField = "labels";

    /// <summary>The field of the labeled events of the override label.</summary>
    public const string EventsField = "overrideLabelEvents";

    /// <summary>Reads the labels from the JSON file of the workflow.</summary>
    /// <param name="filePath">The path of the file.</param>
    /// <returns>The labels and the events.</returns>
    /// <exception cref="FileNotFoundException">The file is absent. The message names the path.</exception>
    /// <exception cref="InvalidDataException">The file is not of the expected form. The message names the path and the field.</exception>
    public static PullRequestLabels Read(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"The labels file '{filePath}' does not exist. The workflow writes it from the GitHub API before the gate runs.", filePath);
        }

        return Parse(File.ReadAllText(filePath), filePath);
    }

    /// <summary>Reads the labels from JSON text.</summary>
    /// <param name="json">The JSON text.</param>
    /// <param name="source">The name of the source, for the message of a fault.</param>
    /// <returns>The labels and the events.</returns>
    /// <exception cref="InvalidDataException">The text is not of the expected form. The message names the source and the field.</exception>
    public static PullRequestLabels Parse(string json, string source)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentException.ThrowIfNullOrEmpty(source);

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException fault)
        {
            throw new InvalidDataException($"The labels file '{source}' is not valid JSON: {fault.Message}", fault);
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidDataException($"The labels file '{source}' holds a JSON {root.ValueKind}. The expected form is an object.");
            }

            List<string> names = [];
            foreach (JsonElement name in RequiredArray(root, LabelsField, source))
            {
                names.Add(RequiredText(name, $"{LabelsField}[{names.Count}]", source));
            }

            List<LabelEvent> events = [];
            foreach (JsonElement labelEvent in RequiredArray(root, EventsField, source))
            {
                events.Add(ReadEvent(labelEvent, $"{EventsField}[{events.Count}]", source));
            }

            return new PullRequestLabels(names, events);
        }
    }

    private static LabelEvent ReadEvent(JsonElement element, string field, string source)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException($"The field '{field}' of the labels file '{source}' holds a JSON {element.ValueKind}. The expected form is an object.");
        }

        string createdAtText = RequiredText(RequiredProperty(element, "createdAt", field, source), $"{field}.createdAt", source);
        if (!DateTimeOffset.TryParse(createdAtText, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset createdAt))
        {
            throw new InvalidDataException($"The field '{field}.createdAt' of the labels file '{source}' holds '{createdAtText}'. The expected form is an ISO 8601 time.");
        }

        string actor = RequiredText(RequiredProperty(element, "actor", field, source), $"{field}.actor", source);
        return new LabelEvent(createdAt, actor);
    }

    private static JsonElement.ArrayEnumerator RequiredArray(JsonElement root, string field, string source)
    {
        JsonElement value = RequiredProperty(root, field, "the root", source);
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidDataException($"The field '{field}' of the labels file '{source}' holds a JSON {value.ValueKind}. The expected form is an array.");
        }

        return value.EnumerateArray();
    }

    private static JsonElement RequiredProperty(JsonElement element, string property, string owner, string source)
    {
        if (!element.TryGetProperty(property, out JsonElement value))
        {
            throw new InvalidDataException($"The labels file '{source}' has no field '{property}' in {owner}.");
        }

        return value;
    }

    private static string RequiredText(JsonElement element, string field, string source)
    {
        string? text = element.ValueKind == JsonValueKind.String ? element.GetString() : null;
        if (string.IsNullOrEmpty(text))
        {
            throw new InvalidDataException($"The field '{field}' of the labels file '{source}' holds a JSON {element.ValueKind} with no text. The expected form is a string that is not empty.");
        }

        return text;
    }
}
