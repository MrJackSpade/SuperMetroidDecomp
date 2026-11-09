namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The Speed Booster blocks' PLM programs (#1296), in place of their instruction lists. A block crumbles through
/// four images; a respawning block holds the last, plays them back and restores its saved block.
/// </summary>
internal static class SpeedBoosterBlockObjectProgramDefinitions
{
    /// <summary>The break sound.</summary>
    internal static readonly SoundEffectId BreakSoundId = EnemySound.ContactBlockBreak;
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
