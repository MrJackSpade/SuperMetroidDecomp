namespace SuperMetroid.Core.Audio;

/// <summary>One sound-effect library's command as a driver poll acted on it.</summary>
/// <param name="Command">The library-relative request latched at the previous poll, now echoed to the CPU: zero for none, otherwise a sound number from 1 through 255.</param>
/// <param name="Changed">Whether it differs from the value acted on at the previous poll; the driver starts a sound only for a changed nonzero command.</param>
public readonly record struct SpcSoundEffectPoll(int Command, bool Changed)
{
    /// <summary>The latched request, validated to the range a sound-effect library can carry.</summary>
    public int Command { get; } = Ensure.BetweenInclusive(0, byte.MaxValue, Command);
}
