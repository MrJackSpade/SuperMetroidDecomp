using System.Reflection;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop;

/// <summary>One shared diagnostic payload for fatal console logs and opt-in GitHub reports.</summary>
internal static class GameplayFrameFailureDiagnostics
{
    /// <summary>Builds the diagnostic text shared by fatal frame logs and recoverable failure reports.</summary>
    /// <param name="before">Gameplay context captured before the attempted update.</param>
    /// <param name="failed">Context captured after the update failed, which may reflect partial mutation.</param>
    /// <param name="attemptedInput">Controller bitmask supplied to the failed update.</param>
    /// <param name="recordingPath">Path or description of the input recording, when available.</param>
    /// <param name="recordingIndex">Zero-based recording input index associated with the attempted update, when available.</param>
    /// <returns>A report containing build identities, input details, and before/after gameplay contexts.</returns>
    internal static string Format(string before, string failed, ushort attemptedInput,
        string? recordingPath, int? recordingIndex) =>
        $"Build: {typeof(GameplayFrameFailureDiagnostics).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown"}\n" +
        $"Core build ID: {typeof(SuperMetroidGame).Module.ModuleVersionId}\n" +
        $"Host build ID: {typeof(GameplayFrameFailureDiagnostics).Module.ModuleVersionId}\n" +
        $"Attempted input: ${attemptedInput:X4}\n" +
        $"Input recording: {recordingPath ?? "unavailable"}\n" +
        $"Recording input index (zero-based): {recordingIndex?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unavailable"}\n" +
        $"Before frame: {before}\n" +
        $"Failed frame (may be partially mutated): {failed}";
}
