using System.Reflection;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Desktop;

/// <summary>One shared diagnostic payload for fatal console logs and opt-in GitHub reports.</summary>
internal static class GameplayFrameFailureDiagnostics
{
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
