using SuperMetroid.AssetExtraction.Enemies.Corpses;
using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Core.World.Artwork;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Verification.Enemies.Projectiles;
using SuperMetroid.Verification.Enemies.Deaths;
using SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.CreditsPresentationVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyLegacyOverridesVerification;
using static SuperMetroid.Verification.Samus.Grapple.GrappleConnectionDefinitionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainContactHitboxesVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainCorpseArtworkVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainEscapeTextArtworkVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainTurretDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Map.Artwork;
namespace SuperMetroid.Verification.Harness;

using SuperMetroid.Core.Progression;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Escape;

/// <summary>Checks stream-three compiled enemy, projectile, and room lookup contracts against native sources.</summary>
internal static class LookupStream3Verification
{
    /// <summary>One game-over sprite's draw call with its expected screen anchor and palette.</summary>
    private sealed record GameOverSpriteDraw(string Name, Action<ScreenSprites> Draw, int X, int Y, int Palette);

    /// <summary>$94:B18D-$B199: the four grapple-rope segment attribute words.</summary>
    private static readonly int[] GrappleSegmentAttributeAddresses =
        [0x94b18d, 0x94b191, 0x94b195, 0x94b199];

    /// <summary>Confirms the five typed tube pieces' native records and production spawn bindings.</summary>
    internal static void VerifyStream3FallingTubePopulationLayout(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyMotherBrainFallingTubePopulationDefinitions), () => VerifyMotherBrainFallingTubePopulationDefinitions(rom));
        Console.WriteLine("Falling-tube layout: five typed pieces match their native records; production spawns and independent fixture behavior pass.");
    }
    /// <summary>Compares all 256 intro colors with ROM and checks edited RGB5 channels through transfer bytes and live CGRAM.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifyStream3IntroPaletteRows(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, SnesCgram.ColorCount)
            .Select(color => ReadVerificationWord(rom, IntroCinematicRomData.Assets.Palette + 2 * color)).ToArray();
        IntroCinematicPalette stock = Load(native);
        Confirm(stock, native);
        var rows = (Array)stock.rows;
        AssertEqual(0, rows.Length, "intro complete stock palette stores no rows or paint words");
        for (int color = 0; color < native.Length; color++)
        for (int channel = 0; channel < 4; channel++)
        {
            ushort[] edited = (ushort[])native.Clone();
            edited[color] ^= channel == 3 ? (ushort)0x0421 : (ushort)(1 << (channel * 5));
            Confirm(Load(edited), edited);
            Confirm(stock, native);
        }
        Console.WriteLine("Intro palette rows:256 native colors and768 single-channel and256 combined RGB5 edits preserve transfer bytes/CGRAM/instance isolation; reviewed material choices and all shade calculations keep no stock rows.");
        static void Confirm(IntroCinematicPalette selected, ushort[] expected)
        {
            ReadOnlySpan<byte> bytes = selected.Transfer.Span;
            for (int color = 0; color < expected.Length; color++)
                AssertEqual(expected[color], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes[(color * 2)..]), "intro palette transfer word");
            var cgram = new SnesCgram();
            selected.LoadTo(cgram);
            AssertTrue(ToColors(expected).SequenceEqual(cgram.Colors), "intro palette CGRAM load");
        }
        static IntroCinematicPalette Load(ushort[] words)
        {
            var document = new IntroCinematicPaletteDocument
            {
                Version = IntroCinematicPaletteFormat.Version,
                Colors = words.Select(word => new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 }).ToArray(),
            };
            using var json = new MemoryStream();
            IntroCinematicPalette.Write(json, document);
            json.Position = 0;
            return IntroCinematicPalette.Load(json);
        }
    }
    /// <summary>Checks four eye rectangles against ROM and their actual portrait tilemap writes, including independent cell edits.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyStream3IntroEyeRectangles(ISnesAddressSpace rom)
    {
        ushort[][] native = Enumerable.Range(0, 4).Select(frame => Enumerable.Range(0, 6)
            .Select(cell => ReadVerificationWord(rom, 0x8cd785 + 16 * frame + 2 * cell)).ToArray()).ToArray();
        IntroEyeTilemapPresentation stock = Load(native);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (var rectangle in stock.frames)
        {
            AssertTrue(rectangle.supplied is null,
                "stock eye rectangle stores no native word payload");
            AssertTrue(!MetadataShape.DeclaresField(rectangle.GetType(), "origin", fields),
                "stock eye rectangle no longer stores independent origin words");
        }
        Confirm(stock, native);
        for (int frame = 0; frame < 4; frame++)
        for (int cell = 0; cell < 6; cell++)
        {
            ushort[][] edited = native.Select(words => (ushort[])words.Clone()).ToArray();
            edited[frame][cell] ^= 0x4401;
            Confirm(Load(edited), edited);
            Confirm(stock, native);
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.FrameWords(-1).ToArray(), "negative eye frame");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.FrameWords(4).ToArray(), "upper eye frame");
        Console.WriteLine("Intro eye rectangles:24 native cells,24 independent edits, content identities and bounds pass; actual portrait draw copies pass; only reviewed patch identities/display policy remain.");

        static void Confirm(IntroEyeTilemapPresentation selected, ushort[][] expected)
        {
            for (int frame = 0; frame < expected.Length; frame++)
                AssertTrue(expected[frame].SequenceEqual(selected.FrameWords(frame)), $"eye rectangle {frame}");
            var tilemaps = new IntroTilemaps();
            var objects = new IntroCinematicObjectSystem(tilemaps, new ushort[1024], new IntroJapaneseSubtitles(enabled: false), eyeArtwork: selected);
            IntroEyeFrame[] eyeFrames = [IntroEyeFrame.Open, IntroEyeFrame.HalfOpen, IntroEyeFrame.Closed, IntroEyeFrame.Deadpan];
            for (int frame = 0; frame < expected.Length; frame++)
            {
                objects.ShowEyeFrame(eyeFrames[frame]);
                for (int cell = 0; cell < 6; cell++)
                    AssertEqual(expected[frame][cell], tilemaps.Portrait[(13 + cell / 3) * 32 + 17 + cell % 3],
                        "actual eye patch draw preserves native and independently supplied cells");
            }
            string identity = SelectedPresentationHash.Create(nameof(IntroEyeTilemapPresentation), content =>
            {
                content.Append("frames", expected.Length);
                foreach (ushort[] words in expected) content.AppendWords("frame", words);
            });
            AssertEqual(identity, selected.ContentIdentity, "eye rectangle original identity framing");
        }
        static IntroEyeTilemapPresentation Load(ushort[][] frames)
        {
            var document = new IntroEyeTilemapDocument
            {
                Version = IntroEyeTilemapFormat.Version,
                Frames = frames.Select((words, frame) => new IntroEyeTilemapFrame
                {
                    Id = IntroEyeTilemapFormat.FrameId(frame),
                    Cells = words.Select(raw =>
                    {
                        var word = new SnesBgTilemapWord(raw);
                        return new RoomBackgroundTilemapCell
                        {
                            TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                            TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                            Palette = word.PaletteIndex, Priority = word.HasPriority,
                            FlipX = word.FlipHorizontally, FlipY = word.FlipVertically,
                        };
                    }).ToArray(),
                }).ToArray(),
            };
            using var json = new MemoryStream();
            IntroEyeTilemapPresentation.Write(json, document);
            json.Position = 0;
            return IntroEyeTilemapPresentation.Load(json);
        }
    }
    /// <summary>Checks the 128 native final-line cells and that edited divider cells remain isolated per loaded presentation.</summary>
    internal static void VerifyStream3IntroDivider(ISnesAddressSpace rom)
    {
        var native = new ushort[IntroFinalLineTilemapFormat.CellCount];
        for (int index = 0; index < native.Length; index++)
            native[index] = ReadVerificationWord(rom, IntroCinematicRomData.Assets.FinalTextLine + 2 * index);
        IntroFinalLineTilemap stock = Load(native);
        AssertTrue(native.SequenceEqual(stock.Words.Span), "all128 native intro divider cells");
        AssertTrue(stock.suppliedWords is null,
            "native divider retains no generated lookup");
        for (int changed = 0; changed < native.Length; changed++)
        {
            var edited = (ushort[])native.Clone();
            edited[changed] ^= 0x4401;
            IntroFinalLineTilemap selected = Load(edited);
            AssertTrue(edited.SequenceEqual(selected.Words.Span), $"independent divider cell edit {changed}");
            AssertTrue(native.SequenceEqual(stock.Words.Span), "previous divider instance remains immutable");
        }
        Console.WriteLine("Japanese subtitle staging:128 native cells and128 independent tile/palette/flip edits pass; only reviewed display policy remains.");

        static IntroFinalLineTilemap Load(ushort[] words)
        {
            var cells = words.Select(raw =>
            {
                var word = new SnesBgTilemapWord(raw);
                return new RoomBackgroundTilemapCell
                {
                    TileColumn = word.CharacterIndex % RoomBackgroundTilemapFormat.TileColumns,
                    TileRow = word.CharacterIndex / RoomBackgroundTilemapFormat.TileColumns,
                    Palette = word.PaletteIndex, Priority = word.HasPriority,
                    FlipX = word.FlipHorizontally, FlipY = word.FlipVertically,
                };
            }).ToArray();
            using var json = new MemoryStream();
            IntroFinalLineTilemap.Write(json, new IntroFinalLineTilemapDocument
            {
                Version = IntroFinalLineTilemapFormat.Version, Cells = cells,
            });
            json.Position = 0;
            return IntroFinalLineTilemap.Load(json);
        }
    }
    /// <summary>Compares options heading borders with ROM and OAM, then checks individual edits, reordering, and expansion.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for options borders")]
    internal static void VerifyStream3OptionsBorders(ISnesAddressSpace rom, byte[] imported)
    {
        GameOptionsPresentationDocument Read() => RequiredJson.Deserialize<GameOptionsPresentationDocument>(imported, ArtworkJson.Options);
        Dictionary<GameOptionsPage, SpriteComposition> Load(GameOptionsPresentationDocument document)
        {
            var value = GameOptionsPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
            return value.headingSprites;
        }
        void Confirm(SpriteComposition actual, SpriteVisualPart[] parts)
        {
            var expected = MenuSpriteCompiler.Compile(parts, "independent border oracle");
            AssertEqual(expected.PartCount, actual.PartCount, "border ordered part count");
            for (int index = 0; index < expected.PartCount; index++)
                AssertEqual(expected.Part(index), actual.Part(index), "border exact coordinates, appearance and order");
        }
        foreach ((GameOptionsPage page, int address, int count) in new[]
        {
            new VerifyStream3OptionsBordersCase(GameOptionsPage.Primary, 0x82d24b, 34), new VerifyStream3OptionsBordersCase(GameOptionsPage.Controller, 0x82d2f7, 58), new VerifyStream3OptionsBordersCase(GameOptionsPage.Special, 0x82d41b, 52),
        })
        {
            string name = GameOptionsPresentationDefinitions.HeadingSpriteName(page);
            var original = Read();
            var stock = Load(original)[page];
            AssertTrue(stock.parts is MenuHeadingBorderDefinitions,
                "native heading stores only semantic title identity instead of supplied bounds/order/style");
            AssertEqual(count, stock.PartCount, "native perimeter count");
            Confirm(stock, original.Sprites[name]);
            var presentation = GameOptionsPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(original, ArtworkJson.Options)));
            string anchorKey = GameOptionsPresentationDefinitions.MenuName(page);
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            presentation.DrawHeading(actualOam, MenuSheetFixtures.GameOptions(presentation), page, 3);
            var point = original.HeadingAnchors[anchorKey];
            MenuSpriteCompiler.Compile(original.Sprites[name], name).DrawOnScreen(expectedOam,
                new(point.X, unchecked((ushort)(point.Y - 3))), (ushort)(original.CursorPalette << 9));
            actualOam.FinishFrame(); expectedOam.FinalizeFrame();
            AssertTrue(actualOam.ImageEquals(expectedOam),
                "actual heading draw preserves exact native ordered OAM");
            AssertEqual((ushort)count, CartridgeImportSource.Require(rom).ReadCartridgeWord(address), "native border header");
            for (int index = 0; index < count; index++)
            {
                int entry = address + 2 + index * 5;
                var part = stock.Part(index);
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry), part.X.Raw, "native border X/size");
                AssertEqual(rom.ReadCartridgeByte(entry + 2), part.Y.Raw, "native border Y");
                AssertEqual((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord((entry + 3)) & ~0x0e00), part.Attributes.Raw, "native border inherited-palette attributes");
                AssertTrue(part.InheritPalette, "border palette remains owner supplied");
                foreach (int field in new[] { 0, 1, 2, 3 })
                {
                    var edited = Read();
                    var source = edited.Sprites[name][index];
                    edited.Sprites[name][index] = field switch
                    {
                        0 => source with { OffsetX = source.OffsetX + 1 },
                        1 => source with { OffsetY = source.OffsetY + 1 },
                        2 => source with { TileColumn = source.TileColumn ^ 1 },
                        _ => source with { Palette = 2, FlipX = !source.FlipX },
                    };
                    Confirm(Load(edited)[page], edited.Sprites[name]);
                }
            }
            var reversed = Read();
            Array.Reverse(reversed.Sprites[name]);
            Confirm(Load(reversed)[page], reversed.Sprites[name]);
            var expanded = Read();
            expanded.Sprites[name] = [.. expanded.Sprites[name], expanded.Sprites[name][0]];
            Confirm(Load(expanded)[page], expanded.Sprites[name]);
        }
    }

    /// <summary>Checks file-select border geometry and ordered sprite parts against ROM and edited presentation data.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select borders")]
    internal static void VerifyStream3FileSelectBorders(ISnesAddressSpace rom, byte[] imported)
    {
        FileSelectPresentationDocument Read() => RequiredJson.Deserialize<FileSelectPresentationDocument>(imported, ArtworkJson.Options);
        Dictionary<FileSelectBorder, SpriteComposition> Load(FileSelectPresentationDocument document)
        {
            var value = FileSelectPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
            return value.borderSprites;
        }
        void Confirm(SpriteComposition actual, SpriteVisualPart[] parts)
        {
            var expected = MenuSpriteCompiler.Compile(parts, "independent border oracle");
            AssertEqual(expected.PartCount, actual.PartCount, "border ordered part count");
            for (int index = 0; index < expected.PartCount; index++)
                AssertEqual(expected.Part(index), actual.Part(index), "border exact coordinates, appearance and order");
        }
        foreach ((FileSelectBorder border, int address, int count) in new[]
        {
            new VerifyStream3FileSelectBordersCase(FileSelectBorder.Main, 0x82d00b, 32), new VerifyStream3FileSelectBordersCase(FileSelectBorder.Copy, 0x82d0ad, 40), new VerifyStream3FileSelectBordersCase(FileSelectBorder.Clear, 0x82d177, 42),
        })
        {
            string name = FileSelectPresentationDefinitions.BorderSpriteName(border);
            var original = Read();
            var stock = Load(original)[border];
            AssertTrue(stock.parts is MenuHeadingBorderDefinitions,
                "native file border uses semantic title and calculated perimeter");
            AssertEqual(count, stock.PartCount, "native perimeter count");
            Confirm(stock, original.Sprites[name]);
            var presentation = FileSelectPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(original, ArtworkJson.Options)));
            string anchorKey = FileSelectPresentationDefinitions.BorderName(border);
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            presentation.DrawBorder(actualOam, MenuSheetFixtures.FileSelect(presentation), border);
            var anchor = original.BorderAnchors[anchorKey];
            MenuSpriteCompiler.Compile(original.Sprites[name], name).DrawOnScreen(expectedOam,
                new(anchor.X, anchor.Y), (ushort)(original.ObjectPalette << 9));
            actualOam.FinishFrame(); expectedOam.FinalizeFrame();
            AssertTrue(actualOam.ImageEquals(expectedOam),
                "actual file-select border draw preserves native OAM");
            AssertEqual((ushort)count, CartridgeImportSource.Require(rom).ReadCartridgeWord(address), "native border header");
            for (int index = 0; index < count; index++)
            {
                int entry = address + 2 + index * 5;
                var part = stock.Part(index);
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry), part.X.Raw, "native border X/size");
                AssertEqual(rom.ReadCartridgeByte(entry + 2), part.Y.Raw, "native border Y");
                AssertEqual((ushort)(CartridgeImportSource.Require(rom).ReadCartridgeWord((entry + 3)) & ~0x0e00), part.Attributes.Raw, "native border inherited-palette attributes");
                AssertTrue(part.InheritPalette, "border palette remains owner supplied");
                foreach (int field in new[] { 0, 1, 2, 3 })
                {
                    var edited = Read();
                    var source = edited.Sprites[name][index];
                    edited.Sprites[name][index] = field switch
                    {
                        0 => source with { OffsetX = source.OffsetX + 1 },
                        1 => source with { OffsetY = source.OffsetY + 1 },
                        2 => source with { TileColumn = source.TileColumn ^ 1 },
                        _ => source with { Palette = 2, FlipX = !source.FlipX },
                    };
                    Confirm(Load(edited)[border], edited.Sprites[name]);
                }
            }
            var reversed = Read();
            Array.Reverse(reversed.Sprites[name]);
            Confirm(Load(reversed)[border], reversed.Sprites[name]);
            var expanded = Read();
            expanded.Sprites[name] = [.. expanded.Sprites[name], expanded.Sprites[name][0]];
            Confirm(Load(expanded)[border], expanded.Sprites[name]);
        }
    }

    /// <summary>Decodes the seven grapple transfers and checks derived tile pixels against ROM and edited atlas data.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple tile patterns")]
    internal static void VerifyStream3GrappleTilePatterns(ISnesAddressSpace rom, GrappleTileTransfer[] transfers)
    {
        byte[] planar = transfers.SelectMany(transfer => Enumerable.Range(0, transfer.ByteCount)
            .Select(index => rom.ReadCartridgeByte(transfer.SourceAddress + index))).ToArray();
        GrappleTileAtlas Load()
        {
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, 16, out Extent16<ImagePixels> size);
            using var png = new MemoryStream();
            IndexedPng.Write(png, size, pixels, SnesGraphics.DiagnosticPalette(16));
            return GrappleTileAtlas.Load(new MemoryStream(png.ToArray()));
        }
        void Check(GrappleTileAtlas atlas)
        {
            foreach (var transfer in transfers)
            {
                AssertTrue(planar.AsSpan(transfer.AtlasOffset, transfer.ByteCount).SequenceEqual(atlas.Resolve(transfer.Asset).Span),
                    "stream 3 exact Grapple calculated/upload bytes");
                AssertTrue(atlas.TryResolve(transfer.SourceAddress, transfer.ByteCount, out var resolved),
                    "stream 3 existing Grapple transfer identity binding");
                AssertTrue(resolved.Span.SequenceEqual(atlas.Resolve(transfer.Asset).Span),
                    "stream 3 rebound Grapple bytes preserve transfer boundaries");
            }
        }
        var stock = Load();
        Check(stock);
        AssertEqual(64, stock.independentTiles.Length,
            "stream 3 Grapple retains only eight independent binary coverage patterns");
        foreach (object? cache in new object?[] { stock.firstPoint, stock.secondPoint, stock.thirdPoint, stock.fourthPoint, stock.verticalSegments })
            AssertTrue(cache is null,
                "stream 3 calculated Grapple pixels have no cached stock characters");
        for (int tile = 0; tile < 16; tile++)
        for (int plane = 0; plane < 4; plane++)
        {
            int index = tile * 32 + plane / 2 * 16 + plane % 2;
            planar[index] ^= 128;
            Check(Load());
            planar[index] ^= 128;
        }
        for (int tile = 4; tile < 12; tile++)
        {
            for (int plane = 0; plane < 4; plane++) planar[tile * 32 + plane / 2 * 16 + plane % 2] ^= 128;
            Check(Load());
            for (int plane = 0; plane < 4; plane++) planar[tile * 32 + plane / 2 * 16 + plane % 2] ^= 128;
        }
        Check(stock);
        AssertTrue(!stock.TryResolve(transfers[0].SourceAddress, 31, out _), "stream 3 Grapple rejects partial transfer");
    }

    /// <summary>Checks narration glyph coordinates and line breaks against native records and independently edited pages.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for narration layout")]
    internal static void VerifyStream3NarrationLayout(ISnesAddressSpace rom, byte[] json, IntroNarrationPresentation stock)
    {
        int nativeCharacters = 0;
        foreach (var source in IntroNarrationScriptSources.Pages)
        {
            var selected = (IntroNarrationPresentation.NarrationPage)stock.GetLines(source.Id);
            AssertTrue(selected.supplied is null,
                "native narration calculates line boundaries and rows");
            string text = selected.text ?? throw new InvalidOperationException("native narration keeps its reviewed page text");
            AssertEqual(source.Id == IntroNarrationPageId.Page1 ? 1 : 0, text.Count(character => character == '\n'),
                "narration retains only the one independently chosen hard break");
            var expected = new List<IntroNarrationCharacter>();
            int cursor = 0x8c0000 | (source.InstructionPointer + 8);
            while ((CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor) & 0x8000) == 0)
            {
                ushort position = CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 2));
                ushort glyph = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8c0000 | (CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 4) + 4)));
                expected.Add(new(SnesLittleEndian.LowByte(position), SnesLittleEndian.HighByte(position), glyph, glyph == (ushort)IntroNarrationSymbolWord.Blank));
                cursor += 6;
            }
            AssertTrue(stock.Compile(source.Id).SequenceEqual(expected), "calculated narration matches every native glyph/column/row record");
            nativeCharacters += expected.Count;
            AssertThrows<ArgumentOutOfRangeException>(() => _ = stock.GetLines(source.Id)[-1], "narration line lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = stock.GetLines(source.Id)[stock.GetLines(source.Id).Count], "narration line upper bound");
        }
        AssertEqual(770, nativeCharacters, "six native narration pages contain770 characters");
        var document = RequiredJson.Deserialize<IntroNarrationDocument>(json, ArtworkJson.Options);
        void ConfirmEdit()
        {
            using var stream = new MemoryStream();
            IntroNarrationPresentation.Write(stream, document);
            stream.Position = 0;
            var edited = IntroNarrationPresentation.Load(stream);
            foreach (IntroNarrationPageId page in Enum.GetValues<IntroNarrationPageId>())
                AssertTrue(edited.GetLines(page).SequenceEqual(document.Pages[page.ToString()].Lines),
                    "narration preserves independently supplied text, spacing and row choices");
        }
        foreach (IntroNarrationPage page in document.Pages.Values)
        for (int line = 0; line < page.Lines.Length; line++)
        {
            IntroNarrationLine original = page.Lines[line];
            page.Lines[line] = original with { Text = (original.Text[0] == 'A' ? "Z" : "A") + original.Text[1..] };
            ConfirmEdit();
            page.Lines[line] = original;
        }
        IntroNarrationPage sixth = document.Pages["Page6"];
        document.Pages["Page6"] = sixth with { Lines = sixth.Lines.Select(line => line with { Row = line.Row + 4 }).ToArray() };
        ConfirmEdit();
        document.Pages["Page6"] = sixth with { Lines = [sixth.Lines[0] with { Text = " " + sixth.Lines[0].Text + " " }, sixth.Lines[1]] };
        ConfirmEdit();
        document.Pages["Page6"] = sixth with { Lines = [sixth.Lines[0], sixth.Lines[1] with { Row = 10 }] };
        ConfirmEdit();
        document.Pages["Page6"] = sixth;
    }

    /// <summary>Checks six Mochtroid frame registrations and their calculated native spritemap pointers.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mochtroid visuals")]
    internal static void VerifyStream3MochtroidVisuals(ISnesAddressSpace rom)
    {
        NativeEnemySpritemapDefinition[] frames = MochtroidVisualDefinitions.Frames().ToArray();
        AssertEqual(6, frames.Length, "Mochtroid calculated registration count");
        for (int family = 0; family < 2; family++)
        {
            int pointer = family == 0 ? 0xa9b0 : 0xaa06;
            for (int pose = 0; pose < 3; pose++)
            {
                var frame = frames[family * 3 + pose];
                AssertEqual((byte)0xa3, frame.Bank, "Mochtroid native frame bank");
                AssertEqual((ushort)pointer, frame.Pointer, "Mochtroid native counted-record stride");
                AssertEqual($"mochtroid_{(family == 0 ? "flight" : "attached")}_{pose}", frame.Name, "Mochtroid legacy frame name");
                int count = CartridgeImportSource.Require(rom).ReadCartridgeWordFixedBank(0xa30000 | pointer);
                pointer += 2 + 5 * count;
            }
        }
    }
    /// <summary>Checks menu compositions against native OAM parts and preserves edited geometry and order.</summary>
    internal static void VerifyStream3MenuSpriteGeometry(ISnesAddressSpace rom)
    {
        static Dictionary<string, SpriteComposition> Sprites(GameOverPresentation presentation) => new(
            [.. Enum.GetValues<GameOverBabyFrame>().Select(frame =>
                KeyValuePair.Create(GameOverPresentationDefinitions.BabyFrameName(frame), presentation.babySprites[frame])),
            KeyValuePair.Create(GameOverPresentationDefinitions.EggFrame, presentation.eggSprite),
            .. Enumerable.Range(0, GameOverPresentationDefinitions.CursorFrameCount).Select(frame =>
                KeyValuePair.Create(GameOverPresentationDefinitions.CursorFrameName(frame), presentation.cursorSprites[frame]))]);
        static void Confirm(SpriteComposition actual, SpriteVisualPart[] expectedParts)
        {
            var expected = MenuSpriteCompiler.Compile(expectedParts, "native or independently edited menu composition");
            AssertEqual(expected.PartCount, actual.PartCount, "menu composition exact part count");
            for (int part = 0; part < expected.PartCount; part++)
                AssertEqual(expected.Part(part), actual.Part(part), "menu composition exact ordered geometry, tiles, attributes and palette inheritance");
        }
        static bool Calculated(SpriteComposition composition) =>
            composition.parts is not CompiledSpritePart[];

        byte[] imported = SuperMetroid.AssetExtraction.Frontend.GameOverPresentationExtractor.Extract(rom);
        GameOverPresentationDocument Read() => RequiredJson.Deserialize<GameOverPresentationDocument>(imported, ArtworkJson.Options);
        GameOverPresentation Load(GameOverPresentationDocument document) => GameOverPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        var original = Read();
        var stock = Sprites(Load(original));
        // The stock presentation draws the installation's sheet, composed from it.
        SpriteSheet GameOverSheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.GameOver);
        var presentation = Load(original);
        List<GameOverSpriteDraw> draws =
        [
            .. Enum.GetValues<GameOverBabyFrame>().Select(frame => new GameOverSpriteDraw(GameOverPresentationDefinitions.BabyFrameName(frame),
                oam => presentation.DrawBaby(oam, GameOverSheet, frame), original.BabyAnchor.X, original.BabyAnchor.Y, presentation.BabyPaletteIndex)),
            new(GameOverPresentationDefinitions.EggFrame, oam => presentation.DrawEgg(oam, GameOverSheet), original.BabyAnchor.X, original.BabyAnchor.Y, presentation.EggPaletteIndex),
            .. Enumerable.Range(0, GameOverPresentationDefinitions.CursorFrameCount).Select(frame => new GameOverSpriteDraw(
                GameOverPresentationDefinitions.CursorFrameName(frame), oam => presentation.DrawCursor(oam, GameOverSheet, frame, false),
                original.CursorX, original.YesCursorY, presentation.CursorPaletteIndex)),
        ];
        foreach (GameOverSpriteDraw draw in draws)
        {
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            draw.Draw(actualOam);
            MenuSpriteCompiler.Compile(original.Sprites[draw.Name], draw.Name).DrawOnScreen(expectedOam,
                new((ushort)draw.X, (ushort)draw.Y), (ushort)(draw.Palette << 9));
            actualOam.FinishFrame(); expectedOam.FinalizeFrame();
            AssertTrue(actualOam.ImageEquals(expectedOam),
                "actual game-over draw emits exact native ordered OAM " + draw.Name);
        }
        foreach (string name in GameOverPresentationDefinitions.SpriteNames)
        {
            if (name == GameOverPresentationDefinitions.CursorFrameName(0)) for (int part = 0; part < 2; part++) AssertEqual(stock[name].Part(part), new MenuCursorParts(MenuMissileAnimationDefinitions.SpritemapId(0) - 0x34)[part], "exact first cursor candidate part");
            AssertTrue(Calculated(stock[name]), "stock game-over geometry retains no full part array " + name);
            Confirm(stock[name], original.Sprites[name]);
            for (int part = 0; part < original.Sprites[name].Length; part++)
            for (int field = 0; field < 9; field++)
            {
                var document = Read();
                var source = document.Sprites[name][part];
                document.Sprites[name][part] = field switch
                {
                    0 => source with { OffsetX = source.OffsetX + 1 },
                    1 => source with { OffsetY = source.OffsetY + 1 },
                    2 => source with { TileColumn = source.TileColumn ^ 1 },
                    3 => source with { TileRow = source.TileRow ^ 1 },
                    4 => source with { Size = source.Size == 8 ? 16 : 8 },
                    5 => source with { Priority = source.Priority ^ 1 },
                    6 => source with { Palette = 0 },
                    7 => source with { FlipX = !source.FlipX },
                    _ => source with { FlipY = !source.FlipY },
                };
                var changed = document.Sprites[name][part];
                if (changed.TileColumn > MapSpriteFormat.TileColumns - changed.Size / 8 ||
                    changed.TileRow > MapSpriteFormat.TileRows - changed.Size / 8)
                    AssertThrows<InvalidDataException>(() => Load(document), "menu edited size still rejects out-of-sheet footprint");
                else
                    Confirm(Sprites(Load(document))[name], document.Sprites[name]);
            }
            var reordered = Read();
            Array.Reverse(reordered.Sprites[name]);
            Confirm(Sprites(Load(reordered))[name], reordered.Sprites[name]);
            var expanded = Read();
            expanded.Sprites[name] = [.. expanded.Sprites[name], expanded.Sprites[name][0] with { OffsetX = 17 }];
            Confirm(Sprites(Load(expanded))[name], expanded.Sprites[name]);
        }
        ConfirmCursorCaller(SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(rom), source => FileSelectPresentation.Load(source).cursorSprites);
        ConfirmCursorCaller(SuperMetroid.AssetExtraction.Frontend.GameOptionsPresentationExtractor.Extract(rom), source => GameOptionsPresentation.Load(source).cursorSprites);

        void ConfirmCursorCaller(byte[] json, Func<Stream, SpriteComposition[]> load)
        {
            var presentation = load(new MemoryStream(json));
            for (int frame = 0; frame < 4; frame++)
            {
                string name = GameOverPresentationDefinitions.CursorFrameName(frame);
                var node = RequiredJson.Parse(json);
                var parts = RequiredJson.Deserialize<SpriteVisualPart[]>(node.Required("sprites").Required(name).ToJsonString(), ArtworkJson.Options);
                AssertTrue(Calculated(presentation[frame]), "shared menu cursor caller uses calculated geometry");
                Confirm(presentation[frame], parts);
                parts[0] = parts[0] with { OffsetX = parts[0].OffsetX + 1, TileColumn = parts[0].TileColumn ^ 1 };
                node.Required("sprites")[name] = System.Text.Json.JsonSerializer.SerializeToNode(parts, ArtworkJson.Options);
                var edited = load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(node.ToJsonString())));
                Confirm(edited[frame], parts);
            }
        }
    }

    /// <summary>Checks the ordered named frame registry and derived room-sprite additions without duplicate identities.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for enemy frame registration")]
    internal static void VerifyStream3EnemyFrameRegistration(ISnesAddressSpace rom)
    {
        // Ordered bank:pointer:name snapshot of the 472 source registrations at commit 723df7b36.
        const string originalNamedIdentity = "421A60158BA376F9A6AD58EA817315553AD268A44B411B87A4B25059EE5DFE92";
        var named = EnemySpritemapDefinitions.Frames.Take(472).ToArray();
        string identity = string.Join("|", named.Select(frame => $"{frame.Bank:x2}:{frame.Pointer:x4}:{frame.Name}"));
        AssertEqual(originalNamedIdentity, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(identity))), "enemy named registration exact ordered source identities");
        var all = EnemySpritemapDefinitions.Frames.ToArray();
        AssertEqual(all.Length, EnemySpritemapDefinitions.Frames.Length, "enemy calculated registry count");
        AssertTrue(EnemySpritemapDefinitions.Frames[..472].SequenceEqual(named), "enemy legacy named prefix slice");
        for (int index = 0; index < 472; index++)
            AssertEqual(named[index], EnemySpritemapDefinitions.Frames[index], "enemy legacy registration ordinal identity");
        var additionalPointers = new SortedSet<ushort>();
        var namedSpritePointers = named.Where(frame => frame.Bank == 0xb4).Select(frame => frame.Pointer).ToHashSet();
        foreach (ushort pointer in SuperMetroid.Verification.Rooms.SpriteObjects.RoomSpriteObjectNativeLists.SpritemapPointers(rom))
        {
            if (!namedSpritePointers.Contains(pointer)) additionalPointers.Add(pointer);
        }
        var expectedAdditional = additionalPointers.Select(pointer =>
            new NativeEnemySpritemapDefinition(0xb4, pointer, $"room_sprite_b4_{pointer:x4}")).ToArray();
        AssertTrue(all.Skip(472).Take(expectedAdditional.Length).Select(frame => frame.Native).SequenceEqual(expectedAdditional),
            "enemy additional room-sprite sort and duplicate exclusion preserve named prefix");
        AssertEqual(all.Length, all.Select(frame => new FrameAddressKey(frame.Bank, frame.Pointer)).Distinct().Count(), "enemy frame identities remain unique");
        Suite(nameof(VerifyEnemyLegacyOverrides), () => VerifyEnemyLegacyOverrides());
    }

    /// <summary>Checks file-select patch cells and terminators against ROM, then verifies edited placement and order.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select patches")]
    internal static void VerifyStream3FileSelectPatches(ISnesAddressSpace rom, byte[] imported, FileSelectPresentation stock)
    {
        FileSelectPresentationDocument Read() => RequiredJson.Deserialize<FileSelectPresentationDocument>(imported, ArtworkJson.Options);
        FileSelectPresentation Load(FileSelectPresentationDocument document) => FileSelectPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        foreach ((FileSelectPatch patch, int address, int count) in new[]
        {
            new VerifyStream3FileSelectPatchesCase(FileSelectPatch.Energy, 0x81b496, 4),
            new VerifyStream3FileSelectPatchesCase(FileSelectPatch.TimeColon, 0x81b4a8, 1),
            new VerifyStream3FileSelectPatchesCase(FileSelectPatch.NoData, 0x81b4ac, 11),
        })
        {
            string name = FileSelectPresentationDefinitions.PatchName(patch);
            FileSelectCompiledPatch Compiled(FileSelectPresentation value) => value.patches[patch];
            AssertTrue(Compiled(stock).suppliedCells is null,
                "file-select stock patch retains no cells " + name);
            for (int index = 0; index < count; index++)
            {
                ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + index * 2);
                AssertEqual(new FileSelectCompiledPatchCell(new(checked((short)index), 0), word), FileSelectPresentationDefinitions.PatchCell(patch, index),
                    "file-select calculated patch exact native cell");
                foreach (int bit in new[] { 1, 32, 0x400, 0x2000, 0x4000, 0x8000 })
                {
                    var document = Read();
                    var original = document.Patches[name].Cells[index];
                    var cell = original.Cell;
                    cell = bit switch
                    {
                        1 => cell with { TileColumn = cell.TileColumn ^ 1 },
                        32 => cell with { TileRow = cell.TileRow ^ 1 },
                        0x400 => cell with { Palette = cell.Palette ^ 1 },
                        0x2000 => cell with { Priority = !cell.Priority },
                        0x4000 => cell with { FlipX = !cell.FlipX },
                        _ => cell with { FlipY = !cell.FlipY },
                    };
                    document.Patches[name].Cells[index] = original with { Cell = cell };
                    Confirm(Load(document), document.Patches[name].Cells);
                }
                var moved = Read();
                moved.Patches[name].Cells[index] = moved.Patches[name].Cells[index] with { X = count, Y = 1 };
                Confirm(Load(moved), moved.Patches[name].Cells);
            }
            AssertEqual((byte)0xff, rom.ReadCartridgeByte(address + count * 2), "file-select native patch terminator low");
            AssertEqual((byte)0xff, rom.ReadCartridgeByte(address + count * 2 + 1), "file-select native patch terminator high");
            Confirm(stock, Read().Patches[name].Cells);
            var reversed = Read();
            Array.Reverse(reversed.Patches[name].Cells);
            Confirm(Load(reversed), reversed.Patches[name].Cells);

            void Confirm(FileSelectPresentation presentation, FileSelectPatchCellDocument[] cells)
            {
                var expected = new ushort[1024];
                var actual = new ushort[1024];
                var compiled = Compiled(presentation);
                AssertEqual(cells.Length, compiled.Count, "file-select supplied patch count");
                for (int index = 0; index < cells.Length; index++)
                {
                    var source = cells[index];
                    var cell = source.Cell;
                    ushort word = SnesBgTilemapWord.Create(cell.TileRow * MapTileAtlasFormat.TileColumns + cell.TileColumn,
                        cell.Palette, cell.Priority, (cell.FlipX ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
                        (cell.FlipY ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)).Raw;
                    AssertEqual(new FileSelectCompiledPatchCell(source.Offset - Position16<TilemapCells>.Zero, word), compiled.Cell(index),
                        "file-select independent supplied order and coordinates");
                    expected[(3 + source.Y) * 32 + 4 + source.X] = word;
                }
                presentation.ApplyPatch(actual, patch, new Position16<TilemapCells>(4, 3));
                AssertTrue(actual.SequenceEqual(expected), "file-select exact stock or independently edited patch placement");
            }
        }
    }
    /// <summary>Checks game-over text words in VRAM for native and edited tilemap cells, including index bounds.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyStream3GameOverText(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Frontend.GameOverPresentationExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<GameOverPresentationDocument>(imported, ArtworkJson.Options);
        GameOverPresentation Load() => GameOverPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        var stock = Load();
        AssertTrue(stock.tilemap is null, "game-over stock text retains no tilemap row");
        Confirm(stock);
        foreach (GameOverTextStream stream in GameOverRomData.Text.All)
        {
            int cell = stream.DestinationByteOffset / sizeof(ushort);
            document.Tilemap[cell] = document.Tilemap[cell] with { Palette = 1, FlipY = true };
        }
        document.Tilemap[0] = document.Tilemap[0] with { TileColumn = 1 };
        Confirm(Load());
        AssertThrows<ArgumentOutOfRangeException>(() => GameOverPresentationDefinitions.TilemapWord(-1), "game-over text negative cell");
        AssertThrows<ArgumentOutOfRangeException>(() => GameOverPresentationDefinitions.TilemapWord(1024), "game-over text final cell boundary");

        void Confirm(GameOverPresentation presentation)
        {
            var words = new ushort[GameOverPresentationDefinitions.TilemapCellCount];
            presentation.LoadTilemapTo(words);
            for (int cell = 0; cell < document.Tilemap.Length; cell++)
            {
                MapPresentationCell source = document.Tilemap[cell];
                ushort expected = SnesBgTilemapWord.Create(source.TileRow * MapTileAtlasFormat.TileColumns + source.TileColumn,
                    source.Palette, source.Priority, (source.FlipX ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
                    (source.FlipY ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)).Raw;
                AssertEqual(expected, words[cell], "game-over native/edited text word and placement");
            }
        }
    }

    /// <summary>Checks options anchors, labels, language regions, toggles, and heading draw placement against ROM and edits.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for options geometry")]
    private static void VerifyStream3OptionsGeometry(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Frontend.GameOptionsPresentationExtractor.Extract(rom);
        GameOptionsPresentationDocument ReadDocument() => RequiredJson.Deserialize<GameOptionsPresentationDocument>(
            imported, ArtworkJson.Options);
        GameOptionsPresentation Load(GameOptionsPresentationDocument document) => GameOptionsPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        var stock = ReadDocument();
        var loaded = Load(stock);
        foreach (var (field, geometry) in new VerifyStream3OptionsGeometryCase[]
        {
            new VerifyStream3OptionsGeometryCase("controllerLabelAnchors", loaded.controllerLabelAnchors), new VerifyStream3OptionsGeometryCase("languageRegions", loaded.languageRegions),
            new VerifyStream3OptionsGeometryCase("specialToggles", loaded.specialToggles), new VerifyStream3OptionsGeometryCase("headingAnchors", loaded.headingAnchors), new VerifyStream3OptionsGeometryCase("cursorAnchors", loaded.cursorAnchors),
        })
            AssertTrue(geometry is null, "stock options geometry calculates " + field);
        for (int action = 0; action < 7; action++)
        {
            ushort offset = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x82f639 + action * 2));
            AssertEqual(new Position16<TilemapCells>(checked((ushort)(offset / 2 % 32)), checked((ushort)(offset / 2 / 32))),
                GameOptionsPresentationDefinitions.ControllerAnchor(action), "native options label anchor");
        }
        foreach ((GameOptionsPage page, int address, int count) in new[]
        {
            new VerifyStream3OptionsBordersCase(GameOptionsPage.Primary, 0x82f307, 5),
            new VerifyStream3OptionsBordersCase(GameOptionsPage.Controller, 0x82f31b, 9),
            new VerifyStream3OptionsBordersCase(GameOptionsPage.Special, 0x82f33f, 3),
        })
            for (int row = 0; row < count; row++)
                AssertEqual(new Position16<OamPixels>(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + row * 4)), CartridgeImportSource.Require(rom).ReadCartridgeWord((address + row * 4 + 2))),
                    loaded.CursorPosition(page, row), "native options cursor anchor");
        var stockLabels = loaded.controllerLabels;
        AssertEqual(0, stockLabels.Count, "options stock button glyphs retain no tilemap rows");
        for (int button = 0; button < 7; button++)
        for (int cell = 0; cell < 6; cell++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x82f659 + button * 12 + cell * 2)),
                GameOptionsPresentationDefinitions.ControllerLabelWord(GameOptionsPresentationDefinitions.ControllerLabelButton(button), cell),
                "native options glyph composition preserves each tile and flip");
        Confirm(stock, loaded);
        var edited = ReadDocument();
        for (int action = 0; action < edited.ControllerLabelAnchors.Length; action++)
        {
            Position16<TilemapCells> old = edited.ControllerLabelAnchors[action];
            edited.ControllerLabelAnchors[action] = new(checked((ushort)(old.X + 1)), old.Y);
        }
        foreach (MapPresentationCell[] label in edited.ControllerLabels.Values)
            for (int cell = 0; cell < label.Length; cell++)
                label[cell] = label[cell] with { FlipX = !label[cell].FlipX, Palette = 1 };
        edited.LanguageRegions[0].Cells[0] = 0;
        edited.SpecialToggles[GameOptionsPresentationDefinitions.ToggleName(GameOptionsToggle.IconCancel)].EnabledCells[0] = 0;
        foreach (string page in edited.HeadingAnchors.Keys.ToArray())
        {
            Position16<OamPixels> old = edited.HeadingAnchors[page];
            edited.HeadingAnchors[page] = new(checked((ushort)(old.X + 1)), old.Y);
            for (int row = 0; row < edited.CursorAnchors[page].Length; row++)
            {
                old = edited.CursorAnchors[page][row];
                edited.CursorAnchors[page][row] = new(checked((ushort)(old.X + 1)), old.Y);
            }
        }
        Confirm(edited, Load(edited));

        void Confirm(GameOptionsPresentationDocument document, GameOptionsPresentation presentation)
        {
            foreach (bool japanese in new[] { false, true })
            {
                byte[] actual = presentation.CreatePage(GameOptionsTilemap.Primary);
                byte[] expected = (byte[])actual.Clone();
                foreach (var region in document.LanguageRegions)
                    Paint(expected, region.Cells, japanese == region.HighlightWhenJapanese ? document.SelectedPalette : document.UnselectedPalette);
                presentation.ApplyLanguage(actual, japanese);
                AssertTrue(actual.SequenceEqual(expected), "options language boxes preserve exact stock/edited cells");
            }
            foreach (GameOptionsToggle toggle in GameOptionsPresentationDefinitions.Toggles)
            foreach (bool enabled in new[] { false, true })
            {
                GameOptionsToggleVisualDocument visual = document.SpecialToggles[GameOptionsPresentationDefinitions.ToggleName(toggle)];
                byte[] actual = presentation.CreatePage(GameOptionsTilemap.SpecialEnglish);
                byte[] expected = (byte[])actual.Clone();
                Paint(expected, visual.EnabledCells, enabled ? document.SelectedPalette : document.UnselectedPalette);
                Paint(expected, visual.DisabledCells, enabled ? document.UnselectedPalette : document.SelectedPalette);
                presentation.ApplySpecialToggle(actual, toggle, enabled);
                AssertTrue(actual.SequenceEqual(expected), "options toggle boxes preserve exact stock/edited cells");
            }
            for (int action = 0; action < 7; action++)
            for (int button = 0; button < 7; button++)
            {
                byte[] actual = presentation.CreatePage(GameOptionsTilemap.ControllerEnglish);
                byte[] expected = (byte[])actual.Clone();
                Position16<TilemapCells> point = document.ControllerLabelAnchors[action];
                for (int cell = 0; cell < 6; cell++)
                {
                    MapPresentationCell source = document.ControllerLabels[GameOptionsPresentationDefinitions.ControllerLabelName(GameOptionsPresentationDefinitions.ControllerLabelButton(button))][cell];
                    ushort word = SnesBgTilemapWord.Create(source.TileRow * MapTileAtlasFormat.TileColumns + source.TileColumn, source.Palette, source.Priority,
                        (source.FlipX ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
                        (source.FlipY ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)).Raw;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(((point.Y + cell / 3) * 32 + point.X + cell % 3) * 2), word);
                }
                presentation.ApplyControllerLabel(actual, action, button);
                AssertTrue(actual.SequenceEqual(expected), "options controller boxes preserve exact stock/edited placement");
            }
            foreach (GameOptionsPage page in GameOptionsPresentationDefinitions.MenuPages)
            {
                Position16<OamPixels>[] cursors = document.CursorAnchors[GameOptionsPresentationDefinitions.MenuName(page)];
                for (int row = 0; row < cursors.Length; row++)
                    AssertEqual(cursors[row], presentation.CursorPosition(page, row), "options stock/edited cursor");
            }
            foreach (GameOptionsPage page in GameOptionsPresentationDefinitions.MenuPages)
            {
                Position16<OamPixels> heading = document.HeadingAnchors[GameOptionsPresentationDefinitions.MenuName(page)];
                var actual = ScreenSpritesFixtures.Blank();
                var expected = new OamBuffer();
                presentation.DrawHeading(actual, MenuSheetFixtures.GameOptions(presentation), page, 3);
                MenuSpriteCompiler.Compile(document.Sprites[GameOptionsPresentationDefinitions.HeadingSpriteName(page)], "heading oracle")
                    .DrawOnScreen(expected, new(checked(heading.X), unchecked((ushort)(heading.Y - 3))),
                        SnesObjAttributeWord.Create(0, document.CursorPalette, 0).PaletteBits);
                actual.FinishFrame(); expected.FinalizeFrame();
                AssertTrue(actual.ImageEquals(expected),
                    "options stock/edited heading origin and scroll");
            }
            AssertThrows<ArgumentOutOfRangeException>(() => presentation.ApplyControllerLabel(new byte[2048], -1, 0), "options invalid action");
            AssertThrows<ArgumentOutOfRangeException>(() => presentation.CursorPosition((GameOptionsPage)byte.MaxValue, 0), "options invalid page");
        }
        static void Paint(byte[] page, int[] cells, int palette)
        {
            foreach (int cell in cells)
            {
                Span<byte> destination = page.AsSpan(cell * 2);
                ushort old = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(destination);
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(destination, (ushort)((old & ~0x1c00) | palette << 10));
            }
        }
    }
    /// <summary>Runs the stream-three ROM oracle for inline lookup tables and presentation boundaries.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for stream-3 inline tables")]
    internal static void VerifyLookupStream3(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyStream3GameOverText), () => VerifyStream3GameOverText(rom));
        Suite(nameof(VerifyStream3OptionsGeometry), () => VerifyStream3OptionsGeometry(rom));
        DoorSetupCode[] expectedDoorCallbacks =
        [
            DoorSetupCode.DoorCode_Scroll6_Green,
            DoorSetupCode.DoorASM_Scroll_0_Blue,
            DoorSetupCode.DoorASM_Scroll_13_Blue,
            DoorSetupCode.DoorASM_Scroll_4_Red_8_Green,
            DoorSetupCode.DoorASM_Scroll_8_9_A_B_Red,
            DoorSetupCode.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red,
            DoorSetupCode.DoorASM_Scroll_1_4_Green,
            DoorSetupCode.DoorASM_Scroll_2_Blue,
            DoorSetupCode.DoorASM_Scroll_17_Blue,
            DoorSetupCode.DoorASM_Scroll_4_Blue,
            DoorSetupCode.DoorASM_Scroll_6_Green_duplicate,
            DoorSetupCode.DoorASM_Scroll_3_Green,
            DoorSetupCode.DoorASM_Scroll_18_1C_Green,
            DoorSetupCode.DoorASM_Scroll_5_6_Blue,
            DoorSetupCode.DoorASM_Scroll_1D_Blue,
            DoorSetupCode.DoorASM_Scroll_2_3_Green,
            DoorSetupCode.DoorASM_Scroll_0_Red_1_Green,
            DoorSetupCode.DoorASM_Scroll_B_Green,
            DoorSetupCode.DoorASM_Scroll_Scroll_1C_Red_1D_Blue,
            DoorSetupCode.DoorASM_Scroll_4_Red,
            DoorSetupCode.DoorASM_Scroll_20_24_25_Green,
            DoorSetupCode.DoorASM_Scroll_2_Blue_duplicate,
            DoorSetupCode.DoorASM_Scroll_0_Green,
            DoorSetupCode.DoorASM_Scroll_6_7_Green,
            DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red,
            DoorSetupCode.DoorASM_Scroll_1_Blue_3_Red,
            DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue,
            DoorSetupCode.DoorASM_Scroll_2_3_Blue,
            DoorSetupCode.DoorASM_Scroll_0_1_Green,
            DoorSetupCode.DoorASM_Scroll_1_Green,
            DoorSetupCode.DoorASM_Scroll_F_12_Green,
            DoorSetupCode.DoorASM_Scroll_6_Green_duplicate_again,
            DoorSetupCode.DoorASM_Scroll_0_Green_1_Blue,
            DoorSetupCode.DoorASM_Scroll_2_Green,
            DoorSetupCode.DoorASM_Scroll_3_4_Red_6_7_8_Blue,
            DoorSetupCode.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red,
            DoorSetupCode.DoorASM_Scroll_0_1_Blue,
            DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red,
            DoorSetupCode.DoorASM_Scroll_A_Green,
            DoorSetupCode.DoorASM_Scroll_0_2_Green,
            DoorSetupCode.DoorASM_Scroll_6_7_Blue_8_Red,
            DoorSetupCode.DoorASM_Scroll_2_Red_3_Blue,
            DoorSetupCode.DoorASM_Scroll_7_Green,
            DoorSetupCode.DoorASM_Scroll_1_Red_2_Blue,
            DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red,
            DoorSetupCode.DoorASM_Scroll_1_Blue_4_Red,
            DoorSetupCode.DoorASM_Scroll_0_Blue_1_2_3_Red,
            DoorSetupCode.DoorASM_Scroll_0_Green_duplicate,
            DoorSetupCode.DoorASM_Scroll_0_1_Blue_4_Red,
            DoorSetupCode.DoorASM_Scroll_0_Blue_3_Red_duplicate,
            DoorSetupCode.DoorASM_Scroll_0_Blue_duplicate,
            DoorSetupCode.DoorASM_Scroll_0_Blue_1_Red_duplicate,
            DoorSetupCode.DoorASM_Scroll_18_Blue,
            DoorSetupCode.DoorASM_Scroll_2_Blue_3_Red,
            DoorSetupCode.DoorASM_Scroll_E_Red,
            DoorSetupCode.DoorASM_Scroll_1_Blue,
            DoorSetupCode.DoorASM_Scroll_0_Green_duplicate_again,
            DoorSetupCode.DoorASM_Scroll_3_Red_4_Blue,
            DoorSetupCode.DoorASM_Scroll_29_Blue,
            DoorSetupCode.DoorASM_Scroll_28_2E_Green,
            DoorSetupCode.DoorASM_Scroll_6_7_8_9_A_B_Red,
            DoorSetupCode.DoorASM_Scroll_A_Red_B_Blue,
            DoorSetupCode.DoorASM_Scroll_0_Red_4_Blue_duplicate,
            DoorSetupCode.DoorASM_Scroll_0_Red_1_Blue,
            DoorSetupCode.DoorASM_Scroll_9_Red_A_Blue,
            DoorSetupCode.DoorASM_Scroll_0_2_Red_1_Blue,
            DoorSetupCode.DoorASM_Scroll_1_Blue_duplicate,
            DoorSetupCode.DoorASM_Scroll_6_Blue,
            DoorSetupCode.DoorASM_Scroll_4_Red_duplicate,
            DoorSetupCode.DoorASM_Scroll_4_7_Red,
            DoorSetupCode.DoorASM_Scroll_1_Blue_2_Red_duplicate,
            DoorSetupCode.DoorASM_Scroll_0_2_Green_duplicate,
            DoorSetupCode.DoorASM_Scroll_0_1_Green_duplicate,
            DoorSetupCode.DoorASM_Scroll_18_Blue_19_Red,
        ];
        AssertTrue(expectedDoorCallbacks.SequenceEqual(SuperMetroid.Core.Rooms.Doors.DoorScrollPrograms.Pointers),
            "stream 3 pure door callback original registration order");
        var expectedCallbacks = expectedDoorCallbacks.ToHashSet();
        foreach (DoorSetupCode pointer in Enum.GetValues<DoorSetupCode>())
            AssertEqual(expectedCallbacks.Contains(pointer),
                SuperMetroid.Core.Rooms.Doors.DoorScrollPrograms.Contains(pointer), "stream 3 exact callback ownership");
        foreach (ushort undefined in new ushort[] { 0x0001, 0xb972, 0xffff })
            AssertThrows<InvalidOperationException>(
                () => SuperMetroid.Core.Rooms.Doors.DoorScrollPrograms.Contains((DoorSetupCode)undefined),
                "stream 3 undefined door setup code");
        foreach (DoorSetupCode pointer in expectedDoorCallbacks)
        {
            byte[] expected = Enumerable.Repeat((byte)0x7f, RoomScrollGrid.StorageByteCount).ToArray();
            var actual = RoomScrollGrid.LoadCompiled(new RoomScrollStorage(), expected, new Extent16<RoomScreens>(10, 5));
            ExecuteStream3DoorScroll(rom, (ushort)pointer, expected);
            AssertTrue(SuperMetroid.Core.Rooms.Doors.DoorScrollPrograms.TryApply(pointer, actual), "stream 3 callback recognized");
            AssertTrue(actual.StorageBytes.ToArray().SequenceEqual(expected), "stream 3 native ordered door writes and all untouched cells");
        }
        var unchangedScroll = RoomScrollGrid.LoadCompiled(new RoomScrollStorage(), new byte[50], new Extent16<RoomScreens>(10, 5));
        AssertTrue(!SuperMetroid.Core.Rooms.Doors.DoorScrollPrograms.TryApply(DoorSetupCode.None, unchangedScroll), "stream 3 setup without scroll writes is a no-op");
        for (int pair = 0; pair < 16; pair++)
        {
            int address = 0xadde5f + 2 * pair;
            int pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            var expected = pointer switch
            {
                0xe1a6 => MotherBrainBeamRomData.Direction.Down,
                0xde7f => MotherBrainBeamRomData.Direction.Right,
                0xdf6e => MotherBrainBeamRomData.Direction.Up,
                0xde5e => MotherBrainBeamRomData.Direction.Retain,
                0 => MotherBrainBeamRomData.Direction.Unsupported,
                _ => throw new InvalidDataException("Unexpected native beam quadrant target."),
            };
            AssertEqual(expected, MotherBrainBeamRomData.DirectionForQuadrants(pair),
                "stream 3 native beam quadrant dispatcher including null and retaining entries");
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainBeamRomData.DirectionForQuadrants(invalid),
                "stream 3 beam dispatcher preserves span index bounds");
        IReadOnlyList<IntroPaletteSpan>[] introRegions =
        [IntroCinematicRomData.Palette.Gameplay, IntroCinematicRomData.Palette.GameplayClear,
         IntroCinematicRomData.Palette.Narration, IntroCinematicRomData.Palette.Discovery];
        IntroPaletteSpan[][] expectedIntroRegions =
        [
            [new(0, 20), new(48, 16), new(233, 6)],
            [new(0, 16), new(48, 16), new(233, 6)],
            [new(20, 3), new(112, 16), new(192, 32), new(240, 16)],
            [new(32, 16), new(224, 9)],
        ];
        int[][] nativeIntroOperands =
        [ [0x8bb258, 0x8bb261, 0x8bb26a], [0x8bb3c8, 0x8bb3d1, 0x8bb3da],
          [0x8bb273, 0x8bb27c, 0x8bb285, 0x8bb28e], [0x8bb2f5, 0x8bb2fe] ];
        for (int group = 0; group < introRegions.Length; group++)
        {
            AssertTrue(introRegions[group].SequenceEqual(expectedIntroRegions[group]),
                "stream 3 intro scene region order, offsets and color counts");
            for (int index = 0; index < nativeIntroOperands[group].Length; index++)
            {
                int address = nativeIntroOperands[group][index];
                AssertEqual((byte)0xa2, rom.ReadCartridgeByte(address), "stream 3 intro native LDX region");
                AssertEqual((byte)0xa0, rom.ReadCartridgeByte(address + 3), "stream 3 intro native LDY color count");
                // The native X operand is the first color's byte offset into the two-byte palette entries.
                AssertEqual(introRegions[group][index].FirstColor * 2,
                    CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 1), "stream 3 intro native offset");
                AssertEqual(introRegions[group][index].ColorCount,
                    CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 4), "stream 3 intro native count");
            }
            foreach (int invalid in new[] { -1, introRegions[group].Count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => _ = introRegions[group][invalid], "stream 3 intro region bounds");
        }
        ushort[] motherBrainRoots =
        [
            0xa586, 0xa5bf, 0xa5f8, 0xa62c, 0xa660, 0xa694, 0xa69b, 0xa6d9, 0xa717,
            0xa750, 0xa789, 0xad3e, 0xad6d, 0xada1, 0xadd5, 0xae09, 0xae33, 0xae5d,
        ];
        int[] nativeCharacterCounts = [11, 11, 10, 10, 10, 1, 12, 12, 11, 11, 11, 9, 10, 10, 10, 8, 8, 26];
        AssertTrue(MotherBrainVisualDefinitionsConstants.Frames().Select(frame => frame.Pointer).SequenceEqual(motherBrainRoots),
            "stream 3 all original Mother Brain root identities and enumeration");
        for (int index = 0; index < motherBrainRoots.Length; index++)
        {
            var actual = MotherBrainVisualDefinitionsConstants.Frame(index);
            AssertEqual((byte)0xa9, actual.Bank, "stream 3 Mother Brain visual bank");
            AssertEqual($"mother_brain_a9_{motherBrainRoots[index]:x4}", actual.Name, "stream 3 Mother Brain visual identity name");
            int address = 0xa90000 | actual.Pointer;
            AssertEqual(nativeCharacterCounts[index], CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                "stream 3 native OAM record widths used for root strides");
        }
        foreach (int invalid in new[] { -1, 18, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = MotherBrainVisualDefinitionsConstants.Frame(invalid), "stream 3 visual root bounds");
        MotherBrainSpecialSpriteSheetDefinition[] expectedSheets =
        [
            new("mother-brain-leg-tiles.png", 0xb79000, 8, 0x7400),
            new("mother-brain-baby-tiles.png", 0xb18800, 4, 0x7c00),
            new("mother-brain-attack-tiles.png", 0xb7a000, 4, 0x7c00),
            new("mother-brain-exploded-door-tiles.png", 0xabf400, 2, 0x7000),
        ];
        AssertTrue(expectedSheets.SequenceEqual(MotherBrainSpecialSpriteArtworkDefinitions.All),
            "stream 3 named special sheet order and every field");
        foreach (var sheet in expectedSheets)
            foreach (int source in new[] { sheet.SourceAddress, sheet.SourceAddress + sheet.ByteCount - 1 })
            {
                AssertTrue(MotherBrainSpecialSpriteArtworkDefinitions.TryForSource((uint)source, out var found),
                    "stream 3 special sheet boundary lookup");
                AssertEqual(sheet, found, "stream 3 special sheet source owner");
            }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MotherBrainSpecialSpriteArtworkDefinitions.All[invalid],
                "stream 3 special sheet original index exception");
        // Confirm the identified copy-list conversion against native STA operands.
        int[][] nativeDoorCopies =
        [
            [0x82e1f4, 0x82e1fa, 0x82e200, 0x82e206, 0x82e20c, 0x82e212, 0x82e218, 0x82e21e],
            [0x82e22f, 0x82e235, 0x82e23b, 0x82e241, 0x82e247],
            [0x82e252, 0x82e258, 0x82e25e, 0x82e264],
        ];
        int targetPaletteBase = CartridgeImportSource.Require(rom).ReadCartridgeWord(0x82e1f5) - 18;
        ushort[] sourceColors = Enumerable.Range(1, 256).Select(value => (ushort)value).ToArray();
        for (int group = 0; group < nativeDoorCopies.Length; group++)
        {
            // The fade target the copy writes into; slots it leaves alone stay black.
            Bgr555[] actual = ToColors(new ushort[256]);
            ushort[] expected = new ushort[256];
            foreach (int instruction in nativeDoorCopies[group])
            {
                AssertEqual((byte)0x8d, rom.ReadCartridgeByte(instruction), "stream 3 native door color store opcode");
                int destination = CartridgeImportSource.Require(rom).ReadCartridgeWord(instruction + 1);
                int color = (destination - targetPaletteBase) / 2;
                expected[color] = sourceColors[color];
            }
            switch (group)
            {
                case 0: DoorTransitionPaletteDefinitions.PreserveHud(ToColors(sourceColors), actual); break;
                case 1: DoorTransitionPaletteDefinitions.PreserveCommonCre(ToColors(sourceColors), actual); break;
                case 2: DoorTransitionPaletteDefinitions.PreserveEscapeTimer(ToColors(sourceColors), actual); break;
                default:
                    break;
            }
            AssertTrue(ToColors(expected).SequenceEqual(actual), "stream 3 exact native door fade preserved and black slots");
        }
        // Confirm the replaced selector for its complete ushort input domain.
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            int address = 0x9bc346 + 2 * (angle >> 10);
            int expected = (0x9a0000|CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
            var asset = GrappleTileDefinitions.SegmentAssetFor(SnesAngle.FromRaw((ushort)angle));
            AssertEqual(expected, GrappleTileDefinitions.TransferFor(asset).SourceAddress,
                "stream 3 native grapple angle sector");
        }

        GrappleTileTransfer[] expectedTransfers =
        [
            new(VramAssetId.GrapplePointFirstTiles, 0x9a8200, 0, 32),
            new(VramAssetId.GrapplePointSecondTiles, 0x9a8400, 32, 32),
            new(VramAssetId.GrapplePointThirdTiles, 0x9a8600, 64, 32),
            new(VramAssetId.GrapplePointFourthTiles, 0x9a8800, 96, 32),
            new(VramAssetId.GrappleHorizontalSegmentTiles, 0x9a8220, 128, 128),
            new(VramAssetId.GrappleDiagonalSegmentTiles, 0x9a8a20, 256, 128),
            new(VramAssetId.GrappleVerticalSegmentTiles, 0x9a9220, 384, 128),
        ];
        var actualTransfers = GrappleTileDefinitions.Transfers;
        AssertTrue(expectedTransfers.SequenceEqual(actualTransfers),
            "stream 3 grapple transfer enumeration and every field");
        for (int index = 0; index < expectedTransfers.Length; index++)
        {
            AssertEqual(expectedTransfers[index], actualTransfers[index],
                "stream 3 grapple transfer index");
            AssertEqual(expectedTransfers[index], GrappleTileDefinitions.TransferFor(expectedTransfers[index].Asset),
                "stream 3 grapple transfer asset dispatch");
        }
        foreach (int invalid in new[] { -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = actualTransfers[invalid],
                "stream 3 grapple transfer bounds");
        AssertThrows<InvalidDataException>(() => GrappleTileDefinitions.TransferFor((VramAssetId)(-1)),
            "stream 3 invalid grapple asset");

        for (int parameter = 0; parameter <= ushort.MaxValue; parameter++)
        {
            int offset = 2 * (parameter & 3);
            ushort list = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa8e682 + offset);
            var function = (SparkEnemyFunction)CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa8e688 + offset);
            SparkMovementDefinition definition = SparkMovementDefinitions.InitialState((ushort)parameter);
            // Selector three's list word is the following main-AI function pointer, which starts no animation.
            AssertEqual(list, definition.Program is { } program
                    ? NativeEnemyProgramOracle.EntryOf(SparkNativeListAddresses.Entries, program)
                    : (ushort)SparkEnemyFunction.AlwaysActive,
                "stream 3 native Spark list selector including adjacent-word case");
            AssertEqual(function, definition.Function, "stream 3 native Spark function selector including adjacent-word case");
        }
        for (int bucket = 0; bucket <= ushort.MaxValue; bucket++)
        {
            ushort direction = (ushort)bucket;
            if ((bucket & 31) != 0 || bucket > 224)
                AssertThrows<InvalidDataException>(() => ShaktoolInstructionDefinitions.ForOrientationBucket(direction),
                    "stream 3 invalid Shaktool orientation");
            else
            {
                int address = 0xaadd15 + 2 * (bucket >> 5);
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                AssertEqual(expected, NativeEnemyProgramOracle.EntryOf(ShaktoolNativeListAddresses.Entries, ShaktoolInstructionDefinitions.ForOrientationBucket(direction)),
                    "stream 3 native Shaktool orientation");
            }
        }
        for (int segment = 0; segment < 7; segment++)
        {
            int offset = segment * 2;
            ushort collision = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xaadf13 + offset);
            AssertEqual(collision, NativeEnemyProgramOracle.EntryOf(ShaktoolNativeListAddresses.Entries, ShaktoolInstructionDefinitions.CollisionForSegment(segment)),
                "stream 3 native Shaktool collision program");
        }
        foreach (int invalid in new[] { -1, 7, int.MinValue, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => ShaktoolInstructionDefinitions.CollisionForSegment(invalid),
                "stream 3 invalid Shaktool collision segment");
        }
        Suite(nameof(VerifyStream3WorkRobotColors), () => VerifyStream3WorkRobotColors(rom));
        Suite(nameof(VerifyStream3PickupAndFirefleaPrograms), () => VerifyStream3PickupAndFirefleaPrograms(rom));
        Suite(nameof(VerifyStream3ChootControl), () => VerifyStream3ChootControl(rom));
        Suite(nameof(VerifyStream3MotherBrainFades), () => VerifyStream3MotherBrainFades(rom));
        Suite(nameof(VerifyStream3BabyFade), () => VerifyStream3BabyFade(rom));
        Suite(nameof(VerifyStream3DrainFades), () => VerifyStream3DrainFades(rom));
        Suite(nameof(VerifyStream3ShitroidPulse), () => VerifyStream3ShitroidPulse(rom));
        Suite(nameof(VerifyStream3HealthTint), () => VerifyStream3HealthTint(rom));
        Suite(nameof(VerifyStream3RecoveryLights), () => VerifyStream3RecoveryLights(rom));
        Suite(nameof(VerifyStream3RoomFlash), () => VerifyStream3RoomFlash(rom));
        Suite(nameof(VerifyStream3CorpseGeometry), () => VerifyStream3CorpseGeometry(rom));
        Suite(nameof(VerifyStream3EscapeGeometry), () => VerifyStream3EscapeGeometry(rom));
        Suite(nameof(VerifyStream3PainfulWalking), () => VerifyStream3PainfulWalking(rom));
        Suite(nameof(VerifyStream3DeathSelectors), () => VerifyStream3DeathSelectors(rom));
        Suite(nameof(VerifyMotherBrainContactHitboxes), () => VerifyMotherBrainContactHitboxes());
        byte[] grappleSpriteJson = SuperMetroid.AssetExtraction.Samus.GrappleSpriteExtractor.Extract(rom);
        var grappleSprites = GrappleSpriteCatalog.Load(new MemoryStream(grappleSpriteJson));
        AssertTrue(grappleSprites.segments is null, "stream 3 stock rope attribute rows discarded");
        AssertTrue(GrappleSpriteDefinitions.SegmentAttributeAddresses.SequenceEqual(GrappleSegmentAttributeAddresses),
            "stream 3 segment address enumeration");
        for (int frame = 0; frame < 4; frame++)
        {
            int address = 0x94b18d + 4 * frame;
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(address), grappleSprites.Segment(frame), "stream 3 calculated native rope attributes");
            foreach (string field in new[] { "tileColumn", "tileRow", "palette", "priority", "flipX", "flipY" })
            {
                var node = RequiredJson.Parse(grappleSpriteJson);
                var style = node.Required("segments").Required(frame);
                int bit = field switch { "tileColumn" => 1, "tileRow" => 16, "palette" => 0x200, "priority" => 0x1000, "flipX" => 0x4000, _ => 0x8000 };
                if (field is "flipX" or "flipY") style[field] = !style.Required(field).GetValue<bool>();
                else style[field] = style.Required(field).GetValue<int>() ^ 1;
                var edited = GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(node.ToJsonString())));
                for (int check = 0; check < 4; check++)
                    AssertEqual((ushort)(grappleSprites.Segment(check) ^ (check == frame ? bit : 0)), edited.Segment(check), "stream 3 independent rope attribute edit");
            }
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => _ = GrappleSpriteDefinitions.SegmentAttributeAddresses[invalid], "stream 3 rope operand address bounds");
            AssertThrows<InvalidDataException>(() => grappleSprites.Segment(invalid), "stream 3 rope attribute bounds");
        }
        var swingFrames = GrappleSwingFrameCatalog.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.Samus.GrappleSwingFrameExtractor.Extract(rom)));
        for (int angle = 0; angle < 256; angle++)
            AssertEqual(rom.ReadCartridgeByte(0x9bc1c2 + angle), swingFrames.Resolve(new SnesByteAngle((byte)angle)), "stream 3 native calculated swing art frame");
        AssertTrue(swingFrames.frames is null, "stream 3 stock swing frame lookup discarded");
        int[] editedFrames = Enumerable.Range(0, 256).Select(angle => (int)rom.ReadCartridgeByte(0x9bc1c2 + angle)).ToArray();
        for (int editedAngle = 0; editedAngle < 256; editedAngle++)
        {
            int original = editedFrames[editedAngle];
            editedFrames[editedAngle] = (original + 1) & 31;
            var edited = GrappleSwingFrameCatalog.Load(new MemoryStream(GrappleSwingFrameCatalog.Write(new()
            { Version = 1, Frames = editedFrames })));
            for (int angle = 0; angle < 256; angle++)
                AssertEqual((byte)editedFrames[angle], edited.Resolve(new SnesByteAngle((byte)angle)), "stream 3 independently edited swing art frame");
            editedFrames[editedAngle] = original;
        }
        byte[] fileSelectJson = SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(rom);
        var fileSelect = FileSelectPresentation.Load(new MemoryStream(fileSelectJson));
        foreach (object? sequence in new object?[] { fileSelect.digits, fileSelect.slotLetters })
            AssertTrue(sequence is null, "stream 3 stock file-select glyph sequence discarded");
        var glyphTilemap = new ushort[1024];
        for (int glyph = 0; glyph < 13; glyph++)
        {
            bool digit = glyph < 10;
            int index = digit ? glyph : glyph - 10;
            ushort expected = (ushort)(digit ? 0x2060 + index : 0x206a + index);
            var anchor = new Position16<TilemapCells>(4, 3);
            if (digit) fileSelect.WriteDigit(glyphTilemap, anchor, 2, index);
            else fileSelect.WriteSlotLetter(glyphTilemap, anchor, index);
            AssertEqual(expected, glyphTilemap[3 * 32 + 4 + (digit ? 2 : 0)], "stream 3 actual file-select calculated glyph write");
            foreach (string field in new[] { "tileColumn", "tileRow", "palette", "priority", "flipX", "flipY" })
            {
                var node = RequiredJson.Parse(fileSelectJson);
                var cell = node.Required(digit ? "digits" : "slotLetters").Required(index);
                int bit = field switch { "tileColumn" => 1, "tileRow" => 32, "palette" => 0x400, "priority" => 0x2000, "flipX" => 0x4000, _ => 0x8000 };
                if (field is "priority" or "flipX" or "flipY") cell[field] = !cell.Required(field).GetValue<bool>();
                else cell[field] = cell.Required(field).GetValue<int>() ^ 1;
                var edited = FileSelectPresentation.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(node.ToJsonString())));
                if (digit) edited.WriteDigit(glyphTilemap, anchor, 2, index);
                else edited.WriteSlotLetter(glyphTilemap, anchor, index);
                AssertEqual((ushort)(expected ^ bit), glyphTilemap[3 * 32 + 4 + (digit ? 2 : 0)], "stream 3 independent file-select glyph attribute edit");
            }
        }
        string[] expectedCreditRoles = ["staff-heading", "producer-heading", "producer-name", "director-heading", "director-name", "background-designers-heading", "background-designer-1", "background-designer-2", "background-designer-3", "object-designers-heading", "object-designer-1", "object-designer-2", "samus-original-designer-heading", "samus-original-designer-name", "samus-designer-heading", "samus-designer-name", "sound-program-heading", "sound-effects-heading", "sound-programmer-name", "music-composers-heading", "music-composer-1", "music-composer-2", "program-director-heading", "program-director-name", "system-coordinator-heading", "system-coordinator-name", "system-programmer-heading", "system-programmer-name", "samus-programmer-heading", "samus-programmer-name", "event-programmer-heading", "event-programmer-name", "enemy-programmer-heading", "enemy-programmer-name", "map-programmer-heading", "map-programmer-name", "assistant-programmer-heading", "assistant-programmer-name", "coordinators-heading", "coordinator-1", "coordinator-2", "printed-art-work-heading", "printed-art-work-1", "printed-art-work-2", "printed-art-work-3", "printed-art-work-4", "printed-art-work-5", "printed-art-work-6", "special-thanks-heading", "special-thanks-01", "special-thanks-02", "special-thanks-03", "special-thanks-04", "special-thanks-05", "special-thanks-06", "special-thanks-07", "special-thanks-08", "special-thanks-09", "special-thanks-10", "special-thanks-11", "special-thanks-12", "special-thanks-13", "special-thanks-14", "special-thanks-15", "special-thanks-r-and-d", "general-manager-heading", "general-manager-name"];
        AssertTrue(CreditsPresentationDefinitions.Lines.Select(line => line.Id).SequenceEqual(expectedCreditRoles), "stream 3 all credit role identities and order");
        Suite(nameof(VerifyCreditsPresentation), () => VerifyCreditsPresentation(Path.GetFullPath("Super Metroid.smc")));
        byte[] creditsJson = SuperMetroid.AssetExtraction.Frontend.Ending.CreditsPresentationExtractor.Extract(rom);
        var credits = CreditsPresentation.Load(new MemoryStream(creditsJson));
        AssertTrue(credits.fixtureRows is null, "stream 3 credits stock has no cached row storage");
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(creditsJson)), credits.ContentIdentity, "stream 3 credits source identity preserved");
        foreach (int invalid in new[] { -1, credits.RowCount, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = credits.GetRow(invalid).Length, "stream 3 calculated credits row bounds");
        var escapePrograms = CrateriaEscapeLightningPaletteFxColorDefinitions.All;
        AssertEqual(2, escapePrograms.Count, "stream 3 escape lightning owner count");
        AssertTrue(escapePrograms.Select(program => program.Owner).SequenceEqual(new[] { CrateriaEscapeLightningPaletteOwner.YellowLightning, CrateriaEscapeLightningPaletteOwner.CreBlockPixel }), "stream 3 escape lightning owner order");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = escapePrograms[invalid], "stream 3 escape lightning owner bounds");
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CrateriaEscapeLightningPaletteFxColorDefinitionsConstants.Duration(invalid), "stream 3 escape lightning duration bounds");
        Suite(nameof(VerifyGrappleConnectionDefinitions), () => VerifyGrappleConnectionDefinitions((SuperMetroidAddressSpace)rom));
        Suite(nameof(VerifyMotherBrainTurretDefinitions), () => VerifyMotherBrainTurretDefinitions((SuperMetroidAddressSpace)rom));
        foreach (MotherBrainContactPart part in Enum.GetValues<MotherBrainContactPart>())
        {
            var regions = MotherBrainContactHitboxDefinitions.Get(part);
            foreach (int invalid in new[] { -1, regions.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = regions[invalid], "stream 3 contact region bounds");
        }
        Console.WriteLine("Lookup stream 3: all implemented mapping conversions match their original values and accepted domains.");
    }

    /// <summary>Checks death explosion program selectors, native scatter anchors, and their generated burst cadence.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for death selectors")]
    private static void VerifyStream3DeathSelectors(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 3; parameter++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c929 + 2 * parameter)), MiscDustProjectileNativeListAddresses.Entry(MotherBrainDeathExplosionDefinitions.Program(parameter)),
                "stream 3 native death explosion variant");
        foreach (ushort invalid in new ushort[] { 3, 4, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainDeathExplosionDefinitions.Program(invalid),
                "stream 3 death explosion selector bounds");
        // Confirm the retained visual payload's catalog move and the independent group arithmetic.
        for (int index = 0; index < 28; index++)
            AssertEqual(new Offset16<RoomPixels>((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9b099 + 4 * index)), (short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9b09b + 4 * index))),
                MotherBrainDeathExplosionDefinitions.Anchor(index), "stream 3 native decorative death anchor");
        foreach (bool mixed in new[] { false, true })
        {
            var scatter = new MotherBrainRainbowBeamAttackSequence();
            scatter.State.Body.Position = new(320, 192);
            for (int burst = 0; burst < 8; burst++)
            {
                scatter.ThirdPhaseDeath.DeathExplosionIntervalTimer = WrappingCountdown.Start(0);
                int calls = 0;
                Func<int> random = () => { calls++; return ushort.MaxValue; };
                var requests = new List<MotherBrainDeathExplosionRequest>();
                scatter.ThirdPhaseDeath.GenerateDeathExplosions(mixed ? MotherBrainDeathExplosionStyle.Mixed : MotherBrainDeathExplosionStyle.Smoky, random, requests);
                int group = 6 - burst % 7;
                AssertEqual(mixed ? 4 : 2, requests.Count, "stream 3 death scatter burst size");
                AssertEqual(requests.Count, calls, "stream 3 one RNG call per death projectile");
                for (int item = 0; item < requests.Count; item++)
                {
                    short x = (short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9b099 + 16 * group + 4 * item));
                    short y = (short)CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9b09b + 16 * group + 4 * item));
                    AssertEqual((ushort)group, scatter.ThirdPhaseDeath.DeathAndEscapeExplosionIndex.RawValue, "stream 3 death scatter reverse group order");
                    AssertEqual(new OctantVector(x, y), new OctantVector(requests[item].Offset.X, requests[item].Offset.Y), "stream 3 selected death scatter anchors");
                    AssertEqual((ushort)(320 + x), unchecked((ushort)(scatter.State.Body.Position.X + requests[item].Offset.X)), "stream 3 death scatter body-relative X");
                    AssertEqual((ushort)(192 + y), unchecked((ushort)(scatter.State.Body.Position.Y + requests[item].Offset.Y)), "stream 3 death scatter body-relative Y");
                    AssertEqual(mixed ? (ushort)2 : (ushort)1, requests[item].ProjectileParameter, "stream 3 independent death type case");
                }
            }
        }
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        sequence.State.Phase = MotherBrainBodyFunction.ThirdPhaseDeathStartEscape;
        var bus = new TestAddressSpace();
        var samus = new SamusState { Health = 99 };
        var firstPage = sequence.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        AssertEqual(0, firstPage.EscapePaletteFxRequests.Count, "stream 3 escape palette effects wait for door graphics");
        var handoff = sequence.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        AssertEqual(4, handoff.EscapePaletteFxRequests.Count, "stream 3 four escape palette registrations");
        for (int effect = 0; effect < 4; effect++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9b296 + 7 * effect)), PaletteFxObjectAccess.NativeAddress(handoff.EscapePaletteFxRequests[effect]),
                "stream 3 native escape palette registration order");
        var text = sequence.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        AssertEqual(0, text.EscapePaletteFxRequests.Count, "stream 3 escape palette registrations occur once");
    }

    /// <summary>Checks eight painful-walk delay, neck, and pause rows against ROM and the sequence's stage behavior.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for painful walking")]
    private static void VerifyStream3PainfulWalking(ISnesAddressSpace rom)
    {
        for (int stage = 0; stage < 8; stage++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9beee + 2 * stage)), MotherBrainPainfulWalkingDefinitions.AnimationDelay(stage), "stream 3 native stagger animation delay");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9befe + 2 * stage)), MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(stage), "stream 3 native stagger neck angle delta");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9c049 + 2 * stage)), MotherBrainPainfulWalkingDefinitions.FunctionTimer(stage), "stream 3 native stagger pause timer");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.AnimationDelay(invalid), "stream 3 stagger delay bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(invalid), "stream 3 stagger neck bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.FunctionTimer(invalid), "stream 3 stagger timer bounds");
        }
        foreach (ushort stage in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, ushort.MaxValue })
        {
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            sequence.State.Body.Position = sequence.State.Body.Position.WithX(0);
            sequence.DrainedByBaby.PainfulWalkingStage = stage;
            // As the stagger's setup does before each walk: the stage selects its delay (terminal stages keep the last).
            sequence.DrainedByBaby.PainfulWalkingAnimationDelay = MotherBrainPainfulWalkingDefinitions.AnimationDelay(Math.Min(stage, (ushort)7));
            sequence.DrainedByBaby.StepPainfulWalking();
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9c049 + 2 * Math.Min(stage, (ushort)7))), sequence.DrainedByBaby.PainfulWalkingFunctionTimer.RawValue,
                "stream 3 real stagger timer clamps terminal stages");
        }
    }

    /// <summary>Checks escape-timer and exploded-door tile transfers against native records and sequence cursors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for escape geometry")]
    private static void VerifyStream3EscapeGeometry(ISnesAddressSpace rom)
    {
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        Func<MotherBrainSpriteTileTransferRequest> timer = sequence.ThirdPhaseDeath.CreateNextEscapeTimerTileTransfer;
        Func<MotherBrainSpriteTileTransferRequest> door = sequence.ThirdPhaseDeath.CreateNextExplodedDoorTileTransfer;
        foreach (bool timerList in new[] { true, false })
        {
            int count = timerList ? 7 : 2;
            int start = timerList ? 0xa6c4cb : 0xa9902f;
            var method = timerList ? timer : door;
            for (int index = 0; index < count; index++)
            {
                int address = start + index * 7;
                var expected = new MotherBrainSpriteTileTransferRequest((ushort)index, CartridgeImportSource.Require(rom).ReadCartridgeWord(address),
                    (uint)(CartridgeImportSource.Require(rom).ReadCartridgeLong((address + 2))), CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 5)));
                var calculated = timerList ? EscapeTextArtworkDefinitions.Transfer(index)
                    : MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(index);
                AssertEqual(expected, calculated, "stream 3 native escape graphics record");
                AssertEqual(expected, method(),
                    "stream 3 production escape transfer selection");
                AssertEqual((ushort)(index + 1), timerList ? sequence.ThirdPhaseDeath.EscapeTimerTileTransferIndex : sequence.ThirdPhaseDeath.ExplodedDoorTileTransferIndex,
                    "stream 3 production escape cursor advances once");
            }
            try
            {
                method();
                throw new InvalidDataException("Completed escape transfer list unexpectedly produced another entry.");
            }
            catch (InvalidOperationException) { }
        }
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTextArtworkDefinitions.PageSource(invalid), "stream 3 text page source bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTextArtworkDefinitions.PageDestination(invalid), "stream 3 text page destination bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTextArtworkDefinitions.PageByteCount(invalid), "stream 3 text page size bounds");
        }
        foreach (int invalid in new[] { -1, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTextArtworkDefinitions.Transfer(invalid), "stream 3 escape transfer bounds");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(invalid), "stream 3 door transfer bounds");
        using var temporary = new TestScratchDirectory("map-catalog");
        SuperMetroid.AssetExtraction.Enemies.EnemyTileArtworkFiles.Extract(rom, temporary.Root, SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256);
        Suite(nameof(VerifyInstalledMotherBrainEscapeTextArtwork), () => VerifyInstalledMotherBrainEscapeTextArtwork(temporary.Root,
            SuperMetroid.AssetExtraction.Enemies.EnemyTileArtworkFiles.Load(temporary.Root, null)));
    }

    /// <summary>Checks corpse row offsets and rot/page transfer geometry against native source and destination words.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for stream-3 inline tables")]
    private static void VerifyStream3CorpseGeometry(ISnesAddressSpace rom)
    {
        for (int row = 0; row < 8; row++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9e262 + row * 2), MotherBrainCorpseArtworkDefinitionsConstants.TileRowOffset(row),
                "stream 3 corpse native tile row offset");
        var transfers = MotherBrainCorpseArtworkDefinitions.RotTransfers;
        AssertEqual(6, transfers.Count, "stream 3 corpse transfer count");
        var enumerated = transfers.ToArray();
        for (int row = 0; row < 6; row++)
        {
            int entry = 0xa9e1f4 + row * 8;
            var expected = new MotherBrainSpriteTileTransferRequest((ushort)row, CartridgeImportSource.Require(rom).ReadCartridgeWord(entry),
                (uint)(int)new SnesAddress(SnesLittleEndian.HighByte(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord(entry + 4)), CartridgeImportSource.Require(rom).ReadCartridgeWord(entry + 6));
            AssertEqual(expected, transfers[row], "stream 3 native corpse rot transfer");
            AssertEqual(expected, enumerated[row], "stream 3 corpse transfer enumeration order");
            int pageEntry = 0xa99003 + row * 7;
            uint source = (uint)(CartridgeImportSource.Require(rom).ReadCartridgeLong(pageEntry + 2));
            AssertEqual(source, MotherBrainCorpseArtworkDefinitions.VramPageSource(row), "stream 3 corpse source page");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(pageEntry + 5), MotherBrainCorpseArtworkDefinitions.VramPageDestination(row), "stream 3 corpse destination page");
        }
        int[] minimumY = [16, 8, 0, 0, 0, 8, 32]; // CMP/BCC gates in native $A9:EA40/$EB0B.
        for (int column = 0; column < 7; column++)
            AssertEqual(minimumY[column], MotherBrainCorpseArtworkDefinitions.ColumnMinimumY(column), "stream 3 corpse native outline gate");
        // Confirm the changed row/column geometry through the real pixel-copy path.
        for (ushort y = 0; y < 48; y++)
        foreach (bool move in new[] { false, true })
        {
            // The extraction stages the corpse from its art and rots it to picture each step (#1392): stage a pattern
            // through art whose rows hold it.
            byte[] expectedTiles = new byte[0x600];
            for (int offset = 0; offset < expectedTiles.Length; offset++)
                expectedTiles[offset] = (byte)(offset * 37 + 11);
            byte[] atlas = new byte[MotherBrainCorpseArtworkDefinitions.ByteCount];
            for (int row = 0; row < MotherBrainCorpseArtworkDefinitions.RowCount; row++)
                expectedTiles.AsSpan(row * MotherBrainCorpseArtworkDefinitions.RowBytes, MotherBrainCorpseArtworkDefinitions.InitialCopyLength(row))
                    .CopyTo(atlas.AsSpan(MotherBrainCorpseArtworkDefinitions.InitialCopySource(row) - MotherBrainCorpseArtworkDefinitions.SourceAddress));
            CorpseStaging actualTiles = CorpseStaging.MotherBrain(atlas);
            int source = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9e262 + (y >> 3) * 2) + (y & 7) * 2;
            int destination = source + ((y & 7) < 6 ? 0 : 0xd4);
            for (int column = 0; column < 7; column++)
            {
                if (y < minimumY[column]) continue;
                int start = column * 32;
                if (y < 46)
                {
                    // Read both plane words before copying or clearing either source.
                    byte lo = expectedTiles[start + source];
                    byte hi = expectedTiles[start + source + 1];
                    byte lo2 = expectedTiles[start + source + 16];
                    byte hi2 = expectedTiles[start + source + 17];
                    expectedTiles[start + destination + 2] = lo;
                    expectedTiles[start + destination + 3] = hi;
                    expectedTiles[start + destination + 18] = lo2;
                    expectedTiles[start + destination + 19] = hi2;
                }
                if (move)
                {
                    expectedTiles[start + source] = 0;
                    expectedTiles[start + source + 1] = 0;
                    expectedTiles[start + source + 16] = 0;
                    expectedTiles[start + source + 17] = 0;
                }
            }
            actualTiles.MoveRow(CorpseRowGeometry.MotherBrain, y, move ? CorpseRowOperation.Move : CorpseRowOperation.Copy);
            // The movers never write below the sixth row, so the staged rows hold every effect.
            for (int row = 0; row < MotherBrainCorpseArtworkDefinitions.RowCount; row++)
            {
                int first = row * MotherBrainCorpseArtworkDefinitions.RowBytes;
                byte[] staged = actualTiles.Upload(new CorpseRotRow(row, MotherBrainCorpseArtworkDefinitions.StagingAddress + first,
                    MotherBrainCorpseArtworkDefinitions.InitialCopyLength(row), 0));
                for (int offset = 0; offset < staged.Length; offset++)
                    AssertEqual(expectedTiles[first + offset], staged[offset], "stream 3 corpse row geometry");
            }
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainCorpseArtworkDefinitions.VramPageSource(invalid), "stream 3 corpse source bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainCorpseArtworkDefinitions.VramPageDestination(invalid), "stream 3 corpse destination bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = transfers[invalid], "stream 3 corpse transfer bounds");
        }
        Suite(nameof(VerifyMotherBrainCorpseStockArtwork), () => VerifyMotherBrainCorpseStockArtwork());
    }

    /// <summary>Checks Shitroid pulse and Baby cutscene colors against ROM while exercising palette catalog edits.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Shitroid pulse")]
    internal static void VerifyStream3ShitroidPulse(ISnesAddressSpace rom)
    {
        PaletteRgb5 Rgb(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        ushort[] Words(int address, int count) => Enumerable.Range(0, count).Select(i => CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2 * i))).ToArray();
        var normal = Enumerable.Range(0, 8).Select(frame => Words(0xa9f6d1 + 8 * frame, 4)).ToArray();
        var sidehopper = Words(0xa9f8c6, 16);
        var shitroid = Words(0xa9f8e6, 16);
        var dead = Words(0xa9f8a6, 16);
        var rows = normal.Select(row => row.Select(Rgb).ToArray()).ToArray();
        var document = new ShitroidColorDocument
        {
            Version = 1, Normal = rows, Sidehopper = sidehopper.Select(Rgb).ToArray(),
            Shitroid = shitroid.Select(Rgb).ToArray(), DeadSidehopper = dead.Select(Rgb).ToArray(),
        };
        ShitroidColorCatalog Load() => ShitroidColorCatalog.Load(new MemoryStream(ShitroidColorCatalog.Write(document)));
        var babyDocument = new BabyMetroidCutsceneColorDocument
        {
            Version = 1,
            Initial = Words(0xa994d4, 15).Select(Rgb).ToArray(),
            Fade = Enumerable.Range(0, 6).Select(frame => Words(0xade90c + frame * 28, 14).Select(Rgb).ToArray()).ToArray(),
        };
        BabyMetroidCutsceneColorCatalog LoadBaby() => BabyMetroidCutsceneColorCatalog.Load(
            new MemoryStream(BabyMetroidCutsceneColorCatalog.Write(babyDocument)));
        var baby = LoadBaby();
        void Check(ShitroidColorCatalog colors)
        {
            for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 4; color++)
                AssertEqual(normal[frame][color], colors.NormalColor(frame, color), "stream 3 Shitroid pulse RGB5");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(shitroid[color], colors.TargetColor(ShitroidColorTarget.Shitroid, color), "stream 3 complete Shitroid target palette");
                AssertEqual(sidehopper[color], colors.TargetColor(ShitroidColorTarget.Sidehopper, color), "stream 3 independent Sidehopper target");
                AssertEqual(dead[color], colors.TargetColor(ShitroidColorTarget.DeadSidehopper, color), "stream 3 independent corpse target");
            }
            string identity = SelectedPresentationHash.Create("ShitroidColorCatalog-v1", content =>
            {
                content.AppendWords("sidehopper", sidehopper);
                content.AppendWords("shitroid", shitroid);
                content.AppendWords("deadSidehopper", dead);
                content.AppendWordFrames("normal", normal);
            });
            AssertEqual(identity, colors.ContentIdentity, "stream 3 Shitroid pulse original identity framing");
        }
        var stock = Load();
        Check(stock);
        var selectedSidehopper = stock.sidehopper;
        AssertEqual(0, selectedSidehopper.independentOrSupplied.Length,
            "stream 3 complete standalone Sidehopper target keeps no stock color array");
        var selectedCorpse = stock.deadSidehopper;
        AssertEqual(0, selectedCorpse.independentOrSupplied.Length,
            "stream 3 complete standalone corpse target keeps no stock color array");
        var pulse = stock.normal;
        AssertTrue(pulse.supplied is null, "stream 3 Shitroid original pulse rows discarded");
        AssertTrue(!MetadataShape.Fields(pulse.GetType(), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Any(field => field.FieldType == typeof(uint[]) || field.FieldType == typeof(ushort) || field.FieldType == typeof(Bgr555)),
            "stream 3 Shitroid pulse owns no duplicate paint origins or floor words");
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 4; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = normal[frame][color];
            normal[frame][color] ^= (ushort)(1 << (5 * channel));
            rows[frame][color] = Rgb(normal[frame][color]);
            Check(Load());
            normal[frame][color] = original;
            rows[frame][color] = Rgb(original);
        }
        for (int color = 0; color < 15; color++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa994d4 + color * 2)), stock.TargetColor(ShitroidColorTarget.Shitroid, color + 1),
                "stream 3 native Baby/Shitroid palette duplication");
        for (int color = 0; color < 16; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = shitroid[color];
            shitroid[color] ^= (ushort)(1 << (5 * channel));
            document.Shitroid[color] = Rgb(shitroid[color]);
            Check(Load());
            for (int other = 0; other < 15; other++)
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa994d4 + other * 2)), baby.InitialColor(other), "stream 3 target edits retain independent Baby colors");
            shitroid[color] = original;
            document.Shitroid[color] = Rgb(original);
            if (color == 0) continue;
            babyDocument.Initial[color - 1] = Rgb((ushort)(original ^ 1 << (5 * channel)));
            AssertEqual((ushort)(original ^ 1 << (5 * channel)), LoadBaby().InitialColor(color - 1), "stream 3 shared resolver preserves Baby edit");
            Check(stock);
            babyDocument.Initial[color - 1] = Rgb(original);
        }
        for (int color = 0; color < 16; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = sidehopper[color];
            sidehopper[color] ^= (ushort)(1 << (5 * channel));
            document.Sidehopper[color] = Rgb(sidehopper[color]);
            Check(Load());
            AssertEqual(original, stock.TargetColor(ShitroidColorTarget.Sidehopper, color),
                "stream 3 Sidehopper supplied changes preserve independent catalog instances");
            sidehopper[color] = original;
            document.Sidehopper[color] = Rgb(original);
        }
        for (int color = 0; color < 16; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = dead[color];
            dead[color] ^= (ushort)(1 << (5 * channel));
            document.DeadSidehopper[color] = Rgb(dead[color]);
            Check(Load());
            AssertEqual(original, stock.TargetColor(ShitroidColorTarget.DeadSidehopper, color),
                "stream 3 corpse edits preserve independent catalog instances");
            dead[color] = original;
            document.DeadSidehopper[color] = Rgb(original);
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TargetColor(ShitroidColorTarget.Shitroid, invalid), "stream 3 Shitroid target bounds");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(invalid, 0), "stream 3 Shitroid pulse frame bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(0, invalid), "stream 3 Shitroid pulse color bounds");
    }

    /// <summary>Checks rainbow, drain, fake-death, and revival palette frames against their native rows and edited colors.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for drain fades")]
    internal static void VerifyStream3DrainFades(ISnesAddressSpace rom)
    {
        PaletteRgb5 Color(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        ushort Word(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);

        MotherBrainRainbowPaletteFrameDocument ReadFull(int bodySource, int legSource) => new()
        {
            Body = Enumerable.Range(0, 15).Select(color => Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((bodySource + 2 * color)))).ToArray(),
            BackLegs = Enumerable.Range(0, 15).Select(color => Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((legSource + 2 * color)))).ToArray(),
        };
        var rainbow = Enumerable.Range(0, 10).Select(frame =>
        {
            int source = 0xad0000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xade434 + 2 * frame));
            return ReadFull(source, source + 30);
        }).ToArray();
        var drain = new MotherBrainRainbowPaletteFrameDocument[8];
        var fake = new PaletteRgb5[8][];
        var revival = new MotherBrainRainbowPaletteFrameDocument[8];
        for (int frame = 0; frame < 8; frame++)
        {
            int source = 0xad0000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xadef87 + 2 * frame));
            drain[frame] = new()
            {
                Body = Enumerable.Range(0, 15).Select(color => Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((source + 2 * color)))).ToArray(),
                BackLegs = Enumerable.Range(0, 5).Select(color => Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((source + 30 + 2 * color)))).ToArray(),
                TrailingColor = Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((source + 40))),
            };
            int revivalSource = 0xad0000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaded9c + 2 * frame));
            revival[frame] = new()
            {
                Body = Enumerable.Range(0, 13).Select(color => Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((revivalSource + 2 * color)))).ToArray(),
                BackLegs = Enumerable.Range(0, 5).Select(color => Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((revivalSource + 26 + 2 * color)))).ToArray(),
                TrailingColor = Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((revivalSource + 36))),
            };
            int fakeSource = 0xad0000 | CartridgeImportSource.Require(rom).ReadCartridgeWord((0xaded8a + 2 * frame));
            fake[frame] = Enumerable.Range(0, 3).Select(color => Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((fakeSource + 2 * color)))).ToArray();
        }
        var document = new MotherBrainRainbowPaletteDocument
        {
            Version = 3, Rainbow = rainbow,
            ToGrey = drain, FromGrey = revival,
            FakeDeathToGrey = fake, Normal = ReadFull(0xa99474, 0xa99494), BeamInitial = Color(0x3ce0),
            BeamCycle = Enumerable.Range(0, 38).Select(index => Color(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x88e833 + 4 * index)))).ToArray(),
        };
        MotherBrainRainbowPalettePresentation Load(MotherBrainRainbowPaletteDocument value,
            MotherBrainRainbowPalettePresentation? stock = null) => MotherBrainRainbowPalettePresentation.Load(
                new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, ArtworkJson.Options)), stock);
        void Check(MotherBrainRainbowPalettePresentation palette)
        {
            var cgram = new SnesCgram();
            var bus = new TestAddressSpace();
            for (int frame = 0; frame < 8; frame++)
            {
                palette.ApplyToGrey(cgram, frame);
                for (int color = 0; color < 15; color++)
                {
                    AssertEqual(Word(drain[frame].Body[color]), cgram.Colors[0x41 + color], "stream 3 drain body RGB5");
                    AssertEqual(Word(drain[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 drain brain RGB5");
                }
                for (int color = 0; color < 5; color++)
                    AssertEqual(Word(drain[frame].BackLegs[color]), cgram.Colors[180 + color], "stream 3 drain legs RGB5");
                palette.ApplyFakeDeathToGrey(cgram, frame);
                for (int color = 0; color < 3; color++)
                    AssertEqual(Word(fake[frame][color]), cgram.Colors[0x91 + color], "stream 3 fake-death brain RGB5");
            }
        }
        var stock = Load(document);
        void CheckBeam(MotherBrainRainbowPalettePresentation palette)
        {
            for (int index = 0; index < 38; index++)
                AssertTrue(palette.TryReadBeamColor(index * 4, out Bgr555 beam) && beam == Bgr555.FromWord(Word(document.BeamCycle[index])),
                    "stream 3 beam wheel matches every native sampled color");
            AssertTrue(!palette.TryReadBeamColor(152, out _), "stream 3 beam signed loop terminator");
            AssertEqual(0x3ce0, palette.BeamInitialColor, "stream 3 beam initial fixed color unchanged");
        }
        CheckBeam(stock);
        var beamColors = stock.beamCycle;
        AssertTrue(beamColors.supplied is null,
            "stream 3 beam wheel retains no stock lookup row");
        for (int index = 0; index < 38; index++)
        for (int channel = 0; channel < 3; channel++)
        {
            PaletteRgb5 original = document.BeamCycle[index];
            document.BeamCycle[index] = Color((ushort)(Word(original) ^ 1 << (5 * channel)));
            CheckBeam(Load(document));
            document.BeamCycle[index] = original;
        }
        foreach (int invalid in new[] { -1, 1, 2, 3, 153, 156, int.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.TryReadBeamColor(invalid, out _), "stream 3 beam cursor bounds and alignment");
        Check(stock);
        void CheckRainbow(MotherBrainRainbowPalettePresentation palette)
        {
            var cgram = new SnesCgram();
            for (int frame = 0; frame <= 10; frame++)
            {
                var expected = frame == 10 ? document.Normal : rainbow[frame];
                if (frame == 10) palette.ApplyNormal(cgram);
                else palette.ApplyRainbow(cgram, frame);
                for (int color = 0; color < 15; color++)
                {
                    AssertEqual(Word(expected.Body[color]), cgram.Colors[0x41 + color], "stream 3 rainbow body");
                    AssertEqual(Word(expected.Body[color]), cgram.Colors[0x91 + color], "stream 3 rainbow brain");
                    AssertEqual(Word(expected.BackLegs[color]), cgram.Colors[0xb1 + color], "stream 3 rainbow shadow");
                }
            }
        }
        void CheckRevival(MotherBrainRainbowPalettePresentation palette)
        {
            var bus = new TestAddressSpace();
            var cgram = new SnesCgram();
            for (int frame = 0; frame < 8; frame++)
            {
                cgram.SetColor(0x4e, new Bgr555(27, 3, 0));
                cgram.SetColor(0x9e, new Bgr555(27, 3, 0));
                palette.ApplyFromGrey(cgram, frame);
                for (int color = 0; color < 13; color++)
                {
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x41 + color], "stream 3 revival body");
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 revival brain");
                }
                AssertEqual(123, cgram.Colors[0x4e], "stream 3 revival preserves body tail");
                AssertEqual(123, cgram.Colors[0x9e], "stream 3 revival preserves brain tail");
                for (int color = 0; color < 5; color++)
                    AssertEqual(Word(revival[frame].BackLegs[color]), cgram.Colors[180 + color], "stream 3 revival legs");
                palette.ApplyFakeDeathFromGrey(cgram, frame);
                for (int color = 0; color < 3; color++)
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 fake-death revival");
            }
        }
        CheckRevival(stock);
        var revivalFade = stock.fromGrey;
        var revivalOverrides = revivalFade.suppliedOverrides;
        AssertEqual(0, revivalOverrides.Count, "revival calculates all words with one reviewed tissue-shade hold");
        AssertEqual(Word(revival[1].Body[10]), Word(revival[2].Body[10]),
            "native revival EE80 repeats EEA6 without an independently stored paint word");
        foreach (var endpoint in new[] { revivalFade.first, revivalFade.last })
        {
            AssertTrue(endpoint.Body.supplied is null,
                "revival endpoint body reuses reviewed health/drained paint calculations");
            AssertTrue(endpoint.backLegs is null,
                "revival rear endpoint calculates from reviewed normal/half-shadow paint");
        }
        // The trailing color keeps a backing field only for an independently edited endpoint; each revival endpoint must
        // take the shared branch, reading its trailing ink from the rainbow rear source or its own drained or normal rear.
        foreach (var endpoint in new[] { revivalFade.first, revivalFade.last })
        {
            AssertTrue(endpoint.rearSource is not null || endpoint.drainedRear || endpoint.normalRearSubset,
                "revival trailing endpoint shares its calculated rear highlight");
        }
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 19; color++)
        for (int component = 0; component < 3; component++)
        {
            var row = revival[frame];
            PaletteRgb5 original = (color < 13 ? row.Body[color] : color < 18 ? row.BackLegs[color - 13] : row.TrailingColor ?? throw new InvalidOperationException("Verification requires color < 13 ? row.Body[color] : color < 18 ? row.BackLegs[color - 13] : row.TrailingColor."));
            void Set(PaletteRgb5 value)
            {
                if (color < 13) row.Body[color] = value;
                else if (color < 18) row.BackLegs[color - 13] = value;
                else revival[frame] = row with { TrailingColor = value };
            }
            Set(Color((ushort)(Word((original ?? throw new InvalidOperationException("Verification requires original."))) ^ 1 << (5 * component))));
            CheckRevival(Load(document));
            Set(original);
        }
        CheckRainbow(stock);
        var fakeFade = stock.fakeDeathToGrey;
        foreach (var endpoint in new[] { fakeFade.first, fakeFade.last })
            AssertTrue(endpoint.Body.supplied is null,
                "stream 3 fake-death endpoints reuse approved normal/drained cortex paint without stored rows");
        var drainFade = stock.toGrey;
        var drainEnd = drainFade.last;
        var drainBody = drainEnd.Body;
        var drainedPaint = drainBody.drained;
        AssertTrue((drainedPaint ?? throw new InvalidOperationException("Verification requires drainedPaint.")).Calculated, "stream 3 drained final gray body palette has no stored row");
        var normalFrame = stock.normal;
        var normalBody = normalFrame.Body;
        AssertTrue(normalBody.supplied is null,
            "stream 3 normal body calculates from shared health paint without a stored row");
        AssertTrue(normalFrame.backLegs is null,
            "stream 3 normal rear palette calculates from shared health lighting without a stored row");
        var storedRainbow = stock.rainbow;
        MotherBrainRainbowPalettePresentation.BodyColors RainbowBody(int phase) => storedRainbow[phase].Body;
        if (RainbowBody(0).redOrigin is not { } redBasis)
            throw new InvalidOperationException("native rainbow actually selects calculated red-origin shades");
        int independentChannels = 0;
        foreach (int phase in new[] { 1, 2, 5, 6, 7 })
        {
            if (RainbowBody(phase).sharedChannels is not { } sharing)
                throw new InvalidOperationException($"rainbow phase {phase} actually uses shared channels");
            independentChannels += sharing.independent.inputs.Length;
        }
        AssertEqual(28, independentChannels, "remaining independently supplied channels outside red-origin basis");
        foreach (int phase in new[] { 3, 4, 8, 9 })
            AssertTrue(RainbowBody(phase).tintSource is not null,
                $"rainbow phase {phase} actually uses exact whole-palette tint");
        var drainStart = drainFade.first;
        AssertTrue(ReferenceEquals(drainStart.Body, RainbowBody(6)),
            "stream 3 drain body starts from the exact matching rainbow phase-six source");
        AssertTrue(ReferenceEquals(drainStart.rearSource, storedRainbow[6]),
            "stream 3 drain rear and trailing start share only matching rainbow content");
        AssertTrue(drainEnd.backLegs is null,
            "stream 3 drained rear endpoint calculates from approved normal rear lighting");
        AssertTrue((drainEnd.drainedRear || drainEnd.normalRearSubset) && drainEnd.TrailingColor == drainEnd.Leg(1),
            "stream 3 drained trailing word reuses its rear highlight without a stored value");
        foreach (var frame in storedRainbow)
            AssertTrue(frame.backLegs is null, "stream 3 rainbow shadow tables discarded");
        for (int frame = 0; frame <= 10; frame++)
        for (int color = 0; color < 30; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var selected = frame == 10 ? document.Normal : rainbow[frame];
            PaletteRgb5[] row = color < 15 ? selected.Body : selected.BackLegs;
            int index = color % 15;
            PaletteRgb5 original = row[index];
            row[index] = Color((ushort)(Word(original) ^ 1 << (5 * channel)));
            var editedPalette = Load(document);
            CheckRainbow(editedPalette);
            if (frame == 6) Check(editedPalette);
            if (frame == 10) CheckRevival(editedPalette);
            row[index] = original;
        }
        foreach (int invalid in new[] { -1, 10, int.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.ApplyRainbow(new SnesCgram(), invalid), "stream 3 rainbow bounds");
        foreach (var fade in new[] { stock.toGrey, stock.fakeDeathToGrey })
            AssertTrue(fade.supplied is null, "stream 3 drain stock intermediate rows discarded");
        Check(Load(document with { Version = 2, FakeDeathToGrey = null }, stock));
        // Independently edited channels must still be delivered exactly, including endpoints and WRAM.
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 24; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var row = drain[frame];
            PaletteRgb5 original = color < 15 ? row.Body[color] : color < 20 ? row.BackLegs[color - 15]
                : color == 20 ? row.TrailingColor ?? throw new InvalidOperationException("Verification requires the drain frame's trailing color.")
                : fake[frame][color - 21];
            void Set(PaletteRgb5 value)
            {
                if (color < 15) row.Body[color] = value;
                else if (color < 20) row.BackLegs[color - 15] = value;
                else if (color == 20) drain[frame] = row with { TrailingColor = value };
                else fake[frame][color - 21] = value;
            }
            Set(Color((ushort)(Word((original ?? throw new InvalidOperationException("Verification requires original."))) ^ 1 << (5 * channel))));
            var editedPalette = Load(document);
            Check(editedPalette);
            if (frame == 0 && color < 21) CheckRainbow(editedPalette);
            Set(original);
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => stock.ApplyToGrey(new SnesCgram(), invalid), "stream 3 drain bounds");
            AssertThrows<InvalidDataException>(() => stock.ApplyFakeDeathToGrey(new SnesCgram(), invalid), "stream 3 fake-death bounds");
        }
    }

    /// <summary>Checks the Baby Metroid initial and six fade palettes against ROM and independently edited colors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Baby fade")]
    internal static void VerifyStream3BabyFade(ISnesAddressSpace rom)
    {
        var initial = new ushort[15];
        var fade = new ushort[6][];
        for (int color = 0; color < initial.Length; color++)
            initial[color] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa994d4 + 2 * color));
        for (int frame = 0; frame < fade.Length; frame++)
        {
            fade[frame] = new ushort[14];
            for (int color = 0; color < fade[frame].Length; color++)
                fade[frame][color] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xade90c + 28 * frame + 2 * color));
        }
        static PaletteRgb5 Rgb(ushort value) => new()
        {
            Red = value & 31, Green = (value >> 5) & 31, Blue = (value >> 10) & 31,
        };
        BabyMetroidCutsceneColorCatalog Load() => BabyMetroidCutsceneColorCatalog.Load(new MemoryStream(
            BabyMetroidCutsceneColorCatalog.Write(new BabyMetroidCutsceneColorDocument
            {
                Version = 1,
                Initial = initial.Select(Rgb).ToArray(),
                Fade = fade.Select(row => row.Select(Rgb).ToArray()).ToArray(),
            }), writable: false));
        void Check(BabyMetroidCutsceneColorCatalog catalog)
        {
            for (int color = 0; color < initial.Length; color++)
                AssertEqual(initial[color], catalog.InitialColor(color), "stream 3 Baby initial color");
            for (int frame = 0; frame < fade.Length; frame++)
            for (int color = 0; color < fade[frame].Length; color++)
                AssertEqual(fade[frame][color], catalog.FadeColor(frame + 1, color), "stream 3 Baby fade selected color");
            string identity = SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
            {
                content.AppendWords("initial", initial);
                content.AppendWordFrames("fade", fade);
            });
            AssertEqual(identity, catalog.ContentIdentity, "stream 3 Baby fade selected identity");
        }
        var stock = Load();
        Check(stock);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var selectedInitial = stock.initial;
        AssertTrue(selectedInitial.supplied is null,
            "stream 3 Baby initial calculates from reviewed paints without a stock color array");
        for (int color = 0; color < initial.Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = initial[color];
            initial[color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            initial[color] = original;
        }
        var selectedFade = stock.fade;
        AssertTrue(selectedFade.supplied is null,
            "stream 3 original Baby fade discards its stored frame table");
        AssertEqual(0, MetadataShape.Fields(selectedFade.GetType(), fields).Count(field => field.FieldType == typeof(ushort) || field.FieldType == typeof(ushort[]) || field.FieldType == typeof(Bgr555) || field.FieldType == typeof(Bgr555[])), "stream 3 Baby fade keeps no stock endpoint payload");
        static ushort ScaleHealth(ushort value, int remaining)
        {
            int result = 0;
            for (int channel = 0; channel < 3; channel++)
                result |= (((value >> (5 * channel) & 31) * remaining / 7) << (5 * channel));
            return (ushort)result;
        }
        for (int color = 0; color < 14; color++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xade8f0 + 2 * color)), ScaleHealth((BabyMetroidCutsceneColorCatalog.ColorFade.Endpoint(color)).ToWord(), 6),
                "stream 3 reviewed final-health endpoint reproduces original undisplayed fade step");
            int healthAddress = color < 4 ? 0xade870 + 2 * color :
                color < 9 ? 0xade8d8 + 2 * (color - 4) : 0xade878 + 2 * (color - 9);
            ushort nativeHealth = CartridgeImportSource.Require(rom).ReadCartridgeWord(healthAddress);
            AssertEqual(nativeHealth, BabyMetroidCutsceneColorCatalog.ColorFade.Endpoint(color), "stream 3 exact native final-health endpoint");
            for (int frame = 0; frame < fade.Length; frame++)
                AssertEqual(fade[frame][color], ScaleHealth(nativeHealth, 5 - frame),
                    "stream 3 native final health colors produce the exact seven-part fade");
        }
        for (int frame = 0; frame < fade.Length; frame++)
        for (int color = 0; color < fade[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = fade[frame][color];
            fade[frame][color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            fade[frame][color] = original;
        }
        foreach (int invalid in new[] { -1, 0, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.FadeColor(invalid, 0), "stream 3 Baby fade frame bounds");
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.FadeColor(1, invalid), "stream 3 Baby fade color bounds");
    }

    /// <summary>Checks Mother Brain body, leg, and corpse fade frames against the native color sequences.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain fades")]
    internal static void VerifyStream3MotherBrainFades(ISnesAddressSpace rom)
    {
        var body = new ushort[16][];
        var leg = new ushort[16][];
        var corpse = new ushort[8][];
        for (int frame = 0; frame < 16; frame++)
        {
            body[frame] = new ushort[14];
            leg[frame] = new ushort[14];
            for (int color = 0; color < 14; color++)
            {
                body[frame][color] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xadea0a + 56 * frame + 2 * color));
                leg[frame][color] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xadea26 + 56 * frame + 2 * color));
            }
        }
        for (int frame = 0; frame < 8; frame++)
        {
            corpse[frame] = new ushort[15];
            for (int color = 0; color < 15; color++)
                corpse[frame][color] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xadf119 + 30 * frame + 2 * color));
        }
        var door = new ushort[14];
        for (int color = 0; color < door.Length; color++)
            door[color] = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa99534 + 2 * color));

        static PaletteRgb5 Rgb(ushort value) => new()
        {
            Red = value & 31, Green = (value >> 5) & 31, Blue = (value >> 10) & 31,
        };
        MotherBrainDeathColorCatalog Load() => MotherBrainDeathColorCatalog.Load(new MemoryStream(
            MotherBrainDeathColorCatalog.Write(new MotherBrainDeathColorDocument
            {
                Version = 1,
                BodyFade = body.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                LegFade = leg.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                CorpseFade = corpse.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                ExplodedDoor = door.Select(Rgb).ToArray(),
            }), writable: false));
        void Check(MotherBrainDeathColorCatalog catalog)
        {
            for (int frame = 0; frame < 16; frame++)
            for (int color = 0; color < 14; color++)
            {
                AssertEqual(body[frame][color], catalog.BodyColor(frame, color), "stream 3 selected body fade color");
                AssertEqual(leg[frame][color], catalog.LegColor(frame, color), "stream 3 selected leg fade color");
            }
            for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 15; color++)
                AssertEqual(corpse[frame][color], catalog.CorpseColor(frame, color), "stream 3 selected corpse fade color");
            for (int color = 0; color < door.Length; color++)
                AssertEqual(door[color], catalog.ExplodedDoorColor(color), "stream 3 unchanged door color");
            string identity = SelectedPresentationHash.Create("MotherBrainDeathColorCatalog-v1", content =>
            {
                content.AppendWords("explodedDoor", door);
                content.AppendWordFrames("bodyFade", body);
                content.AppendWordFrames("legFade", leg);
                content.AppendWordFrames("corpseFade", corpse);
            });
            AssertEqual(identity, catalog.ContentIdentity, "stream 3 death fade identity keeps exact selected values");
        }
        var stock = Load();
        Check(stock);
        AssertTrue(stock.explodedDoor is null,
            "stream 3 exploded-door stock paint keeps no stored word array");
        for (int color = 0; color < door.Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = door[color];
            door[color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            door[color] = original;
        }
        Check(stock);
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.ExplodedDoorColor(invalid), "stream 3 exploded-door color bounds");
        foreach (var fade in new[] { stock.bodyFade, stock.legFade, stock.corpseFade })
        {
            AssertTrue(fade.first is null,
                "stream 3 death starts reuse calculated health state three without stored endpoint rows");
            AssertTrue(fade.supplied is null,
                "stream 3 original death fade discards its stored frame table");
            if (ReferenceEquals(fade, stock.corpseFade))
            {
                var endpoint = fade.last ?? throw new InvalidOperationException("stream 3 corpse fade has a final gray endpoint");
                AssertTrue(endpoint.Calculated, "stream 3 corpse final gray palette has no stored row");
                AssertEqual(0, endpoint[14], "stream 3 corpse neutral black endpoint");
            }
        }
        foreach (ushort[][] frames in new[] { body, leg, corpse })
        for (int frame = 0; frame < frames.Length; frame++)
        for (int color = 0; color < frames[frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = frames[frame][color];
            frames[frame][color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            frames[frame][color] = original;
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.BodyColor(invalid, 0), "stream 3 body fade frame bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.LegColor(invalid, 0), "stream 3 leg fade frame bounds");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CorpseColor(invalid, 0), "stream 3 corpse fade frame bounds");
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => stock.BodyColor(0, invalid), "stream 3 body fade color bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => stock.LegColor(0, invalid), "stream 3 leg fade color bounds");
        }
        foreach (int invalid in new[] { -1, 15, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.CorpseColor(0, invalid), "stream 3 corpse fade color bounds");
    }

    /// <summary>Checks Choot falling patterns, loop distances, and separated mechanics/presentation operands.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Choot control")]
    private static void VerifyStream3ChootControl(ISnesAddressSpace rom)
    {
        for (ushort pattern = 0; pattern < 5; pattern++)
        {
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2df5e + 2 * pattern));
            ushort distancePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2df6a + 2 * pattern));
            AssertEqual(new ChootPatternDefinition((ChootFallingPath)pointer, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | distancePointer))),
                ChootPatternDefinitions.ForIndex(pattern), "stream 3 Choot pattern identity and loop advance");
        }
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => ChootPatternDefinitions.ForIndex(invalid),
                "stream 3 Choot rejects alias and invalid patterns");
    }

    /// <summary>Checks pickup and death animation dispatch plus Fireflea instruction words against native tables.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for pickup and Fireflea programs")]
    private static void VerifyStream3PickupAndFirefleaPrograms(ISnesAddressSpace rom)
    {
        for (int kind = 0; kind < 6; kind++)
        {
            EnemyPickupAnimationDefinition animation = EnemyPickupDefinitions.Animation((EnemyPickupKind)kind);
            AssertEqual((ushort)(2 * kind), animation.TableOffset, "stream 3 pickup kind table offset");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86ef04 + 2 * kind)),
                animation.Program is { } program ? EnemyDeathProjectileNativeListAddresses.Entry(program) : (ushort)0, "stream 3 pickup kind dispatch");
        }
        for (ushort animation = 0; animation < 5; animation++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86efd5 + 2 * animation)),
                EnemyDeathProjectileNativeListAddresses.Entry(EnemyDeathExplosionDefinitions.Program(animation)), "stream 3 death variant dispatch");
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => EnemyPickupDefinitions.Animation((EnemyPickupKind)invalid),
                "stream 3 invalid pickup kind");
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EnemyDeathExplosionDefinitions.Program(invalid),
                "stream 3 invalid death variant");
    }

    /// <summary>Checks six four-color Work Robot palette frames against ROM and edited RGB5 source colors.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Work Robot colors")]
    private static void VerifyStream3WorkRobotColors(ISnesAddressSpace rom)
    {
        var words = new ushort[6][];
        var colors = new PaletteRgb5[6][];
        for (int frame = 0; frame < 6; frame++)
        {
            words[frame] = new ushort[4];
            colors[frame] = new PaletteRgb5[4];
            for (int color = 0; color < 4; color++)
            {
                int address = 0xa8ccc1 + 10 * frame + 2 * color;
                ushort value = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                words[frame][color] = value;
                colors[frame][color] = new PaletteRgb5
                {
                    Red = value & 31,
                    Green = (value >> 5) & 31,
                    Blue = (value >> 10) & 31,
                };
            }
        }
        var document = new WorkRobotPaletteCycleDocument { Version = 1, Frames = colors };
        WorkRobotPaletteCycle Load() => WorkRobotPaletteCycle.Load(
            new MemoryStream(WorkRobotPaletteCycle.Write(document), writable: false));
        void Check(WorkRobotPaletteCycle cycle)
        {
            var cgram = new SnesCgram();
            for (int frame = 0; frame < 6; frame++)
            {
                cycle.ApplyFrame(cgram, frame, 9);
                for (int color = 0; color < 4; color++)
                {
                    AssertEqual(words[frame][color], cycle.Resolve(frame, color),
                        "stream 3 Work Robot selected color");
                    AssertEqual(words[frame][color], cgram.Colors[9 + color],
                        "stream 3 Work Robot applied color");
                }
            }
            string expectedIdentity = SelectedPresentationHash.Create("WorkRobotPaletteCycle-v1",
                content => content.AppendWordFrames("frames", words));
            AssertEqual(expectedIdentity, cycle.ContentIdentity,
                "stream 3 Work Robot identity preserves original row framing");
        }
        var stock = Load();
        Check(stock);
        AssertTrue(stock.frames is null,
            "stream 3 original Work Robot colors discard their stored frame table");
        for (int frame = 0; frame < 6; frame++)
        for (int color = 0; color < 4; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            PaletteRgb5 original = colors[frame][color];
            ushort originalWord = words[frame][color];
            colors[frame][color] = channel switch
            {
                0 => original with { Red = original.Red ^ 1 },
                1 => original with { Green = original.Green ^ 1 },
                _ => original with { Blue = original.Blue ^ 1 },
            };
            words[frame][color] ^= (ushort)(1 << (channel * 5));
            Check(Load());
            colors[frame][color] = original;
            words[frame][color] = originalWord;
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(invalid, 0),
                "stream 3 Work Robot frame bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(0, invalid),
                "stream 3 Work Robot color bounds");
    }

    /// <summary>Checks Mother Brain's health tint from paint anchors without retaining native intermediate rows.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for health tint")]
    private static void VerifyStream3HealthTint(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainHealthPaletteExtractor.Extract(rom);
        var stock = MotherBrainHealthPalettePresentation.Load(new MemoryStream(json));
        var node = RequiredJson.Parse(json);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (var palette in new[] { stock.body, stock.backLegs })
        {
            AssertTrue(palette.supplied is null,
                "stream 3 native health tint has no stored intermediate rows");
            var basis = palette.basis;
            AssertTrue(basis.supplied is null,
                "stream 3 native health shade ramps calculated from paint endpoints");
            AssertEqual(6, MetadataShape.Fields(basis.GetType(), flags).Count(field => field.FieldType == typeof(Bgr555)),
                "stream 3 health base exposes only six possible paint anchors");
            Bgr555[] anchors = [basis.highlight, basis.midtone, basis.shadow, basis.brown];
            AssertTrue(ReferenceEquals(palette, stock.body) ? anchors.All(value => value != Bgr555.Black) : anchors.All(value => value == Bgr555.Black),
                "stream 3 stock rear palette stores no independent paint colors");
        }
        for (int state = 0; state < 4; state++)
        {
            var actual = new SnesCgram();
            stock.Apply(actual, state);
            foreach (var (table, destination) in new[]
            {
                new VerifyStream3HealthTintCase(MotherBrainHealthPaletteRomData.BrainTable, MotherBrainRainbowPaletteRomData.BodyColor),
                new VerifyStream3HealthTintCase(MotherBrainHealthPaletteRomData.BackLegTable, MotherBrainRainbowPaletteRomData.SecondaryColor),
            })
            {
                int pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(table + state * 2);
                for (int color = 0; color < 15; color++)
                {
                    int address = 0xad0000 | pointer + color * 2;
                    ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                    AssertEqual(expected, actual.Colors[destination + color], "stream 3 all native health tint words");
                }
            }
            for (int color = 0; color < 15; color++)
                AssertEqual(actual.Colors[MotherBrainRainbowPaletteRomData.BodyColor + color],
                    actual.Colors[MotherBrainRainbowPaletteRomData.BrainColor + color], "stream 3 health tint body/brain copies");
        }
        foreach (string group in new[] { "body", "backLegs" })
            for (int state = 0; state < 4; state++)
                for (int color = 0; color < 15; color++)
                    foreach (string component in new[] { "red", "green", "blue" })
                    {
                        var editedNode = node.DeepClone();
                        var rgb = editedNode.Required(group).Required(state).Required(color);
                        rgb[component] = rgb.Required(component).GetValue<int>() ^ 1;
                        var edited = MotherBrainHealthPalettePresentation.Load(new MemoryStream(
                            System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                        for (int checkState = 0; checkState < 4; checkState++)
                        {
                            var actual = new SnesCgram();
                            edited.Apply(actual, checkState);
                            foreach (var (checkGroup, destination) in new[]
                            {
                                new LookupStream3VerifyStream3HealthTintCase("body", MotherBrainRainbowPaletteRomData.BodyColor),
                                new LookupStream3VerifyStream3HealthTintCase("backLegs", MotherBrainRainbowPaletteRomData.SecondaryColor),
                            })
                                for (int checkColor = 0; checkColor < 15; checkColor++)
                                {
                                    var expectedRgb = editedNode.Required(checkGroup).Required(checkState).Required(checkColor);
                                    ushort expected = (ushort)(expectedRgb.Required("red").GetValue<int>() |
                                        expectedRgb.Required("green").GetValue<int>() << 5 |
                                        expectedRgb.Required("blue").GetValue<int>() << 10);
                                    AssertEqual(expected, actual.Colors[destination + checkColor],
                                        "stream 3 independent health palette edit and unaffected channels");
                                }
                        }
                    }
    }

    /// <summary>Checks recovery-light colors and room-palette routing against ROM and independent edits.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for recovery lights")]
    internal static void VerifyStream3RecoveryLights(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var node = RequiredJson.Parse(json);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var fade = stock.recoveryLights;
        AssertTrue(fade.supplied is null,
            "stream 3 stock recovery light rows discarded");
        AssertTrue(ReferenceEquals(fade.finalRoom,
            stock.finalRoom),
            "stream 3 recovery endpoint reuses the final room palette");
        AssertEqual(0, MetadataShape.Fields(fade.GetType(), flags).Count(field => field.FieldType == typeof(ushort) || field.FieldType == typeof(Bgr555)),
            "stream 3 reviewed recovery paints reside only in their domain catalog");
        for (int frame = 0; frame < 7; frame++)
        {
            var actual = new SnesCgram();
            stock.ApplyRecoveryLights(actual, frame);
            for (int color = 0; color < 28; color++)
            {
                int address = 0xadf3d3 - frame * 0x38 + color * 2;
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                int destination = color < 14 ? MotherBrainRoomColorRomData.RecoveryLightsFirstColor + color :
                    MotherBrainRoomColorRomData.RecoveryLightsSecondColor + color - 14;
                AssertEqual(expected, actual.Colors[destination], "stream 3 all native recovery light words");
            }
        }
        for (int frame = 0; frame < 7; frame++)
            for (int color = 0; color < 28; color++)
                foreach (string component in new[] { "red", "green", "blue" })
                {
                    var editedNode = node.DeepClone();
                    var rgb = editedNode.Required("recoveryLights").Required(frame).Required(color);
                    rgb[component] = rgb.Required(component).GetValue<int>() ^ 1;
                    var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                        System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                    for (int checkFrame = 0; checkFrame < 7; checkFrame++)
                    {
                        var actual = new SnesCgram();
                        edited.ApplyRecoveryLights(actual, checkFrame);
                        for (int checkColor = 0; checkColor < 28; checkColor++)
                        {
                            var expectedRgb = editedNode.Required("recoveryLights").Required(checkFrame).Required(checkColor);
                            ushort expected = (ushort)(expectedRgb.Required("red").GetValue<int>() |
                                expectedRgb.Required("green").GetValue<int>() << 5 |
                                expectedRgb.Required("blue").GetValue<int>() << 10);
                            int destination = checkColor < 14 ? MotherBrainRoomColorRomData.RecoveryLightsFirstColor + checkColor :
                                MotherBrainRoomColorRomData.RecoveryLightsSecondColor + checkColor - 14;
                            AssertEqual(expected, actual.Colors[destination], "stream 3 independent recovery light edit");
                        }
                    }
                }
        for (int color = 0; color < 24; color++)
            foreach (string component in new[] { "red", "green", "blue" })
            {
                var editedNode = node.DeepClone();
                var rgb = editedNode.Required("finalRoom").Required(color);
                rgb[component] = rgb.Required(component).GetValue<int>() ^ 1;
                var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                    System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                for (int frame = 0; frame < 7; frame++)
                {
                    var expected = new SnesCgram();
                    var actual = new SnesCgram();
                    stock.ApplyRecoveryLights(expected, frame);
                    edited.ApplyRecoveryLights(actual, frame);
                    AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                        "stream 3 independent final-room edit cannot change recovery content");
                    for (int phase = 0; phase < 2; phase++)
                    {
                        int flashFrame = frame * 2 + phase;
                        stock.ApplyFlash(expected, flashFrame);
                        edited.ApplyFlash(actual, flashFrame);
                        AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)),
                            "stream 3 independent final-room edit cannot change flash content");
                    }
                }
            }
        var legacy = node.DeepClone();
        legacy["version"] = MotherBrainRoomColorFormatConstants.PreRecoveryLightsVersion;
        legacy.AsObject().Remove("recoveryLights");
        var oldOverride = MotherBrainRoomColorPresentation.Load(new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(legacy.ToJsonString())), stock);
        for (int frame = 0; frame < 7; frame++)
        {
            var expected = new SnesCgram();
            var actual = new SnesCgram();
            stock.ApplyRecoveryLights(expected, frame);
            oldOverride.ApplyRecoveryLights(actual, frame);
            AssertTrue(new CgramPalettes(expected).ImageEquals(new CgramPalettes(actual)), "stream 3 legacy room override reuses calculated stock recovery");
        }
    }

    /// <summary>Checks timed room-flash colors and mirrors without storing native output rows.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for room flash")]
    internal static void VerifyStream3RoomFlash(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var node = RequiredJson.Parse(json);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var flash = stock.flash;
        AssertTrue(flash.supplied is null,
            "stream 3 stock flash rows discarded");
        AssertTrue(ReferenceEquals(flash.basis,
            stock.finalRoom),
            "stream 3 stock flash reuses final room paint basis");
        AssertEqual(0, MetadataShape.Fields(flash.GetType(), flags).Count(field => field.FieldType == typeof(ushort) || field.FieldType == typeof(Bgr555)),
            "stream 3 room flash keeps no per-instance selected paint");
        for (int frame = 0; frame < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount; frame++)
        {
            ushort entry = checked((ushort)(SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomFlashSourceDefinitions.FlashList + frame * MotherBrainRoomColorRomDataTooling.TimedEntryByteCount));
            int operand = MotherBrainRoomColorRomData.SourceBank + entry + MotherBrainRoomColorRomDataTooling.PaletteOperandByteOffset;
            int source = (MotherBrainRoomColorRomData.SourceBank|CartridgeImportSource.Require(rom).ReadCartridgeWord(operand));
            var actual = new SnesCgram();
            stock.ApplyFlash(actual, frame);
            for (int color = 0; color < MotherBrainRoomColorRomData.SliceColors * 2; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                int destination = color < MotherBrainRoomColorRomData.SliceColors
                    ? MotherBrainRoomColorRomData.FirstColor + color
                    : MotherBrainRoomColorRomData.SecondColor + color - MotherBrainRoomColorRomData.SliceColors;
                AssertEqual(native, actual.Colors[destination], "stream 3 all native room-flash colors");
                if (color >= MotherBrainRoomColorRomData.SliceColors)
                    AssertEqual(native, actual.Colors[MotherBrainRoomColorRomData.MirroredSecondColor + color - MotherBrainRoomColorRomData.SliceColors], "stream 3 native room-flash mirror");
            }
        }
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.FlashPaletteOperandAddress(invalid),
                "stream 3 room palette operand index bounds");
        for (int frame = 0; frame < 14; frame++)
            for (int color = 0; color < 24; color++)
                foreach (string component in new[] { "red", "green", "blue" })
                {
                    var editedNode = node.DeepClone();
                    var rgb = editedNode.Required("flash").Required(frame).Required(color);
                    rgb[component] = rgb.Required(component).GetValue<int>() ^ 1;
                    var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                        System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                    for (int checkFrame = 0; checkFrame < 14; checkFrame++)
                    {
                        var actual = new SnesCgram();
                        edited.ApplyFlash(actual, checkFrame);
                        for (int checkColor = 0; checkColor < 24; checkColor++)
                        {
                            var expectedRgb = editedNode.Required("flash").Required(checkFrame).Required(checkColor);
                            ushort expected = (ushort)(expectedRgb.Required("red").GetValue<int>() |
                                expectedRgb.Required("green").GetValue<int>() << 5 |
                                expectedRgb.Required("blue").GetValue<int>() << 10);
                            int destination = checkColor < 12 ? 0x34 + checkColor : 0x53 + checkColor - 12;
                            AssertEqual(expected, actual.Colors[destination], "stream 3 independent room flash edit");
                            if (checkColor >= 12)
                                AssertEqual(expected, actual.Colors[0x73 + checkColor - 12], "stream 3 edited room flash mirror");
                        }
                    }
                }
    }

    /// <summary>Interprets the supported bank-$8F door callback opcodes to calculate the native scroll writes.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for door scroll")]
    private static void ExecuteStream3DoorScroll(
        ISnesAddressSpace bus,
        ushort pointer,
        Span<byte> scrolls)
    {
        int pc = 0x8f0000 | pointer;
        bool accumulatorIsEightBit = false;
        ushort accumulator = 0;

        for (int instruction = 0; instruction < 32; instruction++)
        {
            byte opcode = bus.ReadCartridgeByte(pc++);
            switch (opcode)
            {
                case 0x08: // PHP
                case 0x28: // PLP
                    break;

                case 0xe2: // SEP #$20
                    byte sepMask = bus.ReadCartridgeByte(pc++);
                    if (sepMask != 0x20)
                        throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 2);
                    accumulatorIsEightBit = true;
                    break;

                case 0xa9: // LDA immediate
                    accumulator = bus.ReadCartridgeByte(pc++);
                    if (!accumulatorIsEightBit)
                        accumulator |= SnesLittleEndian.Word(0, bus.ReadCartridgeByte(pc++));
                    break;

                case 0x8f: // STA long
                    int destination = (CartridgeImportSource.Require(bus).ReadCartridgeLong(pc));
                    pc += 3;
                    int storageIndex = destination - RoomScrollGrid.WorkRamAddress;
                    if ((uint)storageIndex >= RoomScrollGrid.StorageByteCount)
                        throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 4);
                    scrolls[storageIndex] = unchecked((byte)accumulator);
                    if (!accumulatorIsEightBit)
                    {
                        if (storageIndex + 1 >= scrolls.Length)
                            throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 4);
                        scrolls[storageIndex + 1] = SnesLittleEndian.HighByte(accumulator);
                    }
                    break;

                case 0x60: // RTS
                    return;

                default:
                    throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 1);
            }
        }

        throw new InvalidDataException(
            $"Door callback $8F:{pointer:X4} did not return within 32 instructions.");
    }

    /// <summary>Reports an opcode outside the door-scroll reference interpreter with its banked address.</summary>
    private static InvalidDataException UnsupportedStream3DoorScroll(
        ushort pointer,
        byte opcode,
        int opcodeAddress) =>
        new(
            $"Door callback $8F:{pointer:X4} uses unsupported reference-audit opcode " +
            $"${opcode:X2} at ${SnesAddressMath.BankNumber(opcodeAddress):X2}:{SnesAddressMath.OffsetInBank(opcodeAddress):X4}.");
    /// <summary>Checks the phase-two rear-leg palette's CGRAM output against ROM and its calculated health basis.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for phase two rear leg")]
    internal static void VerifyStream3PhaseTwoRearLeg(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var node = RequiredJson.Parse(json);
        AssertTrue(stock.phaseTwoRearLeg is null,
            "stream 3 phase-two rear uses approved calculated health basis without stored rows");
        var phaseTwo = new SnesCgram();
        stock.ApplyPhaseTwoInitial(phaseTwo);
        for (int color = 0; color < 15; color++)
        {
            int address = 0xa99494 + color * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, phaseTwo.Colors[0xb1 + color], "stream 3 native phase-two rear color and destination");
            foreach (string component in new[] { "red", "green", "blue" })
            {
                var editedNode = node.DeepClone();
                var rgb = editedNode.Required("phaseTwoRearLeg").Required(color);
                rgb[component] = rgb.Required(component).GetValue<int>() ^ 1;
                var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                    System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                var actual = new SnesCgram();
                edited.ApplyPhaseTwoInitial(actual);
                for (int checkColor = 0; checkColor < 15; checkColor++)
                {
                    var expectedRgb = editedNode.Required("phaseTwoRearLeg").Required(checkColor);
                    ushort expected = (ushort)(expectedRgb.Required("red").GetValue<int>() |
                        expectedRgb.Required("green").GetValue<int>() << 5 | expectedRgb.Required("blue").GetValue<int>() << 10);
                    AssertEqual(expected, actual.Colors[0xb1 + checkColor], "stream 3 independent phase-two rear edit");
                    AssertEqual(phaseTwo.Colors[0xa1 + checkColor], actual.Colors[0xa1 + checkColor],
                        "stream 3 rear edit preserves independently installed attack palette");
                }
            }
        }
    }

    /// <summary>Checks auxiliary palette definitions and resolved rows for Face Block, Sidehopper, and Golden Torizo.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for auxiliary palettes")]
    internal static void VerifyStream3AuxiliaryPalettes(ISnesAddressSpace rom)
    {
        EnemyAuxiliaryPaletteDefinition[] definitions =
        [
            new(EnemyAuxiliaryPalette.FaceBlock, 0xa8e7cc, 8, 4, 4),
            new(EnemyAuxiliaryPalette.DeadSidehopper, 0xa9ebcc, 7, 15, 16),
            new(EnemyAuxiliaryPalette.GoldenTorizoBody, 0x848032, 8, 16, 16),
            new(EnemyAuxiliaryPalette.GoldenTorizoBelly, 0x848132, 8, 16, 16),
        ];
        AssertTrue(definitions.SequenceEqual(EnemyAuxiliaryColorDefinitions.All), "stream 3 auxiliary definition fields/order");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = EnemyAuxiliaryColorDefinitions.All[invalid], "stream 3 auxiliary definition bounds");
        var words = definitions.ToDictionary(definition => definition.Id, definition =>
            Enumerable.Range(0, definition.FrameCount).Select(frame =>
                Enumerable.Range(0, definition.ColorCount).Select(color =>
                {
                    int address = definition.SourceAddress + 2 * (frame * definition.NativeFrameStrideColors + color);
                    return CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                }).ToArray()).ToArray());
        EnemyAuxiliaryColorCatalog Load()
        {
            var document = new EnemyAuxiliaryColorDocument
            {
                Version = 1,
                Palettes = words.ToDictionary(pair => pair.Key, pair => pair.Value.Select(row => row.Select(word =>
                    new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 }).ToArray()).ToArray()),
            };
            return EnemyAuxiliaryColorCatalog.Load(new MemoryStream(EnemyAuxiliaryColorCatalog.Write(document), writable: false));
        }
        void Check(EnemyAuxiliaryColorCatalog catalog)
        {
            foreach (var pair in words)
                for (int frame = 0; frame < pair.Value.Length; frame++)
                    for (int color = 0; color < pair.Value[frame].Length; color++)
                        AssertEqual(pair.Value[frame][color], catalog.Resolve(pair.Key, frame, color), "stream 3 exact auxiliary selected color");
            string expectedIdentity = SelectedPresentationHash.Create("enemy-auxiliary-colors-v1", content =>
            {
                foreach (var pair in words.OrderBy(pair => pair.Key))
                {
                    content.Append("palette", (int)pair.Key);
                    content.AppendWordFrames("frames", pair.Value);
                }
            });
            AssertEqual(expectedIdentity, catalog.ContentIdentity, "stream 3 auxiliary selected-content identity unchanged");
        }
        var stock = Load();
        Check(stock);
        foreach (var palette in new[] { stock.faceBlock, stock.deadSidehopper, stock.torizoBody, stock.torizoBelly })
        {
            AssertTrue(palette.supplied is null,
                "stream 3 calculated auxiliary cycles have no stored frame rows");
            foreach (Bgr555[] endpoint in new[] { palette.first, palette.last })
                AssertEqual(0, endpoint.Length,
                    "stream 3 complete auxiliary palette keeps no stock endpoint arrays");
            AssertTrue(palette.corpse is null,
                "stream 3 complete auxiliary palette keeps no stock corpse array");
        }
        foreach (var palette in definitions.Select(definition => definition.Id))
        for (int frame = 0; frame < words[palette].Length; frame++)
        for (int color = 0; color < words[palette][frame].Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = words[palette][frame][color];
            words[palette][frame][color] ^= (ushort)(1 << (channel * 5));
            Check(Load());
            words[palette][frame][color] = original;
        }
        Check(stock); // Loading independent edits must not mutate the already installed stock instance.
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBody, invalid, 0), "stream 3 Torizo band bounds");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBody, 0, invalid), "stream 3 Torizo color bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve((EnemyAuxiliaryPalette)invalid, 0, 0), "stream 3 auxiliary palette identities reject unknown values");
    }

    /// <summary>Checks Baby Metroid sprite parts, reflected OAM placement, and imported schema behavior.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Baby sprite reflection")]
    internal static void VerifyStream3BabySpriteReflection(ISnesAddressSpace rom)
    {
        var definitions = EnemySpritemapDefinitions.Frames.ToArray();
        var document = new EnemySpritemapDocument
        {
            Version = (int)EnemySpritemapSchema.Current,
            Frames = definitions.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal),
            DisplayFrames = definitions.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        var selected = definitions.Where(frame => frame.Bank == 0xa9 && frame.Pointer is 0xf9a8 or 0xfa40 or 0xfad8).ToArray();
        AssertEqual(3, selected.Length, "stream 3 identified native Baby compositions");
        foreach (var frame in selected)
        {
            int source = ((int)new SnesAddress(frame.Bank, frame.Pointer));
            AssertEqual(30, CartridgeImportSource.Require(rom).ReadCartridgeWord(source), "stream 3 native Baby counted-record extent");
            document.Frames[frame.Name] = Enumerable.Range(0, 30).Select(index =>
            {
                int entry = source + 2 + index * 5;
                var x = new SnesSpritemapXWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry));
                var attributes = new SnesObjAttributeWord(CartridgeImportSource.Require(rom).ReadCartridgeWord((entry + 3)));
                return new SpriteVisualPart
                {
                    OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)rom.ReadCartridgeByte(entry + 2)), Size = x.IsLarge ? 16 : 8,
                    TileColumn = attributes.TileNumber % 16, TileRow = attributes.TileNumber / 16,
                    Palette = attributes.PaletteIndex, Priority = attributes.Priority,
                    FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                };
            }).ToArray();
        }
        EnemySpritemapCatalog Load() => EnemySpritemapDocuments.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        void Check(EnemySpritemapCatalog catalog)
        {
            foreach (var frame in selected)
            {
                AssertTrue(catalog.TryGet(frame.Bank, frame.Pointer, out var parts), "stream 3 Baby view remains installed");
                AssertTrue(parts.SequenceEqual(EnemySpritemapCatalog.CompileParts(document.Frames[frame.Name], frame.Name)),
                    "stream 3 exact supplied Baby part order and every visual field");
            }
            string expected = SelectedPresentationHash.Create("enemy-oam-v1", content =>
            {
                foreach (var frame in definitions.OrderBy(frame => ((int)new SnesAddress(frame.Bank, frame.Pointer))))
                {
                    content.Append("frame", ((int)new SnesAddress(frame.Bank, frame.Pointer)));
                    content.AppendEnemyParts(EnemySpritemapCatalog.CompileParts(document.Frames[frame.Name], frame.Name));
                }
                foreach (var frame in definitions.OrderBy(frame => ((int)new SnesAddress(frame.Bank, frame.Pointer))))
                {
                    content.Append("native-binding", ((int)new SnesAddress(frame.Bank, frame.Pointer)));
                    content.Append("selected-binding", ((int)new SnesAddress(frame.Bank, frame.Pointer)));
                }
            });
            AssertEqual(expected, catalog.ContentIdentity, "stream 3 indexed OAM view retains canonical hash framing");
        }
        var stock = Load();
        Check(stock);
        foreach (var frame in selected)
        {
            AssertTrue(stock.TryGetDisplay(frame.Bank, frame.Pointer, out var parts), "stream 3 Baby display view");
            AssertTrue(parts is BabyMetroidSpriteParts, "stream 3 stock Baby full part arrays are discarded");
            var baby = (BabyMetroidSpriteParts)parts;
            AssertEqual(baby.sharedBody is not null ? 6 : 15, baby.halfParts.Length,
                "stream 3 Baby shares nine upper-body halves and stores independent lower halves");
            var nativeOam = new OamBuffer();
            var calculatedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            calculatedOam.AddEnemySpritemap(parts, new Position16<OamPixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(calculatedOam),
                "stream 3 calculated Baby view matches native OAM drawing");
            SpriteVisualPart[] original = document.Frames[frame.Name];
            for (int index = 0; index < original.Length; index++)
            {
                var saved = original[index];
                original[index] = saved with { OffsetX = saved.OffsetX + 1 };
                Check(Load());
                original[index] = saved;
            }
            int upper = Array.FindIndex(original, value => value.OffsetY < 0);
            SpriteVisualPart upperPart = original[upper];
            int opposite = Array.FindIndex(original, value => value == (upperPart with
            {
                OffsetX = -upperPart.OffsetX - upperPart.Size,
                FlipX = !upperPart.FlipX,
            }));
            AssertTrue(opposite >= 0, "native Baby upper-body part has its reflected pair");
            SpriteVisualPart oppositePart = original[opposite];
            original[upper] = upperPart with { OffsetY = upperPart.OffsetY + 1 };
            original[opposite] = oppositePart with { OffsetY = oppositePart.OffsetY + 1 };
            Check(Load());
            AssertTrue(parts.SequenceEqual(EnemySpritemapCatalog.CompileParts(
                original.Select((value, index) => index == upper ? upperPart : index == opposite ? oppositePart : value).ToArray(), frame.Name)),
                "previous Baby component remains immutable after a symmetric supplied edit");
            original[upper] = upperPart;
            original[opposite] = oppositePart;
            var part = original[0];
            SpriteVisualPart[] edits =
            [
                part with { OffsetX = part.OffsetX + 1 }, part with { OffsetY = part.OffsetY ^ 1 },
                part with { Size = part.Size == 8 ? 16 : 8 }, part with { TileColumn = (part.TileColumn + 1) % 16 },
                part with { TileRow = (part.TileRow + 1) % 32 }, part with { Palette = ((part.Palette ?? throw new InvalidOperationException("Verification requires part.Palette.")) + 1) % 8 },
                part with { Priority = (part.Priority + 1) % 4 }, part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY },
            ];
            foreach (var edited in edits) { original[0] = edited; Check(Load()); }
            original[0] = part;
            document.Frames[frame.Name] = original.Reverse().ToArray(); Check(Load());
            document.Frames[frame.Name] = original.Append(part).ToArray(); Check(Load());
            document.Frames[frame.Name] = []; Check(Load());
            document.Frames[frame.Name] = original;
            AssertThrows<ArgumentOutOfRangeException>(() => _ = parts[-1], "stream 3 calculated part lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = parts[30], "stream 3 calculated part upper bound");
        }
        Suite(nameof(VerifyEnemyLegacyOverrides), () => VerifyEnemyLegacyOverrides());
    }
    /// <summary>Checks the phase-two attack palette's live colors and rear-leg routing without stored stock rows.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Mother Brain attack palette")]
    internal static void VerifyStream3MotherBrainAttackPalette(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var document = RequiredJson.Parse(json);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var attack = stock.phaseTwoAttack;
        AssertTrue(attack.supplied is null,
            "stock attack palette calculates every color without a stored row");
        var native = new SnesCgram();
        stock.ApplyPhaseTwoInitial(native);
        for (int color = 0; color < MotherBrainRoomColorRomData.PhaseTwoColors; color++)
        {
            int address = MotherBrainRoomColorRomData.PhaseTwoAttackSource + color * sizeof(ushort);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, native.Colors[MotherBrainRoomColorRomData.PhaseTwoAttackColor + color],
                "attack palette exact native color and CGRAM destination");
            foreach (string component in new[] { "red", "green", "blue" })
            {
                var edited = document.DeepClone();
                var rgb = edited.Required("phaseTwoAttack").Required(color);
                rgb[component] = rgb.Required(component).GetValue<int>() ^ 1;
                Confirm(Load(edited), edited);
            }
        }
        var independentRecovery = document.DeepClone();
        var recoveryRows = independentRecovery.Required("recoveryLights").AsArray();
        var neutral = recoveryRows.Required(recoveryRows.Count - 1).Required(14);
        neutral["red"] = neutral.Required("red").GetValue<int>() ^ 1;
        Confirm(Load(independentRecovery), independentRecovery);
        Confirm(stock, document);

        static MotherBrainRoomColorPresentation Load(System.Text.Json.Nodes.JsonNode document) =>
            MotherBrainRoomColorPresentation.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        static ushort Word(System.Text.Json.Nodes.JsonNode rgb) => (ushort)(rgb.Required("red").GetValue<int>()
            | rgb.Required("green").GetValue<int>() << 5 | rgb.Required("blue").GetValue<int>() << 10);
        static void Confirm(MotherBrainRoomColorPresentation palette, System.Text.Json.Nodes.JsonNode expected)
        {
            var cgram = new SnesCgram();
            palette.ApplyPhaseTwoInitial(cgram);
            for (int color = 0; color < MotherBrainRoomColorRomData.PhaseTwoColors; color++)
            {
                AssertEqual(Word(expected.Required("phaseTwoAttack").Required(color)),
                    cgram.Colors[MotherBrainRoomColorRomData.PhaseTwoAttackColor + color], "independent attack color preserved");
                AssertEqual(Word(expected.Required("phaseTwoRearLeg").Required(color)),
                    cgram.Colors[MotherBrainRoomColorRomData.PhaseTwoRearLegColor + color], "paired rear palette remains independent");
            }
        }
    }

    /// <summary>Checks final-room paint colors and dependent palette isolation against native and edited inputs.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for final room paints")]
    internal static void VerifyStream3FinalRoomPaints(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<MotherBrainRoomColorDocument>(imported, ArtworkJson.Options);
        MotherBrainRoomColorPresentation Load() => MotherBrainRoomColorPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        var stock = Load();
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var finalRoom = stock.finalRoom;
        AssertTrue(finalRoom.supplied is null, "final-room stock has no supplied output array");
        AssertEqual(0, MetadataShape.Fields(finalRoom.GetType(), fields).Count(field => field.FieldType == typeof(ushort)), "final-room paint choices live only in reviewed catalog");
        for (int color = 0; color < document.FinalRoom.Length; color++)
        {
            int address = MotherBrainRoomColorRomData.SourceBank + SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomFlashSourceDefinitions.FinalPalette + color * sizeof(ushort);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, Word(document.FinalRoom[color]), "final-room direct native word");
        }
        Confirm(stock);
        for (int color = 0; color < document.FinalRoom.Length; color++)
        for (int component = 0; component < 3; component++)
        {
            var original = document.FinalRoom[color];
            document.FinalRoom[color] = component switch
            {
                0 => original with { Red = original.Red ^ 1 },
                1 => original with { Green = original.Green ^ 1 },
                _ => original with { Blue = original.Blue ^ 1 },
            };
            Confirm(Load());
            document.FinalRoom[color] = original;
        }
        Confirm(stock);
        static ushort Word(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        void Confirm(MotherBrainRoomColorPresentation palette)
        {
            var colors = new SnesCgram();
            palette.ApplyFinal(colors);
            for (int color = 0; color < document.FinalRoom.Length; color++)
            {
                int destination = color < 12 ? 0x34 + color : 0x53 + color - 12;
                AssertEqual(Word(document.FinalRoom[color]), colors.Colors[destination], "final-room exact supplied paint/shade");
                if (color >= 12) AssertEqual(Word(document.FinalRoom[color]), colors.Colors[0x73 + color - 12], "final-room mirrored colors");
            }
            palette.ApplyRoomEntry(colors);
            for (int color = 0; color < 15; color++)
            {
                AssertEqual(Word((document.InitialGlassShard ?? throw new InvalidOperationException("Verification requires document.InitialGlassShard."))[color]), colors.Colors[0xb1 + color], "final-room edit cannot alter supplied glass");
                AssertEqual(Word((document.InitialTubeProjectile ?? throw new InvalidOperationException("Verification requires document.InitialTubeProjectile."))[color]), colors.Colors[0xf1 + color], "final-room edit cannot alter supplied tube");
            }
            for (int frame = 0; frame < (document.RecoveryLights ?? throw new InvalidOperationException("Verification requires document.RecoveryLights.")).Length; frame++)
            {
                palette.ApplyRecoveryLights(colors, frame);
                for (int color = 0; color < 28; color++)
                    AssertEqual(Word(document.RecoveryLights[frame][color]), colors.Colors[color < 14 ? 0x31 + color : 0x51 + color - 14], "final-room edit cannot alter supplied recovery");
            }
            for (int frame = 0; frame < document.Flash.Length; frame++)
            {
                palette.ApplyFlash(colors, frame);
                for (int color = 0; color < 24; color++)
                    AssertEqual(Word(document.Flash[frame][color]), colors.Colors[color < 12 ? 0x34 + color : 0x53 + color - 12], "final-room edit cannot alter supplied flash");
            }
        }
    }

    /// <summary>Checks room-entry glass and tube colors, recovery independence, and legacy palette inheritance.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for room entry palettes")]
    internal static void VerifyStream3RoomEntryPalettes(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        MotherBrainRoomColorDocument Read() => RequiredJson.Deserialize<MotherBrainRoomColorDocument>(
            imported, ArtworkJson.Options);
        MotherBrainRoomColorPresentation Load(MotherBrainRoomColorDocument document, MotherBrainRoomColorPresentation? current = null) =>
            MotherBrainRoomColorPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                document, ArtworkJson.Options)), current);
        var document = Read();
        var stock = Load(document);
        for (int color = 0; color < MotherBrainRoomColorRomData.InitialColors; color++)
        {
            int glassAddress = MotherBrainRoomColorRomData.InitialGlassShardSource + color * sizeof(ushort);
            int tubeAddress = MotherBrainRoomColorRomData.InitialTubeProjectileSource + color * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(glassAddress), Word((document.InitialGlassShard ?? throw new InvalidOperationException("Verification requires document.InitialGlassShard."))[color]), "glass direct native color");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(tubeAddress), Word((document.InitialTubeProjectile ?? throw new InvalidOperationException("Verification requires document.InitialTubeProjectile."))[color]), "tube direct native color");
        }
        Confirm(stock, document);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (var (name, supplied) in new VerifyStream3OptionsGeometryCase[]
        {
            new VerifyStream3OptionsGeometryCase("finalRoom", stock.finalRoom.supplied),
            new VerifyStream3OptionsGeometryCase("phaseTwoAttack", stock.phaseTwoAttack.supplied),
            new VerifyStream3OptionsGeometryCase("initialGlassShard", stock.initialGlassShard.supplied),
            new VerifyStream3OptionsGeometryCase("initialTubeProjectile", stock.initialTubeProjectile.supplied),
        })
            AssertTrue(supplied is null, "stream 3 room initial palette calculates dependent stock shades: " + name);
        var finalRoom = stock.finalRoom;
        AssertEqual(0, MetadataShape.Fields(finalRoom.GetType(), fields).Count(field => field.FieldType == typeof(ushort)),
            "final room calculates from reviewed catalog choices without per-instance paint words");
        var attack = stock.phaseTwoAttack;
        AssertTrue(!MetadataShape.Fields(attack.GetType(), fields).Any(field => field.FieldType == typeof(ushort)),
            "stock attack keeps no duplicated scalar paint endpoints");
        var glass = stock.initialGlassShard;
        AssertTrue(!MetadataShape.Fields(glass.GetType(), fields).Any(field => field.FieldType == typeof(ushort)),
            "stock glass keeps no duplicated scalar paint endpoint");
        foreach (PaletteRgb5[]? palette in new[] { document.InitialGlassShard, document.InitialTubeProjectile })
        for (int color = 0; color < (palette ?? throw new InvalidOperationException("Verification requires palette.")).Length; color++)
        for (int component = 0; component < 3; component++)
        {
            PaletteRgb5 original = palette[color];
            palette[color] = Change(original, component);
            Confirm(Load(document), document);
            palette[color] = original;
        }
        // Editing either shared supplied source must not change separately supplied attack/glass/tube output.
        for (int color = 0; color < 24; color++)
        for (int component = 0; component < 3; component++)
        {
            PaletteRgb5 original = document.FinalRoom[color];
            document.FinalRoom[color] = Change(original, component);
            Confirm(Load(document), document);
            document.FinalRoom[color] = original;
        }
        for (int component = 0; component < 3; component++)
        {
            PaletteRgb5 original = (document.RecoveryLights ?? throw new InvalidOperationException("Verification requires document.RecoveryLights."))[6][14];
            document.RecoveryLights[6][14] = Change(original, component);
            Confirm(Load(document), document);
            document.RecoveryLights[6][14] = original;
        }
        foreach (int version in new[] { MotherBrainRoomColorFormatConstants.PreRoomEntryVersion, MotherBrainRoomColorFormatConstants.PreRecoveryLightsVersion })
        {
            var legacy = document with { Version = version, RecoveryLights = null };
            if (version == MotherBrainRoomColorFormatConstants.PreRoomEntryVersion)
                legacy = legacy with { InitialGlassShard = null, InitialTubeProjectile = null };
            Confirm(Load(legacy, stock), document);
        }

        static PaletteRgb5 Change(PaletteRgb5 rgb, int component) => component switch
        {
            0 => rgb with { Red = rgb.Red ^ 1 },
            1 => rgb with { Green = rgb.Green ^ 1 },
            _ => rgb with { Blue = rgb.Blue ^ 1 },
        };
        static ushort Word(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        static void Confirm(MotherBrainRoomColorPresentation palette, MotherBrainRoomColorDocument expected)
        {
            var final = new SnesCgram();
            palette.ApplyFinal(final);
            for (int color = 0; color < expected.FinalRoom.Length; color++)
            {
                int destination = color < 12 ? 0x34 + color : 0x53 + color - 12;
                AssertEqual(Word(expected.FinalRoom[color]), final.Colors[destination], "final room exact native/edited shade");
                if (color >= 12) AssertEqual(Word(expected.FinalRoom[color]), final.Colors[0x73 + color - 12], "final room mirrored shade");
            }
            var phase = new SnesCgram();
            var entry = new SnesCgram();
            palette.ApplyPhaseTwoInitial(phase);
            palette.ApplyRoomEntry(entry);
            for (int color = 0; color < 15; color++)
            {
                AssertEqual(Word(expected.PhaseTwoAttack[color]), phase.Colors[0xa1 + color], "room attack exact native/edited shade");
                AssertEqual(Word(expected.PhaseTwoRearLeg[color]), phase.Colors[0xb1 + color], "room rear independence");
                AssertEqual(Word((expected.InitialGlassShard ?? throw new InvalidOperationException("Verification requires expected.InitialGlassShard."))[color]), entry.Colors[0xb1 + color], "room glass exact native/edited shade");
                AssertEqual(Word((expected.InitialTubeProjectile ?? throw new InvalidOperationException("Verification requires expected.InitialTubeProjectile."))[color]), entry.Colors[0xf1 + color], "room tube exact native/edited shade");
            }
        }
    }

    /// <summary>Checks Baby Metroid's initial cutscene paints and their separation from live Shitroid colors.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Baby initial paints")]
    internal static void VerifyStream3BabyInitialPaints(ISnesAddressSpace rom)
    {
        var babyDocument = RequiredJson.Parse(SuperMetroid.AssetExtraction.Bosses.MotherBrain.BabyMetroidCutsceneColorExtractor.Extract(rom));
        var liveDocument = RequiredJson.Parse(SuperMetroid.AssetExtraction.Bosses.MotherBrain.ShitroidColorExtractor.Extract(rom));
        var stock = LoadBaby(babyDocument);
        var liveStock = LoadLive(liveDocument);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var initial = stock.initial;
        AssertTrue(initial.supplied is null, "Baby stock initial has no stored color array");
        AssertEqual(0, MetadataShape.Fields(initial.GetType(), fields).Count(field => field.FieldType == typeof(ushort)), "Baby initial paint choices reside only in dedicated catalog");
        for (int color = 0; color < BabyMetroidCutsceneColorRomData.InitialColorCount; color++)
        {
            int address = BabyMetroidCutsceneColorRomData.InitialSource + color * sizeof(ushort);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, stock.InitialColor(color), "Baby initial exact native color");
            AssertEqual(native, liveStock.TargetColor(ShitroidColorTarget.Shitroid, color + 1), "Baby live initial alias");
        }
        Check(stock, babyDocument);
        for (int color = 0; color < BabyMetroidCutsceneColorRomData.InitialColorCount; color++)
        foreach (string component in new[] { "red", "green", "blue" })
        {
            var editedBabyDocument = babyDocument.DeepClone();
            var rgb = editedBabyDocument.Required("initial").Required(color);
            rgb[component] = rgb.Required(component).GetValue<int>() ^ 1;
            Check(LoadBaby(editedBabyDocument), editedBabyDocument);
            var editedLiveDocument = liveDocument.DeepClone();
            rgb = editedLiveDocument.Required("shitroid")[color + 1];
            (rgb ?? throw new InvalidOperationException("Verification requires rgb."))[component] = rgb.Required(component).GetValue<int>() ^ 1;
            var editedLive = LoadLive(editedLiveDocument);
            for (int index = 0; index < BabyMetroidCutsceneColorRomData.InitialColorCount; index++)
            {
                AssertEqual(Word(editedLiveDocument.Required("shitroid").Required(index + 1)), editedLive.TargetColor(ShitroidColorTarget.Shitroid, index + 1), "independent live alias edit");
                AssertEqual(Word(babyDocument.Required("initial").Required(index)), stock.InitialColor(index), "live edit cannot mutate Baby instance");
                AssertEqual(Word(liveDocument.Required("shitroid").Required(index + 1)), liveStock.TargetColor(ShitroidColorTarget.Shitroid, index + 1), "Baby edit cannot mutate live instance");
            }
        }
        Check(stock, babyDocument);
        foreach (int invalid in new[] { -1, BabyMetroidCutsceneColorRomData.InitialColorCount, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.InitialColor(invalid), "Baby initial index domain preserved");
        static BabyMetroidCutsceneColorCatalog LoadBaby(System.Text.Json.Nodes.JsonNode document) =>
            BabyMetroidCutsceneColorCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        static ShitroidColorCatalog LoadLive(System.Text.Json.Nodes.JsonNode document) =>
            ShitroidColorCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        static ushort Word(System.Text.Json.Nodes.JsonNode rgb) => (ushort)(rgb.Required("red").GetValue<int>() |
            rgb.Required("green").GetValue<int>() << 5 | rgb.Required("blue").GetValue<int>() << 10);
        static void Check(BabyMetroidCutsceneColorCatalog catalog, System.Text.Json.Nodes.JsonNode document)
        {
            ushort[] colors = document.Required("initial").AsArray().RequiredElements().Select(rgb => Word(rgb)).ToArray();
            ushort[][] fade = document.Required("fade").AsArray().RequiredElements().Select(row => row.AsArray().RequiredElements().Select(rgb => Word(rgb)).ToArray()).ToArray();
            for (int color = 0; color < colors.Length; color++) AssertEqual(colors[color], catalog.InitialColor(color), "Baby independently edited initial colors");
            string identity = SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
            {
                content.AppendWords("initial", colors);
                content.AppendWordFrames("fade", fade);
            });
            AssertEqual(identity, catalog.ContentIdentity, "Baby initial canonical content identity preserved");
        }
    }

    /// <summary>Checks the first narration tilemap against decompressed ROM words and the intro upload path.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry")]
    [CpuBusBoundary("exact bus test: retail ROM oracle decompressing the initial narration tilemap and opening font")]
    internal static void VerifyStream3InitialNarrationMap()
    {
        using var rootScratch = new TestScratchDirectory("initial-narration");
        string root = rootScratch.Root;
        try
        {
            string source = Path.GetFullPath("Super Metroid.smc");
            var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
            byte[] native = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
                IntroCinematicRomData.Assets.FirstNarrationTilemap, maximumOutputBytes: 2048);
            var installation = SuperMetroid.AssetExtraction.Installation.GameAssetInstaller.Install(source, root);
            IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
            static RoomBackgroundTilemapAtlas Page(byte[] bytes) => new(bytes);
            RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(stock.BackgroundPages.Slice(index * 2048, 2048).ToArray())).ToArray();
            IntroCinematicArtworkCatalog Copy(byte[] narration) => new(new IntroCinematicArtworkParts { BackgroundCharacters = stock.BackgroundCharacters, IntroObjectCharacters = stock.IntroObjectCharacters, CinematicObjectCharacters = stock.CinematicObjectCharacters, BackgroundPages = pages, PortraitTilemap = Page(stock.PortraitTilemap.ToArray()), InitialNarrationTilemap = Page(narration), FinalLine = stock.FinalLine, EyeFrames = stock.EyeFrames, CaretSprites = stock.CaretSprites, MotherBrainSprites = stock.MotherBrainSprites, MotherBrainExplosionSprites = stock.MotherBrainExplosionSprites, RinkaSprites = stock.RinkaSprites, EggEffectSprites = stock.EggEffectSprites, DiscoveryActorSprites = stock.DiscoveryActorSprites, ScientistSprites = stock.ScientistSprites, Palette = stock.Palette, CeresFlight = stock.CeresFlight, CeresDestruction = stock.CeresDestruction });
            AssertTrue(stock.InitialNarrationTilemap.Span.SequenceEqual(native), "all 1024 initial narration words match native");
            AssertTrue(stock.suppliedInitialNarration is null, "stock initial narration stores no page array");
            var font = IntroFontAtlas.Load(new MemoryStream(SuperMetroid.AssetExtraction.Frontend.Intro.IntroFontAtlasExtractor.Extract(rom)));
            var intro = new IntroCinematicState(introFont: font, characterArtwork: stock,
                beamArtwork: installation.LoadProjectiles().BeamTiles) { Random = new CartridgeRandom() };
            byte[] nativeFont = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
                IntroCinematicRomData.Assets.FontOne, maximumOutputBytes: IntroFontAtlasFormat.ByteCount);
            AssertTrue(intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                (IntroVramLayout.FontOne.Offset * 2), nativeFont.Length).SequenceEqual(nativeFont),
                "actual opening constructor transfers all calculated font bytes");
            AssertTrue(intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                (IntroVramLayout.NarrationTilemap.Offset * 2), native.Length).SequenceEqual(native),
                "actual opening constructor transfers calculated narration to native VRAM destination");
            intro.SetupFirstIllustratedPage();
            var subtitleUpload = intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                (IntroVramLayout.NarrationTilemap.Offset * 2) + IntroCinematicRomData.Text.FinalLineDestinationStart * 2,
                IntroFinalLineTilemapFormat.CellCount * 2);
            for (int index = 0; index < IntroFinalLineTilemapFormat.CellCount; index++)
                AssertEqual(ReadVerificationWord(rom, IntroCinematicRomData.Assets.FinalTextLine + index * 2),
                    SnesLittleEndian.Word(subtitleUpload[index * 2], subtitleUpload[index * 2 + 1]), "actual Japanese subtitle staging upload");
            string hash = stock.ContentIdentity;
            for (int word = 0; word < 1024; word++)
            {
                byte[] edited = native.ToArray();
                edited[word * 2] ^= 0xff;
                edited[word * 2 + 1] ^= 0xff;
                var selected = Copy(edited);
                AssertTrue(selected.InitialNarrationTilemap.Span.SequenceEqual(edited), $"independent narration word {word}");
                if (word == 0) AssertTrue(selected.ContentIdentity != hash, "edited narration changes selected bundle identity");
                edited[word * 2] ^= 1;
                AssertTrue(selected.InitialNarrationTilemap.Span[word * 2] != edited[word * 2], "narration owns supplied bytes");
            }
            AssertTrue(stock.InitialNarrationTilemap.Span.SequenceEqual(native), "edits do not cross-mutate stock");
            AssertEqual(hash, Copy(native).ContentIdentity, "equivalent stock bundle preserves canonical identity");
            byte[] first = stock.InitialNarrationTilemap.ToArray();
            first[0] ^= 1;
            AssertTrue(stock.InitialNarrationTilemap.Span.SequenceEqual(native), "calculated page is not a mutable cached table");
            Console.WriteLine("Initial narration: 1024 native words, 1024 independent word edits, ownership, empty stock storage and canonical bundle identity pass.");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    /// <summary>Checks file-select sprite compositions, border parts, and edited layout against native OAM data.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for file select sprites")]
    internal static void VerifyStream3FileSelectSprites(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyStream3FileSelectBorders), () => VerifyStream3FileSelectBorders(rom, imported));
        FileSelectPresentationDocument Read() => RequiredJson.Deserialize<FileSelectPresentationDocument>(imported, ArtworkJson.Options);
        FileSelectPresentation Load(FileSelectPresentationDocument document) => FileSelectPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        SpriteComposition Composition(FileSelectPresentation owner, int frame) => owner.helmetSprites[frame];
        var original = Read();
        var stock = Load(original);
        for (int frame = 0; frame < 8; frame++)
        {
            string name = FileSelectPresentationDefinitions.HelmetSpriteName(frame);
            var composition = Composition(stock, frame);
            AssertTrue(composition.parts is FileSelectHelmetParts,
                "stock helmet retains semantic frame only");
            Confirm(stock, original, frame);
            int pointer = ReadVerificationWord(rom, 0x82c569 + FileSelectHelmetAnimation.SpritemapId(frame) * 2);
            int address = 0x820000 | pointer;
            AssertEqual(ReadVerificationWord(rom, address), composition.PartCount, "exact native helmet part count");
            for (int part = 0; part < composition.PartCount; part++)
            {
                int entry = address + 2 + part * 5;
                var nativeX = new SnesSpritemapXWord(ReadVerificationWord(rom, entry));
                AssertEqual(nativeX.SignedOffset, composition.Part(part).X.SignedOffset, "native helmet X");
                AssertEqual(nativeX.IsLarge, composition.Part(part).X.IsLarge, "native helmet size");
                AssertEqual(rom.ReadCartridgeByte(entry + 2), composition.Part(part).Y.Raw, "native helmet Y");
                AssertEqual((ushort)(ReadVerificationWord(rom, entry + 3) & ~0x0e00), composition.Part(part).Attributes.Raw, "native helmet attributes with caller palette inheritance");
            }
            for (int part = 0; part < original.Sprites[name].Length; part++)
            for (int field = 0; field < 9; field++)
            {
                var edited = Read();
                var source = edited.Sprites[name][part];
                edited.Sprites[name][part] = field switch
                {
                    0 => source with { OffsetX = source.OffsetX + 1 },
                    1 => source with { OffsetY = source.OffsetY + 1 },
                    2 => source with { TileColumn = source.TileColumn ^ 1 },
                    3 => source with { TileRow = source.TileRow ^ 1 },
                    4 => source with { Size = source.Size == 8 ? 16 : 8 },
                    5 => source with { Priority = source.Priority ^ 1 },
                    6 => source with { Palette = 0 },
                    7 => source with { FlipX = !source.FlipX },
                    _ => source with { FlipY = !source.FlipY },
                };
                var changed = edited.Sprites[name][part];
                if (changed.TileColumn > MapSpriteFormat.TileColumns - changed.Size / 8 ||
                    changed.TileRow > MapSpriteFormat.TileRows - changed.Size / 8)
                    AssertThrows<InvalidDataException>(() => Load(edited), "helmet edited footprint remains bounded");
                else Confirm(Load(edited), edited, frame);
            }
            var reversed = Read();
            Array.Reverse(reversed.Sprites[name]);
            Confirm(Load(reversed), reversed, frame);
            var expanded = Read();
            expanded.Sprites[name] = [.. expanded.Sprites[name], expanded.Sprites[name][0]];
            Confirm(Load(expanded), expanded, frame);
            Confirm(stock, original, frame);
        }
        Console.WriteLine("File-select sprites: 114 border parts/456 edits, 42 helmet parts/378 field cases, ordering/count edits and actual helmet OAM pass; cursor dependency previously confirmed.");
        void Confirm(FileSelectPresentation actual, FileSelectPresentationDocument expected, int frame)
        {
            string name = FileSelectPresentationDefinitions.HelmetSpriteName(frame);
            var expectedParts = MenuSpriteCompiler.Compile(expected.Sprites[name], name);
            var selected = Composition(actual, frame);
            AssertEqual(expectedParts.PartCount, selected.PartCount, "helmet part count");
            for (int part = 0; part < expectedParts.PartCount; part++)
                AssertEqual(expectedParts.Part(part), selected.Part(part), "exact selected helmet part");
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            actual.DrawHelmet(actualOam, MenuSheetFixtures.FileSelect(actual), frame, 0);
            var anchor = expected.HelmetAnchors[0];
            expectedParts.DrawOnScreen(expectedOam, new(anchor.X, anchor.Y), (ushort)(expected.ObjectPalette << 9));
            actualOam.FinishFrame(); expectedOam.FinalizeFrame();
            AssertTrue(actualOam.ImageEquals(expectedOam),
                "actual helmet draw preserves native and independent edited ordered OAM");
        }
    }

    /// <summary>Checks all 144 intro font tiles against decompressed ROM bytes and edited atlas uploads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle decompressing the opening font")]
    internal static void VerifyStream3IntroFont()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] native = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
            IntroCinematicRomData.Assets.FontOne, maximumOutputBytes: IntroFontAtlasFormat.ByteCount);
        var stock = IntroFontAtlas.Load(new MemoryStream(SuperMetroid.AssetExtraction.Frontend.Intro.IntroFontAtlasExtractor.Extract(rom)));
        AssertTrue(stock.Transfer.Span.SequenceEqual(native), "144 calculated font tiles match native 2304 bytes");
        AssertTrue(stock.suppliedTransfer is null, "native font retains no planar output array");
        for (int index = 0; index < native.Length; index++)
        {
            byte[] edit = native.ToArray();
            edit[index] ^= 0xff;
            var selected = new IntroFontAtlas(edit.ToArray());
            AssertTrue(selected.Transfer.Span.SequenceEqual(edit), "independent font planar-byte edit survives");
        }
        AssertTrue(stock.Transfer.Span.SequenceEqual(native), "edited glyphs do not cross-couple stock or aliases");
        byte[] output = stock.Transfer.ToArray();
        output[0] ^= 1;
        AssertTrue(stock.Transfer.Span.SequenceEqual(native), "font output is freshly calculated");
        int additions = 0, removals = 0;
        for (int tile = 0; tile < 144; tile++)
        {
            int glyph = tile < 48 ? tile : 48 + (tile - 48) / 32 * 32 + (tile - 48) % 16;
            int dy = tile < 48 ? 0 : (tile - 48) % 32 / 16 * 8;
            for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int gx = glyph == 0x26 ? x - 1 : x;
                int source = glyph == 0x26 ? 0x24 : glyph;
                if (gx < 0) continue;
                var point = new Position16<ImagePixels>((ushort)gx, (ushort)(y + dy));
                if (IntroFontGlyphDefinitions.AddedContour(source, point)) additions++;
                if (IntroFontGlyphDefinitions.RemovedContour(source, point)) removals++;
            }
        }
        AssertEqual(4, additions, "only four authored outline additions");
        AssertEqual(91, removals, "only 91 authored corner removals including derived period alias");
        for (int y = 0; y < 8; y++)
        for (int plane = 0; plane < 2; plane++)
            AssertEqual((byte)(native[0x24 * 16 + y * 2 + plane] >> 1), native[0x26 * 16 + y * 2 + plane], "whole period drawing shifts right one pixel");
        Console.WriteLine("Intro font: 144 native tiles/2304 bytes, 2304 independent edits, four outline additions/91 removals, exact period alias and no stock output storage pass.");
    }

    /// <summary>Checks intro background map pages against decompressed ROM and loaded presentation edits.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry")]
    [CpuBusBoundary("exact bus test: retail ROM oracle decompressing the intro background page tilemaps")]
    internal static void VerifyStream3BackgroundMaps()
    {
        using var rootScratch = new TestScratchDirectory("background-maps");
        string root = rootScratch.Root;
        try
        {
            string source = Path.GetFullPath("Super Metroid.smc");
            var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
            byte[] native = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
                IntroCinematicRomData.Assets.BackgroundPageTilemaps, maximumOutputBytes: 8192);
            var installation = SuperMetroid.AssetExtraction.Installation.GameAssetInstaller.Install(source, root);
            IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
            static RoomBackgroundTilemapAtlas Page(byte[] bytes) => new(bytes);
            IntroCinematicArtworkCatalog Copy(byte[] background)
            {
                RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(background.AsSpan(index * 2048, 2048).ToArray())).ToArray();
                return new(new IntroCinematicArtworkParts { BackgroundCharacters = stock.BackgroundCharacters, IntroObjectCharacters = stock.IntroObjectCharacters, CinematicObjectCharacters = stock.CinematicObjectCharacters, BackgroundPages = pages, PortraitTilemap = Page(stock.PortraitTilemap.ToArray()), InitialNarrationTilemap = Page(stock.InitialNarrationTilemap.ToArray()), FinalLine = stock.FinalLine, EyeFrames = stock.EyeFrames, CaretSprites = stock.CaretSprites, MotherBrainSprites = stock.MotherBrainSprites, MotherBrainExplosionSprites = stock.MotherBrainExplosionSprites, RinkaSprites = stock.RinkaSprites, EggEffectSprites = stock.EggEffectSprites, DiscoveryActorSprites = stock.DiscoveryActorSprites, ScientistSprites = stock.ScientistSprites, Palette = stock.Palette, CeresFlight = stock.CeresFlight, CeresDestruction = stock.CeresDestruction });
            }
            byte[] calculated = IntroBackgroundTilemapDefinitions.Compile();
            var mismatches = new List<string>();
            for (int cell = 0; cell < native.Length / 2; cell++)
            {
                ushort expected = BitConverter.ToUInt16(native, cell * 2), actual = BitConverter.ToUInt16(calculated, cell * 2);
                if (expected != actual) mismatches.Add($"page{cell / 1024} ({cell % 32},{cell % 1024 / 32}) expected{expected:X4}/actual{actual:X4}");
            }
            AssertTrue(mismatches.Count == 0, "all 4096 background words match native: " + string.Join("; ", mismatches.Take(32)));
            AssertTrue(stock.suppliedBackgroundPages is null,
                "native background pages store no output array");
            string hash = stock.ContentIdentity;
            for (int word = 0; word < native.Length / 2; word++)
            {
                byte[] edit = native.ToArray();
                edit[word * 2] ^= 0xff;
                edit[word * 2 + 1] ^= 0xff;
                var selected = Copy(edit);
                AssertTrue(selected.BackgroundPages.Span.SequenceEqual(edit), "independent background full-word edit");
                if (word % 1024 == 0) AssertTrue(hash != selected.ContentIdentity, "each edited page changes bundle identity");
                edit[word * 2] ^= 1;
                AssertTrue(selected.BackgroundPages.Span[word * 2] != edit[word * 2], "background owns supplied data");
            }
            AssertTrue(stock.BackgroundPages.Span.SequenceEqual(native), "background edits do not cross-couple pages");
            AssertEqual(hash, Copy(native).ContentIdentity, "equivalent backgrounds preserve canonical hash");
            var font = IntroFontAtlas.Load(new MemoryStream(SuperMetroid.AssetExtraction.Frontend.Intro.IntroFontAtlasExtractor.Extract(rom)));
            var intro = new IntroCinematicState(introFont: font, characterArtwork: stock,
                beamArtwork: installation.LoadProjectiles().BeamTiles) { Random = new CartridgeRandom() };
            AssertTrue(intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                (IntroVramLayout.BackgroundPages.Offset * 2), native.Length).SequenceEqual(native),
                "actual opening constructor uploads all four calculated background pages");
            Console.WriteLine("Intro backgrounds: 4096 native words, 4096 independent edits, owned fallback, canonical hash and actual four-page VRAM upload pass.");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    /// <summary>Checks all 27 Work Robot frame identities and their native spritemap ordering.</summary>
    internal static void VerifyStream3WorkRobotRegistry()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        NativeEnemySpritemapDefinition[] frames = WorkRobotVisualDefinitions.Frames().ToArray();
        AssertEqual(27, frames.Length, "all Work Robot registration identities");
        string identities = string.Concat(frames.Select(frame => $"{frame.Bank:x2}:{frame.Pointer:x4}:{frame.Name}\n"));
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(identities)));
        AssertEqual("4ADE0C5ABD612C37688FA6578C9E49D4A902CB4BA9E7EEE3E2CC2F4D131DFB23", hash,
            "all 27 legacy bank/pointer/name/order identities remain exact");
        int cursor = frames[0].Pointer;
        for (int index = 0; index < 24; index++)
        {
            AssertEqual((ushort)cursor, frames[index].Pointer, "powered registration follows actual native record length");
            ushort count = ReadVerificationWord(rom, 0xa80000 | cursor);
            AssertEqual((ushort)12, count, "each native powered header has twelve parts");
            cursor += sizeof(ushort) + count * 5;
        }
        foreach (int index in new[] { 25, 24, 26 })
        {
            AssertEqual((ushort)cursor, frames[index].Pointer, "unpowered semantic identity maps to native physical order");
            ushort count = ReadVerificationWord(rom, 0xa80000 | cursor);
            AssertEqual((ushort)6, count, "each native unpowered header has six parts");
            cursor += sizeof(ushort) + count * 5;
        }
        AssertTrue(WorkRobotVisualDefinitions.Frames().SequenceEqual(frames), "independent enumeration preserves order");
        Console.WriteLine("Work Robot registry: 27 native headers and exact legacy bank/pointer/name/order hash pass; artwork and timing remain separate.");
    }

    /// <summary>Checks all 15 Shaktool frame identities and their native spritemap ordering.</summary>
    internal static void VerifyStream3ShaktoolRegistry()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        NativeEnemySpritemapDefinition[] frames = ShaktoolVisualDefinitions.Frames().ToArray();
        AssertEqual(15, frames.Length, "all Shaktool registration identities");
        string identities = string.Concat(frames.Select(frame => $"{frame.Bank:x2}:{frame.Pointer:x4}:{frame.Name}\n"));
        string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(identities)));
        AssertEqual("1D724428CB3831C0C43A4987E4D05D772ED8762650C5DEEC4C5E68D69B506892", hash,
            "all 15 legacy bank/pointer/name/order identities remain exact");
        int cursor = frames[0].Pointer;
        for (int index = 0; index < frames.Length; index++)
        {
            AssertEqual((ushort)cursor, frames[index].Pointer, "Shaktool registry follows actual native record lengths");
            ushort count = ReadVerificationWord(rom, 0xaa0000 | cursor);
            AssertEqual((ushort)(index is >= 4 and < 12 ? 4 : 1), count, "each native saw/arm/head part count");
            cursor += sizeof(ushort) + count * 5;
        }
        AssertTrue(ShaktoolVisualDefinitions.Frames().SequenceEqual(frames), "independent enumeration preserves order");
        Console.WriteLine("Shaktool registry: 15 native headers and exact legacy bank/pointer/name/order hash pass; artwork and timing remain separate.");
    }

    /// <summary>Checks both Nintendo logo fade entries and their native mechanics/color operand positions.</summary>
    internal static void VerifyStream3NintendoFadeEntries()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var entries = NintendoLogoFadePaletteFxColorDefinitionsTooling.All;
        AssertEqual(2, entries.Count, "both semantic Nintendo fade entries");
        foreach (var entry in new[]
        {
            new VerifyStream3NintendoFadeEntriesCase(DefinitionPointer:NintendoLogoFadePaletteFxColorDefinitions.BootLogoDefinitionPointer,
ProgramStart:NintendoLogoFadePaletteFxColorDefinitionsConstants.BootLogoEntry,
ColorByteIndex:NintendoLogoFadePaletteFxColorDefinitionsConstants.BootLogoColorByteIndex,
BranchesToSharedBody:false),
            new VerifyStream3NintendoFadeEntriesCase(DefinitionPointer:NintendoLogoFadePaletteFxColorDefinitions.CopyrightDefinitionPointer,
ProgramStart:NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightEntry,
ColorByteIndex:NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightColorByteIndex,
BranchesToSharedBody:true),
        })
        {
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | (entry.DefinitionPointer + 2)), entry.ProgramStart,
                "native definition selects exact semantic program entry");
            AssertEqual((ushort)PaletteFxInstruction.SetColorIndex, ReadVerificationWord(rom, 0x8d0000 | entry.ProgramStart),
                "each entry sets its own CGRAM slot");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | (entry.ProgramStart + 2)), entry.ColorByteIndex,
                "each entry preserves the native CGRAM byte index");
            if (entry.BranchesToSharedBody)
            {
                AssertEqual((ushort)PaletteFxInstruction.Goto, ReadVerificationWord(rom, 0x8d0000 | (entry.ProgramStart + 4)),
                    "copyright actually branches");
                AssertEqual(NintendoLogoFadePaletteFxColorDefinitions.FirstFramePointer,
                    ReadVerificationWord(rom, 0x8d0000 | (entry.ProgramStart + 6)), "copyright branch target");
            }
            else AssertEqual(NintendoLogoFadePaletteFxColorDefinitions.FirstFramePointer,
                (ushort)(entry.ProgramStart + 4), "boot actually falls through");
        }
        AssertThrows<IndexOutOfRangeException>(() => _ = entries[-1], "fade entry lower bound");
        AssertThrows<IndexOutOfRangeException>(() => _ = entries[2], "fade entry upper bound");
        AssertTrue(entries.SequenceEqual(new[] { entries[0], entries[1] }), "fade enumeration and indexing agree");
        Console.WriteLine("Nintendo fade entries: both native definition/slot/fallthrough-or-branch contracts, order and bounds pass; timing/colors unchanged.");
    }

    /// <summary>Checks the opening portrait tilemap against decompressed ROM and the actual VRAM upload.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry")]
    [CpuBusBoundary("exact bus test: retail ROM oracle decompressing the intro Samus head tilemap")]
    internal static void VerifyStream3PortraitMap()
    {
        using var rootScratch = new TestScratchDirectory("portrait-map");
        string root = rootScratch.Root;
        try
        {
            string source = Path.GetFullPath("Super Metroid.smc");
            var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
            byte[] native = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
                IntroCinematicRomData.Assets.SamusHeadTilemap, maximumOutputBytes: 2048);
            var installation = SuperMetroid.AssetExtraction.Installation.GameAssetInstaller.Install(source, root);
            IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
            static RoomBackgroundTilemapAtlas Page(byte[] bytes) => new(bytes);
            RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(stock.BackgroundPages.Slice(index * 2048, 2048).ToArray())).ToArray();
            IntroCinematicArtworkCatalog Copy(byte[] portrait) => new(new IntroCinematicArtworkParts { BackgroundCharacters = stock.BackgroundCharacters, IntroObjectCharacters = stock.IntroObjectCharacters, CinematicObjectCharacters = stock.CinematicObjectCharacters, BackgroundPages = pages, PortraitTilemap = Page(portrait), InitialNarrationTilemap = Page(stock.InitialNarrationTilemap.ToArray()), FinalLine = stock.FinalLine, EyeFrames = stock.EyeFrames, CaretSprites = stock.CaretSprites, MotherBrainSprites = stock.MotherBrainSprites, MotherBrainExplosionSprites = stock.MotherBrainExplosionSprites, RinkaSprites = stock.RinkaSprites, EggEffectSprites = stock.EggEffectSprites, DiscoveryActorSprites = stock.DiscoveryActorSprites, ScientistSprites = stock.ScientistSprites, Palette = stock.Palette, CeresFlight = stock.CeresFlight, CeresDestruction = stock.CeresDestruction });
            AssertTrue(stock.PortraitTilemap.Span.SequenceEqual(native), "all 1024 calculated portrait words match native");
            AssertTrue(stock.suppliedPortrait is null,
                "native portrait stores no output word array");
            string hash = stock.ContentIdentity;
            for (int word = 0; word < 1024; word++)
            {
                byte[] edit = native.ToArray();
                edit[word * 2] ^= 0xff;
                edit[word * 2 + 1] ^= 0xff;
                var selected = Copy(edit);
                AssertTrue(selected.PortraitTilemap.Span.SequenceEqual(edit), "independent portrait word preserves identity/style/position edit");
                if (word == 0) AssertTrue(hash != selected.ContentIdentity, "portrait edit changes content identity");
                edit[word * 2] ^= 1;
                AssertTrue(selected.PortraitTilemap.Span[word * 2] != edit[word * 2], "portrait owns supplied data");
            }
            AssertTrue(stock.PortraitTilemap.Span.SequenceEqual(native), "portrait edits do not cross-mutate stock");
            AssertEqual(hash, Copy(native).ContentIdentity, "equivalent portrait preserves canonical hash");
            var font = IntroFontAtlas.Load(new MemoryStream(SuperMetroid.AssetExtraction.Frontend.Intro.IntroFontAtlasExtractor.Extract(rom)));
            var intro = new IntroCinematicState(introFont: font, characterArtwork: stock,
                beamArtwork: installation.LoadProjectiles().BeamTiles) { Random = new CartridgeRandom() };
            AssertTrue(intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                (IntroVramLayout.PortraitTilemap.Offset * 2), native.Length).SequenceEqual(native),
                "actual opening constructor uploads calculated portrait map");
            Console.WriteLine("Portrait map: 1024 native words, 1024 independent edits, owned fallback, canonical hash and actual VRAM upload pass.");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    /// <summary>Runs the retail-ROM room-flash color and mirror comparison from the command line.</summary>
    [VerificationCommand("--lookup-stream3-room-flash")]
    internal static int LookupStream3RoomFlashCommand()
    {
        Suite(nameof(VerifyStream3RoomFlash), () => VerifyStream3RoomFlash(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Room flash:336native colors/mirrors,1008independent edits,zero stock arrays/paint scalars pass.");
        return 0;
    }

    /// <summary>Runs the glass and tube room-entry palette comparison from the command line.</summary>
    [VerificationCommand("--lookup-stream3-room-entry-palettes")]
    internal static int LookupStream3RoomEntryPalettesCommand()
    {
        Suite(nameof(VerifyStream3RoomEntryPalettes), () => VerifyStream3RoomEntryPalettes(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Glass/tube:30native colors,90edits,room/recovery independence and legacy inheritance pass.");
        return 0;
    }

    /// <summary>Runs the recovery-light color and room-routing comparison from the command line.</summary>
    [VerificationCommand("--lookup-stream3-recovery-lights")]
    internal static int LookupStream3RecoveryLightsCommand()
    {
        Suite(nameof(VerifyStream3RecoveryLights), () => VerifyStream3RecoveryLights(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Recovery lights:196native colors,588channel edits,72room edits,routing and legacy inheritance pass.");
        return 0;
    }

    /// <summary>Runs the Mother Brain attack-palette and paired rear-leg comparison.</summary>
    [VerificationCommand("--lookup-stream3-attack-palette")]
    internal static int LookupStream3AttackPaletteCommand()
    {
        Suite(nameof(VerifyStream3MotherBrainAttackPalette), () => VerifyStream3MotherBrainAttackPalette(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Mother Brain attack palette:15 native colors,45 edits,recovery independence and paired rear routing pass.");
        return 0;
    }

    /// <summary>Runs the decompressed intro-font tile comparison against the loaded atlas.</summary>
    [VerificationCommand("--lookup-stream3-intro-font")]
    internal static int LookupStream3IntroFontCommand()
    {
     VerifyStream3IntroFont(); return 0; 
    }

    /// <summary>Runs the decompressed intro background-page comparison.</summary>
    [VerificationCommand("--lookup-stream3-background-maps")]
    internal static int LookupStream3BackgroundMapsCommand()
    {
     VerifyStream3BackgroundMaps(); return 0; 
    }

    /// <summary>Runs the ordered Work Robot frame-registry and native-address comparison.</summary>
    [VerificationCommand("--lookup-stream3-work-robot-registry")]
    internal static int LookupStream3WorkRobotRegistryCommand()
    {
     VerifyStream3WorkRobotRegistry(); return 0; 
    }

    /// <summary>Runs the ordered Shaktool frame-registry and native-address comparison.</summary>
    [VerificationCommand("--lookup-stream3-shaktool-registry")]
    internal static int LookupStream3ShaktoolRegistryCommand()
    {
     VerifyStream3ShaktoolRegistry(); return 0; 
    }

    /// <summary>Runs the two Nintendo logo fade-entry mechanics comparisons.</summary>
    [VerificationCommand("--lookup-stream3-nintendo-fade-entries")]
    internal static int LookupStream3NintendoFadeEntriesCommand()
    {
     VerifyStream3NintendoFadeEntries(); return 0; 
    }

    /// <summary>Runs the opening portrait tilemap and actual VRAM upload comparison.</summary>
    [VerificationCommand("--lookup-stream3-portrait-map")]
    internal static int LookupStream3PortraitMapCommand()
    {
        Suite(nameof(VerifyStream3PortraitMap), () => VerifyStream3PortraitMap());
        return 0;
    }

    /// <summary>Runs the first narration map comparison against decompressed cartridge data.</summary>
    [VerificationCommand("--lookup-stream3-initial-narration")]
    internal static int LookupStream3InitialNarrationCommand()
    {
        Suite(nameof(VerifyStream3InitialNarrationMap), () => VerifyStream3InitialNarrationMap());
        return 0;
    }

    /// <summary>Runs the Baby Metroid reflected-sprite layout and schema checks.</summary>
    [VerificationCommand("--lookup-stream3-baby-sprite-reflection")]
    internal static int LookupStream3BabySpriteReflectionCommand()
    {
        Suite(nameof(VerifyStream3BabySpriteReflection), () => VerifyStream3BabySpriteReflection(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Baby sprite reflection:90 native parts/OAM,90 isolated X edits,27 field edits,order/count/empty layouts,hashes/bounds and64 legacy schemas pass.");
        return 0;
    }

    /// <summary>Runs the final-room paint and dependent-palette isolation checks.</summary>
    [VerificationCommand("--lookup-stream3-final-room-paints")]
    internal static int LookupStream3FinalRoomPaintsCommand()
    {
        var native = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStream3FinalRoomPaints), () => VerifyStream3FinalRoomPaints(native));
        Console.WriteLine("Final room:24native colors,72independent channel edits and dependent-palette isolation pass.");
        return 0;
    }

    /// <summary>Runs the menu sprite geometry and ordered OAM composition checks.</summary>
    [VerificationCommand("--lookup-stream3-menu-sprites")]
    internal static int LookupStream3MenuSpritesCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStream3MenuSpriteGeometry), () => VerifyStream3MenuSpriteGeometry(oracle));
        Console.WriteLine("Menu sprites: 15 native parts, 135 independent field edits, reordered/expanded compositions and shared cursor loaders pass.");
        return 0;
    }

    /// <summary>Runs the file-select sprite and border composition checks.</summary>
    [VerificationCommand("--lookup-stream3-file-select-sprites")]
    internal static int LookupStream3FileSelectSpritesCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStream3FileSelectSprites), () => VerifyStream3FileSelectSprites(oracle));
        return 0;
    }

    /// <summary>Runs the options-heading border and actual OAM draw checks.</summary>
    [VerificationCommand("--lookup-stream3-options-sprites")]
    internal static int LookupStream3OptionsSpritesCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStream3OptionsBorders), () => VerifyStream3OptionsBorders(oracle, SuperMetroid.AssetExtraction.Frontend.GameOptionsPresentationExtractor.Extract(oracle)));
        Console.WriteLine("Options headings: 144 native parts, 576 independent edits, reordered/expanded compositions and actual OAM draw pass.");
        return 0;
    }

    /// <summary>Runs the file-select border geometry, appearance, and ordering checks.</summary>
    [VerificationCommand("--lookup-stream3-file-select-borders")]
    internal static int LookupStream3FileSelectBordersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStream3FileSelectBorders), () => VerifyStream3FileSelectBorders(rom, FileSelectPresentationExtractor.Extract(rom)));
        Console.WriteLine("File-select borders:114native parts,456independent edits,reversed orders and expanded compositions pass;bounds,appearance and order remain required.");
        return 0;
    }

    /// <summary>Loads the seven grapple tile transfers and checks decoded atlas patterns.</summary>
    [VerificationCommand("--lookup-stream3-grapple-tile-patterns")]
    internal static int LookupStream3GrappleTilePatternsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            GrappleTileTransfer[] expectedTransfers =
            [
                new(VramAssetId.GrapplePointFirstTiles, 0x9a8200, 0, 32),
                new(VramAssetId.GrapplePointSecondTiles, 0x9a8400, 32, 32),
                new(VramAssetId.GrapplePointThirdTiles, 0x9a8600, 64, 32),
                new(VramAssetId.GrapplePointFourthTiles, 0x9a8800, 96, 32),
                new(VramAssetId.GrappleHorizontalSegmentTiles, 0x9a8220, 128, 128),
                new(VramAssetId.GrappleDiagonalSegmentTiles, 0x9a8a20, 256, 128),
                new(VramAssetId.GrappleVerticalSegmentTiles, 0x9a9220, 384, 128),
            ];
            VerifyStream3GrappleTilePatterns(rom, expectedTransfers);
        Console.WriteLine("Grapple tiles:512 native bytes,64 plane edits,8 coverage edits and seven transfer bindings pass; only reviewed stroke masks and visual shape choices remain.");
        return 0;
    }

    /// <summary>Runs the intro narration glyph-coordinate and line-layout comparison.</summary>
    [VerificationCommand("--lookup-narration-layout")]
    internal static int LookupNarrationLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var json = IntroNarrationExtractor.Extract(rom);
        Suite(nameof(VerifyStream3NarrationLayout), () => VerifyStream3NarrationLayout(rom, json, IntroNarrationPresentation.Load(new MemoryStream(json))));
        Console.WriteLine("Narration layout: 770 native glyph/coordinate records, six calculated pages, one retained hard break and 34 independent edits pass.");
        return 0;
    }

    /// <summary>Runs file-select patch cell, edit-isolation, and tilemap placement checks.</summary>
    [VerificationCommand("--lookup-file-select-patches")]
    internal static int LookupFileSelectPatchesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var data = SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyStream3FileSelectPatches), () => VerifyStream3FileSelectPatches(rom, data, FileSelectPresentation.Load(new MemoryStream(data))));
        Console.WriteLine("File-select patches: native cells, zero stock fallback, independent edits and actual placement pass.");
        return 0;
    }
}
