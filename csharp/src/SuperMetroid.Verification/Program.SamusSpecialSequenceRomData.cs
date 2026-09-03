using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Validates every extracted special-sequence table and the structural boundaries that
    /// make its decoded records safe to consume. Behavioral sequence tests remain the
    /// authority for phase timing, movement, palette side effects, and completion.
    /// </summary>
    static void VerifySamusSpecialSequenceRomData()
    {
        ReadOnlySpan<byte> initialDeathFrames =
            SamusSpecialSequenceRomData.Death.InitialFramesByMovementType;
        AssertEqual(28, initialDeathFrames.Length,
            "death initial-frame table covers every retail movement type");

        ReadOnlySpan<SamusDeathTileSegment> deathSegments =
            SamusSpecialSequenceRomData.Death.TileSegments;
        AssertEqual(5, deathSegments.Length, "death sequence has five tile segments");
        AssertEqual(
            SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape,
            SamusSpecialSequenceRomData.PowerBomb.WhiteShapeEnd,
            "Power Bomb white shapes end where yellow shapes begin");
        AssertEqual(0,
            (SamusSpecialSequenceRomData.PowerBomb.YellowShapeEnd -
                SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape) %
                    SamusSpecialSequenceRomData.PowerBomb.ShapeStride,
            "yellow Power Bomb shapes occupy whole records");
        AssertEqual(0,
            (SamusSpecialSequenceRomData.PowerBomb.WhiteShapeEnd -
                SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape) %
                    SamusSpecialSequenceRomData.PowerBomb.ShapeStride,
            "white Power Bomb shapes occupy whole records");

        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Samus special sequence ROM data: retail range audit skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (SamusDeathTileSegment segment in deathSegments)
        {
            TouchRange(bus, segment.SourceAddress,
                SamusSpecialSequenceRomData.Death.TileSegmentByteCount,
                $"death tile segment ${segment.SourceAddress:X6}");
        }
        TouchRange(bus, SamusSpecialSequenceRomData.SuitPickup.BeamCurve,
            SamusSpecialSequenceRomData.SuitPickup.WindowScanlineCount / 2,
            "suit-pickup symmetric beam curve");
        TouchRange(bus,
            0x880000 | SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape,
            SamusSpecialSequenceRomData.PowerBomb.WhiteShapeEnd -
                SamusSpecialSequenceRomData.PowerBomb.FirstWhiteShape,
            "Power Bomb white shape stream");
        TouchRange(bus,
            0x880000 | SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape,
            SamusSpecialSequenceRomData.PowerBomb.YellowShapeEnd -
                SamusSpecialSequenceRomData.PowerBomb.FirstYellowShape,
            "Power Bomb yellow shape stream");
        TouchRange(bus, SamusSpecialSequenceRomData.Shinespark.PositiveSineTable,
            SnesAngle.HalfTurn.TableIndex * sizeof(ushort),
            "shinespark positive sine table");

        Console.WriteLine(
            "  Samus special sequence ROM data: death DMA, suit beam, Power Bomb shapes, and shinespark sine ranges are valid.");
    }
}
