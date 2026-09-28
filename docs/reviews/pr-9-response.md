# PR-9 response

Round 1 reviewed the effective head `d57ed3312cb3479166ab48858287cdd862443cc2` with the verdict `Changes required`.

### P2-1
- Disposition: full merit.
- Evidence: the trigger reproduces. Under `$ErrorActionPreference = 'Stop'`, `ConvertFrom-Json` on a malformed `Build.version` ended the script before the engine pin, Git LFS, and the total. The same fault class was in the Visual Studio block: a missing `ProgramFiles(x86)` variable or a bad `vswhere` result ended the script. That block breaks the same contract of one line for each pin (T-2, D-72), so the correction covers it too.
- Correction: the commit of this response, `scripts/toolchain-check.ps1`. Each source has a `Get-` function that throws a message with the path and the cause. The caller turns each throw into a failed pin, and then reports the next pin. The engine read now requires a whole number in each of the three fields, as the Mac command does. The engine path uses `Join-Path` for each part, so the script runs under PowerShell on each platform.
- Regression check: the new `ToolchainScriptTests` runs the script under PowerShell with a temporary engine folder: four malformed files, an absent file, the pinned file, and no Visual Studio. Each failure case asserts the engine line, the Git LFS line, and the total. On the script of `d57ed33`, all 7 cases fail. On the new script, all 7 pass. A portable PowerShell 7.6.6 ran both checks on the Mac. The hosted Ubuntu runner has PowerShell, so CI runs the tests. A local run with no PowerShell skips them with the reason, and CI fails when PowerShell is absent.

### P2-2
- Disposition: full merit.
- Evidence: the PR-9 list in `docs/roadmaps/phase-1-engine-proof.md` had two lines with the number 8.
- Correction: the commit of this response. The hosted-tests line is 9, and M-1 and M-2 are 10 and 11. Exit test 8 stays the cache test, so each citation of it stays true.
- Regression check: the PR-9 list reads 1 to 11 with no repeat. `make ste-check` gives 0 findings.

New ids: none. The final PR head is the commit of this response.
