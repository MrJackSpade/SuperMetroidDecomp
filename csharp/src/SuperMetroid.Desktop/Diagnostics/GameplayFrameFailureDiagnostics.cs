using System.Reflection;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Desktop.Diagnostics;

/// <summary>One shared diagnostic payload for fatal console logs and opt-in GitHub reports.</summary>
internal static class GameplayFrameFailureDiagnostics
{
    /// <summary>Combines build identities, attempted input, recording location, and before/partial-failure frame states for a report.</summary>
    internal static string Format(GameplaySnapshot before, GameplaySnapshot failed, ControllerButton attemptedInput,
        string? recordingPath, int? recordingIndex) =>
        $"Build: {typeof(GameplayFrameFailureDiagnostics).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown"}\n" +
        $"Core build ID: {typeof(SuperMetroidGame).Module.ModuleVersionId}\n" +
        $"Host build ID: {typeof(GameplayFrameFailureDiagnostics).Module.ModuleVersionId}\n" +
        $"Attempted input: {attemptedInput}\n" +
        $"Input recording: {recordingPath ?? "unavailable"}\n" +
        $"Recording input index (zero-based): {recordingIndex?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "unavailable"}\n" +
        $"Before frame: {Describe(before)}\n" +
        $"Failed frame (may be partially mutated): {Describe(failed)}";

    /// <summary>One line naming where the game was and what Samus was doing in <paramref name="snapshot"/>.</summary>
    internal static string Describe(GameplaySnapshot snapshot)
    {
        Ensure.NotNull(snapshot);
        string location = $"frame=${snapshot.FrameNumber:X4}; state={snapshot.GameState}; " +
            $"room={snapshot.Room}; roomState={snapshot.RoomState}; door={snapshot.Door}";
        if (snapshot.Samus is not { } samus)
            return location + "; Samus=not initialized";
        return location + $"; pose={samus.Pose}; animationFrame={samus.AnimationFrame}; animationTimer={samus.AnimationFramesRemaining}; " +
            $"animatedPose={samus.AnimatedPose}; position={samus.Position.X},{samus.Position.Y}; " +
            $"input={samus.HeldButtons}; newlyPressed={samus.NewlyPressedButtons}; inputLocked={samus.InputLocked}; " +
            $"drainedPhase={samus.DrainedPhase}; drainedHandler={samus.DrainedGetUp}; " +
            $"escapeTimer={samus.EscapeTimer}; fx={samus.Fx}; medium={samus.Medium}; " +
            $"waterY={Surface(samus.WaterSurfaceY)}; acidY={Surface(samus.LavaAcidSurfaceY)}";
    }

    /// <summary>A liquid surface's height in hexadecimal, or "none" when the room has no such surface.</summary>
    private static string Surface(int? surface) =>
        surface is int y ? $"${y.ToString("X4", System.Globalization.CultureInfo.InvariantCulture)}" : "none";
}
