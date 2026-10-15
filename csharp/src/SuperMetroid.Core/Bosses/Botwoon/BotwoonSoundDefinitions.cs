namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's library-2 sound effects, queued with a six-entry admission limit.</summary>
internal static class BotwoonSoundDefinitions
{
    /// <summary>Library-2 sound $7C, queued by <c>Instruction_Botwoon_QueueSpitSFX</c> ($B3:9572) as the mouth opens.</summary>
    internal const ushort Spit = 0x007c;

    /// <summary>Library-2 sound $24, the explosion of a landing head or body segment and of the crumbling wall.</summary>
    internal const ushort Explosion = 0x0024;
}
