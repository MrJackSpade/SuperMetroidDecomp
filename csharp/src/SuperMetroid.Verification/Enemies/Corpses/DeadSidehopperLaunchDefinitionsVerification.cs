using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Checks dead Sidehopper launch words and post-landing timer transitions against native behavior.</summary>
internal static class DeadSidehopperLaunchDefinitionsVerification
{
    /// <summary>Checks every phase and timer with the migrated velocity tables forbidden to production reads.</summary>
    internal static void VerifyDeadSidehopperLaunchDefinitions(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemySlot, DeadSidehopperEnemyState, SamusState?, RoomLevelData?, Position16<RoomPixels>> dispatch = enemies.DeadSidehopperBehavior.DispatchDeadSidehopperState;
        var slot = enemies.SlotTable.Slots[0];
        var state = new DeadSidehopperEnemyState(slot, 0, new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount));
        for (ushort phase = 0; phase < 4; phase++)
        {
            ushort vx = CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointersDeadSidehopper.HorizontalVelocityWords + phase * 2));
            ushort vy = CartridgeImportSource.Require(rom).ReadCartridgeWord((EnemyRomTablePointersDeadSidehopper.VerticalVelocityWords + phase * 2));
            AssertEqual(vx, DeadSidehopperLaunchDefinitions.Velocity(phase).X.RawValue, "Native corpse X launch");
            AssertEqual(vy, DeadSidehopperLaunchDefinitions.Velocity(phase).Y.RawValue, "Native corpse Y launch");
            foreach (int drainStage in new[] { 0, 1 })
            // The delay starts at 64 and the phase ends on the step below zero, so every count it can hold is swept.
            for (int raw = 0; raw <= 64; raw++)
            {
                state.Function = DeadSidehopperAiFunction.PostLandingDelay;
                state.StateTimer = raw;
                state.DrainStage = drainStage;
                state.JumpPhase = phase;
                state.Velocity = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(4660, 4660);
                state.Playback = new(DeadTourianAnimations.StartingWith(DeadTourianSegment.SidehopperCorpse), AnimationPlace.StartOfAnimation);
                slot.AnimationClock.HoldFor(9);
                slot.AnimationClock.Repeats = WrappingCountdown.Start(17);
                dispatch(slot, state, null, null, default);
                int timer = raw - 1;
                bool expired = timer < 0;
                bool launch = expired && drainStage == 0;
                AssertEqual(timer, state.StateTimer, "Exact delay decrement, ending below zero");
                AssertEqual(launch ? vx : (ushort)0x1234, state.Velocity.X.RawValue, "Timer and drain gates control X launch");
                AssertEqual(launch ? vy : (ushort)0x1234, state.Velocity.Y.RawValue, "Timer and drain gates control Y launch");
                AssertEqual(expired ? (launch ? DeadSidehopperAiFunction.ActivatedMovement : DeadSidehopperAiFunction.Draining) : DeadSidehopperAiFunction.PostLandingDelay,
                    state.Function, "Exact launch versus transformation handoff");
                AssertEqual(new AnimationPlayback<DeadTourianSegment>(DeadTourianAnimations.StartingWith(launch ? DeadTourianSegment.SidehopperIdle : DeadTourianSegment.SidehopperCorpse), AnimationPlace.StartOfAnimation),
                    state.Playback, "Launch restarts the alive-idle animation");
                AssertEqual(launch ? (ushort)1 : (ushort)9, slot.AnimationClock.Hold.RawValue, "Launch instruction timer");
                AssertEqual(launch ? (ushort)0 : (ushort)17, slot.AnimationClock.Repeats.RawValue, "Launch clears instruction loop timer");
                AssertEqual(phase, state.JumpPhase, "Launch does not advance landing-owned phase");
            }
        }
        Console.WriteLine("Dead sidehopper launches: all eight native words and 524288 actual timer/phase/palette transitions pass without a bus.");
    }
}
