namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The hurt flash's calls (#2111): which of the flash's counted calls show its colours, play the impact, recover the
/// interrupted movement sound and end it.
/// </summary>
public static class SamusHurtFlashDefinitions
{
    /// <summary>The call on which the flash plays the impact sound.</summary>
    public const int ImpactSoundCall = 2;

    /// <summary>The calls, counted from the first, that show the flash's or the restored suit's colours; later calls leave the colours alone.</summary>
    public const int ColouredCalls = 6;

    /// <summary>The count, reached by a call, at which the flash restores the movement sound the hurt interrupted.</summary>
    public const int SoundRecoveryCount = 40;

    /// <summary>The hurt-flash count at which the flash ends and the count is cleared: its largest value.</summary>
    public const int EndingCall = 60;
}
