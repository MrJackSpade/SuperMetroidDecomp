namespace SuperMetroid.Core.Samus;

/// <summary>The frame's audio callbacks Samus's frame update calls after her movement: the speed booster's echo and the low-health check.</summary>
internal interface ISamusFrameAudio
{
    /// <summary>The synchronous speed-booster echo call, returning what the queue did with it; none publishes it after the frame.</summary>
    Func<SoundQueueAdmission>? QueueEchoSound { get; }

    /// <summary>The admitted low-health check; none when this frame makes no check.</summary>
    Action? CheckLowHealth { get; }
}
