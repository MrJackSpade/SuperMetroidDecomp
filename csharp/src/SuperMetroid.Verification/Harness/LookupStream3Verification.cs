using System.Collections.Immutable;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Verification.Enemies.Shaktool;
using SuperMetroid.Verification.Enemies.Sparks;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.ColorWordsVerification;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.CreditsPresentationVerification;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Enemies.EnemyDefinitionsVerification;
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
using SuperMetroid.AssetExtraction.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Harness;

using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;
using SuperMetroid.AssetExtraction.Escape.Artwork;

using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
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
    /// <summary>Compares all 256 intro colors with ROM through the transfer bytes and colour lists.</summary>
    internal static void VerifyStream3IntroPaletteRows(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, SnesCgram.ColorCount)
            .Select(color => ReadVerificationWord(rom, IntroCinematicSourceDefinitions.Palette + 2 * color)).ToArray();
        IntroCinematicPalette stock = Load(native);
        Confirm(stock, native);
        Console.WriteLine("Intro palette rows:256 native colors reach the transfer bytes and colour lists.");
        static void Confirm(IntroCinematicPalette selected, ushort[] expected)
        {
            ReadOnlySpan<byte> bytes = ScreenColourWords.Words(selected.BackgroundColours.AsSpan(), selected.SpriteColours.AsSpan());
            for (int color = 0; color < expected.Length; color++)
                AssertEqual(expected[color], System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes[(color * 2)..]), "intro palette transfer word");
            // The opening's colours are plain lists the import pre-colours its sheets from (#1512): backgrounds, then sprites.
            AssertTrue(ToColors(expected).SequenceEqual([.. selected.BackgroundColours, .. selected.SpriteColours]),
                "intro palette colour lists");
        }
        static IntroCinematicPalette Load(ushort[] words)
        {
            var document = new IntroCinematicPaletteDocument
            {
                Version = IntroCinematicPaletteFile.Version,
                Colors = words.Select(word => new PaletteRgb5 { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 }).ToArray(),
            };
            using var json = new MemoryStream();
            IntroCinematicPaletteFile.Write(json, document);
            json.Position = 0;
            return IntroCinematicPaletteFile.Read(json);
        }
    }
    /// <summary>Checks four eye rectangles against ROM and their actual portrait tilemap writes.</summary>
    internal static void VerifyStream3IntroEyeRectangles(ISnesAddressSpace rom)
    {
        ushort[][] native = Enumerable.Range(0, 4).Select(frame => Enumerable.Range(0, 6)
            .Select(cell => ReadVerificationWord(rom, 0x8cd785 + 16 * frame + 2 * cell)).ToArray()).ToArray();
        IntroEyeTilemapPresentation stock = Load(native);
        Confirm(stock, native);
        AssertThrows<ArgumentOutOfRangeException>(() => stock.FrameTiles(-1).ToArray(), "negative eye frame");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.FrameTiles(4).ToArray(), "upper eye frame");
        Console.WriteLine("Intro eye rectangles:24 native cells, the content identity and bounds pass; actual portrait draw copies pass.");

        static void Confirm(IntroEyeTilemapPresentation selected, ushort[][] expected)
        {
            for (int frame = 0; frame < expected.Length; frame++)
                AssertTrue(expected[frame].SequenceEqual(BackgroundTileWordBytes.Words(selected.FrameTiles(frame))), $"eye rectangle {frame}");
            var objects = new IntroCinematicObjectSystem(new IntroNarrationText(), new IntroJapaneseSubtitles(enabled: false));
            IntroEyeFrame[] eyeFrames = [IntroEyeFrame.Open, IntroEyeFrame.HalfOpen, IntroEyeFrame.Closed, IntroEyeFrame.Deadpan];
            for (int frame = 0; frame < expected.Length; frame++)
            {
                // The portrait shows the frame's installed picture (#1467), composed from these cells.
                objects.ShowEyeFrame(eyeFrames[frame]);
                AssertEqual<IntroEyeFrame?>(eyeFrames[frame], objects.EyeFrame, "the portrait shows the eye frame the record names");
            }
            string identity = SelectedPresentationHash.Create(nameof(IntroEyeTilemapPresentation), content =>
            {
                content.Append("frames", expected.Length);
                foreach (ushort[] words in expected) content.AppendTiles("frame", BackgroundTileWordBytes.Tiles(words));
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
    /// <summary>Checks the 128 native final-line cells.</summary>
    internal static void VerifyStream3IntroDivider(ISnesAddressSpace rom)
    {
        var native = new ushort[IntroFinalLineTilemapFormat.CellCount];
        for (int index = 0; index < native.Length; index++)
            native[index] = ReadVerificationWord(rom, IntroCinematicSourceDefinitions.FinalTextLine + 2 * index);
        IntroFinalLineTilemap stock = Load(native);
        AssertTrue(native.SequenceEqual(BackgroundTileWordBytes.Words(stock.Tiles.Span)), "all128 native intro divider cells");
        Console.WriteLine("Japanese subtitle staging:128 native cells pass.");

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
    /// <summary>Compares options heading borders with ROM and OAM.</summary>
    internal static void VerifyStream3OptionsBorders(ISnesAddressSpace rom, byte[] imported)
    {
        GameOptionsPresentationDocument Read() => RequiredJson.Deserialize<GameOptionsPresentationDocument>(imported, ArtworkJson.Options);
        Dictionary<GameOptionsPage, SpriteComposition> Load(GameOptionsPresentationDocument document)
        {
            var value = GameOptionsSourcePresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
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
            string name = GameOptionsNames.HeadingSpriteName(page);
            var original = Read();
            var stock = Load(original)[page];
            AssertEqual(count, stock.PartCount, "native perimeter count");
            Confirm(stock, original.Sprites[name]);
            var presentation = GameOptionsSourcePresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(original, ArtworkJson.Options)));
            string anchorKey = GameOptionsPresentationFormat.MenuName(page);
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            presentation.Presentation.DrawHeading(actualOam, MenuSheetFixtures.GameOptions(presentation), page, 3);
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
                AssertEqual(NativeOwnerPalettePart(rom, entry), part.Part, "native border offset, size, character, priority and mirroring with inherited palette");
                AssertTrue(part.InheritPalette, "border palette remains owner supplied");
            }
        }
    }

    /// <summary>Checks file-select border geometry and ordered sprite parts against ROM.</summary>
    internal static void VerifyStream3FileSelectBorders(ISnesAddressSpace rom, byte[] imported)
    {
        FileSelectPresentationDocument Read() => RequiredJson.Deserialize<FileSelectPresentationDocument>(imported, ArtworkJson.Options);
        Dictionary<FileSelectBorder, SpriteComposition> Load(FileSelectPresentationDocument document)
        {
            var value = FileSelectSourcePresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
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
            string name = FileSelectParts.BorderSpriteName(border);
            var original = Read();
            var stock = Load(original)[border];
            AssertEqual(count, stock.PartCount, "native perimeter count");
            Confirm(stock, original.Sprites[name]);
            var presentation = FileSelectSourcePresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(original, ArtworkJson.Options)));
            string anchorKey = FileSelectPresentationFormat.BorderName(border);
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            presentation.Presentation.DrawBorder(actualOam, MenuSheetFixtures.FileSelect(presentation), border);
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
                AssertEqual(NativeOwnerPalettePart(rom, entry), part.Part, "native border offset, size, character, priority and mirroring with inherited palette");
                AssertTrue(part.InheritPalette, "border palette remains owner supplied");
            }
        }
    }

    /// <summary>
    /// The piece the native five-byte spritemap record at <paramref name="entry"/> describes, decoded by the import
    /// pipeline's record codec, with the palette row cleared because the drawing owner supplies the palette.
    /// </summary>
    private static SpritePart NativeOwnerPalettePart(ISnesAddressSpace rom, int entry) =>
        new SpritemapPart(new SnesSpritemapXWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry)),
            new SnesSpritemapYByte(rom.ReadCartridgeByte(entry + 2)),
            new SnesObjAttributeWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry + 3)).WithPaletteIndex(0)).ToPart();

    /// <summary>Decodes the seven grapple transfers and checks derived tile pixels against ROM.</summary>
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
        AssertTrue(!stock.TryResolve(transfers[0].SourceAddress, 31, out _), "stream 3 Grapple rejects partial transfer");
    }

    /// <summary>Checks narration glyph coordinates and line breaks against native records.</summary>
    internal static void VerifyStream3NarrationLayout(ISnesAddressSpace rom, IntroNarrationPresentation stock)
    {
        int nativeCharacters = 0;
        foreach (var source in IntroNarrationScriptSources.Pages)
        {
            var expected = new List<IntroNarrationCharacter>();
            int cursor = 0x8c0000 | (source.InstructionPointer + 8);
            while ((CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor) & 0x8000) == 0)
            {
                ushort position = CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 2));
                ushort glyph = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x8c0000 | (CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor + 4) + 4)));
                BackgroundTile glyphTile = BackgroundTileWordBytes.Tile(glyph);
                expected.Add(new(LittleEndianBytes.LowByte(position), LittleEndianBytes.HighByte(position), IntroNarrationGlyphTiles.DecodeGlyph(glyphTile)));
                cursor += 6;
            }
            AssertTrue(stock.Compile(source.Id).SequenceEqual(expected), "installed narration matches every native glyph/column/row record");
            nativeCharacters += expected.Count;
            AssertThrows<ArgumentOutOfRangeException>(() => _ = stock.GetLines(source.Id)[-1], "narration line lower bound");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = stock.GetLines(source.Id)[stock.GetLines(source.Id).Count], "narration line upper bound");
        }
        AssertEqual(770, nativeCharacters, "six native narration pages contain770 characters");
    }

    /// <summary>Checks six Mochtroid frame registrations and their calculated native spritemap pointers.</summary>
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
    /// <summary>Checks menu compositions against native OAM parts.</summary>
    internal static void VerifyStream3MenuSpriteGeometry(ISnesAddressSpace rom)
    {
        static Dictionary<string, SpriteComposition> Sprites(GameOverSourcePresentation presentation) => new(
            [.. Enum.GetValues<GameOverBabyFrame>().Select(frame =>
                KeyValuePair.Create(GameOverPresentationNames.BabyFrameName(frame), presentation.babySprites[frame])),
            KeyValuePair.Create(GameOverPresentationDefinitions.EggFrame, presentation.eggSprite),
            .. Enumerable.Range(0, GameOverPresentationDefinitions.CursorFrameCount).Select(frame =>
                KeyValuePair.Create(GameOverPresentationNames.CursorFrameName(frame), presentation.cursorSprites[frame]))]);
        static void Confirm(SpriteComposition actual, SpriteVisualPart[] expectedParts)
        {
            var expected = MenuSpriteCompiler.Compile(expectedParts, "native menu composition");
            AssertEqual(expected.PartCount, actual.PartCount, "menu composition exact part count");
            for (int part = 0; part < expected.PartCount; part++)
                AssertEqual(expected.Part(part), actual.Part(part), "menu composition exact ordered geometry, tiles, attributes and palette inheritance");
        }
        byte[] imported = SuperMetroid.AssetExtraction.Frontend.GameOverPresentationExtractor.Extract(rom);
        GameOverPresentationDocument Read() => RequiredJson.Deserialize<GameOverPresentationDocument>(imported, ArtworkJson.Options);
        GameOverSourcePresentation Load(GameOverPresentationDocument document) => GameOverSourcePresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        var original = Read();
        var stock = Sprites(Load(original));
        // The stock presentation draws the installation's sheet, composed from it.
        SpriteSheet GameOverSheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.GameOver);
        var presentation = Load(original);
        List<GameOverSpriteDraw> draws =
        [
            .. Enum.GetValues<GameOverBabyFrame>().Select(frame => new GameOverSpriteDraw(GameOverPresentationNames.BabyFrameName(frame),
                oam => presentation.Presentation.DrawBaby(oam, GameOverSheet, frame, GameOverBabyPalette.Idle), original.BabyAnchor.X, original.BabyAnchor.Y, presentation.BabyPaletteIndex)),
            new(GameOverPresentationDefinitions.EggFrame, oam => presentation.Presentation.DrawEgg(oam, GameOverSheet), original.BabyAnchor.X, original.BabyAnchor.Y, presentation.EggPaletteIndex),
            .. Enumerable.Range(0, GameOverPresentationDefinitions.CursorFrameCount).Select(frame => new GameOverSpriteDraw(
                GameOverPresentationNames.CursorFrameName(frame), oam => presentation.Presentation.DrawCursor(oam, GameOverSheet, frame, false),
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
        foreach (string name in GameOverPresentationFormat.SpriteNames)
        {
            Confirm(stock[name], original.Sprites[name]);
            // A part whose footprint leaves the sheet is a malformed document the import rejects.
            var malformed = Read();
            var first = malformed.Sprites[name][0];
            malformed.Sprites[name][0] = first with { TileColumn = MapSpriteFormat.TileColumns - first.Size / 8 + 1 };
            AssertThrows<InvalidDataException>(() => Load(malformed), "menu part footprint outside the sheet is rejected");
        }
        ConfirmCursorCaller(SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(rom), source => FileSelectSourcePresentation.Load(source).cursorSprites);
        ConfirmCursorCaller(SuperMetroid.AssetExtraction.Frontend.GameOptionsPresentationExtractor.Extract(rom), source => GameOptionsSourcePresentation.Load(source).cursorSprites);

        void ConfirmCursorCaller(byte[] json, Func<Stream, SpriteComposition[]> load)
        {
            var presentation = load(new MemoryStream(json));
            for (int frame = 0; frame < 4; frame++)
            {
                string name = GameOverPresentationNames.CursorFrameName(frame);
                var node = RequiredJson.Parse(json);
                var parts = RequiredJson.Deserialize<SpriteVisualPart[]>(node.Required("sprites").Required(name).ToJsonString(), ArtworkJson.Options);
                Confirm(presentation[frame], parts);
            }
        }
    }

    /// <summary>Checks the ordered named frame registry and derived room-sprite additions without duplicate identities.</summary>
    internal static void VerifyStream3EnemyFrameRegistration(ISnesAddressSpace rom)
    {
        // Ordered bank:pointer:name snapshot of the 472 source registrations at commit 723df7b36, under the names they had
        // before frames were named for their meaning (#1465).
        const string originalNamedIdentity = "421A60158BA376F9A6AD58EA817315553AD268A44B411B87A4B25059EE5DFE92";
        var named = EnemySpritemapDefinitions.Frames.Take(472).ToArray();
        string identity = string.Join("|", named.Select(frame => $"{frame.Bank:x2}:{frame.Pointer:x4}:{frame.LegacyName}"));
        AssertEqual(originalNamedIdentity, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(identity))), "enemy named registration exact ordered source identities");
        var all = EnemySpritemapDefinitions.Frames.ToArray();
        AssertEqual(all.Length, EnemySpritemapDefinitions.Frames.Count, "enemy calculated registry count");
        AssertTrue(EnemySpritemapDefinitions.Frames.Take(472).SequenceEqual(named), "enemy legacy named prefix slice");
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
    }

    /// <summary>Checks file-select patch cells, terminators and stock placement against ROM.</summary>
    internal static void VerifyStream3FileSelectPatches(ISnesAddressSpace rom, byte[] imported, FileSelectSourcePresentation stock)
    {
        FileSelectPresentationDocument Read() => RequiredJson.Deserialize<FileSelectPresentationDocument>(imported, ArtworkJson.Options);
        foreach ((FileSelectPatch patch, int address, int count) in new[]
        {
            new VerifyStream3FileSelectPatchesCase(FileSelectPatch.Energy, 0x81b496, 4),
            new VerifyStream3FileSelectPatchesCase(FileSelectPatch.TimeColon, 0x81b4a8, 1),
            new VerifyStream3FileSelectPatchesCase(FileSelectPatch.NoData, 0x81b4ac, 11),
        })
        {
            string name = FileSelectParts.PatchName(patch);
            FileSelectCompiledPatch Compiled(FileSelectSourcePresentation value) => value.patches[patch];
            AssertEqual(count, Compiled(stock).Count, "file-select installed patch native cell count " + name);
            for (int index = 0; index < count; index++)
            {
                ushort word = CartridgeImportSource.Require(rom).ReadCartridgeWord(address + index * 2);
                AssertEqual(new FileSelectCompiledPatchCell(new(checked((short)index), 0), BackgroundTileWords.ToTile(word)), Compiled(stock).Cell(index),
                    "file-select installed patch exact native cell");
            }
            AssertEqual((byte)0xff, rom.ReadCartridgeByte(address + count * 2), "file-select native patch terminator low");
            AssertEqual((byte)0xff, rom.ReadCartridgeByte(address + count * 2 + 1), "file-select native patch terminator high");
            Confirm(stock, Read().Patches[name].Cells);

            void Confirm(FileSelectSourcePresentation presentation, FileSelectPatchCellDocument[] cells)
            {
                var expected = new ushort[1024];
                var actual = new BackgroundTile[1024];
                Array.Fill(actual, BackgroundTileWords.ToTile(0));
                var compiled = Compiled(presentation);
                AssertEqual(cells.Length, compiled.Count, "file-select supplied patch count");
                for (int index = 0; index < cells.Length; index++)
                {
                    var source = cells[index];
                    var cell = source.Cell;
                    ushort word = SnesBgTilemapWord.Create(cell.TileRow * MapTileAtlasFormat.TileColumns + cell.TileColumn,
                        cell.Palette, cell.Priority, (cell.FlipX ? TileFlip.Horizontal : TileFlip.None) |
                        (cell.FlipY ? TileFlip.Vertical : TileFlip.None)).Raw;
                    AssertEqual(new FileSelectCompiledPatchCell(source.Offset - Position16<CharacterCells>.Zero, BackgroundTileWords.ToTile(word)), compiled.Cell(index),
                        "file-select supplied order and coordinates");
                    expected[(3 + source.Y) * 32 + 4 + source.X] = word;
                }
                presentation.ApplyPatch(actual, patch, new Position16<CharacterCells>(4, 3));
                AssertTrue(BackgroundTileWordBytes.Words(actual).SequenceEqual(expected), "file-select exact stock patch placement");
            }
        }
    }
    /// <summary>Checks game-over text words in VRAM for the native tilemap cells, including index bounds.</summary>
    private static void VerifyStream3GameOverText(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Frontend.GameOverPresentationExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<GameOverPresentationDocument>(imported, ArtworkJson.Options);
        GameOverSourcePresentation Load() => GameOverSourcePresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        var stock = Load();
        AssertTrue(stock.page.SequenceEqual(Enumerable.Range(0, GameOverPageDefinitions.TilemapCellCount).Select(GameOverText.TilemapTile)),
            "the stock game-over page is the spelled game-over text");
        Confirm(stock);
        AssertThrows<ArgumentOutOfRangeException>(() => GameOverText.TilemapTile(-1), "game-over text negative cell");
        AssertThrows<ArgumentOutOfRangeException>(() => GameOverText.TilemapTile(1024), "game-over text final cell boundary");

        void Confirm(GameOverSourcePresentation presentation)
        {
            ushort[] words = BackgroundTileWordBytes.Words(presentation.page);
            for (int cell = 0; cell < document.Tilemap.Length; cell++)
            {
                MapPresentationCell source = document.Tilemap[cell];
                ushort expected = SnesBgTilemapWord.Create(source.TileRow * MapTileAtlasFormat.TileColumns + source.TileColumn,
                    source.Palette, source.Priority, (source.FlipX ? TileFlip.Horizontal : TileFlip.None) |
                    (source.FlipY ? TileFlip.Vertical : TileFlip.None)).Raw;
                AssertEqual(expected, words[cell], "game-over native text word and placement");
            }
        }
    }

    /// <summary>Checks options anchors, labels, language regions, toggles, and heading draw placement against ROM.</summary>
    private static void VerifyStream3OptionsGeometry(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Frontend.GameOptionsPresentationExtractor.Extract(rom);
        GameOptionsPresentationDocument ReadDocument() => RequiredJson.Deserialize<GameOptionsPresentationDocument>(
            imported, ArtworkJson.Options);
        GameOptionsSourcePresentation Load(GameOptionsPresentationDocument document) => GameOptionsSourcePresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        var stock = ReadDocument();
        var loaded = Load(stock);
        for (int action = 0; action < 7; action++)
        {
            ushort offset = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x82f639 + action * 2));
            AssertEqual(new Position16<CharacterCells>(checked((ushort)(offset / 2 % 32)), checked((ushort)(offset / 2 / 32))),
                loaded.controllerLabelAnchors[action], "native options label anchor");
        }
        foreach ((GameOptionsPage page, int address, int count) in new[]
        {
            new VerifyStream3OptionsBordersCase(GameOptionsPage.Primary, 0x82f307, 5),
            new VerifyStream3OptionsBordersCase(GameOptionsPage.Controller, 0x82f31b, 9),
            new VerifyStream3OptionsBordersCase(GameOptionsPage.Special, 0x82f33f, 3),
        })
            for (int row = 0; row < count; row++)
                AssertEqual(new Position16<SpritePixels>(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + row * 4)), CartridgeImportSource.Require(rom).ReadCartridgeWord((address + row * 4 + 2))),
                    loaded.Presentation.CursorPosition(page, row), "native options cursor anchor");
        for (int button = 0; button < 7; button++)
        for (int cell = 0; cell < 6; cell++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x82f659 + button * 12 + cell * 2)),
                BackgroundTileWords.ToWord(loaded.controllerLabels[GameOptionsNames.ControllerLabelButton(button)][cell]),
                "native options glyph composition preserves each tile and flip");
        Confirm(stock, loaded);

        void Confirm(GameOptionsPresentationDocument document, GameOptionsSourcePresentation presentation)
        {
            foreach (bool japanese in new[] { false, true })
            {
                byte[] actual = presentation.CreatePageBytes(GameOptionsScreen.Primary);
                byte[] expected = (byte[])actual.Clone();
                foreach (var region in document.LanguageRegions)
                    Paint(expected, region.Cells, japanese == region.HighlightWhenJapanese ? document.SelectedPalette : document.UnselectedPalette);
                presentation.ApplyLanguage(actual, japanese);
                AssertTrue(actual.SequenceEqual(expected), "options language boxes preserve exact stock cells");
            }
            foreach (GameOptionsToggle toggle in GameOptionsPresentationFormat.Toggles)
            foreach (bool enabled in new[] { false, true })
            {
                GameOptionsToggleVisualDocument visual = document.SpecialToggles[GameOptionsNames.ToggleName(toggle)];
                byte[] actual = presentation.CreatePageBytes(GameOptionsScreen.SpecialEnglish);
                byte[] expected = (byte[])actual.Clone();
                Paint(expected, visual.EnabledCells, enabled ? document.SelectedPalette : document.UnselectedPalette);
                Paint(expected, visual.DisabledCells, enabled ? document.UnselectedPalette : document.SelectedPalette);
                presentation.ApplySpecialToggle(actual, toggle, enabled);
                AssertTrue(actual.SequenceEqual(expected), "options toggle boxes preserve exact stock cells");
            }
            for (int action = 0; action < 7; action++)
            for (int button = 0; button < 7; button++)
            {
                byte[] actual = presentation.CreatePageBytes(GameOptionsScreen.ControllerEnglish);
                byte[] expected = (byte[])actual.Clone();
                Position16<CharacterCells> point = document.ControllerLabelAnchors[action];
                for (int cell = 0; cell < 6; cell++)
                {
                    MapPresentationCell source = document.ControllerLabels[GameOptionsNames.ControllerLabelName(GameOptionsNames.ControllerLabelButton(button))][cell];
                    ushort word = SnesBgTilemapWord.Create(source.TileRow * MapTileAtlasFormat.TileColumns + source.TileColumn, source.Palette, source.Priority,
                        (source.FlipX ? TileFlip.Horizontal : TileFlip.None) |
                        (source.FlipY ? TileFlip.Vertical : TileFlip.None)).Raw;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(((point.Y + cell / 3) * 32 + point.X + cell % 3) * 2), word);
                }
                presentation.ApplyControllerLabel(actual, action, button);
                AssertTrue(actual.SequenceEqual(expected), "options controller boxes preserve exact stock placement");
            }
            foreach (GameOptionsPage page in GameOptionsPresentationFormat.MenuPages)
            {
                Position16<SpritePixels>[] cursors = document.CursorAnchors[GameOptionsPresentationFormat.MenuName(page)];
                for (int row = 0; row < cursors.Length; row++)
                    AssertEqual(cursors[row], presentation.Presentation.CursorPosition(page, row), "options stock cursor");
            }
            foreach (GameOptionsPage page in GameOptionsPresentationFormat.MenuPages)
            {
                Position16<SpritePixels> heading = document.HeadingAnchors[GameOptionsPresentationFormat.MenuName(page)];
                var actual = ScreenSpritesFixtures.Blank();
                var expected = new OamBuffer();
                presentation.Presentation.DrawHeading(actual, MenuSheetFixtures.GameOptions(presentation), page, 3);
                MenuSpriteCompiler.Compile(document.Sprites[GameOptionsNames.HeadingSpriteName(page)], "heading oracle")
                    .DrawOnScreen(expected, new(checked(heading.X), unchecked((ushort)(heading.Y - 3))),
                        SnesObjAttributeWord.Create(0, document.CursorPalette, 0).PaletteBits);
                actual.FinishFrame(); expected.FinalizeFrame();
                AssertTrue(actual.ImageEquals(expected),
                    "options stock heading origin and scroll");
            }
            AssertThrows<ArgumentOutOfRangeException>(() => presentation.ApplyControllerLabel(new byte[2048], -1, 0), "options invalid action");
            AssertThrows<ArgumentOutOfRangeException>(() => presentation.Presentation.CursorPosition((GameOptionsPage)byte.MaxValue, 0), "options invalid page");
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
        // DoorSetupCode's values are arbitrary (#1414): zero and values far past the last member name no routine.
        foreach (ushort undefined in new ushort[] { 0x0000, 0xb972, 0xffff })
            AssertThrows<InvalidOperationException>(
                () => SuperMetroid.Core.Rooms.Doors.DoorScrollPrograms.Contains((DoorSetupCode)undefined),
                "stream 3 undefined door setup code");
        // Each callback runs over a grid of red cells and one of blue cells, so every write differs from the cells it
        // replaces in at least one run; the native writes are compared as the cells the camera reads them as.
        foreach (DoorSetupCode pointer in expectedDoorCallbacks)
        foreach (RoomScrollState fill in new[] { RoomScrollState.RedBoundary, RoomScrollState.Blue })
        {
            byte[] expected = Enumerable.Repeat(fill.ToStorageByte(), RoomScrollGrid.KeptCellCount).ToArray();
            var actual = RoomScrollGrid.LoadNativeTable(expected, new Extent16<RoomScreens>(10, 5));
            ExecuteStream3DoorScroll(rom, DoorIdentityWords.SetupRoutines.Word(pointer), expected);
            AssertTrue(SuperMetroid.Core.Rooms.Doors.DoorScrollPrograms.TryApply(pointer, actual), "stream 3 callback recognized");
            AssertTrue(actual.KeptStates.SequenceEqual(RoomScrollStates.FromStorageBytes(expected)),
                "stream 3 native ordered door writes and all untouched cells");
        }
        var unchangedScroll = RoomScrollGrid.LoadNativeTable(new byte[RoomScrollGrid.KeptCellCount], new Extent16<RoomScreens>(10, 5));
        AssertTrue(!SuperMetroid.Core.Rooms.Doors.DoorScrollPrograms.TryApply(DoorSetupCode.None, unchangedScroll), "stream 3 setup without scroll writes is a no-op");
        for (int pair = 0; pair < 16; pair++)
        {
            int address = 0xadde5f + 2 * pair;
            int pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            var expected = pointer switch
            {
                0xe1a6 => MotherBrainBeamDirection.Down,
                0xde7f => MotherBrainBeamDirection.Right,
                0xdf6e => MotherBrainBeamDirection.Up,
                0xde5e => MotherBrainBeamDirection.Retain,
                0 => MotherBrainBeamDirection.Unsupported,
                _ => throw new InvalidDataException("Unexpected native beam quadrant target."),
            };
            AssertEqual(expected, MotherBrainBeamQuadrants.DirectionForPair(pair),
                "stream 3 native beam quadrant dispatcher including null and retaining entries");
        }
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainBeamQuadrants.DirectionForPair(invalid),
                "stream 3 beam dispatcher preserves span index bounds");
        // The cartridge's four crossfade routines at the native boundary, each region the cartridge position of its first colour
        // and its colour count: the narration's colours on the way out and on the way back, the flashback's and the
        // laboratory's. Each crossfaded scene the import shades is the union of its routines' regions (#1597).
        (int FirstColor, int ColorCount)[][] expectedIntroRegions =
        [
            [new(0, 20), new(48, 16), new(233, 6)],
            [new(0, 16), new(48, 16), new(233, 6)],
            [new(20, 3), new(112, 16), new(192, 32), new(240, 16)],
            [new(32, 16), new(224, 9)],
        ];
        IntroCrossfadeScene[] introRoutineScenes =
            [IntroCrossfadeScene.Narration, IntroCrossfadeScene.Narration, IntroCrossfadeScene.Flashback, IntroCrossfadeScene.Laboratory];
        int[][] nativeIntroOperands =
        [ [0x8bb258, 0x8bb261, 0x8bb26a], [0x8bb3c8, 0x8bb3d1, 0x8bb3da],
          [0x8bb273, 0x8bb27c, 0x8bb285, 0x8bb28e], [0x8bb2f5, 0x8bb2fe] ];
        for (int group = 0; group < expectedIntroRegions.Length; group++)
        {
            AssertEqual(expectedIntroRegions[group].Length, nativeIntroOperands[group].Length, "stream 3 intro routine region count");
            for (int index = 0; index < nativeIntroOperands[group].Length; index++)
            {
                int address = nativeIntroOperands[group][index];
                AssertEqual((byte)0xa2, rom.ReadCartridgeByte(address), "stream 3 intro native LDX region");
                AssertEqual((byte)0xa0, rom.ReadCartridgeByte(address + 3), "stream 3 intro native LDY color count");
                // The native X operand is the first color's byte offset into the two-byte palette entries.
                AssertEqual(expectedIntroRegions[group][index].FirstColor * 2,
                    CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 1), "stream 3 intro native offset");
                AssertEqual(expectedIntroRegions[group][index].ColorCount,
                    CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 4), "stream 3 intro native count");
            }
            foreach (int invalid in new[] { -1, expectedIntroRegions[group].Length, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => _ = expectedIntroRegions[group][invalid], "stream 3 intro region bounds");
        }
        var introSceneColours = new HashSet<int>();
        foreach (IntroCrossfadeScene scene in Enum.GetValues<IntroCrossfadeScene>())
        {
            int[] shaded = [.. IntroSceneColours(scene)];
            int[] routines = [.. Enumerable.Range(0, expectedIntroRegions.Length).Where(group => introRoutineScenes[group] == scene)
                .SelectMany(group => expectedIntroRegions[group]).SelectMany(region => Enumerable.Range(region.FirstColor, region.ColorCount)).Distinct().Order()];
            AssertTrue(shaded.SequenceEqual(routines), $"stream 3 intro {scene} shades exactly the colours its native routines move");
            foreach (int colour in shaded)
                AssertTrue(introSceneColours.Add(colour), $"stream 3 intro colour {colour} belongs to one crossfaded scene only");
        }
        // The cartridge positions of the colours the import's crossfade shades of a scene move, in rising order (#1512, #1597).
        static IEnumerable<int> IntroSceneColours(IntroCrossfadeScene scene) =>
            IntroSheetInks.Background(scene).Concat(IntroSheetInks.Sprite(scene).Select(ink => ImageColourGroup.Count * ImageColourGroup.Colours + ink)).Order();
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
            new("mother-brain-leg-tiles.png", 8),
            new("mother-brain-baby-tiles.png", 4),
            new("mother-brain-attack-tiles.png", 4),
            new("mother-brain-exploded-door-tiles.png", 2),
        ];
        AssertTrue(expectedSheets.SequenceEqual(MotherBrainSpecialSpriteSheetDefinitions.All),
            "stream 3 named special sheet order and every field");
        AssertTrue(MotherBrainSpecialSpriteSheetDefinitions.All.Select(sheet => (sheet.Sheet, sheet.FirstCharacter)).SequenceEqual(
            [(MotherBrainSpriteSheet.Legs, 320), (MotherBrainSpriteSheet.BabyMetroid, 448),
             (MotherBrainSpriteSheet.Attack, 448), (MotherBrainSpriteSheet.ExplodedDoor, 256)]),
            "stream 3 special sheet identities and first characters");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MotherBrainSpecialSpriteSheetDefinitions.All[invalid],
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
        // The HUD's door fade shades (#1512) fade every HUD ink but those the door keeps lit: an ink a shade leaves out is
        // one the native copy writes into the fade's target.
        const int hudInks = 32;
        IReadOnlyList<SheetShade> hudShades = DoorFadeHudShades.Of(ToColors(sourceColors).AsSpan(0, hudInks));
        int[] KeptBy(PictureShade shade) =>
            [.. Enumerable.Range(0, hudInks).Except(hudShades.Single(hud => hud.Shade == shade).Inks)];
        int[] keptByEveryDoor = KeptBy(DoorFadeShades.HudFadingOut(0));
        int[] keptBetweenCommonRooms = [.. KeptBy(DoorFadeShades.HudFadingOutBetweenCommonRooms(0)).Except(keptByEveryDoor)];
        AssertTrue(KeptBy(DoorFadeShades.HudFadingIn(0)).SequenceEqual(keptByEveryDoor) &&
            KeptBy(DoorFadeShades.HudFadingInBetweenCommonRooms(0)).Except(keptByEveryDoor).SequenceEqual(keptBetweenCommonRooms),
            "stream 3 door fades in keep the HUD inks their fades out keep");
        // The first two copies keep the HUD's colours, the third the escape timer's, checked below.
        for (int group = 0; group < 2; group++)
        {
            // The fade target the copy writes into; slots it leaves alone stay black.
            RgbColor[] actual = ToColors(new ushort[256]);
            ushort[] expected = new ushort[256];
            foreach (int instruction in nativeDoorCopies[group])
            {
                AssertEqual((byte)0x8d, rom.ReadCartridgeByte(instruction), "stream 3 native door color store opcode");
                int destination = CartridgeImportSource.Require(rom).ReadCartridgeWord(instruction + 1);
                int color = (destination - targetPaletteBase) / 2;
                expected[color] = sourceColors[color];
            }
            int[] kept = group == 0 ? keptByEveryDoor : keptBetweenCommonRooms;
            foreach (int position in kept)
                actual[position] = ToColors(sourceColors)[position];
            AssertTrue(ToColors(expected).SequenceEqual(actual), "stream 3 exact native door fade preserved and black slots");
        }
        VerifyEscapeTimerKeptThroughDoorFade(rom, nativeDoorCopies[2], targetPaletteBase);
        // Confirm the replaced selector for its complete ushort input domain.
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            int address = 0x9bc346 + 2 * (angle >> 10);
            int expected = (0x9a0000|CartridgeImportSource.Require(rom).ReadCartridgeWord(address));
            var asset = GrappleTileSelection.SegmentAssetFor(RotationAngle.FromRaw((ushort)angle));
            AssertEqual(expected, GrappleTileSelection.TransferFor(asset).SourceAddress,
                "stream 3 native grapple angle sector");
        }

        GrappleTileTransfer[] expectedTransfers =
        [
            new(InstalledArtworkId.GrapplePointFirstTiles, 0, 32),
            new(InstalledArtworkId.GrapplePointSecondTiles, 32, 32),
            new(InstalledArtworkId.GrapplePointThirdTiles, 64, 32),
            new(InstalledArtworkId.GrapplePointFourthTiles, 96, 32),
            new(InstalledArtworkId.GrappleHorizontalSegmentTiles, 128, 128),
            new(InstalledArtworkId.GrappleDiagonalSegmentTiles, 256, 128),
            new(InstalledArtworkId.GrappleVerticalSegmentTiles, 384, 128),
        ];
        int[] expectedSources = [0x9a8200, 0x9a8400, 0x9a8600, 0x9a8800, 0x9a8220, 0x9a8a20, 0x9a9220];
        var actualTransfers = GrappleTileDefinitions.Transfers;
        AssertTrue(expectedTransfers.SequenceEqual(actualTransfers),
            "stream 3 grapple transfer enumeration and every field");
        for (int index = 0; index < expectedTransfers.Length; index++)
        {
            AssertEqual(expectedTransfers[index], actualTransfers[index],
                "stream 3 grapple transfer index");
            AssertEqual(expectedTransfers[index], GrappleTileSelection.TransferFor(expectedTransfers[index].Asset),
                "stream 3 grapple transfer asset dispatch");
            AssertEqual(expectedSources[index], actualTransfers[index].SourceAddress, "stream 3 grapple transfer cartridge source");
        }
        AssertThrows<InvalidDataException>(() => GrappleTileSelection.TransferFor((InstalledArtworkId)(-1)),
            "stream 3 invalid grapple asset");

        for (int parameter = 0; parameter < 3; parameter++)
        {
            int offset = 2 * parameter;
            ushort list = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa8e682 + offset);
            var function = SparksNativeWords.SparkEnemyFunctions.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa8e688 + offset), "Spark function");
            SparkMovementDefinition definition = SuperMetroid.AssetExtraction.Enemies.SpawnWordDecodes.SparkStart(parameter);
            AssertEqual(list, NativeEnemyAnimationOracle.EntryOf(SparkNativeListAddresses.Entries, definition.Program),
                "stream 3 native Spark list selector");
            AssertEqual(function, definition.Function, "stream 3 native Spark function selector");
        }
        for (int bucket = 0; bucket <= ushort.MaxValue; bucket++)
        {
            ushort direction = (ushort)bucket;
            if ((bucket & 31) != 0 || bucket > 224)
                AssertThrows<InvalidDataException>(() => ShaktoolPrograms.ForOrientationBucket(direction),
                    "stream 3 invalid Shaktool orientation");
            else
            {
                int address = 0xaadd15 + 2 * (bucket >> 5);
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                AssertEqual(expected, NativeEnemyAnimationOracle.EntryOf(ShaktoolNativeListAddresses.Entries, ShaktoolPrograms.ForOrientationBucket(direction)),
                    "stream 3 native Shaktool orientation");
            }
        }
        for (int segment = 0; segment < 7; segment++)
        {
            int offset = segment * 2;
            ushort collision = CartridgeImportSource.Require(rom).ReadCartridgeWord(0xaadf13 + offset);
            AssertEqual(collision, NativeEnemyAnimationOracle.EntryOf(ShaktoolNativeListAddresses.Entries, ShaktoolPrograms.CollisionForSegment(segment)),
                "stream 3 native Shaktool collision program");
        }
        foreach (int invalid in new[] { -1, 7, int.MinValue, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => ShaktoolPrograms.CollisionForSegment(invalid),
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
        var grappleSprites = InstalledGrappleSprites.Load(new MemoryStream(grappleSpriteJson));
        AssertTrue(Enumerable.Range(0, GrappleSpriteDefinitions.SegmentFrameCount)
                .Select(SuperMetroid.AssetExtraction.Samus.GrappleSpriteExtractor.SegmentAttributeAddress).SequenceEqual(GrappleSegmentAttributeAddresses),
            "stream 3 segment address enumeration");
        for (int frame = 0; frame < 4; frame++)
        {
            int address = 0x94b18d + 4 * frame;
            // The segment appearance has no packed word: the native attribute word is decoded by the import pipeline's
            // codec and compared by character, palette row, priority and mirroring.
            AssertEqual(new SnesObjAttributeWord(CartridgeImportSource.Require(rom).ReadCartridgeWord(address)).ToSinglePart(),
                grappleSprites.Segment(frame), "stream 3 calculated native rope appearance");
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => SuperMetroid.AssetExtraction.Samus.GrappleSpriteExtractor.SegmentAttributeAddress(invalid), "stream 3 rope operand address bounds");
            AssertThrows<InvalidDataException>(() => grappleSprites.Segment(invalid), "stream 3 rope attribute bounds");
        }
        var swingFrames = GrappleSwingFrameFile.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.Samus.GrappleSwingFrameExtractor.Extract(rom)));
        for (int angle = 0; angle < 256; angle++)
            AssertEqual(rom.ReadCartridgeByte(0x9bc1c2 + angle), swingFrames.Resolve(new TrigonometryAngle((byte)angle)), "stream 3 native calculated swing art frame");
        byte[] fileSelectJson = SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(rom);
        var fileSelect = FileSelectSourcePresentation.Load(new MemoryStream(fileSelectJson));
        var glyphTilemap = new BackgroundTile[1024];
        for (int glyph = 0; glyph < 13; glyph++)
        {
            bool digit = glyph < 10;
            int index = digit ? glyph : glyph - 10;
            ushort expected = (ushort)(digit ? 0x2060 + index : 0x206a + index);
            var anchor = new Position16<CharacterCells>(4, 3);
            if (digit) fileSelect.WriteDigit(glyphTilemap, anchor, 2, index);
            else fileSelect.WriteSlotLetter(glyphTilemap, anchor, index);
            AssertEqual(expected, BackgroundTileWords.ToWord(glyphTilemap[3 * 32 + 4 + (digit ? 2 : 0)]), "stream 3 actual file-select calculated glyph write");
        }
        string[] expectedCreditRoles = ["staff-heading", "producer-heading", "producer-name", "director-heading", "director-name", "background-designers-heading", "background-designer-1", "background-designer-2", "background-designer-3", "object-designers-heading", "object-designer-1", "object-designer-2", "samus-original-designer-heading", "samus-original-designer-name", "samus-designer-heading", "samus-designer-name", "sound-program-heading", "sound-effects-heading", "sound-programmer-name", "music-composers-heading", "music-composer-1", "music-composer-2", "program-director-heading", "program-director-name", "system-coordinator-heading", "system-coordinator-name", "system-programmer-heading", "system-programmer-name", "samus-programmer-heading", "samus-programmer-name", "event-programmer-heading", "event-programmer-name", "enemy-programmer-heading", "enemy-programmer-name", "map-programmer-heading", "map-programmer-name", "assistant-programmer-heading", "assistant-programmer-name", "coordinators-heading", "coordinator-1", "coordinator-2", "printed-art-work-heading", "printed-art-work-1", "printed-art-work-2", "printed-art-work-3", "printed-art-work-4", "printed-art-work-5", "printed-art-work-6", "special-thanks-heading", "special-thanks-01", "special-thanks-02", "special-thanks-03", "special-thanks-04", "special-thanks-05", "special-thanks-06", "special-thanks-07", "special-thanks-08", "special-thanks-09", "special-thanks-10", "special-thanks-11", "special-thanks-12", "special-thanks-13", "special-thanks-14", "special-thanks-15", "special-thanks-r-and-d", "general-manager-heading", "general-manager-name"];
        AssertTrue(Enumerable.Range(0, CreditsRoles.Lines.Count).Select(CreditsRoleKeys.Of).SequenceEqual(expectedCreditRoles), "stream 3 all credit role identities and order");
        Suite(nameof(VerifyCreditsPresentation), () => VerifyCreditsPresentation(Path.GetFullPath("Super Metroid.smc")));
        byte[] creditsJson = SuperMetroid.AssetExtraction.Frontend.Ending.CreditsPresentationExtractor.Extract(rom);
        var credits = CreditsPresentation.Load(new MemoryStream(creditsJson));
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(creditsJson)), credits.ContentIdentity, "stream 3 credits source identity preserved");
        foreach (int invalid in new[] { -1, CreditsPresentationDefinitions.RowCount, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = credits.GetRow(invalid).Length, "stream 3 calculated credits row bounds");
        var escapePrograms = CrateriaEscapeLightningPaletteFxColorDefinitions.All;
        AssertEqual(2, escapePrograms.Count, "stream 3 escape lightning owner count");
        AssertTrue(escapePrograms.Select(program => program.Owner).SequenceEqual(new[] { CrateriaEscapeLightningPaletteOwner.YellowLightning, CrateriaEscapeLightningPaletteOwner.ParlorSky }), "stream 3 escape lightning owner order");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = escapePrograms[invalid], "stream 3 escape lightning owner bounds");
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => CrateriaEscapeLightningPaletteFxColorDefinitionsConstants.Duration(invalid), "stream 3 escape lightning duration bounds");
        Suite(nameof(VerifyGrappleConnectionDefinitions), () => VerifyGrappleConnectionDefinitions((SuperMetroidAddressSpace)rom));
        Suite(nameof(VerifyMotherBrainTurretDefinitions), () => VerifyMotherBrainTurretDefinitions((SuperMetroidAddressSpace)rom));
        foreach (MotherBrainContactPart part in Enum.GetValues<MotherBrainContactPart>())
        {
            var regions = MotherBrainContacts.Get(part);
            foreach (int invalid in new[] { -1, regions.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = regions[invalid], "stream 3 contact region bounds");
        }
        Console.WriteLine("Lookup stream 3: all implemented mapping conversions match their original values and accepted domains.");
    }

    /// <summary>
    /// Checks that a door's fade keeps the escape timer's colours lit as the cartridge's third fade-target copy does
    /// (#1512): the colours that copy stores are the timer row's colours its door-fade shades leave unfaded, every other
    /// colour of the row is faded by them, and the timer's draw shows its group's door shade without the display's fade
    /// while the rest of the frame shows the fade.
    /// </summary>
    /// <param name="rom">The cartridge the copy's stores are read from.</param>
    /// <param name="timerCopy">The third copy's store instructions.</param>
    /// <param name="targetPaletteBase">The fade target's first colour's address.</param>
    private static void VerifyEscapeTimerKeptThroughDoorFade(ISnesAddressSpace rom, int[] timerCopy, int targetPaletteBase)
    {
        const int SpriteColoursFirst = 128;
        const int RowColours = 16;
        int timerRowFirstInk = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerSheetComposer.PaletteRow * RowColours;
        int[] nativeKept = [.. timerCopy.Select(instruction =>
            (CartridgeImportSource.Require(rom).ReadCartridgeWord(instruction + 1) - targetPaletteBase) / 2
                - SpriteColoursFirst - timerRowFirstInk).Order()];
        AssertTrue(nativeKept.SequenceEqual(SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerDoorFadeShades.Kept),
            "stream 3 native third door copy keeps exactly the escape timer's kept colours");
        RgbColor[] standard = [.. Enumerable.Repeat(RgbColor.White, SpriteColoursFirst)];
        int[] keptInks = [.. nativeKept.Select(colour => timerRowFirstInk + colour)];
        foreach (SheetShade shade in SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerDoorFadeShades.Of(standard,
                     SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerSheetComposer.PaletteRow))
        {
            AssertTrue(shade.Inks.Concat(keptInks).Order().SequenceEqual(Enumerable.Range(timerRowFirstInk, RowColours)),
                $"escape timer door shade {shade.Shade} fades every colour of the timer's row but the kept ones");
        }

        // A door fade's step: the display fades with the HUD spared, and the door shades the HUD and the escape timer.
        PictureShade step = DoorFadeShades.EscapeTimerFadingOut(3);
        var shadings = Enum.GetValues<GameplayPictures>().ToDictionary(pictures => pictures,
            pictures => pictures == GameplayPictures.EscapeTimer ? PictureShading.Shaded(step) : PictureShading.Standard);
        var fade = new GameplayDisplayFade(new ColourFlash(RgbColor.Black), GameplayFadeSpares.Hud);
        var gameplay = new GameplayShading(RgbColor.Black, shadings.ToImmutableDictionary(), fade);
        PictureShading timer = gameplay.OfEscapeTimer(PictureShading.Standard);
        AssertTrue(timer.Shades.SequenceEqual([step]) && timer.Effects.IsEmpty,
            "the escape timer's draw shows its door shade, which keeps its lit colours, without the display's fade");
        AssertTrue(gameplay.OfSprite(PictureShading.Standard, isSamus: false).Effects.SequenceEqual([fade.Effect]),
            "every other sprite draw shows the door's fade");
    }

    /// <summary>Checks death explosion program selectors, native scatter anchors, and their generated burst cadence.</summary>
    private static void VerifyStream3DeathSelectors(ISnesAddressSpace rom)
    {
        for (ushort parameter = 0; parameter < 3; parameter++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86c929 + 2 * parameter)), MiscDustProjectileNativeListAddresses.Entry(MotherBrainDeathExplosions.Program(parameter)),
                "stream 3 native death explosion variant");
        foreach (ushort invalid in new ushort[] { 3, 4, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainDeathExplosions.Program(invalid),
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
                scatter.ThirdPhaseDeath.DeathExplosionIntervalTimer = 0;
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
                    AssertEqual(group, scatter.ThirdPhaseDeath.DeathAndEscapeExplosionIndex, "stream 3 death scatter reverse group order");
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
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Health = 99 };
        var firstPage = sequence.Step(samus, new RainbowBeamAttackFrame { CameraCheckpoint = new SuperMetroid.Core.Compatibility.RainbowBeamCameraCheckpoint(), EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        AssertEqual(0, firstPage.EscapePaletteFxRequests.Count, "stream 3 escape palette effects wait for door graphics");
        var handoff = sequence.Step(samus, new RainbowBeamAttackFrame { CameraCheckpoint = new SuperMetroid.Core.Compatibility.RainbowBeamCameraCheckpoint(), EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        AssertEqual(4, handoff.EscapePaletteFxRequests.Count, "stream 3 four escape palette registrations");
        for (int effect = 0; effect < 4; effect++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9b296 + 7 * effect)), PaletteFxObjectAccess.NativeAddress(handoff.EscapePaletteFxRequests[effect]),
                "stream 3 native escape palette registration order");
        var text = sequence.Step(samus, new RainbowBeamAttackFrame { CameraCheckpoint = new SuperMetroid.Core.Compatibility.RainbowBeamCameraCheckpoint(), EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0 });
        AssertEqual(0, text.EscapePaletteFxRequests.Count, "stream 3 escape palette registrations occur once");
    }

    /// <summary>Checks eight painful-walk delay, neck, and pause rows against ROM and the sequence's stage behavior.</summary>
    private static void VerifyStream3PainfulWalking(ISnesAddressSpace rom)
    {
        for (int stage = 0; stage < 8; stage++)
        {
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9beee + 2 * stage)), MotherBrainPainfulWalking.AnimationDelay(stage), "stream 3 native stagger animation delay");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9befe + 2 * stage)), MotherBrainPainfulWalking.NeckAngleDelta(stage), "stream 3 native stagger neck angle delta");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa9c049 + 2 * stage)), MotherBrainPainfulWalking.FunctionTimer(stage), "stream 3 native stagger pause timer");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainPainfulWalking.AnimationDelay(invalid), "stream 3 stagger delay bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainPainfulWalking.NeckAngleDelta(invalid), "stream 3 stagger neck bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainPainfulWalking.FunctionTimer(invalid), "stream 3 stagger timer bounds");
        }
        foreach (int stage in new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, ushort.MaxValue })
        {
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            sequence.State.Body.Position = sequence.State.Body.Position.WithX(0);
            sequence.DrainedByBaby.PainfulWalkingStage = stage;
            // As the stagger's setup does before each walk: the stage selects its delay (terminal stages keep the last).
            sequence.DrainedByBaby.PainfulWalkingAnimationDelay = MotherBrainPainfulWalking.AnimationDelay(Math.Min(stage, 7));
            sequence.DrainedByBaby.StepPainfulWalking();
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9c049 + 2 * Math.Min(stage, 7)), sequence.DrainedByBaby.PainfulWalkingFunctionTimer,
                "stream 3 real stagger timer clamps terminal stages");
        }
    }

    /// <summary>
    /// Checks the pages her escape looks copy against the cartridge's lists (#1538): each copy, in list order, holds the
    /// record's bytes where the record places them, and her death readies the escape warning and the exploded door one
    /// update a record, refusing an update past the last.
    /// </summary>
    private static void VerifyStream3EscapeGeometry(ISnesAddressSpace rom)
    {
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        EncounterArtworkUpload[] Copies(params string[] looks) => [.. looks.SelectMany(look => EncounterLookUploads.Uploads(ComposedEncounter.MotherBrain,
            EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, look), InstalledPageResolver))];
        foreach (bool timerList in new[] { true, false })
        {
            int count = timerList ? 7 : 2;
            int start = timerList ? 0xa6c4cb : 0xa9902f;
            Action ready = timerList ? sequence.ThirdPhaseDeath.ReadyEscapeWarning : sequence.ThirdPhaseDeath.ReadyExplodedDoor;
            EncounterArtworkUpload[] copies = timerList
                ? Copies(EncounterLookUploads.MotherBrainEscapeTimerLook, EncounterLookUploads.MotherBrainEscapeTextLook)
                : Copies(EncounterLookUploads.MotherBrainExplodedDoorLook);
            AssertEqual(count, copies.Length, "stream 3 escape looks copy one page a native record");
            AssertEqual(count, timerList ? MotherBrainReadyingDefinitions.EscapeWarningDuration : MotherBrainReadyingDefinitions.ExplodedDoorDuration,
                "stream 3 escape readying takes one update a native record");
            for (int index = 0; index < count; index++)
            {
                int address = start + index * 7;
                int nativeSize = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                int nativeTableByte = (CartridgeImportSource.Require(rom).ReadCartridgeWord(address + 5) - GameplayVramLayoutConstants.ObjectCharacters.Offset) * 2;
                EncounterArtworkUpload shown = copies[index];
                AssertEqual(nativeTableByte, shown.FirstByte, "stream 3 escape page lands where the native record places it");
                AssertEqual(nativeSize, shown.Characters.Length, "stream 3 escape page copies the native record's bytes");
                ready();
                AssertEqual(index + 1, timerList ? sequence.ThirdPhaseDeath.EscapeWarningReadyingElapsed : sequence.ThirdPhaseDeath.ExplodedDoorReadyingElapsed,
                    "stream 3 production escape readying advances once");
            }
            try
            {
                ready();
                throw new InvalidDataException("Completed escape readying unexpectedly readied another update.");
            }
            catch (InvalidOperationException) { }
        }
        int timerPages = Enum.GetValues<SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerTilePage>().Length;
        for (int page = 0; page < EscapeTextSheetFormat.PageCount; page++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeLong(0xa6c4cb + (timerPages + page) * 7 + 2),
                (int)EscapeSourceDefinitions.TextCharacters + EscapeTextPages.PageOffset(page), "stream 3 escape text page source: the page's offset into the installed sheet");
        AssertEqual((uint)CartridgeImportSource.Require(rom).ReadCartridgeLong(0xa9902f + 2),
            (uint)MotherBrainSourceDefinitions.SpecialSprites.Sources[MotherBrainSpriteSheet.ExplodedDoor], "stream 3 exploded door source");
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTextPages.PageOffset(invalid), "stream 3 text page offset bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTextPages.PageFirstCharacter(invalid), "stream 3 text page destination bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EscapeTextPages.PageByteCount(invalid), "stream 3 text page size bounds");
        }
        Suite(nameof(VerifyInstalledMotherBrainEscapeTextArtwork), () => VerifyInstalledMotherBrainEscapeTextArtwork(InstalledFixtures.EnemyArtwork));
    }

    /// <summary>The installation's enemy, boss and area artwork with its escape timer, which Mother Brain's pages copy (#1513).</summary>
    private static SuperMetroid.TestSupport.VerificationOracles.EnemyTileTransfers InstalledPageResolver =>
        new(InstalledFixtures.EnemyArtwork, timer: InstalledFixtures.ImportMaps.EscapeTimerTiles);

    /// <summary>Checks corpse row offsets and rot row/page transfer placement against native source and destination words.</summary>
    private static void VerifyStream3CorpseGeometry(ISnesAddressSpace rom)
    {
        for (int row = 0; row < 8; row++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa9e262 + row * 2), row * MotherBrainCorpseStaging.RowBytes,
                "stream 3 corpse native tile row offset");
        IReadOnlyList<MotherBrainCorpseRotRow> rotRows = MotherBrainCorpseSheet.RotRows;
        AssertEqual(6, rotRows.Count, "stream 3 corpse rot row count");
        for (int row = 0; row < 6; row++)
        {
            int entry = 0xa9e1f4 + row * 8;
            MotherBrainCorpseRotRow rotRow = rotRows[row];
            AssertEqual(row, rotRow.Row, "stream 3 corpse rot rows in row order");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry), MotherBrainCorpseStaging.ByteCount(rotRow), "stream 3 native corpse rot row bytes");
            int nativeStaging = (int)new SnesAddress(LittleEndianBytes.HighByte(CartridgeImportSource.Require(rom).ReadCartridgeWord(entry + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord(entry + 4));
            AssertEqual(nativeStaging, CorpseWorkMemory.CorpseRottingHeap + MotherBrainCorpseStaging.RottingOffset(rotRow), "stream 3 native corpse rot row source");
            AssertEqual((CartridgeImportSource.Require(rom).ReadCartridgeWord(entry + 6) - GameplayVramLayoutConstants.ObjectCharacters.Offset) * 2,
                MotherBrainCorpseSheet.RotRowFirstCharacter(rotRow) * 32, "stream 3 native corpse rot row placement");
            int pageEntry = 0xa99003 + row * 7;
            uint source = (uint)(CartridgeImportSource.Require(rom).ReadCartridgeLong(pageEntry + 2));
            AssertEqual(source, (uint)(MotherBrainSourceDefinitions.Corpse.Characters + MotherBrainCorpseSheet.PageOffset(row)),
                "stream 3 corpse page starts at its offset in the corpse sheet");
            EncounterArtworkUpload page = EncounterLookUploads.Uploads(ComposedEncounter.MotherBrain,
                EncounterLookUploads.IndexOf(ComposedEncounter.MotherBrain, EncounterLookUploads.MotherBrainCorpseLook), InstalledPageResolver)[row];
            AssertEqual((CartridgeImportSource.Require(rom).ReadCartridgeWord(pageEntry + 5) - GameplayVramLayoutConstants.ObjectCharacters.Offset) * 2,
                page.FirstByte, "stream 3 corpse page placement");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(pageEntry), page.Characters.Length, "stream 3 corpse page bytes");
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainCorpseSheet.PageOffset(invalid), "stream 3 corpse page offset bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainCorpseSheet.PageFirstCharacter(invalid), "stream 3 corpse page character bounds");
        }
        Suite(nameof(VerifyMotherBrainCorpseStockArtwork), () => VerifyMotherBrainCorpseStockArtwork());
    }

    /// <summary>Checks Shitroid pulse and Baby cutscene colors against ROM.</summary>
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
        ShitroidColorCatalog Load() => ShitroidColorFile.Read(new MemoryStream(ShitroidColorFile.Write(document)));
        void Check(ShitroidColorCatalog colors)
        {
            for (int frame = 0; frame < 8; frame++)
            for (int color = 0; color < 4; color++)
                AssertEqual(normal[frame][color], colors.NormalColor(frame, color), "stream 3 Shitroid pulse RGB5");
            for (int color = 0; color < 16; color++)
            {
                AssertEqual(shitroid[color], colors.TargetColor(ShitroidColorTarget.Shitroid, color), "stream 3 complete Shitroid target palette");
                AssertEqual(sidehopper[color], colors.TargetColor(ShitroidColorTarget.Sidehopper, color), "stream 3 Sidehopper target");
                AssertEqual(dead[color], colors.TargetColor(ShitroidColorTarget.DeadSidehopper, color), "stream 3 corpse target");
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
        for (int color = 0; color < 15; color++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa994d4 + color * 2)), stock.TargetColor(ShitroidColorTarget.Shitroid, color + 1),
                "stream 3 native Baby/Shitroid palette duplication");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.TargetColor(ShitroidColorTarget.Shitroid, invalid), "stream 3 Shitroid target bounds");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(invalid, 0), "stream 3 Shitroid pulse frame bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.NormalColor(0, invalid), "stream 3 Shitroid pulse color bounds");
    }

    /// <summary>Checks rainbow, drain, fake-death, and revival palette frames against their native rows.</summary>
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
        InstalledMotherBrainRainbowPalette Load(MotherBrainRainbowPaletteDocument value) => MotherBrainRainbowPaletteFile.Read(
            new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, ArtworkJson.Options)));
        void Check(InstalledMotherBrainRainbowPalette palette)
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
                AssertEqual(RgbColor.FromWord(Word(document.BeamCycle[index])), palette.BeamColor(index),
                    "stream 3 beam wheel matches every native sampled color");
            AssertEqual(38, palette.BeamCycleLength, "stream 3 beam cycle ends after its last colour");
            AssertEqual(0x3ce0, palette.BeamInitialColor, "stream 3 beam initial fixed color unchanged");
        }
        CheckBeam(stock.Beam);
        foreach (int invalid in new[] { -1, 38, int.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.Beam.BeamColor(invalid), "stream 3 beam colour index bounds");
        Check(stock);
        void CheckRainbow(InstalledMotherBrainRainbowPalette palette)
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
        void CheckRevival(InstalledMotherBrainRainbowPalette palette)
        {
            var bus = new TestAddressSpace();
            var cgram = new SnesCgram();
            for (int frame = 0; frame < 8; frame++)
            {
                cgram.SetColor(0x4e, new RgbColor(222, 24, 0));
                cgram.SetColor(0x9e, new RgbColor(222, 24, 0));
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
        CheckRainbow(stock);
        foreach (int invalid in new[] { -1, 10, int.MaxValue })
            AssertThrows<InvalidDataException>(() => stock.ApplyRainbow(new SnesCgram(), invalid), "stream 3 rainbow bounds");
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => stock.ApplyToGrey(new SnesCgram(), invalid), "stream 3 drain bounds");
            AssertThrows<InvalidDataException>(() => stock.ApplyFakeDeathToGrey(new SnesCgram(), invalid), "stream 3 fake-death bounds");
        }
    }

    /// <summary>Checks the Baby Metroid initial and six fade palettes against ROM.</summary>
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
        BabyMetroidCutsceneColorCatalog Load() => BabyMetroidCutsceneColorFile.Read(new MemoryStream(
            BabyMetroidCutsceneColorFile.Write(new BabyMetroidCutsceneColorDocument
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
        foreach (int invalid in new[] { -1, 0, 7, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.FadeColor(invalid, 0), "stream 3 Baby fade frame bounds");
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.FadeColor(1, invalid), "stream 3 Baby fade color bounds");
    }

    /// <summary>Checks Mother Brain body, leg, and corpse fade frames against the native color sequences.</summary>
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

        static PaletteRgb5 Rgb(ushort value) => new()
        {
            Red = value & 31, Green = (value >> 5) & 31, Blue = (value >> 10) & 31,
        };
        MotherBrainDeathColorCatalog Load() => MotherBrainDeathColorFile.Read(new MemoryStream(
            MotherBrainDeathColorFile.Write(new MotherBrainDeathColorDocument
            {
                Version = MotherBrainDeathColorFormat.Version,
                BodyFade = body.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                LegFade = leg.Select(row => row.Select(Rgb).ToArray()).ToArray(),
                CorpseFade = corpse.Select(row => row.Select(Rgb).ToArray()).ToArray(),
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
            string identity = SelectedPresentationHash.Create("MotherBrainDeathColorCatalog-v2", content =>
            {
                content.AppendWordFrames("bodyFade", body);
                content.AppendWordFrames("legFade", leg);
                content.AppendWordFrames("corpseFade", corpse);
            });
            AssertEqual(identity, catalog.ContentIdentity, "stream 3 death fade identity keeps exact selected values");
        }
        var stock = Load();
        Check(stock);
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
    private static void VerifyStream3ChootControl(ISnesAddressSpace rom)
    {
        for (ushort pattern = 0; pattern < 5; pattern++)
        {
            ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2df5e + 2 * pattern));
            ushort distancePointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa2df6a + 2 * pattern));
            ChootFallingPath fallingPath = DecodedChootFallingPath(pattern);
            AssertEqual(ChootNativeWords.ChootFallingPaths.Decode(pointer, "Choot falling path"), fallingPath,
                "stream 3 Choot pattern identity");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa20000 | distancePointer)), ChootFalls.LoopYDistance(fallingPath),
                "stream 3 Choot loop advance");
        }
        foreach (int invalid in new[] { 5, 6, byte.MaxValue })
            AssertThrows<InvalidDataException>(() => DecodedChootFallingPath(invalid),
                "stream 3 Choot import rejects alias and invalid patterns");
    }

    /// <summary>The falling path the import decodes from a Choot's pattern selector, the first parameter's high byte.</summary>
    private static ChootFallingPath DecodedChootFallingPath(int pattern) =>
        ((ChootSpawnSettings)EnemySpawnSettingsCodec.Decode(EnemyDefinitionId.Choot, PopulationWords.Parameters(pattern << 8, 0))).FallingPath;

    /// <summary>Checks pickup and death animation dispatch plus Fireflea instruction words against native tables.</summary>
    private static void VerifyStream3PickupAndFirefleaPrograms(ISnesAddressSpace rom)
    {
        for (int kind = 0; kind < 6; kind++)
        {
            EnemyPickupAnimationDefinition animation = EnemyPickups.Animation(EnemyPickupNativeWords.Selectors.Decode(kind, "enemy pickup selector"));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86ef04 + 2 * kind)),
                animation.Program is { } program ? EnemyDeathProjectileNativeListAddresses.Entry(program) : (ushort)0, "stream 3 pickup kind dispatch");
        }
        for (ushort animation = 0; animation < 5; animation++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x86efd5 + 2 * animation)),
                EnemyDeathProjectileNativeListAddresses.Entry(EnemyDeathExplosions.Program(
                    SuperMetroid.AssetExtraction.CartridgeTables.EnemyDeathExplosionWords.Selectors.Decode(animation, "death explosion"))),
                "stream 3 death variant dispatch");
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => EnemyPickups.Animation(EnemyPickupNativeWords.Selectors.Decode(invalid, "enemy pickup selector")),
                "stream 3 invalid pickup kind");
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<InvalidDataException>(
                () => SuperMetroid.AssetExtraction.CartridgeTables.EnemyDeathExplosionWords.Selectors.Decode(invalid, "death explosion"),
                "stream 3 invalid death variant");
    }

    /// <summary>Checks six four-color Work Robot palette frames against ROM.</summary>
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
        WorkRobotPaletteCycle Load() => WorkRobotPaletteCycleFile.Read(
            new MemoryStream(WorkRobotPaletteCycleFile.Write(document), writable: false));
        void Check(WorkRobotPaletteCycle cycle)
        {
            for (int frame = 0; frame < 6; frame++)
            {
                for (int color = 0; color < 4; color++)
                    AssertEqual(words[frame][color], cycle.Resolve(frame, color),
                        "stream 3 Work Robot selected color");
            }
            string expectedIdentity = SelectedPresentationHash.Create("WorkRobotPaletteCycle-v1",
                content => content.AppendWordFrames("frames", words));
            AssertEqual(expectedIdentity, cycle.ContentIdentity,
                "stream 3 Work Robot identity preserves original row framing");
        }
        var stock = Load();
        Check(stock);
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(invalid, 0),
                "stream 3 Work Robot frame bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(0, invalid),
                "stream 3 Work Robot color bounds");
    }

    /// <summary>Checks Mother Brain's health tint from paint anchors without retaining native intermediate rows.</summary>
    private static void VerifyStream3HealthTint(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainHealthPaletteExtractor.Extract(rom);
        var stock = MotherBrainHealthPaletteFile.Read(new MemoryStream(json));
        // Mother Brain shows her health band as a shade (#1512); the import pre-colours each band's shade of her brain and
        // rear legs on her sprite sheets, and of her body on the room's background, from the installed health palette.
        static (RgbColor[] Brain, RgbColor[] RearLegs, RgbColor[] Body) Shown(MotherBrainHealthPalettePresentation health, int band)
        {
            PictureShade shade = MotherBrainShades.Health(band);
            SheetShade sprites = SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.HealthSprites(health).Single(candidate => candidate.Shade == shade);
            SheetShade background = SuperMetroid.AssetExtraction.Bosses.MotherBrainSheetShades.HealthBackground(health).Single(candidate => candidate.Shade == shade);
            const int count = MotherBrainRainbowPaletteGeometry.ColorCount;
            AssertEqual(2 * count, sprites.Colours.Length, "stream 3 health band sprite shade holds her brain's and rear legs' colours");
            return (sprites.Colours[..count], sprites.Colours[count..], background.Colours);
        }
        for (int state = 0; state < 4; state++)
        {
            var (brain, rearLegs, body) = Shown(stock, state);
            foreach (var (table, shown) in new[]
            {
                new VerifyStream3HealthTintCase(MotherBrainSourceDefinitions.Palettes.HealthBodyPointerTable, body),
                new VerifyStream3HealthTintCase(MotherBrainSourceDefinitions.Palettes.HealthRearLegPointerTable, rearLegs),
            })
            {
                int pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord(table + state * 2);
                for (int color = 0; color < 15; color++)
                {
                    int address = 0xad0000 | pointer + color * 2;
                    ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                    AssertEqual(expected, shown[color], "stream 3 all native health tint words reach the band's shades");
                }
            }
            for (int color = 0; color < 15; color++)
                AssertEqual(body[color], brain[color], "stream 3 health tint body/brain copies");
        }
    }

    /// <summary>Checks recovery-light colors and room-palette routing against ROM.</summary>
    internal static void VerifyStream3RecoveryLights(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorFile.Read(new MemoryStream(json));
        for (int frame = 0; frame < 7; frame++)
        {
            var actual = new SnesCgram();
            stock.ApplyRecoveryLights(actual, frame);
            for (int color = 0; color < 28; color++)
            {
                int address = 0xadf3d3 - frame * 0x38 + color * 2;
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                int destination = color < 14 ? MotherBrainRoomColorDestinations.RecoveryLightsFirstColor + color :
                    MotherBrainRoomColorDestinations.RecoveryLightsSecondColor + color - 14;
                AssertEqual(expected, actual.Colors[destination], "stream 3 all native recovery light words");
            }
        }
    }

    /// <summary>Checks timed room-flash colors and mirrors without storing native output rows.</summary>
    internal static void VerifyStream3RoomFlash(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorFile.Read(new MemoryStream(json));
        for (int frame = 0; frame < MotherBrainRoomPaletteProgramDefinitions.FlashFrameCount; frame++)
        {
            ushort entry = checked((ushort)(SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomFlashSourceDefinitions.FlashList + frame * SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomFlashSourceDefinitions.FlashEntryBytes));
            int operand = MotherBrainSourceDefinitions.RoomColors.FlashBank + entry + SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomFlashSourceDefinitions.FlashPaletteOperandOffset;
            int source = (MotherBrainSourceDefinitions.RoomColors.FlashBank|CartridgeImportSource.Require(rom).ReadCartridgeWord(operand));
            var actual = new SnesCgram();
            stock.ApplyFlash(actual, frame);
            for (int color = 0; color < MotherBrainRoomColorGeometry.SliceColors * 2; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
                int destination = color < MotherBrainRoomColorGeometry.SliceColors
                    ? MotherBrainRoomColorDestinations.FirstColor + color
                    : MotherBrainRoomColorDestinations.SecondColor + color - MotherBrainRoomColorGeometry.SliceColors;
                AssertEqual(native, actual.Colors[destination], "stream 3 all native room-flash colors");
                if (color >= MotherBrainRoomColorGeometry.SliceColors)
                    AssertEqual(native, actual.Colors[MotherBrainRoomColorDestinations.MirroredSecondColor + color - MotherBrainRoomColorGeometry.SliceColors], "stream 3 native room-flash mirror");
            }
        }
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.FlashPaletteOperandAddress(invalid),
                "stream 3 room palette operand index bounds");
    }

    /// <summary>Interprets the supported bank-$8F door callback opcodes to calculate the native scroll writes.</summary>
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
                        accumulator |= LittleEndianBytes.Word(0, bus.ReadCartridgeByte(pc++));
                    break;

                case 0x8f: // STA long
                    int destination = (CartridgeImportSource.Require(bus).ReadCartridgeLong(pc));
                    pc += 3;
                    int storageIndex = destination - RoomScrollGrid.WorkRamAddress;
                    if ((uint)storageIndex >= RoomScrollGrid.KeptCellCount)
                        throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 4);
                    scrolls[storageIndex] = unchecked((byte)accumulator);
                    if (!accumulatorIsEightBit)
                    {
                        if (storageIndex + 1 >= scrolls.Length)
                            throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 4);
                        scrolls[storageIndex + 1] = LittleEndianBytes.HighByte(accumulator);
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
    internal static void VerifyStream3PhaseTwoRearLeg(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorFile.Read(new MemoryStream(json));
        var phaseTwo = new SnesCgram();
        stock.ApplyPhaseTwoInitial(phaseTwo);
        for (int color = 0; color < 15; color++)
        {
            int address = 0xa99494 + color * 2;
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, phaseTwo.Colors[0xb1 + color], "stream 3 native phase-two rear color and destination");
        }
    }

    /// <summary>Checks auxiliary palette definitions and resolved rows for Face Block, Sidehopper, and Golden Torizo.</summary>
    internal static void VerifyStream3AuxiliaryPalettes(ISnesAddressSpace rom)
    {
        EnemyAuxiliaryPaletteDefinition[] definitions =
        [
            new(EnemyAuxiliaryPalette.FaceBlock, 8, 4),
            new(EnemyAuxiliaryPalette.DeadSidehopper, 7, 15),
            new(EnemyAuxiliaryPalette.GoldenTorizoBody, 8, 16),
            new(EnemyAuxiliaryPalette.GoldenTorizoBelly, 8, 16),
        ];
        // Where each family's rows sit is the import's knowledge: the oracle reads the retail rows from the import's
        // source catalog, and the stored stride must match the cartridge layout recorded in this check.
        AssertEqual(0xa8e7cc, EnemySourceDefinitions.AuxiliaryPalettes[EnemyAuxiliaryPalette.FaceBlock].FirstRow.Value, "stream 3 face-block rows' source");
        AssertEqual(0xa9ebcc, EnemySourceDefinitions.AuxiliaryPalettes[EnemyAuxiliaryPalette.DeadSidehopper].FirstRow.Value, "stream 3 dead-sidehopper rows' source");
        AssertEqual(16, EnemySourceDefinitions.AuxiliaryPalettes[EnemyAuxiliaryPalette.DeadSidehopper].RowStrideColors, "stream 3 dead-sidehopper row stride");
        AssertEqual(0x848032, EnemySourceDefinitions.AuxiliaryPalettes[EnemyAuxiliaryPalette.GoldenTorizoBody].FirstRow.Value, "stream 3 Golden Torizo body rows' source");
        AssertEqual(0x848132, EnemySourceDefinitions.AuxiliaryPalettes[EnemyAuxiliaryPalette.GoldenTorizoBelly].FirstRow.Value, "stream 3 Golden Torizo belly rows' source");
        AssertTrue(definitions.SequenceEqual(EnemyAuxiliaryColorDefinitions.All), "stream 3 auxiliary definition fields/order");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = EnemyAuxiliaryColorDefinitions.All[invalid], "stream 3 auxiliary definition bounds");
        var words = definitions.ToDictionary(definition => definition.Id, definition =>
            Enumerable.Range(0, definition.FrameCount).Select(frame =>
                Enumerable.Range(0, definition.ColorCount).Select(color =>
                {
                    EnemyAuxiliaryPaletteSource stored = EnemySourceDefinitions.AuxiliaryPalettes[definition.Id];
                    int address = stored.FirstRow.Value + 2 * (frame * stored.RowStrideColors + color);
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
            return EnemyAuxiliaryColorFiles.Read(new MemoryStream(EnemyAuxiliaryColorFiles.Write(document), writable: false));
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
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBody, invalid, 0), "stream 3 Torizo band bounds");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve(EnemyAuxiliaryPalette.GoldenTorizoBody, 0, invalid), "stream 3 Torizo color bounds");
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Resolve((EnemyAuxiliaryPalette)invalid, 0, 0), "stream 3 auxiliary palette identities reject unknown values");
    }

    /// <summary>Checks Baby Metroid sprite parts, reflected OAM placement, and imported schema behavior.</summary>
    internal static void VerifyStream3BabySpriteReflection(ISnesAddressSpace rom)
    {
        var definitions = EnemySpritemapDefinitions.Frames.ToArray();
        var document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapSchema.Revision,
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
                AssertTrue(parts.SequenceEqual(EnemySpritemapCompiler.CompileParts(document.Frames[frame.Name], frame.Name)),
                    "stream 3 exact supplied Baby part order and every visual field");
            }
            // The catalog names each frame by its installed sheet key (#1402, #1465), in key order; every frame here shows itself.
            string expected = SelectedPresentationHash.Create("enemy-oam-v2", content =>
            {
                foreach (var frame in definitions.OrderBy(frame => EnemyFrameSheetKeys.Of(frame.Frame), StringComparer.Ordinal))
                {
                    content.AppendName("frame", EnemyFrameSheetKeys.Of(frame.Frame));
                    content.AppendEnemyParts(EnemySpritemapCompiler.CompileParts(document.Frames[frame.Name], frame.Name));
                }
                foreach (var frame in definitions.OrderBy(frame => EnemyFrameSheetKeys.Of(frame.Frame), StringComparer.Ordinal))
                {
                    content.AppendName("native-binding", EnemyFrameSheetKeys.Of(frame.Frame));
                    content.AppendName("selected-binding", EnemyFrameSheetKeys.Of(frame.Frame));
                }
            });
            AssertEqual(expected, catalog.ContentIdentity, "stream 3 indexed OAM view retains canonical hash framing");
        }
        var stock = Load();
        Check(stock);
        foreach (var frame in selected)
        {
            AssertTrue(stock.TryGetDisplay(frame.Bank, frame.Pointer, out var parts), "stream 3 Baby display view");
            var nativeOam = new OamBuffer();
            var calculatedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, new NativeEnemySpritemapDraw { Bank = frame.Bank, Pointer = frame.Pointer, Origin = new(128, 128), PaletteBits = 0, BaseTileIndex = 0 });
            calculatedOam.AddEnemySpritemap(parts, new Position16<SpritePixels>(128, 128), 0, 0);
            AssertTrue(nativeOam.ImageEquals(calculatedOam),
                "stream 3 installed Baby view matches native OAM drawing");
        }
    }
    /// <summary>Checks the phase-two attack palette's live colors and rear-leg routing without stored stock rows.</summary>
    internal static void VerifyStream3MotherBrainAttackPalette(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var document = RequiredJson.Parse(json);
        var stock = MotherBrainRoomColorFile.Read(new MemoryStream(json));
        var native = new SnesCgram();
        stock.ApplyPhaseTwoInitial(native);
        for (int color = 0; color < MotherBrainRoomColorGeometry.PhaseTwoColors; color++)
        {
            int address = MotherBrainSourceDefinitions.RoomColors.PhaseTwoAttackColors + color * sizeof(ushort);
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(expected, native.Colors[MotherBrainRoomColorDestinations.PhaseTwoAttackColor + color],
                "attack palette exact native color and CGRAM destination");
        }
        Confirm(stock, document);

        static ushort Word(System.Text.Json.Nodes.JsonNode rgb) => (ushort)(rgb.Required("red").GetValue<int>()
            | rgb.Required("green").GetValue<int>() << 5 | rgb.Required("blue").GetValue<int>() << 10);
        static void Confirm(MotherBrainRoomColorPresentation palette, System.Text.Json.Nodes.JsonNode expected)
        {
            var cgram = new SnesCgram();
            palette.ApplyPhaseTwoInitial(cgram);
            for (int color = 0; color < MotherBrainRoomColorGeometry.PhaseTwoColors; color++)
            {
                AssertEqual(Word(expected.Required("phaseTwoAttack").Required(color)),
                    cgram.Colors[MotherBrainRoomColorDestinations.PhaseTwoAttackColor + color], "attack color preserved");
                AssertEqual(Word(expected.Required("phaseTwoRearLeg").Required(color)),
                    cgram.Colors[MotherBrainRoomColorDestinations.PhaseTwoRearLegColor + color], "paired rear palette preserved");
            }
        }
    }

    /// <summary>Checks final-room paint colors and the dependent palettes against native inputs.</summary>
    internal static void VerifyStream3FinalRoomPaints(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        var document = RequiredJson.Deserialize<MotherBrainRoomColorDocument>(imported, ArtworkJson.Options);
        MotherBrainRoomColorPresentation Load() => MotherBrainRoomColorFile.Read(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        var stock = Load();
        for (int color = 0; color < document.FinalRoom.Length; color++)
        {
            int address = MotherBrainSourceDefinitions.RoomColors.FlashBank + SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomFlashSourceDefinitions.FinalPalette + color * sizeof(ushort);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, Word(document.FinalRoom[color]), "final-room direct native word");
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
                AssertEqual(Word((document.InitialGlassShard ?? throw new InvalidOperationException("Verification requires document.InitialGlassShard."))[color]), colors.Colors[0xb1 + color], "final-room stock glass");
                AssertEqual(Word((document.InitialTubeProjectile ?? throw new InvalidOperationException("Verification requires document.InitialTubeProjectile."))[color]), colors.Colors[0xf1 + color], "final-room stock tube");
            }
            for (int frame = 0; frame < (document.RecoveryLights ?? throw new InvalidOperationException("Verification requires document.RecoveryLights.")).Length; frame++)
            {
                palette.ApplyRecoveryLights(colors, frame);
                for (int color = 0; color < 28; color++)
                    AssertEqual(Word(document.RecoveryLights[frame][color]), colors.Colors[color < 14 ? 0x31 + color : 0x51 + color - 14], "final-room stock recovery");
            }
            for (int frame = 0; frame < document.Flash.Length; frame++)
            {
                palette.ApplyFlash(colors, frame);
                for (int color = 0; color < 24; color++)
                    AssertEqual(Word(document.Flash[frame][color]), colors.Colors[color < 12 ? 0x34 + color : 0x53 + color - 12], "final-room stock flash");
            }
        }
    }

    /// <summary>Checks room-entry glass and tube colors.</summary>
    internal static void VerifyStream3RoomEntryPalettes(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor.Extract(rom);
        MotherBrainRoomColorDocument Read() => RequiredJson.Deserialize<MotherBrainRoomColorDocument>(
            imported, ArtworkJson.Options);
        MotherBrainRoomColorPresentation Load(MotherBrainRoomColorDocument document) =>
            MotherBrainRoomColorFile.Read(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                document, ArtworkJson.Options)));
        var document = Read();
        var stock = Load(document);
        for (int color = 0; color < MotherBrainRoomColorGeometry.InitialColors; color++)
        {
            int glassAddress = MotherBrainSourceDefinitions.RoomColors.InitialGlassShardColors + color * sizeof(ushort);
            int tubeAddress = MotherBrainSourceDefinitions.RoomColors.InitialTubeProjectileColors + color * sizeof(ushort);
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(glassAddress), Word((document.InitialGlassShard ?? throw new InvalidOperationException("Verification requires document.InitialGlassShard."))[color]), "glass direct native color");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(tubeAddress), Word((document.InitialTubeProjectile ?? throw new InvalidOperationException("Verification requires document.InitialTubeProjectile."))[color]), "tube direct native color");
        }
        Confirm(stock, document);

        static ushort Word(PaletteRgb5 rgb) => (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        static void Confirm(MotherBrainRoomColorPresentation palette, MotherBrainRoomColorDocument expected)
        {
            var final = new SnesCgram();
            palette.ApplyFinal(final);
            for (int color = 0; color < expected.FinalRoom.Length; color++)
            {
                int destination = color < 12 ? 0x34 + color : 0x53 + color - 12;
                AssertEqual(Word(expected.FinalRoom[color]), final.Colors[destination], "final room exact native shade");
                if (color >= 12) AssertEqual(Word(expected.FinalRoom[color]), final.Colors[0x73 + color - 12], "final room mirrored shade");
            }
            var phase = new SnesCgram();
            var entry = new SnesCgram();
            palette.ApplyPhaseTwoInitial(phase);
            palette.ApplyRoomEntry(entry);
            for (int color = 0; color < 15; color++)
            {
                AssertEqual(Word(expected.PhaseTwoAttack[color]), phase.Colors[0xa1 + color], "room attack exact native shade");
                AssertEqual(Word(expected.PhaseTwoRearLeg[color]), phase.Colors[0xb1 + color], "room rear independence");
                AssertEqual(Word((expected.InitialGlassShard ?? throw new InvalidOperationException("Verification requires expected.InitialGlassShard."))[color]), entry.Colors[0xb1 + color], "room glass exact native shade");
                AssertEqual(Word((expected.InitialTubeProjectile ?? throw new InvalidOperationException("Verification requires expected.InitialTubeProjectile."))[color]), entry.Colors[0xf1 + color], "room tube exact native shade");
            }
        }
    }

    /// <summary>Checks Baby Metroid's initial cutscene paints and their separation from live Shitroid colors.</summary>
    internal static void VerifyStream3BabyInitialPaints(ISnesAddressSpace rom)
    {
        var babyDocument = RequiredJson.Parse(SuperMetroid.AssetExtraction.Bosses.MotherBrain.BabyMetroidCutsceneColorExtractor.Extract(rom));
        var liveDocument = RequiredJson.Parse(SuperMetroid.AssetExtraction.Bosses.MotherBrain.ShitroidColorExtractor.Extract(rom));
        var stock = LoadBaby(babyDocument);
        var liveStock = LoadLive(liveDocument);
        for (int color = 0; color < BabyMetroidCutsceneColorFormat.InitialColorCount; color++)
        {
            int address = MotherBrainSourceDefinitions.BabyMetroid.InitialColors + color * sizeof(ushort);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, stock.InitialColor(color), "Baby initial exact native color");
            AssertEqual(native, liveStock.TargetColor(ShitroidColorTarget.Shitroid, color + 1), "Baby live initial alias");
        }
        Check(stock, babyDocument);
        foreach (int invalid in new[] { -1, BabyMetroidCutsceneColorFormat.InitialColorCount, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.InitialColor(invalid), "Baby initial index domain preserved");
        static BabyMetroidCutsceneColorCatalog LoadBaby(System.Text.Json.Nodes.JsonNode document) =>
            BabyMetroidCutsceneColorFile.Read(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        static ShitroidColorCatalog LoadLive(System.Text.Json.Nodes.JsonNode document) =>
            ShitroidColorFile.Read(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        static ushort Word(System.Text.Json.Nodes.JsonNode rgb) => (ushort)(rgb.Required("red").GetValue<int>() |
            rgb.Required("green").GetValue<int>() << 5 | rgb.Required("blue").GetValue<int>() << 10);
        static void Check(BabyMetroidCutsceneColorCatalog catalog, System.Text.Json.Nodes.JsonNode document)
        {
            ushort[] colors = document.Required("initial").AsArray().RequiredElements().Select(rgb => Word(rgb)).ToArray();
            ushort[][] fade = document.Required("fade").AsArray().RequiredElements().Select(row => row.AsArray().RequiredElements().Select(rgb => Word(rgb)).ToArray()).ToArray();
            for (int color = 0; color < colors.Length; color++) AssertEqual(colors[color], catalog.InitialColor(color), "Baby initial colors");
            string identity = SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
            {
                content.AppendWords("initial", colors);
                content.AppendWordFrames("fade", fade);
            });
            AssertEqual(identity, catalog.ContentIdentity, "Baby initial canonical content identity preserved");
        }
    }

    /// <summary>Checks the first narration tilemap against decompressed ROM words and the intro upload path.</summary>
    internal static void VerifyStream3InitialNarrationMap()
    {
        string source = Path.GetFullPath("Super Metroid.smc");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
        byte[] native = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
            IntroCinematicSourceDefinitions.FirstNarrationTilemap, maximumOutputBytes: 2048);
        var installation = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation;
        ImportIntroCinematicArtwork stock = SuperMetroid.AssetExtraction.Frontend.Intro.IntroCinematicArtworkFiles.LoadForImport(installation.IntroCinematicDirectory, SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName));
        static RoomBackgroundTilemapAtlas Page(byte[] bytes) => new(BackgroundTileWordBytes.Tiles(bytes));
        RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(BackgroundTileWordBytes.Of(stock.BackgroundPages.Span.Slice(index * 1024, 1024)))).ToArray();
        ImportIntroCinematicArtwork Copy(byte[] narration) => new(new IntroCinematicArtworkParts { CaretSprites = stock.CaretSprites, NextPageArrowSprites = stock.NextPageArrowSprites, MotherBrainSprites = stock.MotherBrainSprites, MotherBrainExplosionSprites = stock.MotherBrainExplosionSprites, RinkaSprites = stock.RinkaSprites, EggEffectSprites = stock.EggEffectSprites, DiscoveryActorSprites = stock.DiscoveryActorSprites, ScientistSprites = stock.ScientistSprites, Backdrop = stock.Backdrop, CeresFlight = stock.CeresFlight, CeresDestruction = stock.CeresDestruction, CeresDestructionMaps = stock.CeresDestructionMaps, CeresDestructionSprites = stock.CeresDestructionSprites }, new IntroCinematicLayouts(pages, Page(BackgroundTileWordBytes.Of(stock.PortraitTilemap.Span)), Page(narration), stock.FinalLine, stock.EyeFrames, stock.ZebesRevealMap), new IntroCinematicCharacters(stock.BackgroundCharacters, stock.IntroObjectCharacters, stock.CinematicObjectCharacters, stock.ZebesReveal.Characters, stock.FlightCharacters), stock.Palette, stock.FlightPalette);
        AssertTrue(BackgroundTileWordBytes.Of(stock.InitialNarrationTilemap.Span).AsSpan().SequenceEqual(native), "all 1024 initial narration words match native");
        var intro = new IntroCinematicState(new IntroCinematicOverreads(InstalledFixtures.SamusOverreads, InstalledFixtures.BossAndIntroOverreads.ShellFragmentFallPastCurve), characterArtwork: stock.BoundCatalog)
        {
            Random = new SharedRandom(),
            UnauthoredBoostCadence = new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
        };
        byte[] nativeFont = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
            IntroCinematicSourceDefinitions.FontOne, maximumOutputBytes: IntroFontAtlasFormat.ByteCount);
        AssertTrue(intro.ShownVram().AsSpan(
            (IntroVramLayout.FontOne.Offset * 2), nativeFont.Length).SequenceEqual(nativeFont),
            "the opening's shown memory holds the installed font, which matches the cartridge's");
        AssertTrue(intro.ShownVram().AsSpan(
            (IntroVramLayout.NarrationTilemap.Offset * 2), native.Length).SequenceEqual(native),
            "actual opening constructor transfers installed narration to native VRAM destination");
        intro.SetupFirstIllustratedPage();
        var subtitleUpload = intro.ShownVram().AsSpan(
            (IntroVramLayout.NarrationTilemap.Offset * 2) + IntroNarrationPictures.FinalLineRow * IntroCinematicDefinitions.Planes.Width * 2,
            IntroFinalLineTilemapFormat.CellCount * 2);
        for (int index = 0; index < IntroFinalLineTilemapFormat.CellCount; index++)
            AssertEqual(ReadVerificationWord(rom, IntroCinematicSourceDefinitions.FinalTextLine + index * 2),
                LittleEndianBytes.Word(subtitleUpload[index * 2], subtitleUpload[index * 2 + 1]), "actual Japanese subtitle staging upload");
        AssertEqual(stock.ContentIdentity, Copy(native).ContentIdentity, "equivalent stock bundle preserves canonical identity");
        BackgroundTile[] first = stock.InitialNarrationTilemap.ToArray();
        first[0] = first[0] with { Tile = first[0].Tile ^ 1 };
        AssertTrue(BackgroundTileWordBytes.Of(stock.InitialNarrationTilemap.Span).AsSpan().SequenceEqual(native), "installed page is not mutated through a copy");
        Console.WriteLine("Initial narration: 1024 native words and canonical bundle identity pass.");
    }

    /// <summary>Checks file-select sprite compositions and border parts against native OAM data.</summary>
    internal static void VerifyStream3FileSelectSprites(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyStream3FileSelectBorders), () => VerifyStream3FileSelectBorders(rom, imported));
        FileSelectPresentationDocument Read() => RequiredJson.Deserialize<FileSelectPresentationDocument>(imported, ArtworkJson.Options);
        FileSelectSourcePresentation Load(FileSelectPresentationDocument document) => FileSelectSourcePresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options)));
        SpriteComposition Composition(FileSelectSourcePresentation owner, int frame) => owner.helmetSprites[frame];
        var original = Read();
        var stock = Load(original);
        for (int frame = 0; frame < 8; frame++)
        {
            string name = FileSelectParts.HelmetSpriteName(frame);
            var composition = Composition(stock, frame);
            Confirm(stock, original, frame);
            int pointer = ReadVerificationWord(rom, 0x82c569 + FileSelectHelmetSpritemaps.SpritemapId(frame) * 2);
            int address = 0x820000 | pointer;
            AssertEqual(ReadVerificationWord(rom, address), composition.PartCount, "exact native helmet part count");
            for (int part = 0; part < composition.PartCount; part++)
            {
                int entry = address + 2 + part * 5;
                AssertEqual(NativeOwnerPalettePart(rom, entry), composition.Part(part).Part,
                    "native helmet offset, size, character, priority and mirroring with caller palette inheritance");
            }
            // A part whose footprint leaves the sheet is a malformed document the import rejects.
            var malformed = Read();
            var first = malformed.Sprites[name][0];
            malformed.Sprites[name][0] = first with { TileColumn = MapSpriteFormat.TileColumns - first.Size / 8 + 1 };
            AssertThrows<InvalidDataException>(() => Load(malformed), "helmet part footprint outside the sheet is rejected");
        }
        Console.WriteLine("File-select sprites: 114 border parts, 42 helmet parts, out-of-sheet rejection and actual helmet OAM pass; cursor dependency previously confirmed.");
        void Confirm(FileSelectSourcePresentation actual, FileSelectPresentationDocument expected, int frame)
        {
            string name = FileSelectParts.HelmetSpriteName(frame);
            var expectedParts = MenuSpriteCompiler.Compile(expected.Sprites[name], name);
            var selected = Composition(actual, frame);
            AssertEqual(expectedParts.PartCount, selected.PartCount, "helmet part count");
            for (int part = 0; part < expectedParts.PartCount; part++)
                AssertEqual(expectedParts.Part(part), selected.Part(part), "exact selected helmet part");
            var actualOam = ScreenSpritesFixtures.Blank();
            var expectedOam = new OamBuffer();
            actual.Presentation.DrawHelmet(actualOam, MenuSheetFixtures.FileSelect(actual), frame, 0);
            var anchor = expected.HelmetAnchors[0];
            expectedParts.DrawOnScreen(expectedOam, new(anchor.X, anchor.Y), (ushort)(expected.ObjectPalette << 9));
            actualOam.FinishFrame(); expectedOam.FinalizeFrame();
            AssertTrue(actualOam.ImageEquals(expectedOam),
                "actual helmet draw preserves native ordered OAM");
        }
    }

    /// <summary>Checks all 144 intro font tiles against decompressed ROM bytes.</summary>
    internal static void VerifyStream3IntroFont()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] native = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
            IntroCinematicSourceDefinitions.FontOne, maximumOutputBytes: IntroFontAtlasFormat.ByteCount);
        var stock = IntroFontCharacters.Read(new MemoryStream(SuperMetroid.AssetExtraction.Frontend.Intro.IntroFontAtlasExtractor.Extract(rom)));
        AssertTrue(stock.Characters.Span.SequenceEqual(native), "144 installed font tiles match native 2304 bytes");
        byte[] output = stock.Characters.ToArray();
        output[0] ^= 1;
        AssertTrue(stock.Characters.Span.SequenceEqual(native), "installed font is not mutated through a copy");
        for (int y = 0; y < 8; y++)
        for (int plane = 0; plane < 2; plane++)
            AssertEqual((byte)(native[0x24 * 16 + y * 2 + plane] >> 1), native[0x26 * 16 + y * 2 + plane], "whole period drawing shifts right one pixel");
        Console.WriteLine("Intro font: 144 native tiles/2304 bytes and the exact period alias pass.");
    }

    /// <summary>Checks intro background map pages against decompressed ROM.</summary>
    internal static void VerifyStream3BackgroundMaps()
    {
        string source = Path.GetFullPath("Super Metroid.smc");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
        byte[] native = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
            IntroCinematicSourceDefinitions.BackgroundPageTilemaps, maximumOutputBytes: 8192);
        var installation = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation;
        ImportIntroCinematicArtwork stock = SuperMetroid.AssetExtraction.Frontend.Intro.IntroCinematicArtworkFiles.LoadForImport(installation.IntroCinematicDirectory, SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName));
        static RoomBackgroundTilemapAtlas Page(byte[] bytes) => new(BackgroundTileWordBytes.Tiles(bytes));
        ImportIntroCinematicArtwork Copy(byte[] background)
        {
            RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(background.AsSpan(index * 2048, 2048).ToArray())).ToArray();
            return new(new IntroCinematicArtworkParts { CaretSprites = stock.CaretSprites, NextPageArrowSprites = stock.NextPageArrowSprites, MotherBrainSprites = stock.MotherBrainSprites, MotherBrainExplosionSprites = stock.MotherBrainExplosionSprites, RinkaSprites = stock.RinkaSprites, EggEffectSprites = stock.EggEffectSprites, DiscoveryActorSprites = stock.DiscoveryActorSprites, ScientistSprites = stock.ScientistSprites, Backdrop = stock.Backdrop, CeresFlight = stock.CeresFlight, CeresDestruction = stock.CeresDestruction, CeresDestructionMaps = stock.CeresDestructionMaps, CeresDestructionSprites = stock.CeresDestructionSprites }, new IntroCinematicLayouts(pages, Page(BackgroundTileWordBytes.Of(stock.PortraitTilemap.Span)), Page(BackgroundTileWordBytes.Of(stock.InitialNarrationTilemap.Span)), stock.FinalLine, stock.EyeFrames, stock.ZebesRevealMap), new IntroCinematicCharacters(stock.BackgroundCharacters, stock.IntroObjectCharacters, stock.CinematicObjectCharacters, stock.ZebesReveal.Characters, stock.FlightCharacters), stock.Palette, stock.FlightPalette);
        }
        byte[] installed = BackgroundTileWordBytes.Of(stock.BackgroundPages.Span);
        var mismatches = new List<string>();
        for (int cell = 0; cell < native.Length / 2; cell++)
        {
            ushort expected = BitConverter.ToUInt16(native, cell * 2), actual = BitConverter.ToUInt16(installed, cell * 2);
            if (expected != actual) mismatches.Add($"page{cell / 1024} ({cell % 32},{cell % 1024 / 32}) expected{expected:X4}/actual{actual:X4}");
        }
        AssertTrue(mismatches.Count == 0, "all 4096 installed background words match native: " + string.Join("; ", mismatches.Take(32)));
        AssertEqual(stock.ContentIdentity, Copy(native).ContentIdentity, "equivalent backgrounds preserve canonical hash");
        var intro = new IntroCinematicState(new IntroCinematicOverreads(InstalledFixtures.SamusOverreads, InstalledFixtures.BossAndIntroOverreads.ShellFragmentFallPastCurve), characterArtwork: stock.BoundCatalog)
        {
            Random = new SharedRandom(),
            UnauthoredBoostCadence = new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
        };
        AssertTrue(intro.ShownVram().AsSpan(
            (IntroVramLayout.BackgroundPages.Offset * 2), native.Length).SequenceEqual(native),
            "actual opening constructor uploads all four installed background pages");
        Console.WriteLine("Intro backgrounds: 4096 native words, canonical hash and actual four-page VRAM upload pass.");
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
BranchesToSharedBody:false),
            new VerifyStream3NintendoFadeEntriesCase(DefinitionPointer:NintendoLogoFadePaletteFxColorDefinitions.CopyrightDefinitionPointer,
ProgramStart:NintendoLogoFadePaletteFxColorDefinitionsConstants.CopyrightEntry,
BranchesToSharedBody:true),
        })
        {
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | (entry.DefinitionPointer + 2)), entry.ProgramStart,
                "native definition selects exact semantic program entry");
            AssertEqual((ushort)PaletteFxInstruction.SetColorIndex, ReadVerificationWord(rom, 0x8d0000 | entry.ProgramStart),
                "each entry sets its own CGRAM slot");
            AssertEqual(ReadVerificationWord(rom, 0x8d0000 | (entry.ProgramStart + 2)),
                PaletteFxObjectAccess.FromNative(entry.DefinitionPointer).AnimatedColours.CartridgeByteIndex,
                "each entry's native colours start where its object's colour group is placed");
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
    internal static void VerifyStream3PortraitMap()
    {
        string source = Path.GetFullPath("Super Metroid.smc");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
        byte[] native = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.Decompress(rom,
            IntroCinematicSourceDefinitions.SamusHeadTilemap, maximumOutputBytes: 2048);
        var installation = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.Installation;
        ImportIntroCinematicArtwork stock = SuperMetroid.AssetExtraction.Frontend.Intro.IntroCinematicArtworkFiles.LoadForImport(installation.IntroCinematicDirectory, SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName));
        static RoomBackgroundTilemapAtlas Page(byte[] bytes) => new(BackgroundTileWordBytes.Tiles(bytes));
        RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(BackgroundTileWordBytes.Of(stock.BackgroundPages.Span.Slice(index * 1024, 1024)))).ToArray();
        ImportIntroCinematicArtwork Copy(byte[] portrait) => new(new IntroCinematicArtworkParts { CaretSprites = stock.CaretSprites, NextPageArrowSprites = stock.NextPageArrowSprites, MotherBrainSprites = stock.MotherBrainSprites, MotherBrainExplosionSprites = stock.MotherBrainExplosionSprites, RinkaSprites = stock.RinkaSprites, EggEffectSprites = stock.EggEffectSprites, DiscoveryActorSprites = stock.DiscoveryActorSprites, ScientistSprites = stock.ScientistSprites, Backdrop = stock.Backdrop, CeresFlight = stock.CeresFlight, CeresDestruction = stock.CeresDestruction, CeresDestructionMaps = stock.CeresDestructionMaps, CeresDestructionSprites = stock.CeresDestructionSprites }, new IntroCinematicLayouts(pages, Page(portrait), Page(BackgroundTileWordBytes.Of(stock.InitialNarrationTilemap.Span)), stock.FinalLine, stock.EyeFrames, stock.ZebesRevealMap), new IntroCinematicCharacters(stock.BackgroundCharacters, stock.IntroObjectCharacters, stock.CinematicObjectCharacters, stock.ZebesReveal.Characters, stock.FlightCharacters), stock.Palette, stock.FlightPalette);
        AssertTrue(BackgroundTileWordBytes.Of(stock.PortraitTilemap.Span).AsSpan().SequenceEqual(native), "all 1024 installed portrait words match native");
        AssertEqual(stock.ContentIdentity, Copy(native).ContentIdentity, "equivalent portrait preserves canonical hash");
        var intro = new IntroCinematicState(new IntroCinematicOverreads(InstalledFixtures.SamusOverreads, InstalledFixtures.BossAndIntroOverreads.ShellFragmentFallPastCurve), characterArtwork: stock.BoundCatalog)
        {
            Random = new SharedRandom(),
            UnauthoredBoostCadence = new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(),
        };
        AssertTrue(intro.ShownVram().AsSpan(
            (IntroVramLayout.PortraitTilemap.Offset * 2), native.Length).SequenceEqual(native),
            "actual opening constructor uploads installed portrait map");
        Console.WriteLine("Portrait map: 1024 native words, canonical hash and actual VRAM upload pass.");
    }

    /// <summary>Runs the retail-ROM room-flash color and mirror comparison from the command line.</summary>
    [VerificationCommand("--lookup-stream3-room-flash")]
    internal static int LookupStream3RoomFlashCommand()
    {
        Suite(nameof(VerifyStream3RoomFlash), () => VerifyStream3RoomFlash(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Room flash:336native colors/mirrors,zero stock arrays/paint scalars pass.");
        return 0;
    }

    /// <summary>Runs the glass and tube room-entry palette comparison from the command line.</summary>
    [VerificationCommand("--lookup-stream3-room-entry-palettes")]
    internal static int LookupStream3RoomEntryPalettesCommand()
    {
        Suite(nameof(VerifyStream3RoomEntryPalettes), () => VerifyStream3RoomEntryPalettes(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Glass/tube:30native colors pass.");
        return 0;
    }

    /// <summary>Runs the recovery-light color and room-routing comparison from the command line.</summary>
    [VerificationCommand("--lookup-stream3-recovery-lights")]
    internal static int LookupStream3RecoveryLightsCommand()
    {
        Suite(nameof(VerifyStream3RecoveryLights), () => VerifyStream3RecoveryLights(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Recovery lights:196native colors and routing pass.");
        return 0;
    }

    /// <summary>Runs the Mother Brain attack-palette and paired rear-leg comparison.</summary>
    [VerificationCommand("--lookup-stream3-attack-palette")]
    internal static int LookupStream3AttackPaletteCommand()
    {
        Suite(nameof(VerifyStream3MotherBrainAttackPalette), () => VerifyStream3MotherBrainAttackPalette(CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        Console.WriteLine("Mother Brain attack palette:15 native colors and paired rear routing pass.");
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
        Console.WriteLine("Baby sprite reflection:90 native parts/OAM and hashes/bounds pass.");
        return 0;
    }

    /// <summary>Runs the final-room paint and dependent-palette checks.</summary>
    [VerificationCommand("--lookup-stream3-final-room-paints")]
    internal static int LookupStream3FinalRoomPaintsCommand()
    {
        var native = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStream3FinalRoomPaints), () => VerifyStream3FinalRoomPaints(native));
        Console.WriteLine("Final room:24native colors and the dependent palettes pass.");
        return 0;
    }

    /// <summary>Runs the menu sprite geometry and ordered OAM composition checks.</summary>
    [VerificationCommand("--lookup-stream3-menu-sprites")]
    internal static int LookupStream3MenuSpritesCommand()
    {
        var oracle = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStream3MenuSpriteGeometry), () => VerifyStream3MenuSpriteGeometry(oracle));
        Console.WriteLine("Menu sprites: 15 native parts, out-of-sheet rejection and shared cursor loaders pass.");
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
        Console.WriteLine("Options headings: 144 native parts and actual OAM draw pass.");
        return 0;
    }

    /// <summary>Runs the file-select border geometry, appearance, and ordering checks.</summary>
    [VerificationCommand("--lookup-stream3-file-select-borders")]
    internal static int LookupStream3FileSelectBordersCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyStream3FileSelectBorders), () => VerifyStream3FileSelectBorders(rom, FileSelectPresentationExtractor.Extract(rom)));
        Console.WriteLine("File-select borders:114native parts and actual OAM draw pass;bounds,appearance and order remain required.");
        return 0;
    }

    /// <summary>Loads the seven grapple tile transfers and checks decoded atlas patterns.</summary>
    [VerificationCommand("--lookup-stream3-grapple-tile-patterns")]
    internal static int LookupStream3GrappleTilePatternsCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            GrappleTileTransfer[] expectedTransfers =
            [
                new(InstalledArtworkId.GrapplePointFirstTiles, 0, 32),
                new(InstalledArtworkId.GrapplePointSecondTiles, 32, 32),
                new(InstalledArtworkId.GrapplePointThirdTiles, 64, 32),
                new(InstalledArtworkId.GrapplePointFourthTiles, 96, 32),
                new(InstalledArtworkId.GrappleHorizontalSegmentTiles, 128, 128),
                new(InstalledArtworkId.GrappleDiagonalSegmentTiles, 256, 128),
                new(InstalledArtworkId.GrappleVerticalSegmentTiles, 384, 128),
            ];
            VerifyStream3GrappleTilePatterns(rom, expectedTransfers);
        Console.WriteLine("Grapple tiles:512 native bytes and seven transfer bindings pass; only reviewed stroke masks and visual shape choices remain.");
        return 0;
    }

    /// <summary>Runs the intro narration glyph-coordinate and line-layout comparison.</summary>
    [VerificationCommand("--lookup-narration-layout")]
    internal static int LookupNarrationLayoutCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var json = IntroNarrationExtractor.Extract(rom);
        Suite(nameof(VerifyStream3NarrationLayout), () => VerifyStream3NarrationLayout(rom, IntroNarrationFile.Read(new MemoryStream(json))));
        Console.WriteLine("Narration layout: 770 native glyph/coordinate records, six calculated pages and one retained hard break pass.");
        return 0;
    }

    /// <summary>Runs file-select patch cell and tilemap placement checks.</summary>
    [VerificationCommand("--lookup-file-select-patches")]
    internal static int LookupFileSelectPatchesCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var data = SuperMetroid.AssetExtraction.Frontend.FileSelectPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyStream3FileSelectPatches), () => VerifyStream3FileSelectPatches(rom, data, FileSelectSourcePresentation.Load(new MemoryStream(data))));
        Console.WriteLine("File-select patches: native cells, zero stock fallback and actual placement pass.");
        return 0;
    }
}
