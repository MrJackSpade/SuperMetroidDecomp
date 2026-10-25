using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>
/// Checks the typed portrait-eye programs (<see cref="IntroEyeAnimations"/>) against the cartridge's own bank-$8C eye
/// lists: a minimal reader walks each native list with the background-object timer gate, and the production narration
/// objects must draw the same eye frame and hold the same countdown on every update.
/// </summary>
internal static class IntroEyeInstructionsVerification
{
    /// <summary>Updates compared while pages one to five are shown: more than two full loops of both blinks.</summary>
    private const int BlinkingUpdates = 600;

    /// <summary>Updates compared after the final page starts: its closing and many repeats of the stare.</summary>
    private const int FinalPageUpdates = 300;

    /// <summary>Plays both eye programs through the production objects beside a reader of the native lists.</summary>
    internal static void VerifyIntroEyeInstructions(ISnesAddressSpace rom)
    {
        IntroEyeTilemapPresentation artwork = DistinctFrames();
        var narration = IntroNarrationPresentation.Load(
            new MemoryStream(IntroNarrationExtractor.Extract(rom), writable: false));
        var vram = new IntroTilemaps();
        var objects = new IntroCinematicObjectSystem(vram, new ushort[IntroCinematicDefinitions.Tilemaps.TextTilemapWordCount],
            new IntroJapaneseSubtitles(enabled: false), narrationPresentation: narration, eyeArtwork: artwork);

        var native = new NativeEyeReader(rom, IntroEyeAnimationDefinitionsConstants.StartPointer);
        Compare(objects, vram, artwork, native, BlinkingUpdates, "blinking");

        objects.StartEnglishPageSix();
        native = new NativeEyeReader(rom, IntroEyeAnimationDefinitionsConstants.FinalPagePointer);
        Compare(objects, vram, artwork, native, FinalPageUpdates, "final page");
    }

    /// <summary>Steps the objects and the native reader together, comparing the drawn eye frame and the eye countdown.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void Compare(IntroCinematicObjectSystem objects, IntroTilemaps vram, IntroEyeTilemapPresentation artwork,
        NativeEyeReader native, int updates, string phase)
    {
        Position16<TilemapCells> cell = IntroEyeAnimationDefinitions.PortraitCell;
        Extent16<TilemapCells> size = IntroEyeAnimationDefinitions.FrameSize;
        for (int update = 0; update < updates; update++)
        {
            objects.Step(0);
            native.Step();
            string at = $"intro eye {phase} update {update}";
            AssertEqual(native.Timer, objects.EyeInstructionTimer.RawValue, $"{at}: countdown");
            ReadOnlySpan<ushort> expected = artwork.FrameWords(native.Frame);
            for (int row = 0; row < size.Y; row++)
            for (int column = 0; column < size.X; column++)
            {
                int word = IntroVramWords.PortraitTilemapWord +
                    (cell.Y + row) * IntroCinematicDefinitions.Tilemaps.TilemapWidth + cell.X + column;
                AssertEqual(expected[row * size.X + column], IntroTilemapsFixtures.Vram(vram).ReadWord(word), $"{at}: portrait eye frame");
            }
        }
    }

    /// <summary>Eye artwork whose four frames use distinct characters, so the drawn rectangle names its frame.</summary>
    private static IntroEyeTilemapPresentation DistinctFrames()
    {
        int cells = IntroEyeAnimationDefinitions.FrameSize.Area;
        var document = new IntroEyeTilemapDocument
        {
            Version = IntroEyeTilemapFormat.Version,
            Frames = Enumerable.Range(0, IntroEyeAnimationDefinitionsConstants.FrameCount).Select(frame => new IntroEyeTilemapFrame
            {
                Id = IntroEyeTilemapFormat.FrameId(frame),
                Cells = Enumerable.Range(1 + frame * cells, cells).Select(character => new RoomBackgroundTilemapCell
                {
                    TileColumn = character % RoomBackgroundTilemapFormat.TileColumns,
                    TileRow = character / RoomBackgroundTilemapFormat.TileColumns,
                    Palette = 0, Priority = false, FlipX = false, FlipY = false,
                }).ToArray(),
            }).ToArray(),
        };
        using var json = new MemoryStream();
        IntroEyeTilemapPresentation.Write(json, document);
        json.Position = 0;
        return IntroEyeTilemapPresentation.Load(json);
    }

    /// <summary>
    /// An independent reader of one native bank-$8C eye list with the background-object timer gate: the countdown
    /// starts at one, and when it reaches zero the next record's duration and draw record are taken, following any
    /// goto on the way.
    /// </summary>
    private sealed class NativeEyeReader(ISnesAddressSpace rom, int list)
    {
        /// <summary>The bank-$8C offset of the next record.</summary>
        private int pointer = list;

        /// <summary>The native eye countdown after the latest update.</summary>
        internal int Timer { get; private set; } = 1;

        /// <summary>The draw-record index of the frame shown, in the cartridge's record order.</summary>
        internal int Frame { get; private set; } = -1;

        /// <summary>Runs one native background-object update for the eye.</summary>
        internal void Step()
        {
            Timer -= 1;
            if (Timer != 0)
                return;
            for (int command = 0; command < 2; command++)
            {
                int word = Word(pointer);
                if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
                {
                    int position = Word(pointer + 2);
                    AssertEqual((int)IntroEyeAnimationDefinitions.PortraitCell.X, SnesLittleEndian.LowByte(checked((ushort)position)),
                        "native eye record draws at the portrait column");
                    AssertEqual((int)IntroEyeAnimationDefinitions.PortraitCell.Y, SnesLittleEndian.HighByte(checked((ushort)position)),
                        "native eye record draws at the portrait row");
                    int record = Word(pointer + 4) - FrontendSourceDefinitions.IntroEyeFrameRecords;
                    AssertTrue(record >= 0 && record % FrontendSourceDefinitions.IntroEyeFrameRecordStride == 0,
                        "native eye record names a portrait-eye draw record");
                    Frame = record / FrontendSourceDefinitions.IntroEyeFrameRecordStride;
                    AssertTrue(Frame < IntroEyeAnimationDefinitionsConstants.FrameCount,
                        "native eye record names one of the four portrait-eye draw records");
                    Timer = word;
                    pointer += 6;
                    return;
                }
                AssertEqual(CinematicCodePointersConstants.CinematicBackgroundObject_Instruction_Goto, word,
                    "the native eye lists' only command is a goto");
                pointer = Word(pointer + 2);
            }
            throw new InvalidOperationException("A native eye list followed two gotos without a record.");
        }

        /// <summary>The bank-$8C word at <paramref name="offset"/>.</summary>
        private int Word(int offset) => IntroEyeListWord(rom, (int)new SnesAddress(
            IntroCinematicSourceDefinitions.SpritemapBank, checked((ushort)offset)));
    }

    /// <summary>Reads one cartridge word solely for the verification oracle.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for intro eye lists")]
    private static int IntroEyeListWord(ISnesAddressSpace rom, int address) =>
        CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
}
