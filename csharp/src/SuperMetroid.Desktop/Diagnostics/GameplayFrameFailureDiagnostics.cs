using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Desktop.Diagnostics;

/// <summary>One shared diagnostic payload for fatal console logs and opt-in GitHub reports.</summary>
internal static class GameplayFrameFailureDiagnostics
{
    /// <summary>Combines build identities, attempted input, recording location, and before/partial-failure frame states for a report.</summary>
    internal static string Format(string before, string failed, SnesButton attemptedInput,
        string? recordingPath, int? recordingIndex) =>
        $"Build: {typeof(GameplayFrameFailureDiagnostics).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown"}\n" +
        $"Core build ID: {typeof(SuperMetroidGame).Module.ModuleVersionId}\n" +
        $"Host build ID: {typeof(GameplayFrameFailureDiagnostics).Module.ModuleVersionId}\n" +
        $"Attempted input: ${attemptedInput:X}\n" +
        $"Input recording: {recordingPath ?? "unavailable"}\n" +
        $"Recording input index (zero-based): {recordingIndex?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unavailable"}\n" +
        $"Before frame: {before}\n" +
        $"Failed frame (may be partially mutated): {failed}";
}
