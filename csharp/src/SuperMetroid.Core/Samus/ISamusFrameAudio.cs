namespace SuperMetroid.Core.Samus;

/// <summary>The frame's audio callbacks that normal beta's tail calls.</summary>
internal interface ISamusFrameAudio
{
    /// <summary>The synchronous speed-booster echo call, returning its accumulator; none publishes it after the frame.</summary>
    Func<ushort>? QueueEchoSound { get; }

    /// <summary>The admitted low-health check; none when this frame makes no check.</summary>
    Action? CheckLowHealth { get; }
}
