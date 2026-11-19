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
/// without the held-Down bonus, a rise stopped by a ceiling keeping its fraction of a pixel as a fall, and a bomb that
/// explodes inside a solid block staying where it burned out (#2466).
/// </summary>
internal static class BombSpreadFlightVerification
{
    /// <summary>The width and height, in blocks, of the fixtures' rooms.</summary>
    private const int RoomSide = 8;

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
        VerifyExplodingBombInSolidBlock();
        Console.WriteLine("Bomb spread flight: launch velocities for every hold, the floor bounce, the ceiling's kept fraction and the exploding bomb's rest.");
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
        // The row of solid blocks ends at 48; the bombs start on its lower edge.
        SamusTerrain terrain = RoomWithSolidBlocks(Enumerable.Range(0, RoomSide).Select(x => (x, 2)));
        SamusState samus = MorphedSamusWithBombs(FixedPosition<RoomPixels>.FromWhole(new(64, 48)));
        SignedSixteenSixteen gravity = samus.Kinematics.YAcceleration.AsDisplacement();

        SamusBombProjectileSystem bombs = InstalledFixtures.CreateBombSystem();
        bombs.SpawnBombSpread(samus);
        bombs.HandleBombs(terrain, samus, BombJumpOverlapPublication.Immediate);

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
        bombs.HandleBombs(terrain, samus, BombJumpOverlapPublication.Immediate);
        SamusBomb centre = bombs.Bombs[2];
        AssertEqual(SignedSixteenSixteen.FromParts(0, 0xb800), centre.SpreadFlight!.VerticalVelocity, "the centre bomb's fall gathers gravity after the ceiling");
        AssertEqual(FixedCoordinate.Of(48, 0xb800), FixedCoordinate.Of(centre.Position.Y.Whole, centre.Position.Y.Fraction), "the centre bomb falls 0.71875 pixels the next update");
    }

    /// <summary>
    /// A spread bomb whose fuse runs out with its centre inside a solid block (#2466): it explodes where it is, its
    /// sideways travel is not turned back and gravity no longer acts on it, on the update it burns out and after. The
    /// game's spread collision test, run with the fuse at zero, sets off the explosion's cross and reports no collision,
    /// so the wall turn that would move the bomb back one step never runs.
    /// </summary>
    private static void VerifyExplodingBombInSolidBlock()
    {
        const int rightmost = MorphBallBombDefinitions.PlaceCount - 1;
        // The block whose pixels run from 80 to 95 across and 48 to 63 down holds the bomb's centre.
        SamusTerrain terrain = RoomWithSolidBlocks([(5, 3)]);
        SamusState samus = MorphedSamusWithBombs(FixedPosition<RoomPixels>.FromWhole(new(40, 104)));

        SamusBombProjectileSystem bombs = InstalledFixtures.CreateBombSystem();
        bombs.SpawnBombSpread(samus);
        SamusBomb bomb = bombs.Bombs[rightmost];
        BombSpreadFlight flight = bomb.SpreadFlight ?? throw new InvalidOperationException("The rightmost spread bomb has no flight.");
        FixedPosition<RoomPixels> inBlock = FixedPosition<RoomPixels>.FromWhole(new(88, 56));
        bomb.Position = inBlock;
        bomb.Fuse = UpdateCountdown.For(1);
        BombSpreadLaunchDefinition launch = SamusBombSpreadLaunches.ForPlace(rightmost);

        foreach (string update in new[] { "on the update its fuse runs out", "on the update after" })
        {
            bombs.HandleBombs(terrain, samus, BombJumpOverlapPublication.Immediate);
            AssertTrue(bomb.HasBurnedOut && bomb.HasReactedToTerrain, $"the bomb has exploded {update}");
            AssertEqual(inBlock.X.Whole, bomb.Position.X.Whole, $"the exploding bomb stays inside the block {update}");
            AssertEqual(0, (int)bomb.Position.X.Fraction, $"the exploding bomb is not moved back a step {update}");
            AssertEqual(inBlock.Y.Whole, bomb.Position.Y.Whole, $"the exploding bomb neither rises nor falls {update}");
            AssertEqual(launch.HorizontalVelocity, flight.HorizontalVelocity, $"the exploding bomb's sideways travel is not turned back {update}");
            AssertEqual(launch.VerticalVelocity, flight.VerticalVelocity, $"gravity no longer acts on the exploding bomb {update}");
        }
    }

    /// <summary>A room of air but for solid blocks at <paramref name="solid"/>, holding no room objects.</summary>
    private static SamusTerrain RoomWithSolidBlocks(IEnumerable<(int X, int Y)> solid)
    {
        var blocks = new RoomTerrainBlock[RoomSide * RoomSide];
        foreach ((int x, int y) in solid)
            blocks[y * RoomSide + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(RoomSide, RoomSide),
            new RoomLevelPlanes(blocks, new RoomBlockBehavior[blocks.Length], new RoomBlockAppearance[blocks.Length]),
            RoomLevelData.NothingBeyondGrid(), []);
        return new SamusTerrain { Level = level, RoomObjects = SamusRoomObjectFixtures.Uninstalled };
    }

    /// <summary>Samus rolled up at <paramref name="position"/> with bombs, falling under her ordinary gravity.</summary>
    private static SamusState MorphedSamusWithBombs(FixedPosition<RoomPixels> position)
    {
        var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation,
            new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new BeamChargeState(),
            InstalledFixtures.SamusArtwork())
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs,
            Position = position,
        };
        samus.Kinematics.YAcceleration = new UnsignedSixteenSixteen(0, 0x1c00);
        return samus;
    }
}
