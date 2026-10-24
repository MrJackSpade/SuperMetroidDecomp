using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies.Callbacks;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Reads native 64-byte bank-$A0 enemy headers during cartridge import.</summary>
[CpuBusBoundary("codec: ROM bank-$A0 64-byte enemy headers → RoomEnemyDefinition")]
public static class RoomEnemyDefinitionImporter
{
    /// <summary>Decodes one fixed 64-byte bank-$A0 enemy definition from cartridge data.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">Bank-$A0 pointer to the native enemy definition.</param>
    /// <returns>The decoded enemy definition.</returns>
    public static RoomEnemyDefinition Load(ISnesAddressSpace bus, EnemyDefinitionId pointer)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        ushort Word(int offset) => ReadWord(cartridge, pointer, offset);
        byte Byte(int offset) => ReadByte(cartridge, pointer, offset);
        byte bank = Byte(BankOffset);
        return new RoomEnemyDefinition(
            TileDataSize: Word(0),
            Health: Word(4),
            Damage: Word(6),
            Radius: new Extent16<RoomPixels>(Word(8), Word(10)),
            HurtAiTime: Byte(13),
            HurtSoundEffect: SoundEffectCommands.DecodeOptional(SoundEffectLibrary.Library2, Word(14)),
            BossId: Word(16),
            InitializationAi: EnemyInteractionCallbacks.AiRoutine(bank, Word(18)),
            PartCount: Word(20),
            Unused16: Word(22),
            MainAi: EnemyInteractionCallbacks.AiRoutine(bank, Word(24)),
            GrappleAi: SamusIdentityWords.GrappleAiRoutines.Decode(Word(26), "grapple AI routine"),
            HurtAi: EnemyInteractionCallbacks.HurtRoutine(Word(28)),
            DeathAnimation: Word(34),
            Unused24: Word(36),
            Unused26: Word(38),
            PowerBombReaction: EnemyInteractionCallbacks.TryDecode(bank, Word(40)),
            PowerBombReactionBody: EnemyInteractionCallbacks.Body(Word(40),
                EnemyInteractionCallbacks.IsLiteralNoOpPowerBomb(bank, Word(40))),
            VariantIndex: Word(42),
            Unused2C: Word(44),
            Unused2E: Word(46),
            Touch: EnemyInteractionCallbacks.TryDecode(bank, Word(48)),
            TouchBody: EnemyInteractionCallbacks.Body(Word(48), EnemyInteractionCallbacks.IsLiteralNoOpTouch(bank, Word(48))),
            Shot: EnemyInteractionCallbacks.TryDecode(bank, Word(50)),
            ShotBody: EnemyInteractionCallbacks.Body(Word(50), EnemyInteractionCallbacks.IsLiteralNoOpShot(bank, Word(50))),
            NoOpCallbackBody: EnemyInteractionCallbacks.NoOpCallbackBody(bank),
            TileDataAddress: RomDataReader.ReadLongFixedBank(cartridge, HeaderAddress(pointer, 54)),
            Layer: Byte(57),
            DropChances: EnemyHeaderTables.DropChances(Word(58)),
            Vulnerability: EnemyHeaderTables.Vulnerability(Word(60)));
    }

    /// <summary>The long address of the sixteen-color palette a header names in its enemy's bank.</summary>
    /// <param name="bus">Address space that exposes the import-only cartridge source.</param>
    /// <param name="pointer">Bank-$A0 pointer to the native enemy definition.</param>
    internal static int PaletteAddress(ISnesAddressSpace bus, EnemyDefinitionId pointer)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        return (int)new SnesAddress(ReadByte(cartridge, pointer, BankOffset), ReadWord(cartridge, pointer, PaletteOffset));
    }

    /// <summary>Header offset of the bank byte that qualifies the header's code and palette words.</summary>
    private const int BankOffset = 12;

    /// <summary>Header offset of the palette word.</summary>
    private const int PaletteOffset = 2;

    /// <summary>The long address of the byte <paramref name="offset"/> bytes into the bank-$A0 header at <paramref name="pointer"/>.</summary>
    private static int HeaderAddress(EnemyDefinitionId pointer, int offset) =>
        RoomEnemyRomLayout.DefinitionBank | unchecked((ushort)((ushort)pointer + offset));

    /// <summary>Reads the little-endian header word <paramref name="offset"/> bytes into the header at <paramref name="pointer"/>.</summary>
    private static ushort ReadWord(IImportCartridgeSource cartridge, EnemyDefinitionId pointer, int offset) =>
        RomDataReader.ReadWordFixedBank(cartridge, HeaderAddress(pointer, offset));

    /// <summary>Reads the header byte <paramref name="offset"/> bytes into the header at <paramref name="pointer"/>.</summary>
    private static byte ReadByte(IImportCartridgeSource cartridge, EnemyDefinitionId pointer, int offset) =>
        cartridge.ReadCartridgeByte(HeaderAddress(pointer, offset));
}
