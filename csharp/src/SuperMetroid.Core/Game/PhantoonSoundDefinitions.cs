namespace SuperMetroid.Core.Game;

/// <summary>Compiled sound selections owned by Phantoon's bank-$A7 instruction callbacks.</summary>
internal static class PhantoonSoundDefinitions
{
    /// <summary>$A7:D4BA-D4BD, fight-intro starting-flame spawn: library-three blue-flame sound.</summary>
    internal static SoundEffectId StartingFlame => SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x1d);

    /// <summary>$A7:D4BD uses QueueSound_Lib3_Max6 after each starting-flame spawn.</summary>
    internal const byte StartingFlameQueueCapacity = 6;

    /// <summary>$A7:CF68, SpawnCasualFlame: blue flame launch sound in library three.</summary>
    internal static SoundEffectId CasualFlame => SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x1d);

    /// <summary>$A7:CF6B, SpawnCasualFlame: QueueSound_Lib3_Max6 request capacity.</summary>
    internal const byte CasualFlameQueueCapacity = 6;

    /// <summary>
    /// <c>Phantoon_MaterializationSFX</c> at <c>$A7:CDED-$A7:CDF2</c>, queued from
    /// sound library two in a repeating three-callback cycle.
    /// </summary>
    private static readonly ushort[] Materialization = [0x0079, 0x007a, 0x007b];

    /// <summary>Returns the materialization sound for a native zero-through-two index.</summary>
    internal static ushort MaterializationSound(ushort index)
    {
        if (index >= Materialization.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index), index,
                "Phantoon materialization sound index must be zero through two.");
        }

        return Materialization[index];
    }
}
