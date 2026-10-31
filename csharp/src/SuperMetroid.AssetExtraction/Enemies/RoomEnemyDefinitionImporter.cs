using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies.Callbacks;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads native 64-byte bank-$A0 enemy headers during cartridge import.</summary>
[EncodingOwner("cartridge enemy header words")]
public static class RoomEnemyDefinitionImporter
{
    /// <summary>Decodes one fixed 64-byte bank-$A0 enemy definition from cartridge data.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">The enemy definition, whose header the import's header table locates.</param>
    /// <returns>The decoded enemy definition.</returns>
    public static RoomEnemyDefinition Load(ISnesAddressSpace bus, EnemyDefinitionId pointer) =>
        LoadAt(bus, EnemyDefinitionIds.HeaderPointer(pointer));

    /// <summary>Decodes the fixed 64-byte bank-$A0 enemy header at <paramref name="headerPointer"/>.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="headerPointer">The header's bank-$A0 pointer; a verification fixture may seed one for an identity no member has.</param>
    /// <returns>The decoded enemy definition.</returns>
    internal static RoomEnemyDefinition LoadAt(ISnesAddressSpace bus, int headerPointer)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        ushort Word(int offset) => ReadWord(cartridge, headerPointer, offset);
        byte Byte(int offset) => ReadByte(cartridge, headerPointer, offset);
        int bank = Byte(BankOffset);
        int tileSize = Word(0);
        return new RoomEnemyDefinition(
            TileByteCount: tileSize & EnemySourceDefinitions.EnemyHeaderTileByteCountBits,
            TilePlacement: (tileSize & EnemySourceDefinitions.EnemyHeaderFixedBlockBit) != 0
                ? EnemyTilePlacement.AtFixedBlock : EnemyTilePlacement.FollowingPrevious,
            Health: Word(4),
            Damage: Word(6),
            Radius: new Extent16<RoomPixels>(Word(8), Word(10)),
            HurtFlashFrames: HurtFlashFrames(Byte(13)),
            HurtSoundEffect: SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Library2, Word(14)),
            Boss: AreaBossWords.Decode(Word(16)),
            InitializationAi: EnemyInteractionCallbacks.AiRoutine(bank, Word(18)),
            MainAi: EnemyInteractionCallbacks.AiRoutine(bank, Word(24)),
            GrappleAi: SamusIdentityWords.GrappleAiRoutines.Decode(Word(26), "grapple AI routine"),
            HurtAi: EnemyInteractionCallbacks.HurtRoutine(Word(28)),
            DeathExplosion: EnemyDeathExplosionWords.Selectors.Decode(Word(34), "enemy death explosion"),
            PowerBombReaction: EnemyInteractionCallbacks.TryDecode(bank, Word(40)),
            PowerBombReactionBody: EnemyInteractionCallbacks.Body(Word(40),
                EnemyInteractionCallbacks.IsLiteralNoOpPowerBomb(bank, Word(40))),
            VariantIndex: HopperVariant(Word(42)),
            Touch: EnemyInteractionCallbacks.TryDecode(bank, Word(48)),
            TouchBody: EnemyInteractionCallbacks.Body(Word(48), EnemyInteractionCallbacks.IsLiteralNoOpTouch(bank, Word(48))),
            Shot: EnemyInteractionCallbacks.TryDecode(bank, Word(50)),
            ShotBody: EnemyInteractionCallbacks.Body(Word(50), EnemyInteractionCallbacks.IsLiteralNoOpShot(bank, Word(50))),
            NoOpCallbackBody: EnemyInteractionCallbacks.NoOpCallbackBody(bank),
            Layer: Byte(57),
            DropChances: EnemyHeaderTables.DropChances(cartridge, Word(58)),
            Vulnerability: EnemyHeaderTables.Vulnerability(cartridge, Word(60)));
    }

    /// <summary>The standard hurt flash a zero +$0D byte selects.</summary>
    private const int StandardHurtFlashFrames = 4;

    /// <summary>The hurt flash a header's +$0D byte names: its frames, or the standard flash for zero.</summary>
    private static int HurtFlashFrames(int headerByte) => headerByte == 0 ? StandardHurtFlashFrames : headerByte;

    /// <summary>
    /// The hopper variant a header's +$2A word holds. Headers whose behaviour reads no variant keep leftover prototype
    /// instruction-list pointers there, which all lie in their bank's ROM half; they are not variants and import as zero.
    /// </summary>
    private static int HopperVariant(int word) => word >= SnesBusLayout.RomWindowStart ? 0 : word;

    /// <summary>The long address of the planar tile data a header names: where the import reads the enemy's tile set.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">The enemy definition, whose header the import's header table locates.</param>
    internal static int TileDataSource(ISnesAddressSpace bus, EnemyDefinitionId pointer)
    {
        Ensure.NotNull(bus);
        return RomDataReader.ReadLongFixedBank(CartridgeImportSource.Require(bus),
            HeaderAddress(EnemyDefinitionIds.HeaderPointer(pointer), TileDataOffset));
    }

    /// <summary>Header offset of the 24-bit tile data source.</summary>
    private const int TileDataOffset = 54;

    /// <summary>The long address of the sixteen-color palette a header names in its enemy's bank.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">The enemy definition, whose header the import's header table locates.</param>
    internal static int PaletteAddress(ISnesAddressSpace bus, EnemyDefinitionId pointer)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        int header = EnemyDefinitionIds.HeaderPointer(pointer);
        return (int)new SnesAddress(ReadByte(cartridge, header, BankOffset), ReadWord(cartridge, header, PaletteOffset));
    }

    /// <summary>Header offset of the bank byte that qualifies the header's code and palette words.</summary>
    private const int BankOffset = 12;

    /// <summary>Header offset of the palette word.</summary>
    private const int PaletteOffset = 2;

    /// <summary>The long address of the byte <paramref name="offset"/> bytes into the bank-$A0 header at <paramref name="header"/>.</summary>
    private static int HeaderAddress(int header, int offset) =>
        EnemySourceDefinitions.EnemyHeaderBank | unchecked((ushort)(header + offset));

    /// <summary>Reads the little-endian header word <paramref name="offset"/> bytes into the header at <paramref name="header"/>.</summary>
    private static ushort ReadWord(IImportCartridgeSource cartridge, int header, int offset) =>
        RomDataReader.ReadWordFixedBank(cartridge, HeaderAddress(header, offset));

    /// <summary>Reads the header byte <paramref name="offset"/> bytes into the header at <paramref name="header"/>.</summary>
    private static byte ReadByte(IImportCartridgeSource cartridge, int header, int offset) =>
        cartridge.ReadCartridgeByte(HeaderAddress(header, offset));
}
