using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyDeadSidehopperLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, new SlopeHeightNoReadBus());
        var dispatch = typeof(RoomEnemySystem).GetMethod("DispatchDeadSidehopperState", flags)!
            .CreateDelegate<Action<RoomEnemySlot, DeadSidehopperEnemyState, SamusState?, RoomLevelData?, ushort>>(enemies);
        var slot = enemies.Slots[0];
        var state = new DeadSidehopperEnemyState(slot, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        for (ushort phase = 0; phase < 4; phase++)
        {
            ushort vx = Word(EnemyRomTablePointers.DeadSidehopper.HorizontalVelocityWords + phase * 2);
            ushort vy = Word(EnemyRomTablePointers.DeadSidehopper.VerticalVelocityWords + phase * 2);
            AssertEqual(vx, DeadSidehopperLaunchDefinitions.Horizontal[phase], "Native corpse X launch");
            AssertEqual(vy, DeadSidehopperLaunchDefinitions.Vertical[phase], "Native corpse Y launch");
            foreach (ushort paletteStage in new ushort[] { 0, 1 })
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                state.Function = DeadSidehopperAiFunction.PostLandingDelay;
                state.StateTimer = (ushort)raw;
                state.PaletteStage = paletteStage;
                state.JumpPhase = phase;
                state.VerticalVelocity = state.HorizontalVelocity = 0x1234;
                slot.CurrentInstruction = 0x5678;
                slot.InstructionTimer = 9;
                slot.Timer = 17;
                dispatch(slot, state, null, null, 0);
                ushort timer = unchecked((ushort)(raw - 1));
                bool expired = (timer & 0x8000) != 0;
                bool launch = expired && paletteStage == 0;
                AssertEqual(timer, state.StateTimer, "Exact native delay decrement and wrap");
                AssertEqual(launch ? vx : (ushort)0x1234, state.HorizontalVelocity, "Timer and palette gates control X launch");
                AssertEqual(launch ? vy : (ushort)0x1234, state.VerticalVelocity, "Timer and palette gates control Y launch");
                AssertEqual(expired ? (launch ? DeadSidehopperAiFunction.ActivatedMovement : DeadSidehopperAiFunction.TransformPalette) : DeadSidehopperAiFunction.PostLandingDelay,
                    state.Function, "Exact launch versus transformation handoff");
                AssertEqual(launch ? (ushort)0xece3 : (ushort)0x5678, slot.CurrentInstruction, "Launch restarts native instruction list");
                AssertEqual(launch ? (ushort)1 : (ushort)9, slot.InstructionTimer, "Launch instruction timer");
                AssertEqual(launch ? (ushort)0 : (ushort)17, slot.Timer, "Launch clears instruction loop timer");
                AssertEqual(phase, state.JumpPhase, "Launch does not advance landing-owned phase");
            }
        }
        Console.WriteLine("Dead sidehopper launches: all eight native words and 524288 actual timer/phase/palette transitions pass without a bus.");
    }
}
