using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Pins a bomb spread's launch velocities and its ceiling rule (#2459): each bomb's signed launch velocity with and
/// without the held-Down bonus, and a rise stopped by a ceiling keeping its fraction of a pixel as a fall.
/// </summary>
internal static class BombSpreadFlightVerification
{
    /// <summary>The five bombs' horizontal launch velocities from the leftmost, in sixteenths of a pixel per update.</summary>
    private static readonly int[] HorizontalSixteenths = [-16, -8, 0, 8, 16];

    /// <summary>The five bombs' vertical launch velocities unheld from the leftmost, in sixteenths of a pixel per update, negative upward.</summary>
    private static readonly int[] VerticalSixteenths = [0, -16, -24, -16, 0];

    /// <summary>Checks the launch velocities, the floor bounce and the ceiling rule.</summary>
    internal static void VerifyBombSpreadFlightRules()
    {
        VerifyLaunchVelocities();
        VerifyFlightRules();
        VerifyCeilingThroughBombUpdate();
        Console.WriteLine("Bomb spread flight: launch velocities for every hold, the floor bounce and the ceiling's kept fraction.");
    }

    /// <summary>
    /// Each bomb launches at its authored velocity, the centre rising 1.5 pixels per update, and every 64 updates of held
    /// Down add one whole pixel per update to every bomb's rise, up to three after the full hold.
    /// </summary>
    private static void VerifyLaunchVelocities()
    {
        foreach ((int held, int bonusPixels) in new[] { (0, 0), (63, 0), (64, 1), (127, 1), (128, 2), (191, 2), (192, 3) })
        {
            for (int place = 0; place < MorphBallBombDefinitions.PlaceCount; place++)
            {
                BombSpreadFlight flight = SamusBombSpreadLaunches.Launch(place, held);
                SignedSixteenSixteen expectedVertical = SignedSixteenSixteen.FromSixteenthsOfPixel(VerticalSixteenths[place] - 16 * bonusPixels);
                AssertEqual(SignedSixteenSixteen.FromSixteenthsOfPixel(HorizontalSixteenths[place]), flight.HorizontalVelocity,
                    $"bomb {place} after {held} updates of Down launches sideways at its authored velocity");
                AssertEqual(expectedVertical, flight.LaunchVelocity, $"bomb {place} after {held} updates of Down launches vertically with {bonusPixels} pixels of bonus rise");
                AssertEqual(expectedVertical, flight.VerticalVelocity, $"bomb {place} after {held} updates of Down starts at its launch velocity");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SamusBombSpreadLaunches.Launch(2, MorphBallBombDefinitions.DownChargeTimeoutUpdates + 1),
            "Down is never held past the spread's timeout");
    }

    /// <summary>A floor bounce restores the launch velocity; a ceiling keeps only the velocity's fraction above its whole pixels.</summary>
    private static void VerifyFlightRules()
    {
        BombSpreadFlight centre = SamusBombSpreadLaunches.Launch(2, 128);
        centre.Accelerate(SignedSixteenSixteen.FromSixteenthsOfPixel(40));
        centre.BounceOffFloor();
        AssertEqual(SignedSixteenSixteen.FromSixteenthsOfPixel(-56), centre.VerticalVelocity, "a floor bounce restores the held launch's 3.5-pixel rise");

        // A rise of 1.25 pixels per update is two whole pixels up and three quarters back down.
        centre.VerticalVelocity = SignedSixteenSixteen.FromSixteenthsOfPixel(-20);
        centre.StopRiseAtCeiling();
        AssertEqual(SignedSixteenSixteen.FromSixteenthsOfPixel(12), centre.VerticalVelocity, "a ceiling turns a 1.25-pixel rise into a 0.75-pixel fall");

        BombSpreadFlight near = SamusBombSpreadLaunches.Launch(1, 0);
        near.StopRiseAtCeiling();
        AssertEqual(SignedSixteenSixteen.Zero, near.VerticalVelocity, "a ceiling stops a whole-pixel rise dead");

        near.ReverseAtWall();
        AssertEqual(SignedSixteenSixteen.FromSixteenthsOfPixel(8), near.HorizontalVelocity, "a wall reverses the bomb's sideways travel");
    }

    /// <summary>
    /// A spread launched just below a ceiling: the bombs that rise into it go back where they were and fall from the
    /// fraction of a pixel their gravity-accelerated rise kept; on the next update they fall.
    /// </summary>
    private static void VerifyCeilingThroughBombUpdate()
    {
        const int width = 8;
        const int height = 8;
        var blocks = new RoomTerrainBlock[width * height];
        for (int x = 0; x < width; x++)
            blocks[2 * width + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
        var ceiling = new RoomLevelData(new Extent16<RoomBlocks>(width, height),
            new RoomLevelPlanes(blocks, new RoomBlockBehavior[blocks.Length], new RoomBlockAppearance[blocks.Length]),
            RoomLevelData.NothingBeyondGrid(), []);
        var terrain = new SamusTerrain { Level = ceiling, RoomObjects = SamusRoomObjectFixtures.Uninstalled };

        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation,
            new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new BeamChargeState(),
            InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs,
            // The row of solid blocks ends at 48; the bombs start on its lower edge.
            Position = FixedPosition<RoomPixels>.FromWhole(new(64, 48)),
        };
        samus.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, 0x1c00);
        SignedSixteenSixteen gravity = samus.Kinematics.YAcceleration.AsDisplacement();

        SamusBombProjectileSystem bombs = InstalledFixtures.CreateBombSystem();
        bombs.SpawnBombSpread(samus);
        _ = bombs.HandleBombs(terrain, samus, BombJumpOverlapPublication.Immediate);

        foreach (int place in new[] { 1, 2, 3 })
        {
            SamusBomb bomb = bombs.Bombs[place];
            BombSpreadFlight flight = bomb.SpreadFlight ?? throw new InvalidOperationException($"Spread bomb {place} has no flight.");
            SignedSixteenSixteen risen = SamusBombSpreadLaunches.ForPlace(place).VerticalVelocity + gravity;
            AssertEqual(48, bomb.Position.Y.Whole, $"bomb {place} goes back below the ceiling it rose into");
            AssertEqual(0, (int)bomb.Position.Y.Fraction, $"bomb {place} goes back to the very place it rose from");
            AssertEqual(64, bomb.Position.X.Whole, $"bomb {place} moves no further on the update it meets the ceiling");
            AssertEqual(SignedSixteenSixteen.FromParts(0, risen.Fraction), flight.VerticalVelocity,
                $"bomb {place} keeps the fraction of a pixel its rise held, now falling");
            AssertTrue(flight.VerticalVelocity.IsPositive, $"bomb {place} falls after the ceiling");
        }

        // The centre's rise of 1.5 pixels less gravity's 0.109375 kept 0.609375 pixels; gravity adds to it again.
        _ = bombs.HandleBombs(terrain, samus, BombJumpOverlapPublication.Immediate);
        SamusBomb centre = bombs.Bombs[2];
        AssertEqual(SignedSixteenSixteen.FromParts(0, 0xb800), centre.SpreadFlight!.VerticalVelocity, "the centre bomb's fall gathers gravity after the ceiling");
        AssertEqual(FixedCoordinate.Of(48, 0xb800), FixedCoordinate.Of(centre.Position.Y.Whole, centre.Position.Y.Fraction), "the centre bomb falls 0.71875 pixels the next update");
    }
}
