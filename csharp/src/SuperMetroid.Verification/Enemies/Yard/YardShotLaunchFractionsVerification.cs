using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>
/// Checks the Yard's shot launch (#2184) against the original's, which stores the whole word of each speed and never a
/// fraction word, so a Yard shot while still flying keeps the fractions its flight left.
/// </summary>
internal static class YardShotLaunchFractionsVerification
{
    /// <summary>
    /// Shoots a flying Yard through the production launch with each fraction class a flight's slowing and fall reach
    /// (none, a sixteenth, an eighth, a half, the extremes) on each axis and Samus facing either way: the Yard flies a
    /// pixel an update up and a pixel an update the way she faces, with both fractions it held.
    /// </summary>
    internal static void VerifyYardShotLaunchFractions()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.SetSpawnWords(EnemyDefinitionId.Yard, _ => new SuperMetroid.AssetExtraction.Enemies.PopulationWords(0, 0, 0, 0, 0));
        place.Bind(EnemyDefinitionId.Yard, place.Definition);
        YardEnemyBehavior.InitializeYard(place.HeldOccupant(), place);
        YardEnemyState state = enemies.YardStates[0] ?? throw new InvalidOperationException("Verification requires enemies.YardStates[0].");
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusArtwork());

        int[] fractions = [0x0000, 0x0001, 0x1000, 0x2000, 0x8000, 0xf000, 0xffff];
        int launches = 0;
        foreach ((SamusPoseId pose, int sideways) in new[] { (SamusPoseId.FacingLeftNormalPose, -1), (SamusPoseId.FacingRightNormalPose, 1) })
        {
            samus.Pose = pose;
            foreach (int sidewaysFraction in fractions)
            foreach (int verticalFraction in fractions)
            {
                state.Behavior = YardBehavior.BeamLaunched;
                state.AirborneVelocity = new FixedVector<RoomPixels>(
                    SignedSixteenSixteen.FromParts(2, sidewaysFraction), SignedSixteenSixteen.FromParts(-3, verticalFraction));
                YardEnemyBehavior.ShootYardIntoAir(place, state, samus);
                string at = $"Yard shot facing {pose} with fractions {sidewaysFraction:x4}, {verticalFraction:x4}";
                AssertEqual(YardBehavior.BeamLaunched, state.Behavior, $"{at}: behaviour");
                AssertEqual(SignedSixteenSixteen.FromParts(sideways, sidewaysFraction), state.AirborneVelocity.X,
                    $"{at}: sideways speed keeps its fraction");
                AssertEqual(SignedSixteenSixteen.FromParts(-1, verticalFraction), state.AirborneVelocity.Y,
                    $"{at}: vertical speed keeps its fraction");
                launches++;
            }
        }

        state.AirborneVelocity = FixedVector<RoomPixels>.Zero;
        AssertEqual(new FixedVector<RoomPixels>(SignedSixteenSixteen.FromParts(1, 0), SignedSixteenSixteen.FromParts(-1, 0)),
            YardShotLaunchFractions.Launched(state.AirborneVelocity, new Offset16<RoomPixels>(1, -1)),
            "a Yard at rest launches at whole pixels");
        Console.WriteLine($"  Yard shot launch: {launches} launches of a flying Yard keep both fractions its flight left.");
    }
}
