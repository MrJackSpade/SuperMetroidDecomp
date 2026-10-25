using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Checks dead Sidehopper launch words and post-landing timer transitions against native behavior.</summary>
internal static class DeadSidehopperLaunchDefinitionsVerification
{
    /// <summary>Checks every phase and timer with the migrated velocity tables forbidden to production reads.</summary>
    internal static void VerifyDeadSidehopperLaunchDefinitions(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, DeadSidehopperEnemyState, SamusState?, RoomLevelData?, Position16<RoomPixels>> dispatch = enemies.DeadSidehopperBehavior.DispatchDeadSidehopperState;
        var slot = enemies.SlotTable.Slots[0];
        var state = new DeadSidehopperEnemyState(slot, 0, new CorpseRottingProgram { YLimit = 0, LateMoveEntryIndex = 0, WrapOffset = 0 }, new CorpseRotTable(1));
        for (ushort phase = 0; phase < 4; phase++)
        {
            ushort vx = CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointersDeadSidehopper.HorizontalVelocityWords + phase * 2));
            ushort vy = CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointersDeadSidehopper.VerticalVelocityWords + phase * 2));
            AssertEqual(vx, DeadSidehopperLaunchDefinitions.Velocity(phase).X.RawValue, "Native corpse X launch");
            AssertEqual(vy, DeadSidehopperLaunchDefinitions.Velocity(phase).Y.RawValue, "Native corpse Y launch");
            foreach (ushort paletteStage in new ushort[] { 0, 1 })
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                state.Function = DeadSidehopperAiFunction.PostLandingDelay;
                state.StateTimer = WrappingCountdown.FromRaw((ushort)raw);
                state.PaletteStage = paletteStage;
                state.JumpPhase = phase;
                state.Velocity = new EightEightVector<RoomPixels>(0x1234, 0x1234);
                enemies.DeadSidehopperBehavior.Extensions[0].Program = AnimationProgramCursor<DeadTourianProgram>.Start(DeadTourianProgram.SidehopperCorpse);
                slot.InstructionTimer = WrappingCountdown.Start(9);
                slot.Timer = WrappingCountdown.Start(17);
                dispatch(slot, state, null, null, default);
                ushort timer = unchecked((ushort)(raw - 1));
                bool expired = (timer & 0x8000) != 0;
                bool launch = expired && paletteStage == 0;
                AssertEqual(timer, state.StateTimer.RawValue, "Exact native delay decrement and wrap");
                AssertEqual(launch ? vx : (ushort)0x1234, state.Velocity.X.RawValue, "Timer and palette gates control X launch");
                AssertEqual(launch ? vy : (ushort)0x1234, state.Velocity.Y.RawValue, "Timer and palette gates control Y launch");
                AssertEqual(expired ? (launch ? DeadSidehopperAiFunction.ActivatedMovement : DeadSidehopperAiFunction.TransformPalette) : DeadSidehopperAiFunction.PostLandingDelay,
                    state.Function, "Exact launch versus transformation handoff");
                AssertEqual(AnimationProgramCursor<DeadTourianProgram>.Start(launch ? DeadTourianProgram.SidehopperIdle : DeadTourianProgram.SidehopperCorpse),
                    enemies.DeadSidehopperBehavior.Extensions[0].Program, "Launch restarts the alive-idle animation");
                AssertEqual(launch ? (ushort)1 : (ushort)9, slot.InstructionTimer.RawValue, "Launch instruction timer");
                AssertEqual(launch ? (ushort)0 : (ushort)17, slot.Timer.RawValue, "Launch clears instruction loop timer");
                AssertEqual(phase, state.JumpPhase, "Launch does not advance landing-owned phase");
            }
        }
        Console.WriteLine("Dead sidehopper launches: all eight native words and 524288 actual timer/phase/palette transitions pass without a bus.");
    }
}
