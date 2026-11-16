using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Checks that Crocomire, melted once, shows the first melt's picture of the rows above the acid where he stands on
/// every update of his three rises and sinks between the two melts (#2193), as the cartridge's handlers for those phases
/// select it by his height and restart it each update.
/// </summary>
internal static class CrocomireFirstMeltHeightVerification
{
    /// <summary>The death phases between the two melts, in which the cartridge selects the first melt's picture by height.</summary>
    private static readonly CrocomireDeathPhase[] FirstMeltHops =
    [
        CrocomireDeathPhase.ThirdHopRise, CrocomireDeathPhase.FourthHopSink, CrocomireDeathPhase.FourthHopRise,
        CrocomireDeathPhase.FifthHopSink, CrocomireDeathPhase.FifthHopRise, CrocomireDeathPhase.SixthHopSink,
    ];

    /// <summary>
    /// The heights either side of each band's edge, with the animation and the picture the cartridge selects at each: his
    /// top four rows above 248, three from 248, two from 264 and his top row alone from 280.
    /// </summary>
    private static readonly (int Y, CrocomireBodySegment Animation, EnemyFrameId Frame)[] HeightBands =
    [
        (220, CrocomireBodySegment.MeltingOneTopFourRows, EnemyFrameId.CrocomireBodyMeltingOneTopFourRows),
        (247, CrocomireBodySegment.MeltingOneTopFourRows, EnemyFrameId.CrocomireBodyMeltingOneTopFourRows),
        (248, CrocomireBodySegment.MeltingOneTopThreeRows, EnemyFrameId.CrocomireBodyMeltingOneTopThreeRows),
        (263, CrocomireBodySegment.MeltingOneTopThreeRows, EnemyFrameId.CrocomireBodyMeltingOneTopThreeRows),
        (264, CrocomireBodySegment.MeltingOneTopTwoRows, EnemyFrameId.CrocomireBodyMeltingOneTopTwoRows),
        (279, CrocomireBodySegment.MeltingOneTopTwoRows, EnemyFrameId.CrocomireBodyMeltingOneTopTwoRows),
        (280, CrocomireBodySegment.MeltingOneTopRow, EnemyFrameId.CrocomireBodyMeltingOneTopRow),
        (290, CrocomireBodySegment.MeltingOneTopRow, EnemyFrameId.CrocomireBodyMeltingOneTopRow),
    ];

    /// <summary>
    /// For each rise and sink between the two melts and each height band, one production update of Crocomire's main AI
    /// from his height starts the band's first melt animation afresh, its first picture due on that update, and his
    /// animation then shows the band's picture, whose collision is the jaw strip every first melt picture holds. The bands
    /// are walked down and back up, so an update starts either a different animation from the one before or, within a
    /// band, the same one again from its start.
    /// </summary>
    internal static void VerifyCrocomireFirstMeltPicturesByHeight()
    {
        var fixture = new EnemyEffectsFixtureVerification.CrocomireEffectFixture(EnemyEffectsFixtureVerification.MeltEffectArtwork());
        fixture.InitializeMap(CrocomireMeltingIllustration.First);
        CrocomireEnemyState state = fixture.Actor;
        RoomEnemyPlace body = state.Body;
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusArtwork())
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(256, 128)),
        };
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new RoomTerrainBlock[1024], new RoomBlockBehavior[1024], new RoomBlockAppearance[1024]), RoomLevelData.NothingBeyondGrid(), []);
        var camera = new Position16<RoomPixels>(256, 0);
        IReadOnlyList<EnemyCollisionHitbox> jawStrip = CrocomireBodyCollision.ComponentsAt(EnemyFrameId.CrocomireBodyMeltingOneTopRow)[0].Hitboxes;

        int updates = 0;
        foreach (CrocomireDeathPhase phase in FirstMeltHops)
        foreach ((int y, CrocomireBodySegment animation, EnemyFrameId frame) in HeightBands.Concat(HeightBands.Reverse()))
        {
            string at = $"{phase} from height {y}";
            state.DeathPhase = phase;
            state.VerticalSpeed = SignedEightEight.FromPixels(1);
            body.Position = body.Position.WithWholeY(y);
            fixture.Enemies.CrocomireBehavior.RunCrocomireMain(body, samus, RoomEnemyFrameFixtures.StandalonePseudoScrew(), camera);
            AssertEqual(animation, state.BodyPlayback.PlayingSegment, $"{at}: animation");
            AssertEqual(AnimationPlace.StartOfAnimation, state.BodyPlayback.Place, $"{at}: the animation starts afresh");
            AssertEqual(WrappingCountdown.Start(1), body.AnimationClock.Hold, $"{at}: its first picture is due this update");

            fixture.Enemies.CrocomireBehavior.Fight.AdvanceCrocomireBodyAnimation(body, samus, level, camera.X);
            AssertEqual((EnemyFrameId?)frame, body.Frame, $"{at}: picture");
            ReadOnlySpan<EnemyCollisionComponent> components = CrocomireBodyCollision.ComponentsAt(body.Frame);
            AssertEqual(1, components.Length, $"{at}: collision components");
            AssertEqual(new Offset16<RoomPixels>(0, 0), components[0].Offset, $"{at}: collision offset");
            AssertTrue(ReferenceEquals(jawStrip, components[0].Hitboxes), $"{at}: the picture collides as the jaw strip");
            updates++;
        }
        Console.WriteLine($"  Crocomire first melt: {updates} updates of the rises and sinks between the melts show the picture of the rows above the acid.");
    }
}
