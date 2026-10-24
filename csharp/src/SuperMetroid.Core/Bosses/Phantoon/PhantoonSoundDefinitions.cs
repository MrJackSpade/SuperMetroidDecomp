namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Compiled sound selections owned by Phantoon's bank-$A7 instruction callbacks.</summary>
internal static class PhantoonSoundDefinitions
{
    /// <summary>$A7:D4BA-D4BD, fight-intro starting-flame spawn: library-three blue-flame sound.</summary>
    internal static SoundEffectId StartingFlame => Library3Sound.PhantoonFlame;

    /// <summary>$A7:D4BD uses QueueSound_Lib3_Max6 after each starting-flame spawn.</summary>
    internal const byte StartingFlameQueueCapacity = 6;

    /// <summary>$A7:CF68, SpawnCasualFlame: blue flame launch sound in library three.</summary>
    internal static SoundEffectId CasualFlame => Library3Sound.PhantoonFlame;

    /// <summary>Library-2 sound $73, queued when a shot hurts or kills Phantoon (<c>Phantoon_Shot</c>, $A7:DD9B).</summary>
    internal static readonly SoundEffectId ShotCry = Library2Sound.PhantoonCry;

    /// <summary>Library-2 sound $7E, queued as the dying Phantoon turns wavy (<c>Phantoon_WavyDyingPhantoonAndCry</c>, $A7:DA51).</summary>
    internal static readonly SoundEffectId DyingCry = Library2Sound.MotherBrainHighCry;

    /// <summary>Library-2 sound $24, queued after a big death explosion.</summary>
    internal static readonly SoundEffectId BigDeathExplosionSound = Library2Sound.SmallExplosion;

    /// <summary>Library-2 sound $2B, queued after any other death explosion.</summary>
    internal static readonly SoundEffectId DeathExplosionSound = Library2Sound.FireballImpact;

    /// <summary>$A7:CF6B, SpawnCasualFlame: QueueSound_Lib3_Max6 request capacity.</summary>
    internal const byte CasualFlameQueueCapacity = 6;

    /// <summary>
    /// <c>Phantoon_MaterializationSFX</c> at <c>$A7:CDED-$A7:CDF2</c>: the library-two sounds queued in a repeating
    /// three-callback cycle.
    /// </summary>
    private static readonly SoundEffectId[] MaterializationSounds =
    [
        Library2Sound.PhantoonAppears1,
        Library2Sound.PhantoonAppears2,
        Library2Sound.PhantoonAppears3,
    ];

    /// <summary>The number of sounds in the materialization cycle.</summary>
    internal static int MaterializationSoundCount => MaterializationSounds.Length;

    /// <summary>Returns the materialization sound at a zero-through-two cycle position.</summary>
    internal static SoundEffectId MaterializationSound(int index) =>
        (uint)index < (uint)MaterializationSounds.Length
            ? MaterializationSounds[index]
            : throw new ArgumentOutOfRangeException(
                nameof(index), index, "Phantoon materialization sound index must be zero through two.");
}
