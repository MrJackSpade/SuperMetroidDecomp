namespace SuperMetroid.Core.Samus;

/// <summary>What Samus's frame update asks of the frame after her movement (#2092): whether her low-health check runs.</summary>
internal interface ISamusFrameAudio
{
    /// <summary>Whether the frame runs Samus's low-health check.</summary>
    bool ChecksLowHealth { get; }
}
