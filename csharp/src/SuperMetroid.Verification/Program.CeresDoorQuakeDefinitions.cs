using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyCeresDoorQuakeDefinitions(SuperMetroidAddressSpace rom)
    {
        const int sourceAddress = 0xa6a321;

        for (ushort phase = 0; phase < 4; phase++)
        {
            sbyte expected = unchecked((sbyte)rom.ReadByte(sourceAddress + phase));
            AssertEqual(expected, CeresDoorQuakeDefinitions.XOffset(phase),
                $"Ceres private door quake phase {phase}");
        }

        for (int timer = 0; timer <= ushort.MaxValue; timer++)
        {
            AssertEqual(
                unchecked((sbyte)rom.ReadByte(sourceAddress + (timer & 3))),
                CeresDoorQuakeDefinitions.XOffset((ushort)timer),
                $"Ceres private door quake timer ${timer:X4}");
        }

        var guarded = new CeresDoorQuakeReadGuard(rom);
        var enemies = new RoomEnemySystem
        {
            CeresStatus = 1,
        };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guarded);
        typeof(RoomEnemySystem).GetField("_ridleyState", flags)!.SetValue(
            enemies,
            new RidleyEnemyState { MovementAnimationEnabled = 0 });
        RoomEnemySlot door = enemies.Slots[1];
        door.EnemyDefinitionPointer = 0xe23f;
        door.VariableB = 1;
        door.XPosition = 100;
        door.YPosition = 80;
        short[] expectedOffsets = [0, 0, -4, -1];
        int? baseX = null;
        for (ushort phase = 0; phase < expectedOffsets.Length; phase++)
        {
            enemies.EarthquakeTimer = phase;
            var oam = new OamBuffer();
            oam.BeginFrame();
            enemies.DrawCeresRidleyImmediateBabyAndDoor(oam, 0, 0);
            oam.FinalizeFrame();
            baseX ??= oam.GetEntry(0).X;
            AssertEqual(baseX.Value + expectedOffsets[phase], oam.GetEntry(0).X,
                $"Ceres private door production OAM phase {phase}");
        }

        Console.WriteLine(
            "Ceres door quake definitions: all four native bytes, 65,536 timer aliases and four real OAM phases pass with the malformed source table forbidden.");
    }

    private sealed class CeresDoorQuakeReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa6a321 and < 0xa6a325
                ? throw new InvalidOperationException(
                    $"Ceres private door draw attempted migrated quake read ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
