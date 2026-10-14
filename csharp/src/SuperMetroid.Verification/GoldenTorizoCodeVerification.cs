using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static SharedSlopeHeightsVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

/// <summary>Checks the Golden Torizo inventory code's exact held chord and native reward values.</summary>
internal static class GoldenTorizoCodeVerification
{
    /// <summary>Confirms only an undefeated Golden Torizo with the exact input chord grants the native inventory.</summary>
    internal static void VerifyGoldenTorizoCode()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int address) => (ushort)(SnesLittleEndian.Word(GoldenTorizoCodeRomByte(rom, address), GoldenTorizoCodeRomByte(rom, address + 1)));
        ushort input = Word(0xaac91a);
        ushort[] native = [Word(0xaac91f), Word(0xaac928), Word(0xaac931),
            Word(0xaac93a), Word(0xaac943), Word(0xaac94c), Word(0xaac955)];
        Check(EnemyDefinitionId.GoldenTorizo, input, false, true);
        Check(EnemyDefinitionId.GoldenTorizo, (ushort)(input | 0x20), false, false);
        Check(EnemyDefinitionId.GoldenTorizo, (ushort)(input & ~0x80), false, false);
        Check(EnemyDefinitionId.GoldenTorizo, input, true, false);
        Check(EnemyDefinitionId.BombTorizo, input, false, false);
        Console.WriteLine("Golden Torizo code: production initialization grants exact ROM inventory only for the exact held chord on the undefeated Golden encounter; reserve mode and reserve missiles remain unchanged.");

        void Check(EnemyDefinitionId header, ushort held, bool defeated, bool grants)
        {
            var enemies = new RoomEnemySystem();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            enemies.BindFixtureBus(NewSlopeHeightNoReadBus());
            enemies.Presentation._cgram = new SnesCgram();
            enemies.VerificationServices().IsAreaTorizoDefeated = (Func<bool>)(() => defeated);
            var initialize = new Action<RoomEnemySlot, RoomLevelData?, SamusState?, ushort, ushort>(enemies.RunInitializationAi);
            var actor = enemies.SlotTable.Slots[0];
            actor.EnemyDefinitionPointer = header;
            actor.Definition = RoomEnemyDefinitionCatalog.Get(header);
            var samus = new SamusState
            {
                Health = 1500, MaxHealth = 1599, ReserveEnergy = 400, MaxReserveEnergy = 400,
                Missiles = 200, MaxMissiles = 230, SuperMissiles = 40, MaxSuperMissiles = 50,
                PowerBombs = 40, MaxPowerBombs = 50, EquippedItems = 8, CollectedItems = 8,
                EquippedBeams = 0, CollectedBeams = 0, ReserveTankMode = 2, ReserveMissiles = 7,
            };
            ushort[] before = Snapshot(samus);
            initialize(actor, null, samus, held, 0);
            ushort[] expected = grants
                ? [native[0], native[0], native[1], native[1], native[2], native[2],
                   native[3], native[3], native[4], native[4], native[5], native[5], native[6], native[6]]
                : before;
            AssertTrue(expected.SequenceEqual(Snapshot(samus)),
                $"GT code inventory: header ${(int)header:X4}, held ${held:X4}, defeated {defeated}");
            AssertEqual((ushort)2, samus.ReserveTankMode, "GT code does not set reserve mode");
            AssertEqual((ushort)7, samus.ReserveMissiles, "GT code does not alter reserve missiles");
        }

        static ushort[] Snapshot(SamusState samus) =>
            [samus.Health, samus.MaxHealth, samus.ReserveEnergy, samus.MaxReserveEnergy,
             samus.Missiles, samus.MaxMissiles, samus.SuperMissiles, samus.MaxSuperMissiles,
             samus.PowerBombs, samus.MaxPowerBombs, samus.EquippedItems, samus.CollectedItems,
             samus.EquippedBeams, samus.CollectedBeams];
    }

    /// <summary>Reads a cartridge byte for the native code's input mask and inventory oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Golden Torizo code")]
    private static byte GoldenTorizoCodeRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Runs the focused Golden Torizo code grant check.</summary>
    [VerificationCommand("--golden-torizo-code")]
    internal static int GoldenTorizoCodeCommand()
    {
        Suite(nameof(VerifyGoldenTorizoCode), () => VerifyGoldenTorizoCode());
        return 0;
    }
}
