using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads native 64-byte bank-$A0 enemy headers during cartridge import.</summary>
public static class RoomEnemyDefinitionImporter
{
    public static RoomEnemyDefinition Load(ISnesAddressSpace bus, ushort pointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        ushort Word(int offset) => RomDataReader.ReadWordFixedBank(cartridge,
            RoomEnemyRomLayout.DefinitionBank | unchecked((ushort)(pointer + offset)));
        byte Byte(int offset) => cartridge.ReadCartridgeByte(
            RoomEnemyRomLayout.DefinitionBank | unchecked((ushort)(pointer + offset)));
        int Long(int offset) => RomDataReader.ReadLongFixedBank(cartridge,
            RoomEnemyRomLayout.DefinitionBank | unchecked((ushort)(pointer + offset)));
        return new RoomEnemyDefinition(
            TileDataSize: Word(0),
            PalettePointer: Word(2),
            Health: Word(4),
            Damage: Word(6),
            XRadius: Word(8),
            YRadius: Word(10),
            Bank: Byte(12),
            HurtAiTime: Byte(13),
            HurtSoundEffect: Word(14),
            BossId: Word(16),
            InitializationAiPointer: Word(18),
            PartCount: Word(20),
            Unused16: Word(22),
            MainAiPointer: Word(24),
            GrappleAiPointer: Word(26),
            HurtAiPointer: Word(28),
            FrozenAiPointer: Word(30),
            TimeFrozenAiPointer: Word(32),
            DeathAnimation: Word(34),
            Unused24: Word(36),
            Unused26: Word(38),
            PowerBombReactionPointer: Word(40),
            VariantIndex: Word(42),
            Unused2C: Word(44),
            Unused2E: Word(46),
            TouchAiPointer: Word(48),
            ShotAiPointer: Word(50),
            InitialSpritemapPointer: Word(52),
            TileDataAddress: Long(54),
            Layer: Byte(57),
            ItemDropChancesPointer: Word(58),
            VulnerabilityPointer: Word(60),
            NamePointer: Word(62));
    }
}
