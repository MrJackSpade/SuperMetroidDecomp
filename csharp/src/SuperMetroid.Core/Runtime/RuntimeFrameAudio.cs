using SuperMetroid.Core.Samus;
namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Whether one runtime frame runs Samus's low-health check after her periodic damage (#633, #2092): gameplay frames do,
/// attract playback and an ordinary host frame do not. The Speed Booster's echo always joins Samus's own audio requests
/// as her animation reaches its cue, so no frame configures it.
/// </summary>
public sealed class RuntimeFrameAudio : ISamusFrameAudio
{
    /// <summary>Only the two named frames exist.</summary>
    private RuntimeFrameAudio()
    {
    }

    /// <summary>Gameplay: the low-health check runs after Samus's periodic damage.</summary>
    public static RuntimeFrameAudio WithLowHealthCheck { get; } = new() { ChecksLowHealth = true };

    /// <summary>Attract playback and an ordinary host frame: no low-health check runs.</summary>
    public static RuntimeFrameAudio WithoutLowHealthCheck { get; } = new();

    /// <summary>Whether the frame runs Samus's low-health check.</summary>
    public bool ChecksLowHealth { get; private init; }
}
