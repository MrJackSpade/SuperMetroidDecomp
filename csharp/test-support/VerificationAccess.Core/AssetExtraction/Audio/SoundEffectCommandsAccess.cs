using System.Collections.Frozen;

namespace SuperMetroid.AssetExtraction.Audio;

/// <summary>Verification access to <see cref="SoundEffectCommands"/> decodes production does not use.</summary>
internal static class SoundEffectCommandsAccess
{
    /// <summary>
    /// The cartridge's requests #2580 merged into another request's identical sound: each deleted duplicate's library and
    /// request number, and the surviving sound the game plays in its place. Checks that compare the game with the
    /// cartridge read a merged request as its survivor.
    /// </summary>
    private static readonly FrozenDictionary<(SoundEffectLibrary Library, int Command), SoundEffectId> Merged =
        new Dictionary<(SoundEffectLibrary Library, int Command), SoundEffectId>
        {
            [(SoundEffectLibrary.Samus, 0x0e)] = SamusSound.UnchargedIceBeam,
            [(SoundEffectLibrary.Samus, 0x11)] = SamusSound.UnchargedSpazerIce,
            [(SoundEffectLibrary.Samus, 0x12)] = SamusSound.UnchargedSpazer,
            [(SoundEffectLibrary.Samus, 0x14)] = SamusSound.UnchargedSpazerIce,
            [(SoundEffectLibrary.Samus, 0x15)] = SamusSound.UnchargedSpazerIce,
            [(SoundEffectLibrary.Samus, 0x16)] = SamusSound.UnchargedPlasma,
            [(SoundEffectLibrary.Samus, 0x1a)] = SamusSound.ChargedIceBeam,
            [(SoundEffectLibrary.Samus, 0x1c)] = SamusSound.ChargedIceBeam,
            [(SoundEffectLibrary.Samus, 0x1d)] = SamusSound.ChargedIceBeam,
            [(SoundEffectLibrary.Samus, 0x1e)] = SamusSound.ChargedSpazer,
            [(SoundEffectLibrary.Samus, 0x20)] = SamusSound.ChargedIceBeam,
            [(SoundEffectLibrary.Samus, 0x21)] = SamusSound.ChargedIceBeam,
            [(SoundEffectLibrary.Samus, 0x22)] = SamusSound.ChargedPlasma,
            [(SoundEffectLibrary.Enemies, 0x04)] = EnemySound.AmmoPickup,
            [(SoundEffectLibrary.Enemies, 0x05)] = EnemySound.AmmoPickup,
            [(SoundEffectLibrary.Enemies, 0x08)] = EnemySound.WeaponExplosion,
            [(SoundEffectLibrary.Enemies, 0x39)] = RoomSound.SpeedBooster,
            [(SoundEffectLibrary.Enemies, 0x3c)] = RoomSound.ShinesparkEnd,
            [(SoundEffectLibrary.Enemies, 0x3d)] = RoomSound.StoredShinespark,
            [(SoundEffectLibrary.Enemies, 0x45)] = RoomSound.Typewriter,
            [(SoundEffectLibrary.Enemies, 0x5b)] = EnemySound.Screech,
            [(SoundEffectLibrary.Enemies, 0x6a)] = EnemySound.LowThump,
            [(SoundEffectLibrary.Enemies, 0x76)] = EnemySound.BigExplosion,
            [(SoundEffectLibrary.Room, 0x13)] = EnemySound.SmallExplosion,
            [(SoundEffectLibrary.Room, 0x1e)] = EnemySound.SmallExplosion,
        }.ToFrozenDictionary();

    /// <summary>Decodes cartridge sound requests as the game plays them.</summary>
    extension(SoundEffectCommands)
    {
        /// <summary>
        /// The sound the game plays for the cartridge's request <paramref name="command"/> of <paramref name="library"/>: the
        /// request's own sound or stop, or for a request #2580 merged into another, the survivor, which may be another
        /// library's.
        /// </summary>
        /// <exception cref="InvalidDataException">The library has no such request.</exception>
        internal static SoundEffectId DecodeCartridge(SoundEffectLibrary library, int command) =>
            Merged.TryGetValue((library, command), out SoundEffectId? survivor) ? survivor : SoundEffectCommands.Decode(library, command);

        /// <summary>
        /// The sound a cartridge operand or header word queues through <paramref name="library"/>: the request in its byte,
        /// as the game plays it, or none for zero.
        /// </summary>
        /// <exception cref="InvalidDataException">The word does not fit a request byte, or names no request of the library.</exception>
        internal static SoundEffectId? DecodeOptional(SoundEffectLibrary library, int word)
        {
            if ((uint)word > byte.MaxValue)
                throw new InvalidDataException($"Sound word ${word:X4} for {library} does not fit a request byte.");
            return word == 0 ? null : SoundEffectCommands.DecodeCartridge(library, word);
        }
    }
}
