using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Verification.Enemies;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Draygon;
using SuperMetroid.Verification.Harness;

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
    /// The attached goop's lifetime decrements then tests for zero (DEC then BNE): a lifetime of two holds for one
    /// update and releases the goop on the second, and a lifetime of zero wraps to the full word and stays attached.
    /// </summary>
    private static void VerifyAttachedGoopCountdown()
    {
        (RoomEnemySystem enemies, _) = NewDraygon();
        var samus = new SamusState { XSpeedDivisor = 1 };
        RoomEnemyProjectileSlot goop = AttachedGoop(enemies, lifetime: 2);
        enemies.DraygonBehavior.RunAttachedDraygonGoop(goop, samus);
        AssertTrue(goop.IsActive, "an attached goop with lifetime two holds for its first update");
        AssertEqual((ushort)1, goop.Variable0, "the first update leaves one update of lifetime");
        enemies.DraygonBehavior.RunAttachedDraygonGoop(goop, samus);
        AssertTrue(!goop.IsActive, "the goop falls off on the update its lifetime reaches zero");
        AssertEqual((ushort)0, samus.XSpeedDivisor, "the goop that falls off restores one X-speed divisor step");

        samus.XSpeedDivisor = 1;
        RoomEnemyProjectileSlot cleared = AttachedGoop(enemies, lifetime: 0);
        enemies.DraygonBehavior.RunAttachedDraygonGoop(cleared, samus);
        AssertTrue(cleared.IsActive, "a cleared lifetime wraps below zero instead of releasing the goop");
        AssertEqual(ushort.MaxValue, cleared.Variable0, "a cleared lifetime wraps to the full word");
    }

    /// <summary>Puts an attached Draygon goop with the given lifetime in the last projectile slot.</summary>
    private static RoomEnemyProjectileSlot AttachedGoop(RoomEnemySystem enemies, ushort lifetime)
    {
        RoomEnemyProjectileSlot goop = enemies.Projectiles.EnemyProjectiles[^1];
        goop.Kind = RoomEnemyProjectileKind.DraygonGoop;
        goop.PreInstruction = EnemyProjectilePreInstruction.DraygonGoop_StuckToSamus;
        goop.Variable1 = 1;
        goop.Variable0 = lifetime;
        return goop;
    }

    /// <summary>
    /// The goop pass bobs at 384 plus the cosine of its phase times 32, and the phase wraps from the last table unit
    /// back to zero, as the native byte increment did.
    /// </summary>
    private static void VerifyGoopBobWrap()
    {
        (_, DraygonEnemyState state) = NewDraygon();
        state.SwoopVelocity = 0;
        state.GoopYOscillationAngle = SnesAngle.FromTableIndex(byte.MaxValue);
        DraygonBehavior.MoveDraygonAlongGoopPath(state, movingRight: true);
        AssertEqual(unchecked((ushort)(384 + EnemyMath.ReadEightBitCosineProduct(new SnesByteAngle(byte.MaxValue), 32))), state.Body.Position.Y.Whole,
            "the bob reads the phase before advancing it");
        AssertEqual(SnesAngle.Zero, state.GoopYOscillationAngle, "the phase wraps from the last table unit to zero");
        DraygonBehavior.MoveDraygonAlongGoopPath(state, movingRight: true);
        AssertEqual(unchecked((ushort)(384 + EnemyMath.ReadEightBitCosineProduct(new SnesByteAngle(0), 32))), state.Body.Position.Y.Whole,
            "the next bob reads the wrapped phase");
        AssertEqual(1, (int)state.GoopYOscillationAngle.TableIndex, "the wrapped phase advances by one unit");
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
        state.SpiralXRadius = SnesSignedSixteenSixteen.FromParts((short)radiusWhole, radiusFraction);
        state.SpiralAngle = SnesAngle.FromTableIndex(angle);
        state.SpiralAngleDelta = SnesAngle.FromRaw(angleDeltaRaw);
        // A centre on the spiral's upper limit ends the step at the lift check, before Samus is moved.
        state.SpiralCenter = FixedPosition<RoomPixels>.FromWhole(new(256, 64));
        Position16<RoomPixels> center = state.SpiralCenter.Whole;
        state.Function = DraygonAiFunction.FlailWithSamus;

        enemies.DraygonBehavior.CarrySamusInDraygonSpiral(state, new SamusState());

        Position16<RoomPixels> expectedBody = center + new Offset16<RoomPixels>(
            unchecked((short)EnemyMath.ReadEightBitCosineProduct(new SnesByteAngle(angle), radiusWhole)),
            unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(new SnesByteAngle(angle), (ushort)(radiusWhole >> 2))));
        AssertEqual(expectedBody, state.Body.Position.Whole, "the body is placed from the angle and radius before they advance");
        uint nativeRadius = unchecked(SnesSixteenSixteen.Compose(radiusWhole, radiusFraction) + 8192u);
        AssertEqual(unchecked((int)nativeRadius), state.SpiralXRadius.RawValue, "the radius grows by one eighth of a pixel, carrying into its whole pixels");
        AssertEqual((ushort)6, state.SpiralXRadius.WholeWord, "the carried radius reaches the next whole pixel");
        ushort nativeDelta = unchecked((ushort)(angleDeltaRaw - 1));
        AssertEqual(nativeDelta, state.SpiralAngleDelta.RawValue, "the angular step slows by 1/256 unit");
        AssertEqual(unchecked((byte)(angle + SnesUnsignedEightEight.Whole(nativeDelta))), state.SpiralAngle.TableIndex,
            "the angle advances by the step's whole units and wraps around the turn");
        AssertEqual(DraygonAiFunction.FinalTailWhips, state.Function, "the lifted centre past its limit ends the spiral");
    }

    /// <summary>
    /// Each eye hit adds 8/256 to the swoop acceleration while the result stays below 160 in a signed compare, so it
    /// climbs from 24 to 152 and stops; only the vertical axis changes.
    /// </summary>
    private static void VerifyShotSwoopAcceleration()
    {
        (_, DraygonEnemyState state) = NewDraygon();
        state.SwoopAcceleration = new EightEightVector<RoomPixels>(0, 24);
        ushort native = 24;
        for (int hit = 0; hit < 20; hit++)
        {
            DraygonBehavior.AdvanceDraygonShotAcceleration(state);
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
        (_, DraygonEnemyState state) = NewDraygon();
        const ushort resetX = 65456, resetY = 128;
        state.LeftSideReset = new Position16<RoomPixels>(resetX, resetY);
        state.SwoopAcceleration = new EightEightVector<RoomPixels>(0, 40);
        var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(200, 400)) };

        DraygonBehavior.BuildDraygonSwoopYPositions(state, samus);

        var expected = new List<ushort>();
        ushort y = 384, speed = 0;
        while (true)
        {
            speed = unchecked((ushort)(speed + 40));
            ushort candidate = unchecked((ushort)(y - SnesEightEight.WholeOf(speed)));
            if (unchecked((short)(candidate - resetY)) < 0)
                break;
            y = candidate;
            expected.Add(candidate);
        }
        AssertEqual(expected.Count, state.SwoopPathEntryCount, "swoop path sample count");
        for (int index = 0; index < expected.Count; index++)
            AssertEqual(expected[index], state.SwoopYPositions[index], $"swoop path sample {index}");
        AssertEqual(state.Body.Position.Y.Whole, state.SwoopYPositions[expected.Count], "the path ends at the body's height");
        uint velocity = SnesSixteenSixteen.FromWhole((uint)EnemyMath.WrappedMagnitude(unchecked((ushort)(resetX - 200)))) / (uint)expected.Count;
        AssertEqual(velocity, state.SwoopVelocity, "swoop horizontal speed");
    }

    /// <summary>A Draygon encounter over a fresh enemy system with all four parts bound and asleep.</summary>
    private static (RoomEnemySystem Enemies, DraygonEnemyState State) NewDraygon()
    {
        var enemies = new RoomEnemySystem(new EnemySession());
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
        state.Eye.SetSpeciesState(eyeState, eyeState.ClearWithRecord);
        state.Tail.Bind(EnemyDefinitionId.DraygonTail, state.Tail.Definition);
        state.Arms.Bind(EnemyDefinitionId.DraygonArms, state.Arms.Definition);
        foreach (RoomEnemySlot part in new[] { body, state.Eye, state.Tail, state.Arms })
            state.SetCursor(part, AnimationProgramCursor<DraygonProgram>.Start(DraygonProgram.Sleep));
        enemies.DraygonBehavior.Draygon = state;
        // A nonzero low byte keeps the spiral from inserting a tail whip.
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 4660);
        enemies.VerificationServices().ReadRandom = (Func<ushort>)(() => 4660);
        return (enemies, state);
    }

    /// <summary>Runs the Draygon state-quantity checks from the verification command line.</summary>
    [VerificationCommand("--draygon-state-quantities")]
    internal static int DraygonStateQuantitiesCommand()
    {
        Suite(nameof(VerifyDraygonStateQuantities), () => VerifyDraygonStateQuantities());
        return 0;
    }
}
