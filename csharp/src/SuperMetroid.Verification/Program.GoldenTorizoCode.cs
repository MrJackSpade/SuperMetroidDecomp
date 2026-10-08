using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGoldenTorizoCode()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        ushort input = Word(0xaac91a);
        ushort[] native = [Word(0xaac91f), Word(0xaac928), Word(0xaac931),
            Word(0xaac93a), Word(0xaac943), Word(0xaac94c), Word(0xaac955)];
        Check(RoomEnemySystem.GoldenTorizoDefinition, input, false, true);
        Check(RoomEnemySystem.GoldenTorizoDefinition, (ushort)(input | 0x20), false, false);
        Check(RoomEnemySystem.GoldenTorizoDefinition, (ushort)(input & ~0x80), false, false);
        Check(RoomEnemySystem.GoldenTorizoDefinition, input, true, false);
        Check(RoomEnemySystem.BombTorizoDefinition, input, false, false);
        Console.WriteLine("Golden Torizo code: production initialization grants exact ROM inventory only for the exact held chord on the undefeated Golden encounter; reserve mode and reserve missiles remain unchanged.");

        void Check(ushort header, ushort held, bool defeated, bool grants)
        {
            var enemies = new RoomEnemySystem();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, new SnesCgram());
            typeof(RoomEnemySystem).GetField("_isAreaTorizoDefeated", flags)!.SetValue(enemies, (Func<bool>)(() => defeated));
            var initialize = typeof(RoomEnemySystem).GetMethod("RunInitializationAi", flags)!
                .CreateDelegate<Action<RoomEnemySlot, RoomLevelData?, SamusState?, ushort, ushort, ushort>>(enemies);
            var actor = enemies.Slots[0];
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
            initialize(actor, null, samus, held, 0, 0);
            ushort[] expected = grants
                ? [native[0], native[0], native[1], native[1], native[2], native[2],
                   native[3], native[3], native[4], native[4], native[5], native[5], native[6], native[6]]
                : before;
            AssertTrue(expected.AsSpan().SequenceEqual(Snapshot(samus)),
                $"GT code inventory: header ${header:X4}, held ${held:X4}, defeated {defeated}");
            AssertEqual((ushort)2, samus.ReserveTankMode, "GT code does not set reserve mode");
            AssertEqual((ushort)7, samus.ReserveMissiles, "GT code does not alter reserve missiles");
        }

        static ushort[] Snapshot(SamusState samus) =>
            [samus.Health, samus.MaxHealth, samus.ReserveEnergy, samus.MaxReserveEnergy,
             samus.Missiles, samus.MaxMissiles, samus.SuperMissiles, samus.MaxSuperMissiles,
             samus.PowerBombs, samus.MaxPowerBombs, samus.EquippedItems, samus.CollectedItems,
             samus.EquippedBeams, samus.CollectedBeams];
    }
}
