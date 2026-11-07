namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Compiled sound selections owned by Phantoon's instruction callbacks.</summary>
internal static class PhantoonSoundDefinitions
{
    /// <summary>Fight-intro starting-flame spawn: library-three blue-flame sound.</summary>
    internal static SoundEffectId StartingFlame => Library3Sound.PhantoonFlame;

    /// <summary>The request priority of the sound each starting-flame spawn makes.</summary>
    internal static readonly SoundRequestPriority StartingFlamePriority = SoundRequestPriority.Normal;

    /// <summary>SpawnCasualFlame: blue flame launch sound in library three.</summary>
    internal static SoundEffectId CasualFlame => Library3Sound.PhantoonFlame;

    /// <summary>Phantoon's cry, queued when a shot hurts or kills Phantoon.</summary>
    internal static readonly SoundEffectId ShotCry = Library2Sound.PhantoonCry;

    /// <summary>The high cry, queued as the dying Phantoon turns wavy.</summary>
    internal static readonly SoundEffectId DyingCry = Library2Sound.MotherBrainHighCry;

    /// <summary>The small explosion, queued after a big death explosion.</summary>
    internal static readonly SoundEffectId BigDeathExplosionSound = Library2Sound.SmallExplosion;

    /// <summary>The fireball impact, queued after any other death explosion.</summary>
    internal static readonly SoundEffectId DeathExplosionSound = Library2Sound.FireballImpact;

    /// <summary>The casual-flame sound's request priority.</summary>
    internal static readonly SoundRequestPriority CasualFlamePriority = SoundRequestPriority.Normal;

    /// <summary>
    /// The materialization sounds: the library-two sounds queued in a repeating
    /// three-callback cycle.
    /// </summary>
    internal static readonly SoundEffectId[] MaterializationSounds =
    [
        Library2Sound.PhantoonAppears1,
        Library2Sound.PhantoonAppears2,
        Library2Sound.PhantoonAppears3,
    ];

    /// <summary>The number of sounds in the materialization cycle.</summary>
    internal static int MaterializationSoundCount => MaterializationSounds.Length;
}
