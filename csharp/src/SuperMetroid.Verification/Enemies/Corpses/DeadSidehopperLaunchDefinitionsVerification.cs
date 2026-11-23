using SuperMetroid.Core;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>Checks dead Sidehopper launch words and post-landing timer transitions against native behavior.</summary>
internal static class DeadSidehopperLaunchDefinitionsVerification
{
    /// <summary>Checks every phase and timer with the migrated velocity tables forbidden to production reads.</summary>
    internal static void VerifyDeadSidehopperLaunchDefinitions(CartridgeImportAddressSpace rom)
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        Action<RoomEnemyPlace, DeadSidehopperEnemyState, SamusState, RoomLevelData, Position16<RoomPixels>> dispatch = enemies.DeadSidehopperBehavior.RunDeadSidehopperState;
        var place = enemies.Places.InUpdateOrder[0];
        var state = new DeadSidehopperEnemyState(new CorpseRotSchedule(DeadSidehopperCorpseDefinitions.RowCount), DeadSidehopperAction.PostLandingDelay, SidehopperCorpseLayout.PlacedDead);
        for (ushort phase = 0; phase < 4; phase++)
        {
            ushort vx = rom.ReadCartridgeWord((EnemyRomTablePointersDeadSidehopper.HorizontalVelocityWords + phase * 2));
            ushort vy = rom.ReadCartridgeWord((EnemyRomTablePointersDeadSidehopper.VerticalVelocityWords + phase * 2));
            AssertEqual(vx, DeadSidehopperLaunchDefinitions.Launches[phase].X.RawValue, "Native corpse X launch");
            AssertEqual(vy, DeadSidehopperLaunchDefinitions.Launches[phase].Y.RawValue, "Native corpse Y launch");
            foreach (SidehopperDrainStage drainStage in new[] { SidehopperDrainStage.Untouched, SidehopperDrainStage.LatchedOn })
            // The pause starts at its true duration and ends on the step that takes it to zero; one at zero ends on its next
            // step. Every count it can hold is swept.
            for (int raw = 0; raw <= DeadSidehopperVictimDefinitions.PostLandingPause; raw++)
            {
                state.Action = DeadSidehopperAction.PostLandingDelay;
                state.PostLandingPause = UpdateCountdown.For(raw);
                state.Layout = new SidehopperVictim(drainStage);
                state.NextHop = CyclingSequence<PixelVelocity<RoomPixels>>.At(DeadSidehopperLaunchDefinitions.Launches, phase);
                state.Velocity = new PixelVelocity<RoomPixels>(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4660), SignedEightEight.FromTwoHundredFiftySixthsOfPixel(4660));
                state.Playback = new(DeadTourianAnimations.StartingWith(DeadTourianSegment.SidehopperCorpse), AnimationPlace.StartOfAnimation);
                place.AnimationClock.HoldFor(9);
                dispatch(place, state, RoomEnemyFrameFixtures.StandaloneSamus(), RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), default);
                int timer = raw - 1;
                bool expired = timer <= 0;
                bool launch = expired && drainStage == SidehopperDrainStage.Untouched;
                AssertEqual(Math.Max(0, timer), state.PostLandingPause.UpdatesLeft, "Exact delay count, ending at zero");
                AssertEqual(launch ? vx : (ushort)0x1234, state.Velocity.X.RawValue, "Timer and drain gates control X launch");
                AssertEqual(launch ? vy : (ushort)0x1234, state.Velocity.Y.RawValue, "Timer and drain gates control Y launch");
                AssertEqual(expired ? (launch ? DeadSidehopperAction.ActivatedMovement : DeadSidehopperAction.Draining) : DeadSidehopperAction.PostLandingDelay,
                    state.Action, "Exact launch versus transformation handoff");
                AssertEqual(new AnimationPlayback<DeadTourianSegment>(DeadTourianAnimations.StartingWith(launch ? DeadTourianSegment.SidehopperIdle : DeadTourianSegment.SidehopperCorpse), AnimationPlace.StartOfAnimation),
                    state.Playback, "Launch restarts the alive-idle animation");
                AssertEqual(launch ? (ushort)1 : (ushort)9, place.AnimationClock.Hold, "Launch instruction timer");
                AssertEqual<int>(phase, state.NextHop.Position, "Launch does not advance landing-owned phase");
            }
        }
        Console.WriteLine("Dead sidehopper launches: all eight native words and 524288 actual timer/phase/palette transitions pass without a bus.");
    }
}
