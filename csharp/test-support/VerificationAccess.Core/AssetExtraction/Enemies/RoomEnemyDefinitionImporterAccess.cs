using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies.Callbacks;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The whole-header decode of a bank-$A0 enemy header, which only verification reads: the import installs the
/// definitions it needs without decoding headers this way (#1513).
/// </summary>
internal static class RoomEnemyDefinitionImporterAccess
{
    /// <summary>The standard hurt flash a zero +$0D byte selects.</summary>
    private const int StandardHurtFlashFrames = 4;

    /// <summary>Decodes whole enemy headers.</summary>
    extension(RoomEnemyDefinitionImporter)
    {
        /// <summary>Decodes one fixed 64-byte bank-$A0 enemy definition from cartridge data.</summary>
        /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
        /// <param name="pointer">The enemy definition, whose header the import's header table locates.</param>
        internal static RoomEnemyDefinition Load(ISnesAddressSpace bus, EnemyDefinitionId pointer) =>
            RoomEnemyDefinitionImporter.LoadAt(bus, EnemyDefinitionIds.HeaderPointer(pointer));

        /// <summary>Decodes the fixed 64-byte bank-$A0 enemy header at <paramref name="headerPointer"/>.</summary>
        /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
        /// <param name="headerPointer">The header's bank-$A0 pointer; a verification fixture may seed one for an identity no member has.</param>
        internal static RoomEnemyDefinition LoadAt(ISnesAddressSpace bus, int headerPointer)
        {
            Ensure.NotNull(bus);
            IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
            ushort Word(int offset) => RoomEnemyDefinitionImporter.ReadWord(cartridge, headerPointer, offset);
            byte Byte(int offset) => RoomEnemyDefinitionImporter.ReadByte(cartridge, headerPointer, offset);
            int bank = Byte(RoomEnemyDefinitionImporter.BankOffset);
            return new RoomEnemyDefinition(
                Health: Word(4),
                Damage: Word(6),
                Radius: new Extent16<RoomPixels>(Word(8), Word(10)),
                HurtFlashFrames: HurtFlashFrames(Byte(13)),
                HurtSoundEffect: SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Enemies, Word(14)),
                Boss: AreaBossWords.Decode(Word(16)),
                GrappleResponse: EnemyGrappleResponseWords.Responses.Decode(Word(26), "grapple response"),
                HurtResponse: EnemyInteractionCallbacks.HurtResponse(Word(28)),
                DeathExplosion: EnemyDeathExplosionWords.Selectors.Decode(Word(34), "enemy death explosion"),
                PowerBombReaction: EnemyInteractionCallbacks.DecodePowerBomb(bank, Word(40)),
                Touch: EnemyInteractionCallbacks.DecodeTouch(bank, Word(48)),
                Shot: EnemyInteractionCallbacks.DecodeShot(bank, Word(50)),
                TouchHitboxScan: EnemyInteractionCallbacks.HitboxScan(Word(48)),
                ShotHitboxScan: EnemyInteractionCallbacks.HitboxScan(Word(50)),
                DropChances: EnemyHeaderTables.DropChances(cartridge, Word(58)),
                Vulnerability: EnemyHeaderTables.Vulnerability(cartridge, Word(60)));
        }
    }

    /// <summary>The hurt flash a header's +$0D byte names: its frames, or the standard flash for zero.</summary>
    private static int HurtFlashFrames(int headerByte) => headerByte == 0 ? StandardHurtFlashFrames : headerByte;

}
