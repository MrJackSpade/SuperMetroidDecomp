using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Enemies.WreckedShip;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>The visible ghost's typed bobbing speed and origin keep the cartridge's carry and wrapped comparison.</summary>
internal static class WreckedShipGhostBobbingVerification
{
    /// <summary>
    /// Runs the production main AI for one bobbing update. The ghost moves by its signed 16.16 speed, then accelerates
    /// by 6144/65536 of a pixel toward its origin, compared by the wrapped spatial difference: below the origin it
    /// slows, above it speeds back, and an origin just above the room's top edge (65535) still counts as above a ghost
    /// at Y 0. Speeds and positions are checked against independently computed 16.16 values.
    /// </summary>
    internal static void VerifyWreckedShipGhostBobbing()
    {
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) },
            new FrameCounters(0));
        foreach ((string context, ushort y, ushort originY, short whole, ushort expectedY, int expectedVelocity) in
                 new VerifyWreckedShipGhostBobbingCase[]
                 {
                     new VerifyWreckedShipGhostBobbingCase("moving below its origin", 100, 100, 1, 101, 65536 - 6144),
                     new VerifyWreckedShipGhostBobbingCase("moving above its origin", 100, 100, -1, 99, -65536 + 6144),
                     new VerifyWreckedShipGhostBobbingCase("below an origin wrapped past the top edge", 0, 65535, 1, 1, 65536 - 6144),
                 })
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.Coven, slot.Definition);
            slot.Position = FixedPosition<RoomPixels>.FromWhole(new(64, y));
            var state = new WreckedShipGhostEnemyState
            {
                Action = WreckedShipGhostAction.BobbingWhileVisible,
                PhaseTimer = 120,
                BobbingOrigin = new(0, originY),
                VerticalVelocity = SignedSixteenSixteen.FromParts(whole, 0),
            };
            slot.SetSpeciesState(state);

            WreckedShipGhostBehavior.RunWreckedShipGhostMainAi(slot, frame);

            AssertEqual(expectedY, slot.Position.Y.Whole, $"Ghost {context}: position");
            AssertEqual((ushort)0, slot.Position.Y.Fraction, $"Ghost {context}: subposition");
            AssertEqual(SixteenSixteenWords.ToDisplacement(expectedVelocity), state.VerticalVelocity, $"Ghost {context}: speed");
        }
        Console.WriteLine("  Wrecked Ship ghost bobbing: speed carries and the origin compares by the wrapped difference.");
    }

    /// <summary>Runs the ghost bobbing check.</summary>
    [VerificationCommand("--wrecked-ship-ghost-bobbing")]
    internal static int WreckedShipGhostBobbingCommand()
    {
        Suite(nameof(VerifyWreckedShipGhostBobbing), () => VerifyWreckedShipGhostBobbing());
        return 0;
    }
}
