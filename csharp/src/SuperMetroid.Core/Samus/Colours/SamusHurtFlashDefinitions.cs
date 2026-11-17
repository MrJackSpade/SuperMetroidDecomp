namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// The hurt flash's updates (#2111): which of the flash's counted updates show its colours, play the impact, recover the
/// interrupted movement sound and end it.
/// </summary>
public static class SamusHurtFlashDefinitions
{
    /// <summary>The update on which the flash plays the impact sound.</summary>
    public const int ImpactSoundUpdate = 2;

    /// <summary>The updates, counted from the first, that show the flash's or the restored suit's colours; later updates leave the colours alone.</summary>
    public const int ColouredUpdates = 6;

    /// <summary>The count, reached by an update, at which the flash restores the movement sound the hurt interrupted.</summary>
    public const int SoundRecoveryCount = 40;

    /// <summary>The hurt-flash count at which the flash ends and the count is cleared: its largest value.</summary>
    public const int EndingUpdate = 60;
}
