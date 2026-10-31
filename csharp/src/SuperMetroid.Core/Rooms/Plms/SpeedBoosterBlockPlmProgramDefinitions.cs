namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The Speed Booster blocks' PLM programs (#1296), in place of their bank-$84 instruction lists. A block crumbles through
/// four images; a respawning block holds the last, plays them back and restores its saved block.
/// </summary>
internal static class SpeedBoosterBlockPlmProgramDefinitions
{
    /// <summary>The break sound, library 2.</summary>
    internal static readonly SoundEffectId BreakSoundId = Library2Sound.ContactBlockBreak;
    /// <summary>The break sound's request priority.</summary>
    internal static readonly SoundRequestPriority BreakSoundPriority = SoundRequestPriority.WhenIdle;
    /// <summary>How long each crumble image shows on Brinstar's slow blocks.</summary>
    internal const int SlowCrumbleDuration = 2;
    /// <summary>How long each crumble image shows on the other blocks.</summary>
    internal const int CrumbleDuration = 1;
    /// <summary>How long a respawning block stays gone.</summary>
    internal const int GoneDuration = 48;
    /// <summary>How long each image shows as a respawning block grows back.</summary>
    internal const int RegrowDuration = 4;
}
