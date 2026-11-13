using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Spatial;

/// <summary>
/// Confirms the #1618 conversions of the bosses', front end's and areas' raw fixed-point words: the named-unit speeds
/// equal the words they replace, the up-zero negative cosine read as the sine a quarter turn back equals the combined
/// table's sample, the angle sectors and comparisons equal the bit tests they replace, the typed turns equal the raw
/// angle sums, and the floor zoom keeps every 256th the scenes step it by.
/// </summary>
internal static class TypedTurnsAndZoomVerification
{
    /// <summary>Runs every check of the typed speeds, turns and zoom.</summary>
    internal static void VerifyTypedTurnsAndZoom()
    {
        VerifyNamedUnitSpeeds();
        VerifyNegativeCosineAsSineQuarterTurnBack();
        VerifyAngleSectorsAndComparisons();
        VerifyTypedTurns();
        VerifyFloorZoom();
        Console.WriteLine("  Typed turns and zoom: speeds, negative cosine, sectors, turns and zoom steps equal the words they replace.");
    }

    /// <summary>Every signed speed in 256ths and every whole-pixel speed is the word it replaces, and the range is enforced.</summary>
    private static void VerifyNamedUnitSpeeds()
    {
        for (int twoHundredFiftySixths = short.MinValue; twoHundredFiftySixths <= short.MaxValue; twoHundredFiftySixths++)
        {
            SignedEightEight speed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(twoHundredFiftySixths);
            if (speed.RawValue != unchecked((ushort)twoHundredFiftySixths) || speed.InTwoHundredFiftySixthsOfPixel != twoHundredFiftySixths)
                throw new InvalidDataException($"Speed of {twoHundredFiftySixths} 256ths does not keep its word.");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => SignedEightEight.FromTwoHundredFiftySixthsOfPixel(short.MaxValue + 1), "a speed past the type's range fails");
        for (int pixels = sbyte.MinValue; pixels <= sbyte.MaxValue; pixels++)
            AssertEqual(SignedEightEight.FromTwoHundredFiftySixthsOfPixel(pixels * 256), SignedEightEight.FromPixels(pixels), $"{pixels} whole pixels an update");
        AssertThrows<ArgumentOutOfRangeException>(() => SignedEightEight.FromPixels(sbyte.MaxValue + 1), "a whole-pixel speed past the type's range fails");
        AssertEqual(EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-1472, 40), EightEightVector<RoomPixels>.FromTwoHundredFiftySixthsOfPixel(-1472, 40), "a vector in 256ths keeps both words");
        AssertEqual(default(EightEightVector<RoomPixels>), EightEightVector<RoomPixels>.Zero, "the zero vector is no velocity");
        AssertEqual(SignedSixteenSixteen.FromParts(-4, 32768), KraidPlatformMovement.FiringDisplacement, "Kraid's lint fires three and a half pixels leftward");
    }

    /// <summary>
    /// For every angle, a quarter turn on is the angle turned by a quarter of the turn's steps: the up-zero headings read
    /// their horizontal and vertical speeds as the sine there and a quarter turn back, whose samples the cartridge oracle
    /// of the signed sine checks.
    /// </summary>
    private static void VerifyNegativeCosineAsSineQuarterTurnBack()
    {
        for (int units = 0; units < RotationAngle.StepsPerTurn; units++)
        {
            var angle = new TrigonometryAngle(units);
            AssertEqual(angle.TurnedBySteps(64), angle + TrigonometryAngle.QuarterTurn, $"a quarter turn on from {units}");
        }
    }

    /// <summary>Sectors, octants and quarters equal the bit tests and thresholds they replace; comparisons order by units.</summary>
    private static void VerifyAngleSectorsAndComparisons()
    {
        for (int units = 0; units < RotationAngle.StepsPerTurn; units++)
        {
            var bearing = new Bearing(units);
            AssertEqual(units >> 5, bearing.Sector(8), $"bearing {units} octant");
            AssertEqual(((units + 16) & 255) >> 5, bearing.TurnedBySteps(16).Sector(8), $"bearing {units} half-octant-biased octant");
            int quarter = units switch { < 32 => 0, < 96 => 1, < 160 => 2, < 224 => 3, _ => 0 };
            AssertEqual(quarter, bearing.TurnedBySteps(32).Sector(4), $"bearing {units} look quarter");

            var angle = new TrigonometryAngle(units);
            AssertEqual(units >> 4, angle.ToAngle().Sector(16), $"angle {units} sixteenth");
            AssertEqual(units >> 6, angle.ToAngle().Sector(4), $"angle {units} quarter");
            AssertEqual(units, angle.ToAngle().Sector(RotationAngle.StepsPerTurn), $"angle {units} steps");
            AssertEqual(RidleyTailAngle.StraightDown.TurnedCounterclockwise(units), RidleyTailAngle.StraightDown.TurnedCounterclockwise(angle),
                $"tail turned by direction {units}");
            for (int other = 0; other < RotationAngle.StepsPerTurn; other++)
            {
                var right = new TrigonometryAngle(other);
                if ((angle < right) != (units < other) || (angle >= right) != (units >= other) ||
                    (angle > right) != (units > other) || (angle <= right) != (units <= other))
                    throw new InvalidDataException($"Angles {units} and {other} compare unlike their units.");
            }

            for (int fraction = 0; fraction < 256; fraction += 85)
            {
                RotationAngle width = RotationAngle.FromSteps(units, fraction);
                AssertEqual(units / 2, width.WholeStep.DividedBy(2).ToTrigonometryAngle().RawValue, $"half of width {units}.{fraction}");
            }
        }
    }

    /// <summary>The typed turns equal the raw sums of 256ths of a step they replace, wrapping past either end of the turn.</summary>
    private static void VerifyTypedTurns()
    {
        foreach (int raw in new[] { 0, 1, 47, 48, 2047, 2048, 32768, 65487, 65488, 65534, 65535 })
        {
            RotationAngle angle = RotationAngle.FromRaw(raw);
            foreach (int delta in new[] { -2304, -1536, -768, -512, -384, -32, -1, 0, 1, 32, 64, 1280, 1792, 2304 })
                AssertEqual(angle.AddRaw(delta), angle.TurnedByFractionsOfStep(delta), $"{raw} turned by {delta} 256ths of a step");
            AssertEqual(angle.AddRaw(48), angle + RotationAngle.FromSteps(0, 48), $"getaway turn from {raw}");
            AssertEqual(angle.AddRaw(-1), angle - RotationAngle.FromSteps(0, 1), $"spiral slowing from {raw}");
            AssertEqual(angle.AddRaw(2048), angle + RotationAngle.FromSteps(8), $"upper neck lead from {raw}");
        }
        AssertEqual(RotationAngle.NormalizeRaw(-1024), RotationAngle.Zero - RotationAngle.FromSteps(4), "Ceres swoop's first target");
        AssertEqual(RotationAngle.NormalizeRaw(-16384), RotationAngle.ThreeQuarterTurn, "Ceres swoop's bottom target");
        AssertEqual(RotationAngle.NormalizeRaw(-30720), RotationAngle.Zero - RotationAngle.FromSteps(120), "Ceres swoop's climb target");
        AssertEqual(RotationAngle.NormalizeRaw(short.MinValue), RotationAngle.HalfTurn, "swoop recovery target");
        AssertEqual(RotationAngle.NormalizeRaw(512), RotationAngle.FromSteps(2), "Norfair swoop's first target");
        AssertEqual(RotationAngle.NormalizeRaw(-512), RotationAngle.Zero - RotationAngle.FromSteps(2), "Norfair swoop's mirrored first target");
        AssertEqual(RotationAngle.NormalizeRaw(16384), RotationAngle.QuarterTurn, "Norfair swoop's sideways target");
        AssertEqual(RotationAngle.NormalizeRaw(30720), RotationAngle.FromSteps(120), "Norfair swoop's climb target");
    }

    /// <summary>The floor zoom keeps its 256ths, reads its whole floor pixels as the shade choice did, and never goes negative.</summary>
    private static void VerifyFloorZoom()
    {
        for (int twoHundredFiftySixths = 0; twoHundredFiftySixths <= ushort.MaxValue; twoHundredFiftySixths++)
        {
            FloorZoom zoom = FloorZoom.FromTwoHundredFiftySixths(twoHundredFiftySixths);
            if (zoom.TwoHundredFiftySixths != twoHundredFiftySixths ||
                zoom.WholeFloorPixelsPerScreenPixel != UnsignedEightEight.Whole(twoHundredFiftySixths) ||
                zoom.FloorPixelsPerScreenPixel != twoHundredFiftySixths / 256.0)
                throw new InvalidDataException($"Zoom of {twoHundredFiftySixths} 256ths does not keep its count.");
        }
        AssertEqual(FloorZoom.FromTwoHundredFiftySixths(256), FloorZoom.OwnSize, "own size is one floor pixel per screen pixel");
        AssertEqual(FloorZoom.FromTwoHundredFiftySixths(3072), FloorZoom.FromFloorPixelsPerScreenPixel(12), "twelve floor pixels per screen pixel");
        AssertEqual(FloorZoom.FromTwoHundredFiftySixths(24), FloorZoom.FromTwoHundredFiftySixths(88).Narrowed(64), "narrowing steps down");
        AssertEqual(FloorZoom.FromTwoHundredFiftySixths(0), FloorZoom.FromTwoHundredFiftySixths(64).Narrowed(64), "narrowing may reach zero");
        AssertEqual(FloorZoom.FromTwoHundredFiftySixths(8224), FloorZoom.FromTwoHundredFiftySixths(8192).Widened(32), "widening steps up");
        AssertThrows<ArgumentOutOfRangeException>(() => FloorZoom.FromTwoHundredFiftySixths(28).Narrowed(32), "a zoom never goes negative");
        AssertThrows<ArgumentOutOfRangeException>(() => FloorZoom.FromTwoHundredFiftySixths(-1), "a negative zoom fails");
        AssertTrue(FloorZoom.FromTwoHundredFiftySixths(255) < FloorZoom.OwnSize && FloorZoom.OwnSize >= FloorZoom.FromTwoHundredFiftySixths(256),
            "zooms order by their 256ths");
        AssertEqual(240 / 256.0, FloorView.Turned(default, FloorZoom.FromTwoHundredFiftySixths(240), default, default).Scale,
            "the floor view spans the zoom's floor pixels per screen pixel");
    }
}
