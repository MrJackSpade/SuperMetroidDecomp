namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's library-2 sound effects, queued with a six-entry admission limit.</summary>
internal static class BotwoonSoundDefinitions
{
    /// <summary>Library-2 sound $7C, queued by <c>Instruction_Botwoon_QueueSpitSFX</c> ($B3:9572) as the mouth opens.</summary>
    internal static readonly SoundEffectId Spit = new(SoundEffectLibrary.Library2, 0x7c);

    /// <summary>Library-2 sound $24, the explosion of a landing head or body segment and of the crumbling wall.</summary>
    internal static readonly SoundEffectId Explosion = new(SoundEffectLibrary.Library2, 0x24);
}
