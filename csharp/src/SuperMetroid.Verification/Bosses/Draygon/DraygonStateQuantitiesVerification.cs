using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>
/// Draygon's encounter quantities stored as domain types instead of native words: the goop attachment countdown, the
/// goop bob and carry-spiral angles (which wrap around the turn), the spiral radius, and the swoop acceleration. Each
/// check drives the production routine and compares it with the arithmetic the native words performed.
/// </summary>
internal static class DraygonStateQuantitiesVerification
{
    /// <summary>Runs every Draygon state-quantity check.</summary>
    internal static void VerifyDraygonStateQuantities()
    {
        VerifyAttachedGoopCountdown();
        VerifyGoopBobWrap();
        VerifyCarrySpiralStep();
        VerifyShotSwoopAcceleration();
        VerifySwoopPath();
        Console.WriteLine("Draygon state quantities: goop countdown, bob and spiral angle wraps, spiral radius carry, swoop acceleration and path match the native arithmetic.");
    }

    /// <summary>
    /// The attached goop's lifetime decrements then tests for zero: a lifetime of two holds for one update and releases
    /// the goop on the second.
    /// </summary>
    private static void VerifyAttachedGoopCountdown()
    {
        (RoomEnemySystem enemies, _) = NewDraygon();
        var samus = new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { XSpeedDivisor = 1 };
        EnemyProjectile goop = AttachedGoop(enemies, lifetime: 2);
        enemies.DraygonBehavior.Shots.Goop.RunAttachedDraygonGoop(goop, samus);
        AssertTrue(goop.IsActive, "an attached goop with lifetime two holds for its first update");
        AssertEqual(1, goop.StateOf<DraygonGoopAttachedState>().Lifetime, "the first update leaves one update of lifetime");
        enemies.DraygonBehavior.Shots.Goop.RunAttachedDraygonGoop(goop, samus);
        AssertTrue(!goop.IsActive, "the goop falls off on the update its lifetime reaches zero");
        AssertEqual(0, samus.XSpeedDivisor, "the goop that falls off restores one X-speed divisor step");
    }

    /// <summary>Puts an attached Draygon goop with the given lifetime in the last projectile slot.</summary>
    private static EnemyProjectile AttachedGoop(RoomEnemySystem enemies, int lifetime)
    {
        EnemyProjectile goop = enemies.Projectiles.EnemyProjectiles[^1];
        goop.Kind = RoomEnemyProjectileKind.DraygonGoop;
        goop.FrameBehavior = EnemyProjectileFrameBehavior.DraygonGoopStuckToSamus;
        goop.State = new DraygonGoopAttachedState(stackPosition: 1) { Lifetime = lifetime };
        return goop;
    }

    /// <summary>
    /// The goop pass bobs at 384 plus the cosine of its phase times 32, and the phase wraps from the last table unit
    /// back to zero, as the native byte increment did.
    /// </summary>
    private static void VerifyGoopBobWrap()
    {
        (_, DraygonEnemyState state) = NewDraygon();
        state.SwoopVelocity = default;
        state.GoopYOscillationAngle = RotationAngle.FromSteps(byte.MaxValue);
        DraygonGoop.MoveDraygonAlongGoopPath(state, movingRight: true);
        AssertEqual(unchecked((ushort)(384 + EnemyMath.ReadEightBitCosineProduct(new TrigonometryAngle(byte.MaxValue), 32))), state.Body.Position.Y.Whole,
            "the bob reads the phase before advancing it");
        AssertEqual(RotationAngle.Zero, state.GoopYOscillationAngle, "the phase wraps from the last table unit to zero");
        DraygonGoop.MoveDraygonAlongGoopPath(state, movingRight: true);
        AssertEqual(unchecked((ushort)(384 + EnemyMath.ReadEightBitCosineProduct(new TrigonometryAngle(0), 32))), state.Body.Position.Y.Whole,
            "the next bob reads the wrapped phase");
        AssertEqual(1, (int)state.GoopYOscillationAngle.ToTrigonometryAngle().RawValue, "the wrapped phase advances by one unit");
    }

    /// <summary>
    /// One carry-spiral step places Draygon from the current angle and radius, grows the 16.16 radius by one eighth of
    /// a pixel (carrying into the whole pixels), slows the angular step by 1/256 unit and adds its whole units to the
    /// angle, which wraps around the turn.
    /// </summary>
    private static void VerifyCarrySpiralStep()
    {
        (RoomEnemySystem enemies, DraygonEnemyState state) = NewDraygon();
        const ushort radiusWhole = 5, radiusFraction = 57344, angleDeltaRaw = 2048;
        const byte angle = 250;
        state.SpiralXRadius = SignedSixteenSixteen.FromParts((short)radiusWhole, radiusFraction);
        state.SpiralAngle = RotationAngle.FromSteps(angle);
        state.SpiralAngleDelta = RotationAngle.FromRaw(angleDeltaRaw);
        // A centre on the spiral's upper limit ends the step at the lift check, before Samus is moved.
        state.SpiralCenter = DraygonRegrabSpiralLift.At(FixedPosition<RoomPixels>.FromWhole(new(256, 64)));
        Position16<RoomPixels> center = state.SpiralCenter.Center.Whole;
        state.Phase = DraygonPhase.FlailWithSamus;

        enemies.DraygonBehavior.Grab.CarrySamusInDraygonSpiral(state, new SamusState(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()));

        Position16<RoomPixels> expectedBody = center + new Offset16<RoomPixels>(
            unchecked((short)EnemyMath.ReadEightBitCosineProduct(new TrigonometryAngle(angle), radiusWhole)),
            unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(new TrigonometryAngle(angle), radiusWhole >> 2)));
        AssertEqual(expectedBody, state.Body.Position.Whole, "the body is placed from the angle and radius before they advance");
        uint nativeRadius = unchecked(SixteenSixteenWords.Compose(radiusWhole, radiusFraction) + 8192u);
        AssertEqual(unchecked((int)nativeRadius), state.SpiralXRadius.RawValue, "the radius grows by one eighth of a pixel, carrying into its whole pixels");
        AssertEqual((ushort)6, state.SpiralXRadius.UnsignedWholePixels, "the carried radius reaches the next whole pixel");
        ushort nativeDelta = unchecked(angleDeltaRaw - 1);
        AssertEqual(nativeDelta, state.SpiralAngleDelta.RawValue, "the angular step slows by 1/256 unit");
        AssertEqual(unchecked((byte)(angle + UnsignedEightEight.Whole(nativeDelta))), state.SpiralAngle.ToTrigonometryAngle().RawValue,
            "the angle advances by the step's whole units and wraps around the turn");
        AssertEqual(DraygonPhase.FinalTailWhips, state.Phase, "the lifted centre past its limit ends the spiral");
    }

    /// <summary>
    /// Each eye hit adds 8/256 to the swoop acceleration while the result stays below 160 in a signed compare, so it
    /// climbs from 24 to 152 and stops; only the vertical axis changes.
    /// </summary>
    private static void VerifyShotSwoopAcceleration()
    {
        (_, DraygonEnemyState state) = NewDraygon();
        state.SwoopAcceleration = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, 24);
        ushort native = 24;
        for (int hit = 0; hit < 20; hit++)
        {
            DraygonCombat.AdvanceDraygonShotAcceleration(state);
            ushort candidate = unchecked((ushort)(native + 8));
            if (unchecked((short)(candidate - 160)) < 0)
                native = candidate;
            AssertEqual(native, state.SwoopAcceleration.Y.RawValue, $"swoop acceleration after eye hit {hit}");
            AssertEqual((ushort)0, state.SwoopAcceleration.X.RawValue, $"horizontal swoop acceleration after eye hit {hit}");
        }
        AssertEqual((ushort)152, state.SwoopAcceleration.Y.RawValue, "the swoop acceleration stops below 160");
    }

    /// <summary>
    /// The swoop path climbs from 384 by an 8.8 speed that grows by the swoop acceleration each sample, until the next
    /// height would pass above the reset height; the table ends with the body's height and the horizontal speed covers
    /// the distance from the left reset point to Samus over the path.
    /// </summary>
    private static void VerifySwoopPath()
    {
        // The second path climbs at a 256th of a pixel per update per update to a reset height four pixels below the
        // first's: 513 heights, past the 512 the original game's table holds, which the path keeps (#1627).
        VerifySwoopPath(acceleration: 40, resetY: 128);
        VerifySwoopPath(acceleration: 1, resetY: 124, expectedCount: 513);

        (_, DraygonEnemyState still) = NewDraygon();
        still.LeftSideReset = new Position16<RoomPixels>(65456, 128);
        still.SwoopAcceleration = default;
        AssertThrows<InvalidOperationException>(() => DraygonSwoops.BuildDraygonSwoopYPositions(still, NewSwoopSamus()),
            "a swoop path is not planned without a swoop acceleration");
        AssertTrue(still.SwoopPath is null, "Draygon has no swoop path before he plans one");
    }

    /// <summary>
    /// Builds a swoop path under <paramref name="acceleration"/> 256ths of a pixel per update per update to
    /// <paramref name="resetY"/> and compares it, entry by entry, with the original game's table build.
    /// </summary>
    private static void VerifySwoopPath(ushort acceleration, ushort resetY, int? expectedCount = null)
    {
        (_, DraygonEnemyState state) = NewDraygon();
        const ushort resetX = 65456;
        state.LeftSideReset = new Position16<RoomPixels>(resetX, resetY);
        state.SwoopAcceleration = EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(0, acceleration);

        DraygonSwoops.BuildDraygonSwoopYPositions(state, NewSwoopSamus());

        var expected = new List<ushort>();
        ushort y = 384, speed = 0;
        while (true)
        {
            speed = unchecked((ushort)(speed + acceleration));
            ushort candidate = unchecked((ushort)(y - EightEight.WholeOf(speed)));
            if (unchecked((short)(candidate - resetY)) < 0)
                break;
            y = candidate;
            expected.Add(candidate);
        }
        if (expectedCount is { } count)
            AssertEqual(count, expected.Count, $"the reference table build under acceleration {acceleration} holds {count} heights");
        DraygonSwoopPath path = state.SwoopPath ?? throw new InvalidOperationException("The swoop plans its path.");
        AssertEqual(expected.Count, path.EntryCount, "swoop path sample count");
        for (int index = 0; index < expected.Count; index++)
            AssertEqual(expected[index], path.HeightAt(index), $"swoop path sample {index}");
        AssertEqual(state.Body.Position.Y.Whole, path.HeightAt(expected.Count), "the path ends at the body's height");
        AssertThrows<ArgumentOutOfRangeException>(() => path.HeightAt(expected.Count + 1), "the path has no height past its starting height");
        uint velocity = SixteenSixteenWords.FromWhole((uint)Math.Abs(CoordinateAxis.AsSigned(unchecked(resetX - 200)))) / (uint)expected.Count;
        AssertEqual(velocity, state.SwoopVelocity.RawValue, "swoop horizontal speed");
        AssertEqual(expected.Count, state.SwoopEntry, "the descent starts at the swoop's starting height");
    }

    /// <summary>Samus at the point the swoop paths aim at.</summary>
    private static SamusState NewSwoopSamus() =>
        new(InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(200, 400)) };

    /// <summary>A Draygon encounter over a fresh enemy system with all four parts bound and asleep.</summary>
    private static NewDraygonResult NewDraygon()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.DraygonBody, body.Definition);
        body.Position = body.Position.WithWhole(new(256, 384));
        var state = new DraygonEnemyState(body)
        {
            Eye = enemies.SlotTable.Slots[1],
            Tail = enemies.SlotTable.Slots[2],
            Arms = enemies.SlotTable.Slots[3],
        };
        state.Eye.Bind(EnemyDefinitionId.DraygonEye, state.Eye.Definition);
        var eyeState = new DraygonEyeState();
        state.Eye.SetSpeciesState(eyeState);
        state.Tail.Bind(EnemyDefinitionId.DraygonTail, state.Tail.Definition);
        state.Arms.Bind(EnemyDefinitionId.DraygonArms, state.Arms.Definition);
        foreach (RoomEnemySlot part in new[] { body, state.Eye, state.Tail, state.Arms })
            state.StartAnimation(part, DraygonSegment.Sleep);
        enemies.DraygonBehavior.Parts.Draygon = state;
        // A nonzero low byte keeps the spiral from inserting a tail whip.
        enemies.VerificationServices().NextRandom = () => 4660;
        enemies.VerificationServices().ReadRandom = () => 4660;
        return new NewDraygonResult(enemies, state);
    }

    /// <summary>Runs the Draygon state-quantity checks from the verification command line.</summary>
    [VerificationCommand("--draygon-state-quantities")]
    internal static int DraygonStateQuantitiesCommand()
    {
        Suite(nameof(VerifyDraygonStateQuantities), () => VerifyDraygonStateQuantities());
        return 0;
    }
}
