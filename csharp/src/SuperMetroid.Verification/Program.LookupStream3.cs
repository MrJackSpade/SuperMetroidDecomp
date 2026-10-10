using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyStream3FallingTubePopulationLayout(ISnesAddressSpace rom)
    {
        ushort[] expected = [0x8ae5, 0x8af5, 0x8b05, 0x8b15, 0x8b25];
        IReadOnlyList<ushort> pointers = MotherBrainFallingTubePopulationDefinitions.Pointers;
        AssertTrue(expected.SequenceEqual(pointers), "five native falling-tube record identities");
        AssertThrows<IndexOutOfRangeException>(() => _ = pointers[-1], "negative tube population index");
        AssertThrows<IndexOutOfRangeException>(() => _ = pointers[pointers.Count], "upper tube population index");
        for (int address = 0x8ae4; address <= 0x8b35; address++)
        {
            ushort pointer = (ushort)address;
            if (!expected.Contains(pointer))
                AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainFallingTubePopulationDefinitions.Get(pointer),
                    $"nonrecord tube population address {pointer:X4}");
        }
        Suite(nameof(VerifyMotherBrainFallingTubePopulationDefinitions), () => VerifyMotherBrainFallingTubePopulationDefinitions(rom));
        Console.WriteLine("Falling-tube layout: five native records, all eight columns, production spawns and independent fixture behavior pass; placement magnitudes and delay remain required.");
    }
    private static void VerifyStream3IntroPaletteRows(ISnesAddressSpace rom)
    {
        ushort[] native = Enumerable.Range(0, SnesCgram.ColorCount)
            .Select(color => ReadVerificationWord(rom, IntroCinematicRomData.Assets.Palette + 2 * color)).ToArray();
        IntroCinematicPalette stock = Load(native);
        Confirm(stock, native);
        var rows = (Array)typeof(IntroCinematicPalette).GetField("rows", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
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
            AssertTrue(ToColors(expected).AsSpan().SequenceEqual(cgram.Colors), "intro palette CGRAM load");
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
    private static void VerifyStream3IntroEyeRectangles(ISnesAddressSpace rom)
    {
        ushort[][] native = Enumerable.Range(0, 4).Select(frame => Enumerable.Range(0, 6)
            .Select(cell => ReadVerificationWord(rom, 0x8cd785 + 16 * frame + 2 * cell)).ToArray()).ToArray();
        IntroEyeTilemapPresentation stock = Load(native);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (object rectangle in (Array)typeof(IntroEyeTilemapPresentation).GetField("frames", fields)!.GetValue(stock)!)
        {
            AssertTrue(rectangle.GetType().GetField("supplied", fields)!.GetValue(rectangle) is null,
                "stock eye rectangle stores no native word payload");
            AssertTrue(rectangle.GetType().GetField("origin", fields) is null,
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
                AssertTrue(expected[frame].AsSpan().SequenceEqual(selected.FrameWords(frame)), $"eye rectangle {frame}");
            var vram = new SnesVram();
            var objects = new IntroCinematicObjectSystem(new TestAddressSpace(), vram, new ushort[1024], new IntroJapaneseSubtitles(enabled: false), eyeArtwork: selected);
            var draw = typeof(IntroCinematicObjectSystem).GetMethod("ProcessTileData",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            for (int frame = 0; frame < expected.Length; frame++)
            {
                draw.Invoke(objects, [(ushort)(17 | 13 << 8), (ushort)(0xd781 + frame * 16)]);
                for (int cell = 0; cell < 6; cell++)
                    AssertEqual(expected[frame][cell], vram.ReadWord(0x4800 + (13 + cell / 3) * 32 + 17 + cell % 3),
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
    private static void VerifyStream3IntroDivider(ISnesAddressSpace rom)
    {
        var native = new ushort[IntroFinalLineTilemapFormat.CellCount];
        for (int index = 0; index < native.Length; index++)
            native[index] = ReadVerificationWord(rom, IntroCinematicRomData.Assets.FinalTextLine + 2 * index);
        IntroFinalLineTilemap stock = Load(native);
        AssertTrue(native.AsSpan().SequenceEqual(stock.Words.Span), "all128 native intro divider cells");
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertTrue(typeof(IntroFinalLineTilemap).GetField("suppliedWords", fields)!.GetValue(stock) is null,
            "native divider retains no generated lookup");
        for (int changed = 0; changed < native.Length; changed++)
        {
            var edited = (ushort[])native.Clone();
            edited[changed] ^= 0x4401;
            IntroFinalLineTilemap selected = Load(edited);
            AssertTrue(edited.AsSpan().SequenceEqual(selected.Words.Span), $"independent divider cell edit {changed}");
            AssertTrue(native.AsSpan().SequenceEqual(stock.Words.Span), "previous divider instance remains immutable");
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
    private static void VerifyStream3HandBeamBodyLayout()
    {
        ushort[] nativeOperands = [0x9a46, 0x9a4a, 0x9a4e, 0x9a5a, 0x9a66, 0x9a72,
            0x9a7e, 0x9a8a, 0x9a96, 0x9aa2, 0x9aae, 0x9ab2, 0x9ab8, 0x9abc, 0x9ac0];
        IReadOnlyList<ushort> calculated = MotherBrainHandBeamBodyInstructionDefinitionsTooling.PresentationOperands;
        AssertEqual(nativeOperands.Length, calculated.Count, "hand-beam body visual operand count");
        for (int index = 0; index < nativeOperands.Length; index++)
            AssertEqual(nativeOperands[index], calculated[index], $"hand-beam body native operand {index}");
        AssertTrue(nativeOperands.SequenceEqual(calculated), "hand-beam body indexed/enumerated order");
        AssertThrows<IndexOutOfRangeException>(() => _ = calculated[-1], "hand-beam body negative operand index");
        AssertThrows<IndexOutOfRangeException>(() => _ = calculated[calculated.Count], "hand-beam body upper operand index");
        Suite(nameof(VerifyMotherBrainHandBeamBodyInstructionDefinitions), () => VerifyMotherBrainHandBeamBodyInstructionDefinitions());
        Console.WriteLine("Hand-beam body address layout: fifteen native identities, enumeration and index bounds pass; selected mechanics inputs remain required.");
    }
    private static void VerifyStream3OptionsBorders(ISnesAddressSpace rom, byte[] imported)
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        GameOptionsPresentationDocument Read() => System.Text.Json.JsonSerializer.Deserialize<GameOptionsPresentationDocument>(imported, MapPresentationFormat.JsonOptions)!;
        Dictionary<string, SpriteComposition> Load(GameOptionsPresentationDocument document)
        {
            var value = GameOptionsPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
            return (Dictionary<string, SpriteComposition>)typeof(GameOptionsPresentation).GetField("sprites", flags)!.GetValue(value)!;
        }
        void Confirm(SpriteComposition actual, SpriteVisualPart[] parts)
        {
            var expected = MenuSpriteCompiler.Compile(parts, "independent border oracle");
            AssertEqual(expected.PartCount, actual.PartCount, "border ordered part count");
            for (int index = 0; index < expected.PartCount; index++)
                AssertEqual(expected.Part(index), actual.Part(index), "border exact coordinates, appearance and order");
        }
        foreach ((string name, int address, int count) in new[]
        {
            ("Heading.Primary", 0x82d24b, 34), ("Heading.Controller", 0x82d2f7, 58), ("Heading.Special", 0x82d41b, 52),
        })
        {
            var original = Read();
            var stock = Load(original)[name];
            AssertTrue(typeof(SpriteComposition).GetField("parts", flags)!.GetValue(stock) is MenuHeadingBorderDefinitions,
                "native heading stores only semantic title identity instead of supplied bounds/order/style");
            AssertEqual(count, stock.PartCount, "native perimeter count");
            Confirm(stock, original.Sprites[name]);
            var presentation = GameOptionsPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(original, MapPresentationFormat.JsonOptions)));
            string page = original.HeadingAnchors.Keys.Single(key => GameOptionsPresentationDefinitions.HeadingFrameName(key) == name);
            var actualOam = new OamBuffer();
            var expectedOam = new OamBuffer();
            presentation.DrawHeading(actualOam, page, 3);
            var point = original.HeadingAnchors[page];
            MenuSpriteCompiler.Compile(original.Sprites[name], name).DrawOnScreen(expectedOam,
                (ushort)point.X, unchecked((ushort)(point.Y - 3)), (ushort)(original.CursorPalette << 9));
            AssertTrue(actualOam.LowTable.SequenceEqual(expectedOam.LowTable) && actualOam.HighTable.SequenceEqual(expectedOam.HighTable),
                "actual heading draw preserves exact native ordered OAM");
            ushort Word(int location) => (ushort)(rom.ReadByte(location) | rom.ReadByte(location + 1) << 8);
            AssertEqual((ushort)count, Word(address), "native border header");
            for (int index = 0; index < count; index++)
            {
                int entry = address + 2 + index * 5;
                var part = stock.Part(index);
                AssertEqual(Word(entry), part.X.Raw, "native border X/size");
                AssertEqual(rom.ReadByte(entry + 2), part.Y, "native border Y");
                AssertEqual((ushort)(Word(entry + 3) & ~0x0e00), part.Attributes.Raw, "native border inherited-palette attributes");
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
                    Confirm(Load(edited)[name], edited.Sprites[name]);
                }
            }
            var reversed = Read();
            Array.Reverse(reversed.Sprites[name]);
            Confirm(Load(reversed)[name], reversed.Sprites[name]);
            var expanded = Read();
            expanded.Sprites[name] = [.. expanded.Sprites[name], expanded.Sprites[name][0]];
            Confirm(Load(expanded)[name], expanded.Sprites[name]);
        }
    }
    private static void VerifyStream3FileSelectBorders(ISnesAddressSpace rom, byte[] imported)
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        FileSelectPresentationDocument Read() => System.Text.Json.JsonSerializer.Deserialize<FileSelectPresentationDocument>(imported, MapPresentationFormat.JsonOptions)!;
        Dictionary<string, SpriteComposition> Load(FileSelectPresentationDocument document)
        {
            var value = FileSelectPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
            return (Dictionary<string, SpriteComposition>)typeof(FileSelectPresentation).GetField("sprites", flags)!.GetValue(value)!;
        }
        void Confirm(SpriteComposition actual, SpriteVisualPart[] parts)
        {
            var expected = MenuSpriteCompiler.Compile(parts, "independent border oracle");
            AssertEqual(expected.PartCount, actual.PartCount, "border ordered part count");
            for (int index = 0; index < expected.PartCount; index++)
                AssertEqual(expected.Part(index), actual.Part(index), "border exact coordinates, appearance and order");
        }
        foreach ((string name, int address, int count) in new[]
        {
            ("Border.Main", 0x82d00b, 32), ("Border.Copy", 0x82d0ad, 40), ("Border.Clear", 0x82d177, 42),
        })
        {
            var original = Read();
            var stock = Load(original)[name];
            AssertTrue(typeof(SpriteComposition).GetField("parts", flags)!.GetValue(stock) is MenuHeadingBorderDefinitions,
                "native file border uses semantic title and calculated perimeter");
            AssertEqual(count, stock.PartCount, "native perimeter count");
            Confirm(stock, original.Sprites[name]);
            var presentation = FileSelectPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(original, MapPresentationFormat.JsonOptions)));
            string page = original.BorderAnchors.Keys.Single(key => FileSelectPresentationDefinitions.BorderFrameName(key) == name);
            var actualOam = new OamBuffer();
            var expectedOam = new OamBuffer();
            presentation.DrawBorder(actualOam, page);
            var anchor = original.BorderAnchors[page];
            MenuSpriteCompiler.Compile(original.Sprites[name], name).DrawOnScreen(expectedOam,
                (ushort)anchor.X, (ushort)anchor.Y, (ushort)(original.ObjectPalette << 9));
            AssertTrue(actualOam.LowTable.SequenceEqual(expectedOam.LowTable) && actualOam.HighTable.SequenceEqual(expectedOam.HighTable),
                "actual file-select border draw preserves native OAM");
            ushort Word(int location) => (ushort)(rom.ReadByte(location) | rom.ReadByte(location + 1) << 8);
            AssertEqual((ushort)count, Word(address), "native border header");
            for (int index = 0; index < count; index++)
            {
                int entry = address + 2 + index * 5;
                var part = stock.Part(index);
                AssertEqual(Word(entry), part.X.Raw, "native border X/size");
                AssertEqual(rom.ReadByte(entry + 2), part.Y, "native border Y");
                AssertEqual((ushort)(Word(entry + 3) & ~0x0e00), part.Attributes.Raw, "native border inherited-palette attributes");
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
                    Confirm(Load(edited)[name], edited.Sprites[name]);
                }
            }
            var reversed = Read();
            Array.Reverse(reversed.Sprites[name]);
            Confirm(Load(reversed)[name], reversed.Sprites[name]);
            var expanded = Read();
            expanded.Sprites[name] = [.. expanded.Sprites[name], expanded.Sprites[name][0]];
            Confirm(Load(expanded)[name], expanded.Sprites[name]);
        }
    }
    private static void VerifyStream3GrappleTilePatterns(ISnesAddressSpace rom, GrappleTileTransfer[] transfers)
    {
        byte[] planar = transfers.SelectMany(transfer => Enumerable.Range(0, transfer.ByteCount)
            .Select(index => rom.ReadByte(transfer.SourceAddress + index))).ToArray();
        GrappleTileAtlas Load()
        {
            byte[] pixels = SnesGraphics.DecodePlanarTiles(planar, 4, 16, out int width, out int height);
            using var png = new MemoryStream();
            IndexedPng.Write(png, width, height, pixels, SnesGraphics.DiagnosticPalette(16));
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
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertEqual(64, ((byte[])typeof(GrappleTileAtlas).GetField("independentTiles", fields)!.GetValue(stock)!).Length,
            "stream 3 Grapple retains only eight independent binary coverage patterns");
        foreach (string field in new[] { "firstPoint", "secondPoint", "thirdPoint", "fourthPoint", "verticalSegments" })
            AssertTrue(typeof(GrappleTileAtlas).GetField(field, fields)!.GetValue(stock) is null,
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
    private static void VerifyStream3NarrationLayout(ISnesAddressSpace rom, byte[] json, IntroNarrationPresentation stock)
    {
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        int nativeCharacters = 0;
        foreach (var source in IntroNarrationDefinitions.Pages)
        {
            object selected = stock.GetLines(source.Id);
            AssertTrue(selected.GetType().GetField("supplied", fields)!.GetValue(selected) is null,
                "native narration calculates line boundaries and rows");
            string text = (string)selected.GetType().GetField("text", fields)!.GetValue(selected)!;
            AssertEqual(source.Id == IntroNarrationPageId.Page1 ? 1 : 0, text.Count(character => character == '\n'),
                "narration retains only the one independently chosen hard break");
            var expected = new List<IntroNarrationCharacter>();
            int cursor = 0x8c0000 | (source.InstructionPointer + 8);
            while ((Read(cursor) & 0x8000) == 0)
            {
                ushort position = Read(cursor + 2);
                ushort glyph = Read(0x8c0000 | (Read(cursor + 4) + 4));
                expected.Add(new(position & 255, position >> 8, glyph, glyph == (ushort)IntroNarrationSymbolWord.Blank));
                cursor += 6;
            }
            AssertTrue(stock.Compile(source.Id).SequenceEqual(expected), "calculated narration matches every native glyph/column/row record");
            nativeCharacters += expected.Count;
            AssertThrows<IndexOutOfRangeException>(() => _ = stock.GetLines(source.Id)[-1], "narration line lower bound");
            AssertThrows<IndexOutOfRangeException>(() => _ = stock.GetLines(source.Id)[stock.GetLines(source.Id).Count], "narration line upper bound");
        }
        AssertEqual(770, nativeCharacters, "six native narration pages contain770 characters");
        var document = System.Text.Json.JsonSerializer.Deserialize<IntroNarrationDocument>(json, MapPresentationFormat.JsonOptions)!;
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
    private static void VerifyStream3MochtroidVisuals(ISnesAddressSpace rom)
    {
        EnemySpritemapDefinition[] frames = MochtroidVisualDefinitions.Frames().ToArray();
        AssertEqual(6, frames.Length, "Mochtroid calculated registration count");
        for (int family = 0; family < 2; family++)
        {
            int start = family == 0 ? 0xa747 : 0xa75b;
            int pointer = family == 0 ? 0xa9b0 : 0xaa06;
            for (int pose = 0; pose < 3; pose++)
            {
                var frame = frames[family * 3 + pose];
                AssertEqual((byte)0xa3, frame.Bank, "Mochtroid native frame bank");
                AssertEqual((ushort)pointer, frame.Pointer, "Mochtroid native counted-record stride");
                AssertEqual($"mochtroid_{(family == 0 ? "flight" : "attached")}_{pose}", frame.Name, "Mochtroid legacy frame name");
                int count = rom.ReadByte(0xa30000 | pointer) | rom.ReadByte(0xa30000 | (pointer + 1)) << 8;
                pointer += 2 + 5 * count;
            }
            for (int offset = -1; offset <= 16; offset++)
            {
                ushort operand = (ushort)(start + offset);
                if (offset >= 0 && offset <= 12 && offset % 4 == 0)
                {
                    ushort expected = (ushort)(rom.ReadByte(0xa30000 | operand) | rom.ReadByte(0xa30000 | (operand + 1)) << 8);
                    AssertEqual(expected, MochtroidVisualDefinitions.FrameAt(operand), "Mochtroid native ping-pong selector");
                }
                else
                    AssertThrows<InvalidDataException>(() => MochtroidVisualDefinitions.FrameAt(operand), "Mochtroid rejects non-selector address");
            }
        }
    }
    private static void VerifyStream3MenuSpriteGeometry(ISnesAddressSpace rom)
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        static Dictionary<string, SpriteComposition> Sprites(object presentation) =>
            (Dictionary<string, SpriteComposition>)presentation.GetType().GetField("sprites", flags)!.GetValue(presentation)!;
        static void Confirm(SpriteComposition actual, SpriteVisualPart[] expectedParts)
        {
            var expected = MenuSpriteCompiler.Compile(expectedParts, "native or independently edited menu composition");
            AssertEqual(expected.PartCount, actual.PartCount, "menu composition exact part count");
            for (int part = 0; part < expected.PartCount; part++)
                AssertEqual(expected.Part(part), actual.Part(part), "menu composition exact ordered geometry, tiles, attributes and palette inheritance");
        }
        static bool Calculated(SpriteComposition composition) =>
            typeof(SpriteComposition).GetField("parts", flags)!.GetValue(composition) is not CompiledSpritePart[];

        byte[] imported = SuperMetroid.AssetExtraction.GameOverPresentationExtractor.Extract(rom);
        GameOverPresentationDocument Read() => System.Text.Json.JsonSerializer.Deserialize<GameOverPresentationDocument>(imported, MapPresentationFormat.JsonOptions)!;
        GameOverPresentation Load(GameOverPresentationDocument document) => GameOverPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        var original = Read();
        var stock = Sprites(Load(original));
        var presentation = Load(original);
        foreach (string name in GameOverPresentationDefinitions.SpriteNames)
        {
            var actualOam = new OamBuffer();
            var expectedOam = new OamBuffer();
            int x, y, palette;
            if (name.StartsWith("Cursor.", StringComparison.Ordinal))
            {
                int frame = name[^1] - '0';
                presentation.DrawCursor(actualOam, frame, false);
                x = original.CursorX; y = original.YesCursorY; palette = presentation.CursorPaletteIndex;
            }
            else
            {
                if (name == GameOverPresentationDefinitions.EggFrame) presentation.DrawEgg(actualOam);
                else presentation.DrawBaby(actualOam, name switch { "Baby.Closed" => GameOverBabyFrame.Closed, "Baby.Middle" => GameOverBabyFrame.Middle, _ => GameOverBabyFrame.Open });
                x = original.BabyAnchor.X; y = original.BabyAnchor.Y;
                palette = name == GameOverPresentationDefinitions.EggFrame ? presentation.EggPaletteIndex : presentation.BabyPaletteIndex;
            }
            MenuSpriteCompiler.Compile(original.Sprites[name], name).DrawOnScreen(expectedOam,
                (ushort)x, (ushort)y, (ushort)(palette << 9));
            AssertTrue(actualOam.LowTable.SequenceEqual(expectedOam.LowTable) && actualOam.HighTable.SequenceEqual(expectedOam.HighTable),
                "actual game-over draw emits exact native ordered OAM " + name);
        }
        foreach (string name in GameOverPresentationDefinitions.SpriteNames)
        {
            if (name == "Cursor.0") for (int part = 0; part < 2; part++) AssertEqual(stock[name].Part(part), new MenuCursorParts(MenuMissileAnimationDefinitions.SpritemapId(0) - 0x34)[part], "exact first cursor candidate part");
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
        ConfirmCursorCaller(SuperMetroid.AssetExtraction.FileSelectPresentationExtractor.Extract(rom), source => FileSelectPresentation.Load(source));
        ConfirmCursorCaller(SuperMetroid.AssetExtraction.GameOptionsPresentationExtractor.Extract(rom), source => GameOptionsPresentation.Load(source));

        void ConfirmCursorCaller(byte[] json, Func<Stream, object> load)
        {
            var presentation = Sprites(load(new MemoryStream(json)));
            for (int frame = 0; frame < 4; frame++)
            {
                string name = GameOverPresentationDefinitions.CursorFrameName(frame);
                var node = System.Text.Json.Nodes.JsonNode.Parse(json)!;
                var parts = System.Text.Json.JsonSerializer.Deserialize<SpriteVisualPart[]>(node["sprites"]![name]!.ToJsonString(), MapPresentationFormat.JsonOptions)!;
                AssertTrue(Calculated(presentation[name]), "shared menu cursor caller uses calculated geometry");
                Confirm(presentation[name], parts);
                parts[0] = parts[0] with { OffsetX = parts[0].OffsetX + 1, TileColumn = parts[0].TileColumn ^ 1 };
                node["sprites"]![name] = System.Text.Json.JsonSerializer.SerializeToNode(parts, MapPresentationFormat.JsonOptions);
                var edited = Sprites(load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(node.ToJsonString()))));
                Confirm(edited[name], parts);
            }
        }
    }
    private static void VerifyStream3EnemyFrameRegistration(ISnesAddressSpace rom)
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
        for (int index = 0; index < RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount; index++)
        {
            int address = 0xb40000 | RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(index);
            ushort pointer = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            if (!namedSpritePointers.Contains(pointer)) additionalPointers.Add(pointer);
        }
        var expectedAdditional = additionalPointers.Select(pointer =>
            new EnemySpritemapDefinition(0xb4, pointer, $"room_sprite_b4_{pointer:x4}")).ToArray();
        AssertTrue(all.Skip(472).Take(expectedAdditional.Length).SequenceEqual(expectedAdditional),
            "enemy additional room-sprite sort and duplicate exclusion preserve named prefix");
        AssertEqual(all.Length, all.Select(frame => (frame.Bank, frame.Pointer)).Distinct().Count(), "enemy frame identities remain unique");
        Suite(nameof(VerifyEnemyLegacyOverrides), () => VerifyEnemyLegacyOverrides());
    }
    private static void VerifyStream3HopperOperandPositions(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var operands = new SortedSet<ushort>();
        for (int selector = 0; selector < 16; selector++)
        {
            int cursor = 0xa30000 | Word(0xa3aac2 + selector * 2);
            cursor += 2; // Each native program first changes off-screen processing.
            if (Word(cursor) == EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3)
                cursor += 4;
            while (Word(cursor) < 0x8000)
            {
                operands.Add((ushort)(cursor + 2));
                cursor += 4;
            }
        }
        AssertEqual(40, operands.Count, "hopper native timed-pose operand count");
        int index = 0;
        foreach (ushort operand in operands)
            AssertEqual(operand, HopperInstructionProgramDefinitions.PresentationWordAddress(index++),
                "hopper calculated selector position matches native program structure");
    }
    private static void VerifyStream3FileSelectPatches(ISnesAddressSpace rom, byte[] imported, FileSelectPresentation stock)
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        FileSelectPresentationDocument Read() => System.Text.Json.JsonSerializer.Deserialize<FileSelectPresentationDocument>(imported, MapPresentationFormat.JsonOptions)!;
        FileSelectPresentation Load(FileSelectPresentationDocument document) => FileSelectPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        foreach ((string name, string field, int address, int count) in new[]
        {
            (FileSelectPresentationDefinitions.EnergyPatch, "energyPatch", 0x81b496, 4),
            (FileSelectPresentationDefinitions.TimeColonPatch, "timeColonPatch", 0x81b4a8, 1),
            (FileSelectPresentationDefinitions.NoDataPatch, "noDataPatch", 0x81b4ac, 11),
        })
        {
            FileSelectCompiledPatch Compiled(FileSelectPresentation value) => (FileSelectCompiledPatch)typeof(FileSelectPresentation).GetField(field, flags)!.GetValue(value)!;
            AssertTrue(typeof(FileSelectCompiledPatch).GetField("suppliedCells", flags)!.GetValue(Compiled(stock)) is null,
                "file-select stock patch retains no cells " + name);
            for (int index = 0; index < count; index++)
            {
                ushort word = (ushort)(rom.ReadByte(address + index * 2) | rom.ReadByte(address + index * 2 + 1) << 8);
                AssertEqual(new FileSelectCompiledPatchCell(index, 0, word), FileSelectPresentationDefinitions.PatchCell(name, index),
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
            AssertEqual((byte)0xff, rom.ReadByte(address + count * 2), "file-select native patch terminator low");
            AssertEqual((byte)0xff, rom.ReadByte(address + count * 2 + 1), "file-select native patch terminator high");
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
                    AssertEqual(new FileSelectCompiledPatchCell(source.X, source.Y, word), compiled.Cell(index),
                        "file-select independent supplied order and coordinates");
                    expected[(3 + source.Y) * 32 + 4 + source.X] = word;
                }
                presentation.ApplyPatch(actual, name, new MapLabelPoint(4, 3));
                AssertTrue(actual.SequenceEqual(expected), "file-select exact stock or independently edited patch placement");
            }
        }
    }
    private static void VerifyStream3GameOverText(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.GameOverPresentationExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<GameOverPresentationDocument>(imported, MapPresentationFormat.JsonOptions)!;
        GameOverPresentation Load() => GameOverPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        var stock = Load();
        AssertTrue(typeof(GameOverPresentation).GetField("tilemap", System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic)!.GetValue(stock) is null, "game-over stock text retains no tilemap row");
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
            var vram = new SnesVram();
            presentation.LoadTilemapTo(vram, 0x1000);
            for (int cell = 0; cell < document.Tilemap.Length; cell++)
            {
                MapPresentationCell source = document.Tilemap[cell];
                ushort expected = SnesBgTilemapWord.Create(source.TileRow * MapTileAtlasFormat.TileColumns + source.TileColumn,
                    source.Palette, source.Priority, (source.FlipX ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
                    (source.FlipY ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)).Raw;
                AssertEqual(expected, vram.ReadWord(0x1000 + cell), "game-over native/edited text word and VRAM placement");
            }
        }
    }
    private static void VerifyStream3OptionsGeometry(ISnesAddressSpace rom)
    {
        ushort LookupWord(ISnesAddressSpace source, int address) => (ushort)(source.ReadByte(address) | source.ReadByte(address + 1) << 8);
        byte[] imported = SuperMetroid.AssetExtraction.GameOptionsPresentationExtractor.Extract(rom);
        GameOptionsPresentationDocument ReadDocument() => System.Text.Json.JsonSerializer.Deserialize<GameOptionsPresentationDocument>(
            imported, MapPresentationFormat.JsonOptions)!;
        GameOptionsPresentation Load(GameOptionsPresentationDocument document) => GameOptionsPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        var stock = ReadDocument();
        var loaded = Load(stock);
        foreach (string field in new[] { "controllerLabelAnchors", "languageRegions", "specialToggles", "headingAnchors", "cursorAnchors" })
            AssertTrue(typeof(GameOptionsPresentation).GetField(field, System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic)!.GetValue(loaded) is null, "stock options geometry calculates " + field);
        for (int action = 0; action < 7; action++)
        {
            ushort offset = LookupWord(rom, 0x82f639 + action * 2);
            AssertEqual(new MapLabelPoint(offset / 2 % 32, offset / 2 / 32),
                GameOptionsPresentationDefinitions.ControllerAnchor(action), "native options label anchor");
        }
        foreach ((string page, int address, int count) in new[]
        {
            (GameOptionsPresentationDefinitions.PrimaryMenu, 0x82f307, 5),
            (GameOptionsPresentationDefinitions.ControllerMenu, 0x82f31b, 9),
            (GameOptionsPresentationDefinitions.SpecialMenu, 0x82f33f, 3),
        })
            for (int row = 0; row < count; row++)
                AssertEqual(new MapLabelPoint(LookupWord(rom, address + row * 4), LookupWord(rom, address + row * 4 + 2)),
                    loaded.CursorPosition(page, row), "native options cursor anchor");
        var stockLabels = (Dictionary<string, ushort[]>)typeof(GameOptionsPresentation).GetField("controllerLabels",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(loaded)!;
        AssertEqual(0, stockLabels.Count, "options stock button glyphs retain no tilemap rows");
        for (int button = 0; button < 7; button++)
        for (int cell = 0; cell < 6; cell++)
            AssertEqual(LookupWord(rom, 0x82f659 + button * 12 + cell * 2),
                GameOptionsPresentationDefinitions.ControllerLabelWord(GameOptionsPresentationDefinitions.ControllerLabelName(button), cell),
                "native options glyph composition preserves each tile and flip");
        Confirm(stock, loaded);
        var edited = ReadDocument();
        for (int action = 0; action < edited.ControllerLabelAnchors.Length; action++)
        {
            MapLabelPoint old = edited.ControllerLabelAnchors[action];
            edited.ControllerLabelAnchors[action] = new(old.X + 1, old.Y);
        }
        foreach (MapPresentationCell[] label in edited.ControllerLabels.Values)
            for (int cell = 0; cell < label.Length; cell++)
                label[cell] = label[cell] with { FlipX = !label[cell].FlipX, Palette = 1 };
        edited.LanguageRegions[0].Cells[0] = 0;
        edited.SpecialToggles[GameOptionsPresentationDefinitions.IconCancelToggle].EnabledCells[0] = 0;
        foreach (string page in edited.HeadingAnchors.Keys.ToArray())
        {
            MapLabelPoint old = edited.HeadingAnchors[page];
            edited.HeadingAnchors[page] = new(old.X + 1, old.Y);
            for (int row = 0; row < edited.CursorAnchors[page].Length; row++)
            {
                old = edited.CursorAnchors[page][row];
                edited.CursorAnchors[page][row] = new(old.X + 1, old.Y);
            }
        }
        Confirm(edited, Load(edited));

        void Confirm(GameOptionsPresentationDocument document, GameOptionsPresentation presentation)
        {
            foreach (bool japanese in new[] { false, true })
            {
                byte[] actual = presentation.CreatePage(GameOptionsPresentationDefinitions.PrimaryPage);
                byte[] expected = (byte[])actual.Clone();
                foreach (var region in document.LanguageRegions)
                    Paint(expected, region.Cells, japanese == region.HighlightWhenJapanese ? document.SelectedPalette : document.UnselectedPalette);
                presentation.ApplyLanguage(actual, japanese);
                AssertTrue(actual.AsSpan().SequenceEqual(expected), "options language boxes preserve exact stock/edited cells");
            }
            foreach (var pair in document.SpecialToggles)
            foreach (bool enabled in new[] { false, true })
            {
                byte[] actual = presentation.CreatePage(GameOptionsPresentationDefinitions.SpecialEnglishPage);
                byte[] expected = (byte[])actual.Clone();
                Paint(expected, pair.Value.EnabledCells, enabled ? document.SelectedPalette : document.UnselectedPalette);
                Paint(expected, pair.Value.DisabledCells, enabled ? document.UnselectedPalette : document.SelectedPalette);
                presentation.ApplySpecialToggle(actual, pair.Key, enabled);
                AssertTrue(actual.AsSpan().SequenceEqual(expected), "options toggle boxes preserve exact stock/edited cells");
            }
            for (int action = 0; action < 7; action++)
            for (int button = 0; button < 7; button++)
            {
                byte[] actual = presentation.CreatePage(GameOptionsPresentationDefinitions.ControllerEnglishPage);
                byte[] expected = (byte[])actual.Clone();
                MapLabelPoint point = document.ControllerLabelAnchors[action];
                for (int cell = 0; cell < 6; cell++)
                {
                    MapPresentationCell source = document.ControllerLabels[GameOptionsPresentationDefinitions.ControllerLabelName(button)][cell];
                    ushort word = SnesBgTilemapWord.Create(source.TileRow * MapTileAtlasFormat.TileColumns + source.TileColumn, source.Palette, source.Priority,
                        (source.FlipX ? SnesTileFlipFlags.Horizontal : SnesTileFlipFlags.None) |
                        (source.FlipY ? SnesTileFlipFlags.Vertical : SnesTileFlipFlags.None)).Raw;
                    System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(((point.Y + cell / 3) * 32 + point.X + cell % 3) * 2), word);
                }
                presentation.ApplyControllerLabel(actual, action, button);
                AssertTrue(actual.AsSpan().SequenceEqual(expected), "options controller boxes preserve exact stock/edited placement");
            }
            foreach (var pair in document.CursorAnchors)
                for (int row = 0; row < pair.Value.Length; row++)
                    AssertEqual(pair.Value[row], presentation.CursorPosition(pair.Key, row), "options stock/edited cursor");
            foreach (var pair in document.HeadingAnchors)
            {
                var actual = new OamBuffer();
                var expected = new OamBuffer();
                presentation.DrawHeading(actual, pair.Key, 3);
                MenuSpriteCompiler.Compile(document.Sprites[GameOptionsPresentationDefinitions.HeadingFrameName(pair.Key)], "heading oracle")
                    .DrawOnScreen(expected, checked((ushort)pair.Value.X), unchecked((ushort)(pair.Value.Y - 3)),
                        SnesObjAttributeWord.Create(0, document.CursorPalette, 0).PaletteBits);
                AssertTrue(actual.LowTable.SequenceEqual(expected.LowTable) && actual.HighTable.SequenceEqual(expected.HighTable),
                    "options stock/edited heading origin and scroll");
            }
            AssertThrows<ArgumentOutOfRangeException>(() => presentation.ApplyControllerLabel(new byte[2048], -1, 0), "options invalid action");
            AssertThrows<InvalidDataException>(() => presentation.CursorPosition("invalid", 0), "options invalid page");
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
    private static void VerifyLookupStream3(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyStream3GameOverText), () => VerifyStream3GameOverText(rom));
        Suite(nameof(VerifyStream3OptionsGeometry), () => VerifyStream3OptionsGeometry(rom));
        ushort[] expectedDoorCallbacks =
        [
            DoorCodes.DoorCode_Scroll6_Green,
            DoorCodes.DoorASM_Scroll_0_Blue,
            DoorCodes.DoorASM_Scroll_13_Blue,
            DoorCodes.DoorASM_Scroll_4_Red_8_Green,
            DoorCodes.DoorASM_Scroll_8_9_A_B_Red,
            DoorCodes.DoorASM_Scroll_2_3_4_5_B_C_D_11_Red,
            DoorCodes.DoorASM_Scroll_1_4_Green,
            DoorCodes.DoorASM_Scroll_2_Blue,
            DoorCodes.DoorASM_Scroll_17_Blue,
            DoorCodes.DoorASM_Scroll_4_Blue,
            DoorCodes.DoorASM_Scroll_6_Green_duplicate,
            DoorCodes.DoorASM_Scroll_3_Green,
            DoorCodes.DoorASM_Scroll_18_1C_Green,
            DoorCodes.DoorASM_Scroll_5_6_Blue,
            DoorCodes.DoorASM_Scroll_1D_Blue,
            DoorCodes.DoorASM_Scroll_2_3_Green,
            DoorCodes.DoorASM_Scroll_0_Red_1_Green,
            DoorCodes.DoorASM_Scroll_B_Green,
            DoorCodes.DoorASM_Scroll_Scroll_1C_Red_1D_Blue,
            DoorCodes.DoorASM_Scroll_4_Red,
            DoorCodes.DoorASM_Scroll_20_24_25_Green,
            DoorCodes.DoorASM_Scroll_2_Blue_duplicate,
            DoorCodes.DoorASM_Scroll_0_Green,
            DoorCodes.DoorASM_Scroll_6_7_Green,
            DoorCodes.DoorASM_Scroll_1_Blue_2_Red,
            DoorCodes.DoorASM_Scroll_1_Blue_3_Red,
            DoorCodes.DoorASM_Scroll_0_Red_4_Blue,
            DoorCodes.DoorASM_Scroll_2_3_Blue,
            DoorCodes.DoorASM_Scroll_0_1_Green,
            DoorCodes.DoorASM_Scroll_1_Green,
            DoorCodes.DoorASM_Scroll_F_12_Green,
            DoorCodes.DoorASM_Scroll_6_Green_duplicate_again,
            DoorCodes.DoorASM_Scroll_0_Green_1_Blue,
            DoorCodes.DoorASM_Scroll_2_Green,
            DoorCodes.DoorASM_Scroll_3_4_Red_6_7_8_Blue,
            DoorCodes.DoorASM_Scroll_1_2_3_Blue_4_Green_6_Red,
            DoorCodes.DoorASM_Scroll_0_1_Blue,
            DoorCodes.DoorASM_Scroll_0_Blue_1_Red,
            DoorCodes.DoorASM_Scroll_A_Green,
            DoorCodes.DoorASM_Scroll_0_2_Green,
            DoorCodes.DoorASM_Scroll_6_7_Blue_8_Red,
            DoorCodes.DoorASM_Scroll_2_Red_3_Blue,
            DoorCodes.DoorASM_Scroll_7_Green,
            DoorCodes.DoorASM_Scroll_1_Red_2_Blue,
            DoorCodes.DoorASM_Scroll_0_Blue_3_Red,
            DoorCodes.DoorASM_Scroll_1_Blue_4_Red,
            DoorCodes.DoorASM_Scroll_0_Blue_1_2_3_Red,
            DoorCodes.DoorASM_Scroll_0_Green_duplicate,
            DoorCodes.DoorASM_Scroll_0_1_Blue_4_Red,
            DoorCodes.DoorASM_Scroll_0_Blue_3_Red_duplicate,
            DoorCodes.DoorASM_Scroll_0_Blue_duplicate,
            DoorCodes.DoorASM_Scroll_0_Blue_1_Red_duplicate,
            DoorCodes.DoorASM_Scroll_18_Blue,
            DoorCodes.DoorASM_Scroll_2_Blue_3_Red,
            DoorCodes.DoorASM_Scroll_E_Red,
            DoorCodes.DoorASM_Scroll_1_Blue,
            DoorCodes.DoorASM_Scroll_0_Green_duplicate_again,
            DoorCodes.DoorASM_Scroll_3_Red_4_Blue,
            DoorCodes.DoorASM_Scroll_29_Blue,
            DoorCodes.DoorASM_Scroll_28_2E_Green,
            DoorCodes.DoorASM_Scroll_6_7_8_9_A_B_Red,
            DoorCodes.DoorASM_Scroll_A_Red_B_Blue,
            DoorCodes.DoorASM_Scroll_0_Red_4_Blue_duplicate,
            DoorCodes.DoorASM_Scroll_0_Red_1_Blue,
            DoorCodes.DoorASM_Scroll_9_Red_A_Blue,
            DoorCodes.DoorASM_Scroll_0_2_Red_1_Blue,
            DoorCodes.DoorASM_Scroll_1_Blue_duplicate,
            DoorCodes.DoorASM_Scroll_6_Blue,
            DoorCodes.DoorASM_Scroll_4_Red_duplicate,
            DoorCodes.DoorASM_Scroll_4_7_Red,
            DoorCodes.DoorASM_Scroll_1_Blue_2_Red_duplicate,
            DoorCodes.DoorASM_Scroll_0_2_Green_duplicate,
            DoorCodes.DoorASM_Scroll_0_1_Green_duplicate,
            DoorCodes.DoorASM_Scroll_18_Blue_19_Red,
        ];
        AssertTrue(expectedDoorCallbacks.SequenceEqual(SuperMetroid.Core.Rooms.DoorScrollPrograms.Pointers),
            "stream 3 pure door callback original registration order");
        var expectedCallbacks = expectedDoorCallbacks.ToHashSet();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            AssertEqual(expectedCallbacks.Contains((ushort)pointer),
                SuperMetroid.Core.Rooms.DoorScrollPrograms.Contains((ushort)pointer), "stream 3 exact callback ownership");
        foreach (ushort pointer in expectedDoorCallbacks)
        {
            byte[] expected = Enumerable.Repeat((byte)0x7f, RoomScrollGrid.StorageByteCount).ToArray();
            var actual = RoomScrollGrid.LoadCompiled(new TestAddressSpace(), expected, 10, 5);
            ExecuteStream3DoorScroll(rom, pointer, expected);
            AssertTrue(SuperMetroid.Core.Rooms.DoorScrollPrograms.TryApply(pointer, actual), "stream 3 callback recognized");
            AssertTrue(actual.Storage.SequenceEqual(expected), "stream 3 native ordered door writes and all untouched cells");
        }
        var unchangedScroll = RoomScrollGrid.LoadCompiled(new TestAddressSpace(), new byte[50], 10, 5);
        AssertTrue(!SuperMetroid.Core.Rooms.DoorScrollPrograms.TryApply(0, unchangedScroll), "stream 3 unknown callback no-op");
        AssertThrows<ArgumentNullException>(() => SuperMetroid.Core.Rooms.DoorScrollPrograms.TryApply(0, null!),
            "stream 3 door callback null argument remains rejected before dispatch");
        for (int pair = 0; pair < 16; pair++)
        {
            int address = 0xadde5f + 2 * pair;
            int pointer = rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
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
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainBeamRomData.DirectionForQuadrants(invalid),
                "stream 3 beam dispatcher preserves span index bounds");
        Suite(nameof(VerifyMotherBrainFallingTubeInstructionDefinitions), () => VerifyMotherBrainFallingTubeInstructionDefinitions());
        IntroCinematicRomData.Palette.Regions[] introRegions =
        [IntroCinematicRomData.Palette.Gameplay, IntroCinematicRomData.Palette.GameplayClear,
         IntroCinematicRomData.Palette.Narration, IntroCinematicRomData.Palette.Discovery];
        IntroPaletteSpan[][] expectedIntroRegions =
        [
            [new(0, 20), new(96, 16), new(466, 6)],
            [new(0, 16), new(96, 16), new(466, 6)],
            [new(40, 3), new(224, 16), new(384, 32), new(480, 16)],
            [new(64, 16), new(448, 9)],
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
                AssertEqual((byte)0xa2, rom.ReadByte(address), "stream 3 intro native LDX region");
                AssertEqual((byte)0xa0, rom.ReadByte(address + 3), "stream 3 intro native LDY color count");
                AssertEqual((int)introRegions[group][index].ByteOffset,
                    rom.ReadByte(address + 1) | rom.ReadByte(address + 2) << 8, "stream 3 intro native offset");
                AssertEqual((int)introRegions[group][index].ByteCount,
                    rom.ReadByte(address + 4) | rom.ReadByte(address + 5) << 8, "stream 3 intro native count");
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
        AssertTrue(MotherBrainVisualDefinitions.Frames().Select(frame => frame.Pointer).SequenceEqual(motherBrainRoots),
            "stream 3 all original Mother Brain root identities and enumeration");
        for (int index = 0; index < motherBrainRoots.Length; index++)
        {
            var actual = MotherBrainVisualDefinitions.Frame(index);
            AssertEqual((byte)0xa9, actual.Bank, "stream 3 Mother Brain visual bank");
            AssertEqual($"mother_brain_a9_{motherBrainRoots[index]:x4}", actual.Name, "stream 3 Mother Brain visual identity name");
            int address = 0xa90000 | actual.Pointer;
            AssertEqual(nativeCharacterCounts[index], rom.ReadByte(address) | rom.ReadByte(address + 1) << 8,
                "stream 3 native OAM record widths used for root strides");
        }
        foreach (int invalid in new[] { -1, 18, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MotherBrainVisualDefinitions.Frame(invalid), "stream 3 visual root bounds");
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
            AssertThrows<ArgumentOutOfRangeException>(() => _ = MotherBrainSpecialSpriteArtworkDefinitions.All[invalid],
                "stream 3 special sheet original index exception");
        // Confirm the identified copy-list conversion against native STA operands.
        int[][] nativeDoorCopies =
        [
            [0x82e1f4, 0x82e1fa, 0x82e200, 0x82e206, 0x82e20c, 0x82e212, 0x82e218, 0x82e21e],
            [0x82e22f, 0x82e235, 0x82e23b, 0x82e241, 0x82e247],
            [0x82e252, 0x82e258, 0x82e25e, 0x82e264],
        ];
        int targetPaletteBase = rom.ReadByte(0x82e1f5) + (rom.ReadByte(0x82e1f6) << 8) - 18;
        ushort[] sourceColors = Enumerable.Range(1, 256).Select(value => (ushort)value).ToArray();
        for (int group = 0; group < nativeDoorCopies.Length; group++)
        {
            ushort[] actual = new ushort[256];
            ushort[] expected = new ushort[256];
            foreach (int instruction in nativeDoorCopies[group])
            {
                AssertEqual((byte)0x8d, rom.ReadByte(instruction), "stream 3 native door color store opcode");
                int destination = rom.ReadByte(instruction + 1) + (rom.ReadByte(instruction + 2) << 8);
                int color = (destination - targetPaletteBase) / 2;
                expected[color] = sourceColors[color];
            }
            switch (group)
            {
                case 0: DoorTransitionPaletteDefinitions.PreserveHud(ToColors(sourceColors), ToColors(actual)); break;
                case 1: DoorTransitionPaletteDefinitions.PreserveCommonCre(ToColors(sourceColors), ToColors(actual)); break;
                case 2: DoorTransitionPaletteDefinitions.PreserveEscapeTimer(ToColors(sourceColors), ToColors(actual)); break;
            }
            AssertTrue(expected.SequenceEqual(actual), "stream 3 exact native door fade preserved and black slots");
        }
        // Confirm the replaced selector for its complete ushort input domain.
        for (int angle = 0; angle <= ushort.MaxValue; angle++)
        {
            int address = 0x9bc346 + 2 * (angle >> 10);
            int expected = 0x9a0000 | rom.ReadByte(address) | rom.ReadByte(address + 1) << 8;
            var asset = GrappleTileDefinitions.SegmentAssetFor((ushort)angle);
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
            ushort list = (ushort)(rom.ReadByte(0xa8e682 + offset) | rom.ReadByte(0xa8e683 + offset) << 8);
            var function = (SparkEnemyFunction)(rom.ReadByte(0xa8e688 + offset) | rom.ReadByte(0xa8e689 + offset) << 8);
            AssertEqual(new SparkMovementDefinition(list, function),
                SparkMovementDefinitions.InitialState((ushort)parameter),
                "stream 3 native Spark selector including adjacent-word case");
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
                ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                AssertEqual(expected, ShaktoolInstructionDefinitions.ForOrientationBucket(direction),
                    "stream 3 native Shaktool orientation");
            }
        }
        for (int segment = 0; segment < 7; segment++)
        {
            int offset = segment * 2;
            ushort collision = (ushort)(rom.ReadByte(0xaadf13 + offset) | rom.ReadByte(0xaadf14 + offset) << 8);
            AssertEqual(collision, ShaktoolInstructionDefinitions.CollisionForSegment(segment),
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
        Suite(nameof(VerifyStream3RipperMappings), () => VerifyStream3RipperMappings(rom));
        Suite(nameof(VerifyStream3UniformEnemyLoops), () => VerifyStream3UniformEnemyLoops(rom));
        Suite(nameof(VerifyStream3MotherBrainFades), () => VerifyStream3MotherBrainFades(rom));
        Suite(nameof(VerifyStream3BabyFade), () => VerifyStream3BabyFade(rom));
        Suite(nameof(VerifyStream3DrainFades), () => VerifyStream3DrainFades(rom));
        Suite(nameof(VerifyStream3ShitroidPulse), () => VerifyStream3ShitroidPulse(rom));
        Suite(nameof(VerifyStream3HealthTint), () => VerifyStream3HealthTint(rom));
        Suite(nameof(VerifyStream3RecoveryLights), () => VerifyStream3RecoveryLights(rom));
        Suite(nameof(VerifyStream3RoomFlash), () => VerifyStream3RoomFlash(rom));
        Suite(nameof(VerifyMotherBrainRoomPaletteProgramDefinitions), () => VerifyMotherBrainRoomPaletteProgramDefinitions());
        Suite(nameof(VerifyStream3CorpseGeometry), () => VerifyStream3CorpseGeometry(rom));
        Suite(nameof(VerifyStream3EscapeGeometry), () => VerifyStream3EscapeGeometry(rom));
        Suite(nameof(VerifyStream3PainfulWalking), () => VerifyStream3PainfulWalking(rom));
        Suite(nameof(VerifyStream3DeathSelectors), () => VerifyStream3DeathSelectors(rom));
        Suite(nameof(VerifyMotherBrainContactHitboxes), () => VerifyMotherBrainContactHitboxes());
        byte[] grappleSpriteJson = SuperMetroid.AssetExtraction.GrappleSpriteExtractor.Extract(rom);
        var grappleSprites = GrappleSpriteCatalog.Load(new MemoryStream(grappleSpriteJson));
        AssertTrue(typeof(GrappleSpriteCatalog).GetField("segments", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(grappleSprites) is null, "stream 3 stock rope attribute rows discarded");
        AssertTrue(GrappleSpriteDefinitions.SegmentAttributeAddresses.SequenceEqual(new[] { 0x94b18d, 0x94b191, 0x94b195, 0x94b199 }),
            "stream 3 segment address enumeration");
        for (int frame = 0; frame < 4; frame++)
        {
            int address = 0x94b18d + 4 * frame;
            AssertEqual((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8), grappleSprites.Segment(frame), "stream 3 calculated native rope attributes");
            foreach (string field in new[] { "tileColumn", "tileRow", "palette", "priority", "flipX", "flipY" })
            {
                var node = System.Text.Json.Nodes.JsonNode.Parse(grappleSpriteJson)!;
                var style = node["segments"]![frame]!;
                int bit = field switch { "tileColumn" => 1, "tileRow" => 16, "palette" => 0x200, "priority" => 0x1000, "flipX" => 0x4000, _ => 0x8000 };
                if (field is "flipX" or "flipY") style[field] = !style[field]!.GetValue<bool>();
                else style[field] = style[field]!.GetValue<int>() ^ 1;
                var edited = GrappleSpriteCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(node.ToJsonString())));
                for (int check = 0; check < 4; check++)
                    AssertEqual((ushort)(grappleSprites.Segment(check) ^ (check == frame ? bit : 0)), edited.Segment(check), "stream 3 independent rope attribute edit");
            }
        }
        foreach (int invalid in new[] { -1, 4, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => _ = GrappleSpriteDefinitions.SegmentAttributeAddresses[invalid], "stream 3 rope operand address bounds");
            AssertThrows<InvalidDataException>(() => grappleSprites.Segment(invalid), "stream 3 rope attribute bounds");
        }
        var swingFrames = GrappleSwingFrameCatalog.Load(new MemoryStream(
            SuperMetroid.AssetExtraction.GrappleSwingFrameExtractor.Extract(rom)));
        for (int angle = 0; angle < 256; angle++)
            AssertEqual(rom.ReadByte(0x9bc1c2 + angle), swingFrames.Resolve((byte)angle), "stream 3 native calculated swing art frame");
        AssertTrue(typeof(GrappleSwingFrameCatalog).GetField("frames", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(swingFrames) is null, "stream 3 stock swing frame lookup discarded");
        int[] editedFrames = Enumerable.Range(0, 256).Select(angle => (int)rom.ReadByte(0x9bc1c2 + angle)).ToArray();
        for (int editedAngle = 0; editedAngle < 256; editedAngle++)
        {
            int original = editedFrames[editedAngle];
            editedFrames[editedAngle] = (original + 1) & 31;
            var edited = GrappleSwingFrameCatalog.Load(new MemoryStream(GrappleSwingFrameCatalog.Write(new()
            { Version = 1, Frames = editedFrames })));
            for (int angle = 0; angle < 256; angle++)
                AssertEqual((byte)editedFrames[angle], edited.Resolve((byte)angle), "stream 3 independently edited swing art frame");
            editedFrames[editedAngle] = original;
        }
        byte[] fileSelectJson = SuperMetroid.AssetExtraction.FileSelectPresentationExtractor.Extract(rom);
        var fileSelect = FileSelectPresentation.Load(new MemoryStream(fileSelectJson));
        foreach (string field in new[] { "digits", "slotLetters" })
            AssertTrue(typeof(FileSelectPresentation).GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(fileSelect) is null, "stream 3 stock file-select glyph sequence discarded");
        var glyphTilemap = new ushort[1024];
        for (int glyph = 0; glyph < 13; glyph++)
        {
            bool digit = glyph < 10;
            int index = digit ? glyph : glyph - 10;
            ushort expected = (ushort)(digit ? 0x2060 + index : 0x206a + index);
            var anchor = new MapLabelPoint(4, 3);
            if (digit) fileSelect.WriteDigit(glyphTilemap, anchor, 2, index);
            else fileSelect.WriteSlotLetter(glyphTilemap, anchor, index);
            AssertEqual(expected, glyphTilemap[3 * 32 + 4 + (digit ? 2 : 0)], "stream 3 actual file-select calculated glyph write");
            foreach (string field in new[] { "tileColumn", "tileRow", "palette", "priority", "flipX", "flipY" })
            {
                var node = System.Text.Json.Nodes.JsonNode.Parse(fileSelectJson)!;
                var cell = node[digit ? "digits" : "slotLetters"]![index]!;
                int bit = field switch { "tileColumn" => 1, "tileRow" => 32, "palette" => 0x400, "priority" => 0x2000, "flipX" => 0x4000, _ => 0x8000 };
                if (field is "priority" or "flipX" or "flipY") cell[field] = !cell[field]!.GetValue<bool>();
                else cell[field] = cell[field]!.GetValue<int>() ^ 1;
                var edited = FileSelectPresentation.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(node.ToJsonString())));
                if (digit) edited.WriteDigit(glyphTilemap, anchor, 2, index);
                else edited.WriteSlotLetter(glyphTilemap, anchor, index);
                AssertEqual((ushort)(expected ^ bit), glyphTilemap[3 * 32 + 4 + (digit ? 2 : 0)], "stream 3 independent file-select glyph attribute edit");
            }
        }
        string[] expectedCreditRoles = ["staff-heading", "producer-heading", "producer-name", "director-heading", "director-name", "background-designers-heading", "background-designer-1", "background-designer-2", "background-designer-3", "object-designers-heading", "object-designer-1", "object-designer-2", "samus-original-designer-heading", "samus-original-designer-name", "samus-designer-heading", "samus-designer-name", "sound-program-heading", "sound-effects-heading", "sound-programmer-name", "music-composers-heading", "music-composer-1", "music-composer-2", "program-director-heading", "program-director-name", "system-coordinator-heading", "system-coordinator-name", "system-programmer-heading", "system-programmer-name", "samus-programmer-heading", "samus-programmer-name", "event-programmer-heading", "event-programmer-name", "enemy-programmer-heading", "enemy-programmer-name", "map-programmer-heading", "map-programmer-name", "assistant-programmer-heading", "assistant-programmer-name", "coordinators-heading", "coordinator-1", "coordinator-2", "printed-art-work-heading", "printed-art-work-1", "printed-art-work-2", "printed-art-work-3", "printed-art-work-4", "printed-art-work-5", "printed-art-work-6", "special-thanks-heading", "special-thanks-01", "special-thanks-02", "special-thanks-03", "special-thanks-04", "special-thanks-05", "special-thanks-06", "special-thanks-07", "special-thanks-08", "special-thanks-09", "special-thanks-10", "special-thanks-11", "special-thanks-12", "special-thanks-13", "special-thanks-14", "special-thanks-15", "special-thanks-r-and-d", "general-manager-heading", "general-manager-name"];
        AssertTrue(CreditsPresentationDefinitions.Lines.Select(line => line.Id).SequenceEqual(expectedCreditRoles), "stream 3 all credit role identities and order");
        Suite(nameof(VerifyCreditsPresentation), () => VerifyCreditsPresentation(Path.GetFullPath("Super Metroid.smc")));
        byte[] creditsJson = SuperMetroid.AssetExtraction.CreditsPresentationExtractor.Extract(rom);
        var credits = CreditsPresentation.Load(new MemoryStream(creditsJson));
        AssertTrue(typeof(CreditsPresentation).GetField("fixtureRows", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(credits) is null, "stream 3 credits stock has no cached row storage");
        AssertEqual(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(creditsJson)), credits.ContentIdentity, "stream 3 credits source identity preserved");
        foreach (int invalid in new[] { -1, credits.RowCount, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = credits.GetRow(invalid).Length, "stream 3 calculated credits row bounds");
        Suite(nameof(VerifyCrateriaLightningPaletteFxProgramMechanicsDefinitions), () => VerifyCrateriaLightningPaletteFxProgramMechanicsDefinitions((SuperMetroid.AssetExtraction.CartridgeImportAddressSpace)rom));
        foreach (var lightning in CrateriaLightningPaletteFxProgramMechanicsDefinitions.All)
        {
            foreach (int invalid in new[] { -1, lightning.Frames.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = lightning.Frames[invalid], "stream 3 lightning frame bounds");
            foreach (int invalid in new[] { -1, lightning.MechanicsWords.Count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = lightning.MechanicsWords[invalid], "stream 3 lightning mechanics bounds");
            foreach (int invalid in new[] { -1, 2, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = lightning.MechanicsBytes[invalid], "stream 3 lightning timer bounds");
        }
        Suite(nameof(VerifyCrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions), () => VerifyCrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions((SuperMetroid.AssetExtraction.CartridgeImportAddressSpace)rom));
        var escapePrograms = CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.All;
        AssertEqual(2, escapePrograms.Count, "stream 3 escape lightning owner count");
        AssertTrue(escapePrograms.Select(program => program.Owner).SequenceEqual(new[] { CrateriaEscapeLightningPaletteOwner.YellowLightning, CrateriaEscapeLightningPaletteOwner.CreBlockPixel }), "stream 3 escape lightning owner order");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => _ = escapePrograms[invalid], "stream 3 escape lightning owner bounds");
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.Duration(invalid), "stream 3 escape lightning duration bounds");
        Suite(nameof(VerifyGrappleConnectionDefinitions), () => VerifyGrappleConnectionDefinitions((SuperMetroidAddressSpace)rom));
        Suite(nameof(VerifyWorkRobotLaserInstructionProgramDefinitions), () => VerifyWorkRobotLaserInstructionProgramDefinitions((SuperMetroidAddressSpace)rom));
        Suite(nameof(VerifyMotherBrainTurretDefinitions), () => VerifyMotherBrainTurretDefinitions((SuperMetroidAddressSpace)rom));
        Suite(nameof(VerifyMotherBrainTurretInstructionProgramDefinitions), () => VerifyMotherBrainTurretInstructionProgramDefinitions((SuperMetroidAddressSpace)rom));
        var turretMechanics = new HashSet<int>();
        for (int direction = 0; direction < 8; direction++)
        {
            turretMechanics.Add(0xc101 + 6 * direction);
            turretMechanics.Add(0xc105 + 6 * direction);
            turretMechanics.Add(0xc143 + 6 * direction);
            turretMechanics.Add(0xc147 + 6 * direction);
        }
        for (int selector = 0; selector < 9; selector++)
            turretMechanics.Add(0xc131 + 2 * selector);
        foreach (int address in new[] { 0xc19a, 0xc19c, 0xc19e, 0xc1a2, 0xc1a6, 0xc1aa, 0xc1ae, 0xc1b2 })
            turretMechanics.Add(address);
        foreach (int address in turretMechanics)
            AssertEqual((ushort)(rom.ReadByte(0x860000 | address) | rom.ReadByte(0x860000 | (address + 1)) << 8),
                MotherBrainTurretInstructionProgramDefinitions.ReadMechanicsWord((ushort)address), "stream 3 turret calculated word read");
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(turretMechanics.Contains(address) || turretMechanics.Contains(address - 1),
                MotherBrainTurretInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x860000 | address),
                "stream 3 turret mechanics byte ownership domain");
        foreach (int invalid in new[] { -1, 49, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainTurretInstructionProgramDefinitionsTooling.MechanicsWord(invalid), "stream 3 turret mechanics index bounds");
        foreach (int invalid in new[] { -1, 21, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainTurretInstructionProgramDefinitions.PresentationWordAddress(invalid), "stream 3 turret visual index bounds");
        foreach (MotherBrainContactPart part in Enum.GetValues<MotherBrainContactPart>())
        {
            var regions = MotherBrainContactHitboxDefinitions.Get(part);
            foreach (int invalid in new[] { -1, regions.Count, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => _ = regions[invalid], "stream 3 contact region bounds");
        }
        Console.WriteLine("Lookup stream 3: all implemented mapping conversions match their original values and accepted domains.");
    }

    private static void VerifyStream3DeathSelectors(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort parameter = 0; parameter < 3; parameter++)
            AssertEqual(Read(0x86c929 + 2 * parameter), MotherBrainDeathExplosionDefinitions.InstructionList(parameter),
                "stream 3 native death explosion variant");
        foreach (ushort invalid in new ushort[] { 3, 4, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainDeathExplosionDefinitions.InstructionList(invalid),
                "stream 3 death explosion selector bounds");
        // Confirm the retained visual payload's catalog move and the independent group arithmetic.
        for (int index = 0; index < 28; index++)
            AssertEqual(((short)Read(0xa9b099 + 4 * index), (short)Read(0xa9b09b + 4 * index)),
                MotherBrainDeathExplosionDefinitions.Anchor(index), "stream 3 native decorative death anchor");
        var generate = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("GenerateDeathExplosions",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        foreach (bool mixed in new[] { false, true })
        {
            var scatter = new MotherBrainRainbowBeamAttackSequence();
            scatter.Body.XPosition = 320;
            scatter.Body.YPosition = 192;
            for (int burst = 0; burst < 8; burst++)
            {
                typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("DeathExplosionIntervalTimer")!.SetValue(scatter, (ushort)0);
                int calls = 0;
                Func<ushort> random = () => { calls++; return ushort.MaxValue; };
                var requests = new List<MotherBrainDeathExplosionRequest>();
                generate.Invoke(scatter, [mixed, random, requests]);
                int group = 6 - burst % 7;
                AssertEqual(mixed ? 4 : 2, requests.Count, "stream 3 death scatter burst size");
                AssertEqual(requests.Count, calls, "stream 3 one RNG call per death projectile");
                for (int item = 0; item < requests.Count; item++)
                {
                    short x = (short)Read(0xa9b099 + 16 * group + 4 * item);
                    short y = (short)Read(0xa9b09b + 16 * group + 4 * item);
                    AssertEqual((ushort)group, scatter.DeathAndEscapeExplosionIndex, "stream 3 death scatter reverse group order");
                    AssertEqual((x, y), (requests[item].XOffset, requests[item].YOffset), "stream 3 selected death scatter anchors");
                    AssertEqual((ushort)(320 + x), unchecked((ushort)(scatter.Body.XPosition + requests[item].XOffset)), "stream 3 death scatter body-relative X");
                    AssertEqual((ushort)(192 + y), unchecked((ushort)(scatter.Body.YPosition + requests[item].YOffset)), "stream 3 death scatter body-relative Y");
                    AssertEqual(mixed ? (ushort)2 : (ushort)1, requests[item].ProjectileParameter, "stream 3 independent death type case");
                }
            }
        }
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("Phase")!.SetValue(sequence,
            MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape);
        var bus = new TestAddressSpace();
        var samus = new SamusState { Health = 99 };
        var firstPage = sequence.Step(bus, samus, 0, 0);
        AssertEqual(0, firstPage.EscapePaletteFxRequests.Count, "stream 3 escape palette effects wait for door graphics");
        var handoff = sequence.Step(bus, samus, 0, 0);
        AssertEqual(4, handoff.EscapePaletteFxRequests.Count, "stream 3 four escape palette registrations");
        for (int effect = 0; effect < 4; effect++)
            AssertEqual(Read(0xa9b296 + 7 * effect), handoff.EscapePaletteFxRequests[effect],
                "stream 3 native escape palette registration order");
        var text = sequence.Step(bus, samus, 0, 0);
        AssertEqual(0, text.EscapePaletteFxRequests.Count, "stream 3 escape palette registrations occur once");
    }

    private static void VerifyStream3PainfulWalking(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int stage = 0; stage < 8; stage++)
        {
            AssertEqual(Read(0xa9beee + 2 * stage), MotherBrainPainfulWalkingDefinitions.AnimationDelay(stage), "stream 3 native stagger animation delay");
            AssertEqual(Read(0xa9befe + 2 * stage), MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(stage), "stream 3 native stagger neck angle delta");
            AssertEqual(Read(0xa9c049 + 2 * stage), MotherBrainPainfulWalkingDefinitions.FunctionTimer(stage), "stream 3 native stagger pause timer");
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.AnimationDelay(invalid), "stream 3 stagger delay bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.NeckAngleDelta(invalid), "stream 3 stagger neck bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainPainfulWalkingDefinitions.FunctionTimer(invalid), "stream 3 stagger timer bounds");
        }
        var step = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("StepPainfulWalking",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        foreach (ushort stage in new ushort[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, ushort.MaxValue })
        {
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            sequence.Body.XPosition = 0;
            typeof(MotherBrainRainbowBeamAttackSequence).GetProperty("PainfulWalkingStage")!.SetValue(sequence, stage);
            step.Invoke(sequence, null);
            AssertEqual(Read(0xa9c049 + 2 * Math.Min(stage, (ushort)7)), sequence.PainfulWalkingFunctionTimer,
                "stream 3 real stagger timer clamps terminal stages");
        }
    }

    private static void VerifyStream3EscapeGeometry(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var sequence = new MotherBrainRainbowBeamAttackSequence();
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var timer = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("CreateNextEscapeTimerTileTransfer", flags)!;
        var door = typeof(MotherBrainRainbowBeamAttackSequence).GetMethod("CreateNextExplodedDoorTileTransfer", flags)!;
        foreach (bool timerList in new[] { true, false })
        {
            int count = timerList ? 7 : 2;
            int start = timerList ? 0xa6c4cb : 0xa9902f;
            var method = timerList ? timer : door;
            for (int index = 0; index < count; index++)
            {
                int address = start + index * 7;
                var expected = new MotherBrainSpriteTileTransferRequest((ushort)index, Read(address),
                    (uint)(Read(address + 2) | rom.ReadByte(address + 4) << 16), Read(address + 5));
                var calculated = timerList ? MotherBrainEscapeTextArtworkDefinitions.Transfer(index)
                    : MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(index);
                AssertEqual(expected, calculated, "stream 3 native escape graphics record");
                AssertEqual(expected, (MotherBrainSpriteTileTransferRequest)method.Invoke(sequence, null)!,
                    "stream 3 production escape transfer selection");
                AssertEqual((ushort)(index + 1), timerList ? sequence.EscapeTimerTileTransferIndex : sequence.ExplodedDoorTileTransferIndex,
                    "stream 3 production escape cursor advances once");
            }
            try
            {
                method.Invoke(sequence, null);
                throw new InvalidDataException("Completed escape transfer list unexpectedly produced another entry.");
            }
            catch (System.Reflection.TargetInvocationException error) when (error.InnerException is InvalidOperationException) { }
        }
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageSource(invalid), "stream 3 text page source bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageDestination(invalid), "stream 3 text page destination bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.PageByteCount(invalid), "stream 3 text page size bounds");
        }
        foreach (int invalid in new[] { -1, 7, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainEscapeTextArtworkDefinitions.Transfer(invalid), "stream 3 escape transfer bounds");
        foreach (int invalid in new[] { -1, 2, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainSpecialSpriteArtworkDefinitions.ExplodedDoor.Transfer(invalid), "stream 3 door transfer bounds");
        using var temporary = new TestTempDirectory("map-catalog");
        SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Extract(rom, temporary.Root, SuperMetroid.AssetExtraction.SupportedCartridge.Sha256);
        Suite(nameof(VerifyInstalledMotherBrainEscapeTextArtwork), () => VerifyInstalledMotherBrainEscapeTextArtwork(temporary.Root,
            SuperMetroid.AssetExtraction.EnemyTileArtworkFiles.Load(temporary.Root, null)));
    }

    private static void VerifyStream3CorpseGeometry(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int row = 0; row < 8; row++)
            AssertEqual((int)Read(0xa9e262 + row * 2), MotherBrainCorpseArtworkDefinitions.TileRowOffset(row),
                "stream 3 corpse native tile row offset");
        var transfers = MotherBrainCorpseArtworkDefinitions.RotTransfers;
        AssertEqual(6, transfers.Count, "stream 3 corpse transfer count");
        var enumerated = transfers.ToArray();
        for (int row = 0; row < 6; row++)
        {
            int entry = 0xa9e1f4 + row * 8;
            var expected = new MotherBrainSpriteTileTransferRequest((ushort)row, Read(entry),
                (uint)(Read(entry + 4) | (Read(entry + 2) >> 8) << 16), Read(entry + 6));
            AssertEqual(expected, transfers[row], "stream 3 native corpse rot transfer");
            AssertEqual(expected, enumerated[row], "stream 3 corpse transfer enumeration order");
            int pageEntry = 0xa99003 + row * 7;
            uint source = (uint)(Read(pageEntry + 2) | rom.ReadByte(pageEntry + 4) << 16);
            AssertEqual(source, MotherBrainCorpseArtworkDefinitions.VramPageSource(row), "stream 3 corpse source page");
            AssertEqual(Read(pageEntry + 5), MotherBrainCorpseArtworkDefinitions.VramPageDestination(row), "stream 3 corpse destination page");
        }
        int[] minimumY = [16, 8, 0, 0, 0, 8, 32]; // CMP/BCC gates in native $A9:EA40/$EB0B.
        for (int column = 0; column < 7; column++)
            AssertEqual(minimumY[column], MotherBrainCorpseArtworkDefinitions.ColumnMinimumY(column), "stream 3 corpse native outline gate");
        var copy = typeof(MotherBrainCorpseRottingState).GetMethod("CopyOrMovePixelRow",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!;
        // Confirm the changed row/column geometry through the real pixel-copy path.
        for (ushort y = 0; y < 48; y++)
        foreach (bool move in new[] { false, true })
        {
            var actual = new TestAddressSpace();
            var expected = new TestAddressSpace();
            for (int offset = 0; offset < 0x600; offset++)
            {
                byte value = (byte)(offset * 37 + 11);
                actual.WriteByte(0x7e9000 + offset, value);
                expected.WriteByte(0x7e9000 + offset, value);
            }
            int source = Read(0xa9e262 + (y >> 3) * 2) + (y & 7) * 2;
            int destination = source + ((y & 7) < 6 ? 0 : 0xd4);
            for (int column = 0; column < 7; column++)
            {
                if (y < minimumY[column]) continue;
                int start = 0x7e9000 + column * 32;
                if (y < 46)
                {
                    // Read both plane words before copying or clearing either source.
                    byte lo = expected.ReadByte(start + source);
                    byte hi = expected.ReadByte(start + source + 1);
                    byte lo2 = expected.ReadByte(start + source + 16);
                    byte hi2 = expected.ReadByte(start + source + 17);
                    expected.WriteByte(start + destination + 2, lo);
                    expected.WriteByte(start + destination + 3, hi);
                    expected.WriteByte(start + destination + 18, lo2);
                    expected.WriteByte(start + destination + 19, hi2);
                }
                if (move)
                {
                    expected.WriteByte(start + source, 0);
                    expected.WriteByte(start + source + 1, 0);
                    expected.WriteByte(start + source + 16, 0);
                    expected.WriteByte(start + source + 17, 0);
                }
            }
            copy.Invoke(null, [actual, actual, y, move]);
            for (int offset = 0; offset < 0x600; offset++)
                AssertEqual(expected.ReadByte(0x7e9000 + offset), actual.ReadByte(0x7e9000 + offset), "stream 3 corpse row geometry");
        }
        foreach (int invalid in new[] { -1, 6, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainCorpseArtworkDefinitions.VramPageSource(invalid), "stream 3 corpse source bounds");
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainCorpseArtworkDefinitions.VramPageDestination(invalid), "stream 3 corpse destination bounds");
            AssertThrows<IndexOutOfRangeException>(() => _ = transfers[invalid], "stream 3 corpse transfer bounds");
        }
        Suite(nameof(VerifyMotherBrainCorpseStockArtwork), () => VerifyMotherBrainCorpseStockArtwork());
    }

    private static void VerifyStream3ShitroidPulse(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        PaletteRgb5 Rgb(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        ushort[] Words(int address, int count) => Enumerable.Range(0, count).Select(i => Read(address + 2 * i)).ToArray();
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
        var targetFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object selectedSidehopper = typeof(ShitroidColorCatalog).GetField("sidehopper", targetFlags)!.GetValue(stock)!;
        AssertEqual(0, ((ushort[])selectedSidehopper.GetType().GetField("independentOrSupplied", targetFlags)!.GetValue(selectedSidehopper)!).Length,
            "stream 3 complete standalone Sidehopper target keeps no stock color array");
        object selectedCorpse = typeof(ShitroidColorCatalog).GetField("deadSidehopper", targetFlags)!.GetValue(stock)!;
        AssertEqual(0, ((ushort[])selectedCorpse.GetType().GetField("independentOrSupplied", targetFlags)!.GetValue(selectedCorpse)!).Length,
            "stream 3 complete standalone corpse target keeps no stock color array");
        var pulse = typeof(ShitroidColorCatalog).GetField("normal",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        AssertTrue(pulse.GetType().GetField("supplied", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(pulse) is null, "stream 3 Shitroid original pulse rows discarded");
        AssertTrue(!pulse.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
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
            AssertEqual(Read(0xa994d4 + color * 2), stock.TargetColor(ShitroidColorTarget.Shitroid, color + 1),
                "stream 3 native Baby/Shitroid palette duplication");
        for (int color = 0; color < 16; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = shitroid[color];
            shitroid[color] ^= (ushort)(1 << (5 * channel));
            document.Shitroid[color] = Rgb(shitroid[color]);
            Check(Load());
            for (int other = 0; other < 15; other++)
                AssertEqual(Read(0xa994d4 + other * 2), baby.InitialColor(other), "stream 3 target edits retain independent Baby colors");
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

    private static void VerifyStream3DrainFades(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        PaletteRgb5 Color(ushort word) => new() { Red = word & 31, Green = word >> 5 & 31, Blue = word >> 10 & 31 };
        ushort Word(PaletteRgb5 color) => (ushort)(color.Red | color.Green << 5 | color.Blue << 10);

        MotherBrainRainbowPaletteFrameDocument ReadFull(int bodySource, int legSource) => new()
        {
            Body = Enumerable.Range(0, 15).Select(color => Color(Read(bodySource + 2 * color))).ToArray(),
            BackLegs = Enumerable.Range(0, 15).Select(color => Color(Read(legSource + 2 * color))).ToArray(),
        };
        var rainbow = Enumerable.Range(0, 10).Select(frame =>
        {
            int source = 0xad0000 | Read(0xade434 + 2 * frame);
            return ReadFull(source, source + 30);
        }).ToArray();
        var drain = new MotherBrainRainbowPaletteFrameDocument[8];
        var fake = new PaletteRgb5[8][];
        var revival = new MotherBrainRainbowPaletteFrameDocument[8];
        for (int frame = 0; frame < 8; frame++)
        {
            int source = 0xad0000 | Read(0xadef87 + 2 * frame);
            drain[frame] = new()
            {
                Body = Enumerable.Range(0, 15).Select(color => Color(Read(source + 2 * color))).ToArray(),
                BackLegs = Enumerable.Range(0, 5).Select(color => Color(Read(source + 30 + 2 * color))).ToArray(),
                TrailingColor = Color(Read(source + 40)),
            };
            int revivalSource = 0xad0000 | Read(0xaded9c + 2 * frame);
            revival[frame] = new()
            {
                Body = Enumerable.Range(0, 13).Select(color => Color(Read(revivalSource + 2 * color))).ToArray(),
                BackLegs = Enumerable.Range(0, 5).Select(color => Color(Read(revivalSource + 26 + 2 * color))).ToArray(),
                TrailingColor = Color(Read(revivalSource + 36)),
            };
            int fakeSource = 0xad0000 | Read(0xaded8a + 2 * frame);
            fake[frame] = Enumerable.Range(0, 3).Select(color => Color(Read(fakeSource + 2 * color))).ToArray();
        }
        var document = new MotherBrainRainbowPaletteDocument
        {
            Version = 3, Rainbow = rainbow,
            ToGrey = drain, FromGrey = revival,
            FakeDeathToGrey = fake, Normal = ReadFull(0xa99474, 0xa99494), BeamInitial = Color(0x3ce0),
            BeamCycle = Enumerable.Range(0, 38).Select(index => Color(Read(0x88e833 + 4 * index))).ToArray(),
        };
        MotherBrainRainbowPalettePresentation Load(MotherBrainRainbowPaletteDocument value,
            MotherBrainRainbowPalettePresentation? stock = null) => MotherBrainRainbowPalettePresentation.Load(
                new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, MapPresentationFormat.JsonOptions)), stock);
        void Check(MotherBrainRainbowPalettePresentation palette)
        {
            var cgram = new SnesCgram();
            var bus = new TestAddressSpace();
            for (int frame = 0; frame < 8; frame++)
            {
                palette.ApplyToGrey(bus, cgram, frame);
                for (int color = 0; color < 15; color++)
                {
                    AssertEqual(Word(drain[frame].Body[color]), cgram.Colors[0x41 + color], "stream 3 drain body RGB5");
                    AssertEqual(Word(drain[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 drain brain RGB5");
                }
                for (int color = 0; color < 5; color++)
                    AssertEqual(Word(drain[frame].BackLegs[color]), cgram.Colors[180 + color], "stream 3 drain legs RGB5");
                AssertEqual(Word(drain[frame].TrailingColor!), (ushort)(bus.ReadByte(0x7e017c) | bus.ReadByte(0x7e017d) << 8),
                    "stream 3 drain trailing word");
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
            AssertEqual((ushort)0x3ce0, palette.BeamInitialColor, "stream 3 beam initial fixed color unchanged");
        }
        CheckBeam(stock);
        const System.Reflection.BindingFlags beamFields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object beamColors = typeof(MotherBrainRainbowPalettePresentation).GetField("beamCycle", beamFields)!.GetValue(stock)!;
        AssertTrue(beamColors.GetType().GetField("supplied", beamFields)!.GetValue(beamColors) is null,
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
                palette.ApplyFromGrey(bus, cgram, frame);
                for (int color = 0; color < 13; color++)
                {
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x41 + color], "stream 3 revival body");
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 revival brain");
                }
                AssertEqual((ushort)123, cgram.Colors[0x4e], "stream 3 revival preserves body tail");
                AssertEqual((ushort)123, cgram.Colors[0x9e], "stream 3 revival preserves brain tail");
                for (int color = 0; color < 5; color++)
                    AssertEqual(Word(revival[frame].BackLegs[color]), cgram.Colors[180 + color], "stream 3 revival legs");
                AssertEqual(Word(revival[frame].TrailingColor!), (ushort)(bus.ReadByte(0x7e017c) | bus.ReadByte(0x7e017d) << 8), "stream 3 revival trailing word");
                palette.ApplyFakeDeathFromGrey(cgram, frame);
                for (int color = 0; color < 3; color++)
                    AssertEqual(Word(revival[frame].Body[color]), cgram.Colors[0x91 + color], "stream 3 fake-death revival");
            }
        }
        CheckRevival(stock);
        const System.Reflection.BindingFlags privateFields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object revivalFade = typeof(MotherBrainRainbowPalettePresentation).GetField("fromGrey", privateFields)!.GetValue(stock)!;
        var revivalOverrides = (Dictionary<(int Frame, int Color), ushort>)revivalFade.GetType()
            .GetField("suppliedOverrides", privateFields)!.GetValue(revivalFade)!;
        AssertEqual(0, revivalOverrides.Count, "revival calculates all words with one reviewed tissue-shade hold");
        AssertEqual(Word(revival[1].Body[10]), Word(revival[2].Body[10]),
            "native revival EE80 repeats EEA6 without an independently stored paint word");
        foreach (string endpointName in new[] { "first", "last" })
        {
            object endpoint = revivalFade.GetType().GetField(endpointName, privateFields)!.GetValue(revivalFade)!;
            object body = endpoint.GetType().GetProperty("Body")!.GetValue(endpoint)!;
            AssertTrue(body.GetType().GetField("supplied", privateFields)!.GetValue(body) is null,
                "revival endpoint body reuses reviewed health/drained paint calculations");
            AssertTrue(endpoint.GetType().GetField("backLegs", privateFields)!.GetValue(endpoint) is null,
                "revival rear endpoint calculates from reviewed normal/half-shadow paint");
            AssertTrue(endpoint.GetType().GetField("<TrailingColor>k__BackingField", privateFields)!.GetValue(endpoint) is null,
                "revival trailing endpoint shares its calculated rear highlight");
        }
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 19; color++)
        for (int component = 0; component < 3; component++)
        {
            var row = revival[frame];
            PaletteRgb5 original = color < 13 ? row.Body[color] : color < 18 ? row.BackLegs[color - 13] : row.TrailingColor!;
            void Set(PaletteRgb5 value)
            {
                if (color < 13) row.Body[color] = value;
                else if (color < 18) row.BackLegs[color - 13] = value;
                else revival[frame] = row with { TrailingColor = value };
            }
            Set(Color((ushort)(Word(original) ^ 1 << (5 * component))));
            CheckRevival(Load(document));
            Set(original);
        }
        CheckRainbow(stock);
        object fakeFade = typeof(MotherBrainRainbowPalettePresentation).GetField("fakeDeathToGrey", privateFields)!.GetValue(stock)!;
        foreach (string endpointName in new[] { "first", "last" })
        {
            object endpoint = fakeFade.GetType().GetField(endpointName, privateFields)!.GetValue(fakeFade)!;
            object body = endpoint.GetType().GetProperty("Body")!.GetValue(endpoint)!;
            AssertTrue(body.GetType().GetField("supplied", privateFields)!.GetValue(body) is null,
                "stream 3 fake-death endpoints reuse approved normal/drained cortex paint without stored rows");
        }
        object drainFade = typeof(MotherBrainRainbowPalettePresentation).GetField("toGrey", privateFields)!.GetValue(stock)!;
        object drainEnd = drainFade.GetType().GetField("last", privateFields)!.GetValue(drainFade)!;
        object drainBody = drainEnd.GetType().GetProperty("Body")!.GetValue(drainEnd)!;
        var drainedPaint = (MotherBrainRainbowPalettePresentation.DrainedBodyColors)drainBody.GetType()
            .GetField("drained", privateFields)!.GetValue(drainBody)!;
        AssertTrue(drainedPaint.Calculated, "stream 3 drained final gray body palette has no stored row");
        object normalFrame = typeof(MotherBrainRainbowPalettePresentation).GetField("normal", privateFields)!.GetValue(stock)!;
        object normalBody = normalFrame.GetType().GetProperty("Body")!.GetValue(normalFrame)!;
        AssertTrue(normalBody.GetType().GetField("supplied", privateFields)!.GetValue(normalBody) is null,
            "stream 3 normal body calculates from shared health paint without a stored row");
        AssertTrue(normalFrame.GetType().GetField("backLegs", privateFields)!.GetValue(normalFrame) is null,
            "stream 3 normal rear palette calculates from shared health lighting without a stored row");
        var storedRainbow = (Array)typeof(MotherBrainRainbowPalettePresentation).GetField("rainbow",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
        object RainbowBody(int phase)
        {
            object selected = storedRainbow.GetValue(phase)!;
            return selected.GetType().GetProperty("Body")!.GetValue(selected)!;
        }
        object redBody = RainbowBody(0);
        object redBasis = redBody.GetType().GetField("redOrigin", privateFields)!.GetValue(redBody)!;
        AssertTrue(redBasis is not null, "native rainbow actually selects calculated red-origin shades");
        AssertEqual(11, ((ushort[])redBasis!.GetType().GetField("inputs", privateFields)!.GetValue(redBasis)!).Length,
            "red-origin basis retains eleven packed independent words");
        int independentChannels = 0;
        foreach (int phase in new[] { 1, 2, 5, 6, 7 })
        {
            object body = RainbowBody(phase);
            object sharing = body.GetType().GetField("sharedChannels", privateFields)!.GetValue(body)!;
            AssertTrue(sharing is not null, $"rainbow phase {phase} actually uses shared channels");
            object channel = sharing!.GetType().GetField("independent", privateFields)!.GetValue(sharing)!;
            independentChannels += ((Array)channel.GetType().GetField("inputs", privateFields)!.GetValue(channel)!).Length;
        }
        AssertEqual(28, independentChannels, "remaining independently supplied channels outside red-origin basis");
        foreach (int phase in new[] { 3, 4, 8, 9 })
        {
            object body = RainbowBody(phase);
            AssertTrue(body.GetType().GetField("tintSource", privateFields)!.GetValue(body) is not null,
                $"rainbow phase {phase} actually uses exact whole-palette tint");
        }
        object drainStart = drainFade.GetType().GetField("first", privateFields)!.GetValue(drainFade)!;
        AssertTrue(ReferenceEquals(drainStart.GetType().GetProperty("Body")!.GetValue(drainStart), RainbowBody(6)),
            "stream 3 drain body starts from the exact matching rainbow phase-six source");
        AssertTrue(ReferenceEquals(drainStart.GetType().GetField("rearSource", privateFields)!.GetValue(drainStart), storedRainbow.GetValue(6)),
            "stream 3 drain rear and trailing start share only matching rainbow content");
        AssertTrue(drainEnd.GetType().GetField("backLegs", privateFields)!.GetValue(drainEnd) is null,
            "stream 3 drained rear endpoint calculates from approved normal rear lighting");
        AssertTrue(drainEnd.GetType().GetField("<TrailingColor>k__BackingField", privateFields)!.GetValue(drainEnd) is null,
            "stream 3 drained trailing word reuses its rear highlight without a stored value");
        foreach (object frame in storedRainbow)
            AssertTrue(frame.GetType().GetField("backLegs", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(frame) is null, "stream 3 rainbow shadow tables discarded");
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
        foreach (string field in new[] { "toGrey", "fakeDeathToGrey" })
        {
            var fade = typeof(MotherBrainRainbowPalettePresentation).GetField(field,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock)!;
            AssertTrue(fade.GetType().GetField("supplied", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .GetValue(fade) is null, "stream 3 drain stock intermediate rows discarded");
        }
        Check(Load(document with { Version = 2, FakeDeathToGrey = null }, stock));
        // Independently edited channels must still be delivered exactly, including endpoints and WRAM.
        for (int frame = 0; frame < 8; frame++)
        for (int color = 0; color < 24; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            var row = drain[frame];
            PaletteRgb5 original = color < 15 ? row.Body[color] : color < 20 ? row.BackLegs[color - 15]
                : color == 20 ? row.TrailingColor! : fake[frame][color - 21];
            void Set(PaletteRgb5 value)
            {
                if (color < 15) row.Body[color] = value;
                else if (color < 20) row.BackLegs[color - 15] = value;
                else if (color == 20) drain[frame] = row with { TrailingColor = value };
                else fake[frame][color - 21] = value;
            }
            Set(Color((ushort)(Word(original) ^ 1 << (5 * channel))));
            var editedPalette = Load(document);
            Check(editedPalette);
            if (frame == 0 && color < 21) CheckRainbow(editedPalette);
            Set(original);
        }
        foreach (int invalid in new[] { -1, 8, int.MaxValue })
        {
            AssertThrows<InvalidDataException>(() => stock.ApplyToGrey(new TestAddressSpace(), new SnesCgram(), invalid), "stream 3 drain bounds");
            AssertThrows<InvalidDataException>(() => stock.ApplyFakeDeathToGrey(new SnesCgram(), invalid), "stream 3 fake-death bounds");
        }
    }

    private static void VerifyStream3BabyFade(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var initial = new ushort[15];
        var fade = new ushort[6][];
        for (int color = 0; color < initial.Length; color++)
            initial[color] = Read(0xa994d4 + 2 * color);
        for (int frame = 0; frame < fade.Length; frame++)
        {
            fade[frame] = new ushort[14];
            for (int color = 0; color < fade[frame].Length; color++)
                fade[frame][color] = Read(0xade90c + 28 * frame + 2 * color);
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
        object selectedInitial = typeof(BabyMetroidCutsceneColorCatalog).GetField("initial", fields)!.GetValue(stock)!;
        AssertTrue(selectedInitial.GetType().GetField("supplied", fields)!.GetValue(selectedInitial) is null,
            "stream 3 Baby initial calculates from reviewed paints without a stock color array");
        for (int color = 0; color < initial.Length; color++)
        for (int channel = 0; channel < 3; channel++)
        {
            ushort original = initial[color];
            initial[color] ^= (ushort)(1 << (5 * channel));
            Check(Load());
            initial[color] = original;
        }
        object selectedFade = typeof(BabyMetroidCutsceneColorCatalog).GetField("fade", fields)!.GetValue(stock)!;
        AssertTrue(selectedFade.GetType().GetField("supplied", fields)!.GetValue(selectedFade) is null,
            "stream 3 original Baby fade discards its stored frame table");
        AssertEqual(0, selectedFade.GetType().GetFields(fields).Count(field => field.FieldType == typeof(ushort) || field.FieldType == typeof(ushort[]) || field.FieldType == typeof(Bgr555) || field.FieldType == typeof(Bgr555[])), "stream 3 Baby fade keeps no stock endpoint payload");
        static ushort ScaleHealth(ushort value, int remaining)
        {
            int result = 0;
            for (int channel = 0; channel < 3; channel++)
                result |= (((value >> (5 * channel) & 31) * remaining / 7) << (5 * channel));
            return (ushort)result;
        }
        var endpointMethod = selectedFade.GetType().GetMethod("Endpoint", fields | System.Reflection.BindingFlags.Static)!;
        for (int color = 0; color < 14; color++)
        {
            AssertEqual(Read(0xade8f0 + 2 * color), ScaleHealth(((Bgr555)endpointMethod.Invoke(null, [color])!).ToWord(), 6),
                "stream 3 reviewed final-health endpoint reproduces original undisplayed fade step");
            int healthAddress = color < 4 ? 0xade870 + 2 * color :
                color < 9 ? 0xade8d8 + 2 * (color - 4) : 0xade878 + 2 * (color - 9);
            ushort nativeHealth = Read(healthAddress);
            AssertEqual(nativeHealth, (Bgr555)endpointMethod.Invoke(null, [color])!, "stream 3 exact native final-health endpoint");
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

    private static void VerifyStream3MotherBrainFades(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var body = new ushort[16][];
        var leg = new ushort[16][];
        var corpse = new ushort[8][];
        for (int frame = 0; frame < 16; frame++)
        {
            body[frame] = new ushort[14];
            leg[frame] = new ushort[14];
            for (int color = 0; color < 14; color++)
            {
                body[frame][color] = Read(0xadea0a + 56 * frame + 2 * color);
                leg[frame][color] = Read(0xadea26 + 56 * frame + 2 * color);
            }
        }
        for (int frame = 0; frame < 8; frame++)
        {
            corpse[frame] = new ushort[15];
            for (int color = 0; color < 15; color++)
                corpse[frame][color] = Read(0xadf119 + 30 * frame + 2 * color);
        }
        var door = new ushort[14];
        for (int color = 0; color < door.Length; color++)
            door[color] = Read(0xa99534 + 2 * color);

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
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertTrue(typeof(MotherBrainDeathColorCatalog).GetField("explodedDoor", fields)!.GetValue(stock) is null,
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
        foreach (string name in new[] { "bodyFade", "legFade", "corpseFade" })
        {
            object fade = typeof(MotherBrainDeathColorCatalog).GetField(name, fields)!.GetValue(stock)!;
            AssertTrue(fade.GetType().GetField("first", fields)!.GetValue(fade) is null,
                "stream 3 death starts reuse calculated health state three without stored endpoint rows");
            AssertTrue(fade.GetType().GetField("supplied", fields)!.GetValue(fade) is null,
                "stream 3 original death fade discards its stored frame table");
            if (name == "corpseFade")
            {
                var endpoint = (MotherBrainRainbowPalettePresentation.DrainedBodyColors)fade.GetType()
                    .GetField("last", fields)!.GetValue(fade)!;
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

    private static void VerifyStream3UniformEnemyLoops(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyMochtroidInstructionProgramDefinitions), () => VerifyMochtroidInstructionProgramDefinitions());
        Suite(nameof(VerifyYellowPipeBugInstructionProgramDefinitions), () => VerifyYellowPipeBugInstructionProgramDefinitions());
        Check(0xa30000, [0xa745, 0xa759],
            index =>
            {
                var word = MochtroidInstructionProgramDefinitionsTooling.MechanicsWord(index);
                return (word.Address, word.Value);
            }, MochtroidInstructionProgramDefinitionsTooling.PresentationWordAddress,
            MochtroidInstructionProgramDefinitions.ReadMechanicsWord,
            MochtroidInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte);
        Check(0xb30000, [0x8efc, 0x8f10, 0x8f24, 0x8f38],
            index =>
            {
                var word = YellowPipeBugInstructionProgramDefinitionsTooling.MechanicsWord(index);
                return (word.Address, word.Value);
            }, YellowPipeBugInstructionProgramDefinitionsTooling.PresentationWordAddress,
            YellowPipeBugInstructionProgramDefinitions.ReadMechanicsWord,
            YellowPipeBugInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte);

        void Check(int bank, ushort[] starts,
            Func<int, (ushort Address, ushort Value)> wordAt, Func<int, ushort> visualAt,
            Func<ushort, ushort> read, Func<int, bool> ownsByte)
        {
            var bytes = new HashSet<int>();
            int wordIndex = 0, visualIndex = 0;
            foreach (ushort start in starts)
            {
                foreach (int offset in new[] { 0, 4, 8, 12, 16, 18 })
                {
                    ushort address = (ushort)(start + offset);
                    ushort value = (ushort)(rom.ReadByte(bank | address) | rom.ReadByte(bank | (address + 1)) << 8);
                    AssertEqual((address, value), wordAt(wordIndex++), "stream 3 uniform loop native mechanic");
                    AssertEqual(value, read(address), "stream 3 uniform loop mechanic dispatch");
                    bytes.Add(address);
                    bytes.Add(address + 1);
                }
                for (int frame = 0; frame < 4; frame++)
                {
                    ushort address = (ushort)(start + 4 * frame + 2);
                    AssertEqual(address, visualAt(visualIndex++), "stream 3 uniform loop visual order");
                    AssertThrows<InvalidDataException>(() => read(address), "stream 3 uniform loop visual excluded");
                }
            }
            for (int address = 0; address <= ushort.MaxValue; address++)
                AssertEqual(bytes.Contains(address), ownsByte(bank | address), "stream 3 uniform loop byte ownership");
            foreach (int invalid in new[] { -1, starts.Length * 6, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => wordAt(invalid), "stream 3 uniform loop mechanic bounds");
            foreach (int invalid in new[] { -1, starts.Length * 4, int.MaxValue })
                AssertThrows<IndexOutOfRangeException>(() => visualAt(invalid), "stream 3 uniform loop visual bounds");
        }
    }

    private static void VerifyStream3RipperMappings(ISnesAddressSpace rom)
    {
        Suite(nameof(VerifyRipperInstructionProgramDefinitions), () => VerifyRipperInstructionProgramDefinitions());
        ushort[] programs = [0xe19b, 0xe1af, 0xe2e0, 0xe2f4, 0xe477, 0xe48b];
        var bytes = new HashSet<int>();
        int wordIndex = 0, visualIndex = 0;
        foreach (ushort start in programs)
        {
            foreach (int offset in new[] { 0, 4, 8, 12, 16, 18 })
            {
                ushort address = (ushort)(start + offset);
                ushort value = (ushort)(rom.ReadByte(0xa20000 | address) | rom.ReadByte(0xa20000 | (address + 1)) << 8);
                AssertEqual(new InstructionMechanicsWord(address, value), RipperInstructionProgramDefinitionsTooling.MechanicsWord(wordIndex++),
                    "stream 3 Ripper mechanic order and value");
                bytes.Add(address);
                bytes.Add(address + 1);
            }
            for (int frame = 0; frame < 4; frame++)
            {
                ushort address = (ushort)(start + 4 * frame + 2);
                AssertEqual(address, RipperInstructionProgramDefinitionsTooling.PresentationWordAddress(visualIndex++),
                    "stream 3 Ripper visual operand order");
                if (start < 0xe477)
                {
                    ushort expected = (ushort)(rom.ReadByte(0xa20000 | address) | rom.ReadByte(0xa20000 | (address + 1)) << 8);
                    AssertEqual(expected, RipperVisualDefinitions.FrameAt(EnemyDefinitionId.GRipper, address),
                        "stream 3 GRipper preserves shared operand domain");
                    AssertEqual(expected, RipperVisualDefinitions.FrameAt(EnemyDefinitionId.Ripper2, address),
                        "stream 3 Ripper II preserves shared operand domain");
                }
            }
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(bytes.Contains(address), RipperInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa20000 | address),
                "stream 3 Ripper byte ownership domain");
        foreach (int invalid in new[] { -1, 36, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RipperInstructionProgramDefinitionsTooling.MechanicsWord(invalid),
                "stream 3 Ripper mechanic bounds");
        foreach (int invalid in new[] { -1, 24, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => RipperInstructionProgramDefinitionsTooling.PresentationWordAddress(invalid),
                "stream 3 Ripper visual bounds");
        foreach (ushort invalid in new ushort[] { 0xe19b, 0xe1ad, 0xe1bf, 0xe477, 0xffff })
            AssertThrows<InvalidDataException>(() => RipperVisualDefinitions.FrameAt(EnemyDefinitionId.GRipper, invalid),
                "stream 3 Ripper rejects nonvisual and foreign-family operands");
        AssertThrows<InvalidDataException>(() => RipperVisualDefinitions.FrameAt(0, 0xe19d),
            "stream 3 Ripper rejects foreign enemy");
    }

    private static void VerifyStream3ChootControl(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (ushort pattern = 0; pattern < 5; pattern++)
        {
            ushort pointer = Read(0xa2df5e + 2 * pattern);
            ushort distancePointer = Read(0xa2df6a + 2 * pattern);
            AssertEqual(new ChootPatternDefinition(pointer, Read(0xa20000 | distancePointer)),
                ChootPatternDefinitions.ForIndex(pattern), "stream 3 Choot pattern identity and loop advance");
        }
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => ChootPatternDefinitions.ForIndex(invalid),
                "stream 3 Choot rejects alias and invalid patterns");
        ushort[] mechanics = [0xd82c, 0xd82e, 0xd832, 0xd834, 0xd836, 0xd83a, 0xd83e, 0xd840, 0xd842, 0xd846, 0xd84a];
        ushort[] presentation = [0xd830, 0xd838, 0xd83c, 0xd844, 0xd848];
        AssertEqual(mechanics.Length, ChootInstructionProgramDefinitionsTooling.MechanicsWordCount, "stream 3 Choot mechanics count");
        AssertEqual(presentation.Length, ChootInstructionProgramDefinitionsTooling.PresentationWordCount, "stream 3 Choot visual count");
        for (int index = 0; index < mechanics.Length; index++)
        {
            ushort address = mechanics[index];
            ushort value = Read(0xa20000 | address);
            AssertEqual(new InstructionMechanicsWord(address, value), ChootInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "stream 3 Choot native control instruction");
            AssertEqual(value, ChootInstructionProgramDefinitions.ReadMechanicsWord(address), "stream 3 Choot control dispatch");
        }
        for (int index = 0; index < presentation.Length; index++)
        {
            ushort address = presentation[index];
            AssertEqual(address, ChootInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "stream 3 Choot visual operand");
            AssertThrows<InvalidDataException>(() => ChootInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 Choot visual operand remains excluded");
        }
        var bytes = mechanics.SelectMany(address => new[] { (int)address, address + 1 }).ToHashSet();
        var visualWords = presentation.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            AssertEqual(bytes.Contains(address), ChootInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa20000 | address),
                "stream 3 Choot byte ownership");
            AssertEqual(visualWords.Contains((ushort)address), ChootInstructionProgramDefinitions.IsPresentationWord((ushort)address),
                "stream 3 Choot presentation ownership");
        }
        foreach (int invalid in new[] { -1, 11, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ChootInstructionProgramDefinitionsTooling.MechanicsWord(invalid),
                "stream 3 Choot mechanics bounds");
        foreach (int invalid in new[] { -1, 5, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => ChootInstructionProgramDefinitionsTooling.PresentationWordAddress(invalid),
                "stream 3 Choot visual bounds");
    }

    private static void VerifyStream3PickupAndFirefleaPrograms(ISnesAddressSpace rom)
    {
        ushort Read(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        for (int kind = 0; kind < 6; kind++)
            AssertEqual(new EnemyPickupAnimationDefinition((ushort)(2 * kind), Read(0x86ef04 + 2 * kind)),
                EnemyPickupDefinitions.Animation((EnemyPickupKind)kind), "stream 3 pickup kind dispatch");
        for (ushort animation = 0; animation < 5; animation++)
            AssertEqual(Read(0x86efd5 + 2 * animation), EnemyDeathExplosionDefinitions.InstructionPointer(animation),
                "stream 3 death variant dispatch");
        foreach (ushort invalid in new ushort[] { 6, 7, ushort.MaxValue })
            AssertThrows<InvalidDataException>(() => EnemyPickupDefinitions.Animation((EnemyPickupKind)invalid),
                "stream 3 invalid pickup kind");
        foreach (ushort invalid in new ushort[] { 5, 6, ushort.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EnemyDeathExplosionDefinitions.InstructionPointer(invalid),
                "stream 3 invalid death variant");

        ushort[] pickupMechanics =
        [
            0xed8d, 0xed91, 0xed95, 0xed99, 0xed9d, 0xed9f, 0xeda1,
            0xeda3, 0xeda7, 0xedab, 0xedaf, 0xedb3, 0xedb5, 0xedb7,
            0xedb9, 0xedbd, 0xedc1, 0xedc3, 0xedc5,
            0xeddd, 0xede1, 0xede5, 0xede7, 0xede9,
            0xedeb, 0xedef, 0xedf3, 0xedf7, 0xedfb, 0xedfd,
        ];
        ushort[] pickupPresentation =
        [
            0xed8f, 0xed93, 0xed97, 0xed9b, 0xeda5, 0xeda9, 0xedad, 0xedb1,
            0xedbb, 0xedbf, 0xeddf, 0xede3, 0xeded, 0xedf1, 0xedf5, 0xedf9,
        ];
        AssertEqual(pickupMechanics.Length, EnemyPickupInstructionProgramDefinitionsTooling.MechanicsWordCount,
            "stream 3 pickup mechanic count");
        AssertEqual(pickupPresentation.Length, EnemyPickupInstructionProgramDefinitions.PresentationWordCount,
            "stream 3 pickup visual operand count");
        for (int index = 0; index < pickupMechanics.Length; index++)
        {
            ushort address = pickupMechanics[index];
            AssertEqual(new InstructionMechanicsWord(address, Read(0x860000 | address)),
                EnemyPickupInstructionProgramDefinitionsTooling.MechanicsWord(index), "stream 3 native pickup mechanic");
            AssertEqual(Read(0x860000 | address), EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 pickup mechanic dispatch");
        }
        for (int index = 0; index < pickupPresentation.Length; index++)
        {
            ushort address = pickupPresentation[index];
            AssertEqual(address, EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(index),
                "stream 3 pickup visual operand address");
            AssertThrows<InvalidDataException>(() => EnemyPickupInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 pickup visual operand remains excluded");
        }
        var mechanicsSet = pickupMechanics.ToHashSet();
        var byteSet = pickupMechanics.SelectMany(address => new[] { (int)address, address + 1 }).ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool owned = mechanicsSet.Contains((ushort)address);
            AssertEqual(owned, EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.EnemyDeathPickup, (ushort)address),
                "stream 3 pickup ownership domain");
            AssertEqual(owned, EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.EnemyDeathExplosion, (ushort)address),
                "stream 3 explosion pickup ownership domain");
            AssertEqual(byteSet.Contains(address), EnemyPickupInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x860000 | address),
                "stream 3 pickup byte ownership domain");
        }
        AssertTrue(!EnemyPickupInstructionProgramDefinitions.Owns(RoomEnemyProjectileKind.ShaktoolAttackFrontCircle, pickupMechanics[0]),
            "stream 3 unrelated actor does not own pickup instructions");
        AssertTrue(!EnemyPickupInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa3ed8d),
            "stream 3 pickup excludes other bank");

        AssertEqual(54, FirefleaInstructionProgramDefinitionsTooling.MechanicsWordCount, "stream 3 Fireflea mechanic count");
        AssertEqual(52, FirefleaInstructionProgramDefinitionsTooling.PresentationWordCount, "stream 3 Fireflea visual count");
        var fireBytes = new HashSet<int>();
        for (int index = 0; index < 54; index++)
        {
            ushort address = (ushort)(index < 52 ? 0x8c2f + index * 4 : 0x8cff + (index - 52) * 2);
            ushort value = Read(0xa30000 | address);
            AssertEqual(new InstructionMechanicsWord(address, value), FirefleaInstructionProgramDefinitionsTooling.MechanicsWord(index),
                "stream 3 native Fireflea mechanic");
            AssertEqual(value, FirefleaInstructionProgramDefinitions.ReadMechanicsWord(address),
                "stream 3 Fireflea mechanic dispatch");
            fireBytes.Add(address);
            fireBytes.Add(address + 1);
            if (index < 52)
            {
                ushort visual = (ushort)(address + 2);
                AssertEqual(visual, FirefleaInstructionProgramDefinitionsTooling.PresentationWordAddress(index),
                    "stream 3 Fireflea visual operand");
                AssertThrows<InvalidDataException>(() => FirefleaInstructionProgramDefinitions.ReadMechanicsWord(visual),
                    "stream 3 Fireflea visual remains excluded");
            }
        }
        for (int address = 0; address <= ushort.MaxValue; address++)
            AssertEqual(fireBytes.Contains(address), FirefleaInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0xa30000 | address),
                "stream 3 Fireflea byte ownership domain");
        foreach (int invalid in new[] { -1, 30, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EnemyPickupInstructionProgramDefinitionsTooling.MechanicsWord(invalid),
                "stream 3 pickup mechanic bounds");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => EnemyPickupInstructionProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 pickup visual bounds");
        foreach (int invalid in new[] { -1, 54, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FirefleaInstructionProgramDefinitionsTooling.MechanicsWord(invalid),
                "stream 3 Fireflea mechanic bounds");
        foreach (int invalid in new[] { -1, 52, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => FirefleaInstructionProgramDefinitionsTooling.PresentationWordAddress(invalid),
                "stream 3 Fireflea visual bounds");
    }

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
                ushort value = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
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
        AssertTrue(typeof(WorkRobotPaletteCycle).GetField("frames",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stock) is null,
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
    private static void VerifyStream3HealthTint(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MotherBrainHealthPaletteExtractor.Extract(rom);
        var stock = MotherBrainHealthPalettePresentation.Load(new MemoryStream(json));
        var node = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (string name in new[] { "body", "backLegs" })
        {
            object palette = typeof(MotherBrainHealthPalettePresentation).GetField(name, flags)!.GetValue(stock)!;
            AssertTrue(palette.GetType().GetField("supplied", flags)!.GetValue(palette) is null,
                "stream 3 native health tint has no stored intermediate rows");
            object basis = palette.GetType().GetField("basis", flags)!.GetValue(palette)!;
            AssertTrue(basis.GetType().GetField("supplied", flags)!.GetValue(basis) is null,
                "stream 3 native health shade ramps calculated from paint endpoints");
            Bgr555[] anchors = basis.GetType().GetFields(flags).Where(field => field.FieldType == typeof(Bgr555))
                .Select(field => (Bgr555)field.GetValue(basis)!).ToArray();
            AssertEqual(6, anchors.Length, "stream 3 health base exposes only six possible paint anchors");
            AssertTrue(name == "body" ? anchors.All(value => value != Bgr555.Black) : anchors.All(value => value == Bgr555.Black),
                "stream 3 stock rear palette stores no independent paint colors");
        }
        for (int state = 0; state < 4; state++)
        {
            var actual = new SnesCgram();
            stock.Apply(actual, state);
            foreach (var (table, destination) in new[]
            {
                (MotherBrainHealthPaletteRomData.BrainTable, MotherBrainRainbowPaletteRomData.BodyColor),
                (MotherBrainHealthPaletteRomData.BackLegTable, MotherBrainRainbowPaletteRomData.SecondaryColor),
            })
            {
                int pointer = rom.ReadByte(table + state * 2) | rom.ReadByte(table + state * 2 + 1) << 8;
                for (int color = 0; color < 15; color++)
                {
                    int address = 0xad0000 | pointer + color * 2;
                    ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
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
                        var rgb = editedNode[group]![state]![color]!;
                        rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                        var edited = MotherBrainHealthPalettePresentation.Load(new MemoryStream(
                            System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                        for (int checkState = 0; checkState < 4; checkState++)
                        {
                            var actual = new SnesCgram();
                            edited.Apply(actual, checkState);
                            foreach (var (checkGroup, destination) in new[]
                            {
                                ("body", MotherBrainRainbowPaletteRomData.BodyColor),
                                ("backLegs", MotherBrainRainbowPaletteRomData.SecondaryColor),
                            })
                                for (int checkColor = 0; checkColor < 15; checkColor++)
                                {
                                    var expectedRgb = editedNode[checkGroup]![checkState]![checkColor]!;
                                    ushort expected = (ushort)(expectedRgb["red"]!.GetValue<int>() |
                                        expectedRgb["green"]!.GetValue<int>() << 5 |
                                        expectedRgb["blue"]!.GetValue<int>() << 10);
                                    AssertEqual(expected, actual.Colors[destination + checkColor],
                                        "stream 3 independent health palette edit and unaffected channels");
                                }
                        }
                    }
    }
    private static void VerifyStream3RecoveryLights(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var node = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object fade = typeof(MotherBrainRoomColorPresentation).GetField("recoveryLights", flags)!.GetValue(stock)!;
        AssertTrue(fade.GetType().GetField("supplied", flags)!.GetValue(fade) is null,
            "stream 3 stock recovery light rows discarded");
        AssertTrue(ReferenceEquals(fade.GetType().GetField("finalRoom", flags)!.GetValue(fade),
            typeof(MotherBrainRoomColorPresentation).GetField("finalRoom", flags)!.GetValue(stock)),
            "stream 3 recovery endpoint reuses the final room palette");
        AssertEqual(0, fade.GetType().GetFields(flags).Count(field => field.FieldType == typeof(ushort) || field.FieldType == typeof(Bgr555)),
            "stream 3 reviewed recovery paints reside only in their domain catalog");
        for (int frame = 0; frame < 7; frame++)
        {
            var actual = new SnesCgram();
            stock.ApplyRecoveryLights(actual, frame);
            for (int color = 0; color < 28; color++)
            {
                int address = 0xadf3d3 - frame * 0x38 + color * 2;
                ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
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
                    var rgb = editedNode["recoveryLights"]![frame]![color]!;
                    rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                    var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                        System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                    for (int checkFrame = 0; checkFrame < 7; checkFrame++)
                    {
                        var actual = new SnesCgram();
                        edited.ApplyRecoveryLights(actual, checkFrame);
                        for (int checkColor = 0; checkColor < 28; checkColor++)
                        {
                            var expectedRgb = editedNode["recoveryLights"]![checkFrame]![checkColor]!;
                            ushort expected = (ushort)(expectedRgb["red"]!.GetValue<int>() |
                                expectedRgb["green"]!.GetValue<int>() << 5 |
                                expectedRgb["blue"]!.GetValue<int>() << 10);
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
                var rgb = editedNode["finalRoom"]![color]!;
                rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                    System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                for (int frame = 0; frame < 7; frame++)
                {
                    var expected = new SnesCgram();
                    var actual = new SnesCgram();
                    stock.ApplyRecoveryLights(expected, frame);
                    edited.ApplyRecoveryLights(actual, frame);
                    AssertTrue(expected.Colors.SequenceEqual(actual.Colors),
                        "stream 3 independent final-room edit cannot change recovery content");
                    for (int phase = 0; phase < 2; phase++)
                    {
                        ushort pointer = (ushort)(0xd046 + (frame * 2 + phase) * 4);
                        stock.ApplyFlash(expected, pointer);
                        edited.ApplyFlash(actual, pointer);
                        AssertTrue(expected.Colors.SequenceEqual(actual.Colors),
                            "stream 3 independent final-room edit cannot change flash content");
                    }
                }
            }
        var legacy = node.DeepClone();
        legacy["version"] = MotherBrainRoomColorFormat.PreRecoveryLightsVersion;
        legacy.AsObject().Remove("recoveryLights");
        var oldOverride = MotherBrainRoomColorPresentation.Load(new MemoryStream(
            System.Text.Encoding.UTF8.GetBytes(legacy.ToJsonString())), stock);
        for (int frame = 0; frame < 7; frame++)
        {
            var expected = new SnesCgram();
            var actual = new SnesCgram();
            stock.ApplyRecoveryLights(expected, frame);
            oldOverride.ApplyRecoveryLights(actual, frame);
            AssertTrue(expected.Colors.SequenceEqual(actual.Colors), "stream 3 legacy room override reuses calculated stock recovery");
        }
    }
    private static void VerifyStream3RoomFlash(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var node = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object flash = typeof(MotherBrainRoomColorPresentation).GetField("flash", flags)!.GetValue(stock)!;
        AssertTrue(flash.GetType().GetField("supplied", flags)!.GetValue(flash) is null,
            "stream 3 stock flash rows discarded");
        AssertTrue(ReferenceEquals(flash.GetType().GetField("basis", flags)!.GetValue(flash),
            typeof(MotherBrainRoomColorPresentation).GetField("finalRoom", flags)!.GetValue(stock)),
            "stream 3 stock flash reuses final room paint basis");
        AssertEqual(0, flash.GetType().GetFields(flags).Count(field => field.FieldType == typeof(ushort) || field.FieldType == typeof(Bgr555)),
            "stream 3 room flash keeps no per-instance selected paint");
        for (int frame = 0; frame < MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount; frame++)
        {
            ushort entry = checked((ushort)(MotherBrainRoomPaletteProgramDefinitions.FlashStart + frame * MotherBrainRoomColorRomData.TimedEntryByteCount));
            int operand = MotherBrainRoomColorRomData.SourceBank + entry + MotherBrainRoomColorRomDataTooling.PaletteOperandByteOffset;
            int source = MotherBrainRoomColorRomData.SourceBank | rom.ReadByte(operand) | rom.ReadByte(operand + 1) << 8;
            var actual = new SnesCgram();
            stock.ApplyFlash(actual, entry);
            for (int color = 0; color < MotherBrainRoomColorRomData.SliceColors * 2; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
                int destination = color < MotherBrainRoomColorRomData.SliceColors
                    ? MotherBrainRoomColorRomData.FirstColor + color
                    : MotherBrainRoomColorRomData.SecondColor + color - MotherBrainRoomColorRomData.SliceColors;
                AssertEqual(native, actual.Colors[destination], "stream 3 all native room-flash colors");
                if (color >= MotherBrainRoomColorRomData.SliceColors)
                    AssertEqual(native, actual.Colors[MotherBrainRoomColorRomData.MirroredSecondColor + color - MotherBrainRoomColorRomData.SliceColors], "stream 3 native room-flash mirror");
            }
        }
        for (int offset = 0; offset <= ushort.MaxValue; offset++)
        {
            bool mechanics = offset >= 0xd046 && offset < 0xd07e && (offset - 0xd046) % 4 < 2 ||
                offset is >= 0xd07e and < 0xd082;
            bool presentation = offset >= 0xd046 && offset < 0xd07e && (offset - 0xd046) % 4 >= 2;
            AssertEqual(mechanics, MotherBrainRoomPaletteProgramDefinitions.IsCompiledMechanicsByte(0xa90000 | offset),
                "stream 3 room palette mechanics byte ownership");
            AssertEqual(presentation, MotherBrainRoomPaletteProgramDefinitions.TryGetPresentationWord(0xa90000 | offset, out ushort word),
                "stream 3 room palette operand byte ownership");
            if (presentation) AssertEqual((ushort)(offset & 0xfffe), word, "stream 3 room palette canonical operand");
        }
        foreach (int invalid in new[] { -1, 14, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MotherBrainRoomPaletteProgramDefinitions.PresentationWordAddress(invalid),
                "stream 3 room palette operand index bounds");
        foreach (int invalid in new[] { -1, 16, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => _ = MotherBrainRoomPaletteProgramDefinitions.MechanicsWord(invalid),
                "stream 3 room palette mechanics index bounds");
        for (int frame = 0; frame < 14; frame++)
            for (int color = 0; color < 24; color++)
                foreach (string component in new[] { "red", "green", "blue" })
                {
                    var editedNode = node.DeepClone();
                    var rgb = editedNode["flash"]![frame]![color]!;
                    rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                    var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                        System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                    for (int checkFrame = 0; checkFrame < 14; checkFrame++)
                    {
                        var actual = new SnesCgram();
                        edited.ApplyFlash(actual, (ushort)(0xd046 + checkFrame * 4));
                        for (int checkColor = 0; checkColor < 24; checkColor++)
                        {
                            var expectedRgb = editedNode["flash"]![checkFrame]![checkColor]!;
                            ushort expected = (ushort)(expectedRgb["red"]!.GetValue<int>() |
                                expectedRgb["green"]!.GetValue<int>() << 5 |
                                expectedRgb["blue"]!.GetValue<int>() << 10);
                            int destination = checkColor < 12 ? 0x34 + checkColor : 0x53 + checkColor - 12;
                            AssertEqual(expected, actual.Colors[destination], "stream 3 independent room flash edit");
                            if (checkColor >= 12)
                                AssertEqual(expected, actual.Colors[0x73 + checkColor - 12], "stream 3 edited room flash mirror");
                        }
                    }
                }
    }
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
            byte opcode = bus.ReadByte(pc++);
            switch (opcode)
            {
                case 0x08: // PHP
                case 0x28: // PLP
                    break;

                case 0xe2: // SEP #$20
                    byte sepMask = bus.ReadByte(pc++);
                    if (sepMask != 0x20)
                        throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 2);
                    accumulatorIsEightBit = true;
                    break;

                case 0xa9: // LDA immediate
                    accumulator = bus.ReadByte(pc++);
                    if (!accumulatorIsEightBit)
                        accumulator |= unchecked((ushort)(bus.ReadByte(pc++) << 8));
                    break;

                case 0x8f: // STA long
                    int destination = bus.ReadByte(pc) |
                        (bus.ReadByte(pc + 1) << 8) |
                        (bus.ReadByte(pc + 2) << 16);
                    pc += 3;
                    int storageIndex = destination - RoomScrollGrid.WorkRamAddress;
                    if ((uint)storageIndex >= RoomScrollGrid.StorageByteCount)
                        throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 4);
                    scrolls[storageIndex] = unchecked((byte)accumulator);
                    if (!accumulatorIsEightBit)
                    {
                        if (storageIndex + 1 >= scrolls.Length)
                            throw UnsupportedStream3DoorScroll(pointer, opcode, pc - 4);
                        scrolls[storageIndex + 1] = unchecked((byte)(accumulator >> 8));
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

    private static InvalidDataException UnsupportedStream3DoorScroll(
        ushort pointer,
        byte opcode,
        int opcodeAddress) =>
        new(
            $"Door callback $8F:{pointer:X4} uses unsupported reference-audit opcode " +
            $"${opcode:X2} at ${opcodeAddress >> 16:X2}:{opcodeAddress & 0xffff:X4}.");
    private static void VerifyStream3PhaseTwoRearLeg(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom);
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        var node = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        AssertTrue(typeof(MotherBrainRoomColorPresentation).GetField("phaseTwoRearLeg", flags)!.GetValue(stock) is null,
            "stream 3 phase-two rear uses approved calculated health basis without stored rows");
        var phaseTwo = new SnesCgram();
        stock.ApplyPhaseTwoInitial(phaseTwo);
        for (int color = 0; color < 15; color++)
        {
            int address = 0xa99494 + color * 2;
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, phaseTwo.Colors[0xb1 + color], "stream 3 native phase-two rear color and destination");
            foreach (string component in new[] { "red", "green", "blue" })
            {
                var editedNode = node.DeepClone();
                var rgb = editedNode["phaseTwoRearLeg"]![color]!;
                rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                var edited = MotherBrainRoomColorPresentation.Load(new MemoryStream(
                    System.Text.Encoding.UTF8.GetBytes(editedNode.ToJsonString())));
                var actual = new SnesCgram();
                edited.ApplyPhaseTwoInitial(actual);
                for (int checkColor = 0; checkColor < 15; checkColor++)
                {
                    var expectedRgb = editedNode["phaseTwoRearLeg"]![checkColor]!;
                    ushort expected = (ushort)(expectedRgb["red"]!.GetValue<int>() |
                        expectedRgb["green"]!.GetValue<int>() << 5 | expectedRgb["blue"]!.GetValue<int>() << 10);
                    AssertEqual(expected, actual.Colors[0xb1 + checkColor], "stream 3 independent phase-two rear edit");
                    AssertEqual(phaseTwo.Colors[0xa1 + checkColor], actual.Colors[0xa1 + checkColor],
                        "stream 3 rear edit preserves independently installed attack palette");
                }
            }
        }
    }
    private static void VerifyStream3AuxiliaryPalettes(ISnesAddressSpace rom)
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
                    return (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
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
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (string name in new[] { "faceBlock", "deadSidehopper", "torizoBody", "torizoBelly" })
        {
            object palette = typeof(EnemyAuxiliaryColorCatalog).GetField(name, fields)!.GetValue(stock)!;
            AssertTrue(palette.GetType().GetField("supplied", fields)!.GetValue(palette) is null,
                "stream 3 calculated auxiliary cycles have no stored frame rows");
            foreach (string endpoint in new[] { "first", "last" })
                AssertEqual(0, ((Bgr555[])palette.GetType().GetField(endpoint, fields)!.GetValue(palette)!).Length,
                    "stream 3 complete auxiliary palette keeps no stock endpoint arrays");
            AssertTrue(palette.GetType().GetField("corpse", fields)!.GetValue(palette) is null,
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
    private static void VerifyStream3BabySpriteReflection(ISnesAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var definitions = EnemySpritemapDefinitions.Frames.ToArray();
        var document = new EnemySpritemapDocument
        {
            Version = EnemySpritemapDefinitions.Version,
            Frames = definitions.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>(), StringComparer.Ordinal),
            DisplayFrames = definitions.ToDictionary(frame => frame.Name, frame => frame.Name, StringComparer.Ordinal),
        };
        var selected = definitions.Where(frame => frame.Bank == 0xa9 && frame.Pointer is 0xf9a8 or 0xfa40 or 0xfad8).ToArray();
        AssertEqual(3, selected.Length, "stream 3 identified native Baby compositions");
        foreach (var frame in selected)
        {
            int source = (frame.Bank << 16) | frame.Pointer;
            AssertEqual(30, Word(source), "stream 3 native Baby counted-record extent");
            document.Frames[frame.Name] = Enumerable.Range(0, 30).Select(index =>
            {
                int entry = source + 2 + index * 5;
                var x = new SnesSpritemapXWord(Word(entry));
                var attributes = new SnesObjAttributeWord(Word(entry + 3));
                return new SpriteVisualPart
                {
                    OffsetX = x.SignedOffset, OffsetY = unchecked((sbyte)rom.ReadByte(entry + 2)), Size = x.IsLarge ? 16 : 8,
                    TileColumn = attributes.TileNumber % 16, TileRow = attributes.TileNumber / 16,
                    Palette = attributes.PaletteIndex, Priority = attributes.Priority,
                    FlipX = attributes.FlipHorizontally, FlipY = attributes.FlipVertically,
                };
            }).ToArray();
        }
        EnemySpritemapCatalog Load() => EnemySpritemapCatalog.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
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
                foreach (var frame in definitions.OrderBy(frame => (frame.Bank << 16) | frame.Pointer))
                {
                    content.Append("frame", (frame.Bank << 16) | frame.Pointer);
                    content.AppendEnemyParts(EnemySpritemapCatalog.CompileParts(document.Frames[frame.Name], frame.Name));
                }
                foreach (var frame in definitions.OrderBy(frame => (frame.Bank << 16) | frame.Pointer))
                {
                    content.Append("native-binding", (frame.Bank << 16) | frame.Pointer);
                    content.Append("selected-binding", (frame.Bank << 16) | frame.Pointer);
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
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            bool sharedBody = parts.GetType().GetField("sharedBody", flags)!.GetValue(parts) is not null;
            AssertEqual(sharedBody ? 6 : 15, ((EnemySpritemapPart[])parts.GetType().GetField("halfParts", flags)!.GetValue(parts)!).Length,
                "stream 3 Baby shares nine upper-body halves and stores independent lower halves");
            var nativeOam = new OamBuffer();
            var calculatedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, frame.Bank, frame.Pointer, 128, 128, 0, 0);
            calculatedOam.AddEnemySpritemap(parts, 128, 128, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(calculatedOam.LowTable) && nativeOam.HighTable.SequenceEqual(calculatedOam.HighTable),
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
                part with { TileRow = (part.TileRow + 1) % 32 }, part with { Palette = (part.Palette!.Value + 1) % 8 },
                part with { Priority = (part.Priority + 1) % 4 }, part with { FlipX = !part.FlipX }, part with { FlipY = !part.FlipY },
            ];
            foreach (var edited in edits) { original[0] = edited; Check(Load()); }
            original[0] = part;
            document.Frames[frame.Name] = original.Reverse().ToArray(); Check(Load());
            document.Frames[frame.Name] = original.Append(part).ToArray(); Check(Load());
            document.Frames[frame.Name] = []; Check(Load());
            document.Frames[frame.Name] = original;
            AssertThrows<IndexOutOfRangeException>(() => _ = parts[-1], "stream 3 calculated part lower bound");
            AssertThrows<IndexOutOfRangeException>(() => _ = parts[30], "stream 3 calculated part upper bound");
        }
        Suite(nameof(VerifyEnemyLegacyOverrides), () => VerifyEnemyLegacyOverrides());
    }
    private static void VerifyStream3HandBeamLayout()
    {
        ushort[] stages = [0xc796, 0xc7b7, 0xc7d8];
        int[] frameOffsets = [0, 9, 13, 17, 21, 25, 29];
        var mechanics = new HashSet<int>();
        var presentation = new HashSet<int>();
        int visual = 0;
        for (int stage = 0; stage < stages.Length; stage++)
        {
            AssertEqual((ushort)(stages[stage] + 4), MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallInstruction(stage),
                "hand-beam native external-call instruction");
            foreach (int offset in frameOffsets)
            {
                mechanics.Add(stages[stage] + offset);
                mechanics.Add(stages[stage] + offset + 1);
                presentation.Add(stages[stage] + offset + 2);
                presentation.Add(stages[stage] + offset + 3);
                AssertEqual((ushort)(stages[stage] + offset + 2), MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordAddress(visual++),
                    "hand-beam native visual operand order");
            }
            for (int offset = 4; offset < 9; offset++) mechanics.Add(stages[stage] + offset);
        }
        mechanics.Add(0xc7f9);
        mechanics.Add(0xc7fa);
        for (int address = 0xc795; address <= 0xc7fb; address++)
        {
            AssertEqual(mechanics.Contains(address), MotherBrainHandBeamInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x860000 | address),
                "hand-beam exact mechanics/callback byte ownership");
            AssertEqual(presentation.Contains(address), MotherBrainHandBeamInstructionProgramDefinitions.IsPresentationByte(0x860000 | address),
                "hand-beam exact visual byte ownership");
        }
        AssertTrue(!MotherBrainHandBeamInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(0x85c796), "hand-beam mechanics rejects other bank");
        AssertTrue(!MotherBrainHandBeamInstructionProgramDefinitions.IsPresentationByte(0x85c798), "hand-beam artwork rejects other bank");
        foreach (int index in new[] { -1, 21, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainHandBeamInstructionProgramDefinitions.PresentationWordAddress(index), "hand-beam visual index domain");
        foreach (int index in new[] { -1, 25, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainHandBeamInstructionProgramDefinitionsTooling.NativeWord(index), "hand-beam mechanics index domain");
        foreach (int index in new[] { -1, 3, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => MotherBrainHandBeamInstructionProgramDefinitions.ExternalCallInstruction(index), "hand-beam callback index domain");
    }
    private static void VerifyStream3BabyInstructionLayout(ISnesAddressSpace rom)
    {
        ushort[] mechanics = [0xcfa2,0xcfa6,0xcfaa,0xcfae,0xcfb2,0xcfb8,0xcfbc,0xcfc0,0xcfc4,0xcfc8,0xcfce,0xcfd2];
        ushort[] visual = [0xcfa4,0xcfa8,0xcfac,0xcfb0,0xcfba,0xcfbe,0xcfc2,0xcfc6,0xcfd0];
        AssertEqual(mechanics.Length, MotherBrainBabyInstructionProgramDefinitionsTooling.MechanicsWordCount, "Baby native mechanics count");
        AssertEqual(visual.Length, MotherBrainBabyInstructionProgramDefinitionsTooling.PresentationWordCount, "Baby native visual count");
        for (int index = 0; index < mechanics.Length; index++)
        {
            var word = MotherBrainBabyInstructionProgramDefinitionsTooling.MechanicsWord(index);
            AssertEqual(mechanics[index], word.Address, "Baby exact native mechanics enumeration");
            AssertEqual((ushort)(rom.ReadByte(0xa90000 | word.Address) | rom.ReadByte(0xa90000 | (word.Address + 1)) << 8),
                word.Value, "Baby exact native mechanics operand");
        }
        for (int index = 0; index < visual.Length; index++)
            AssertEqual(visual[index], MotherBrainBabyInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "Baby exact native visual enumeration");
        for (int address = 0xcfa1; address <= 0xcfd4; address++)
            if (!mechanics.Contains((ushort)address))
                AssertThrows<InvalidDataException>(() => MotherBrainBabyInstructionProgramDefinitions.ReadMechanicsWord((ushort)address),
                    "Baby program rejects operand bytes and adjacent callbacks as mechanics");
        foreach (int index in new[] { -1,12,int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainBabyInstructionProgramDefinitionsTooling.MechanicsWord(index), "Baby mechanics enumeration domain");
        foreach (int index in new[] { -1,9,int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => MotherBrainBabyInstructionProgramDefinitionsTooling.PresentationWordAddress(index), "Baby visual enumeration domain");
    }
    private static void VerifyStream3MotherBrainAttackPalette(ISnesAddressSpace rom)
    {
        byte[] json = SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom);
        var document = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        var stock = MotherBrainRoomColorPresentation.Load(new MemoryStream(json));
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object attack = typeof(MotherBrainRoomColorPresentation).GetField("phaseTwoAttack", fields)!.GetValue(stock)!;
        AssertTrue(attack.GetType().GetField("supplied", fields)!.GetValue(attack) is null,
            "stock attack palette calculates every color without a stored row");
        var native = new SnesCgram();
        stock.ApplyPhaseTwoInitial(native);
        for (int color = 0; color < MotherBrainRoomColorRomData.PhaseTwoColors; color++)
        {
            int address = MotherBrainRoomColorRomData.PhaseTwoAttackSource + color * sizeof(ushort);
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, native.Colors[MotherBrainRoomColorRomData.PhaseTwoAttackColor + color],
                "attack palette exact native color and CGRAM destination");
            foreach (string component in new[] { "red", "green", "blue" })
            {
                var edited = document.DeepClone();
                var rgb = edited["phaseTwoAttack"]![color]!;
                rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
                Confirm(Load(edited), edited);
            }
        }
        var independentRecovery = document.DeepClone();
        var recoveryRows = independentRecovery["recoveryLights"]!.AsArray();
        var neutral = recoveryRows[recoveryRows.Count - 1]![14]!;
        neutral["red"] = neutral["red"]!.GetValue<int>() ^ 1;
        Confirm(Load(independentRecovery), independentRecovery);
        Confirm(stock, document);

        static MotherBrainRoomColorPresentation Load(System.Text.Json.Nodes.JsonNode document) =>
            MotherBrainRoomColorPresentation.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        static ushort Word(System.Text.Json.Nodes.JsonNode rgb) => (ushort)(rgb["red"]!.GetValue<int>()
            | rgb["green"]!.GetValue<int>() << 5 | rgb["blue"]!.GetValue<int>() << 10);
        static void Confirm(MotherBrainRoomColorPresentation palette, System.Text.Json.Nodes.JsonNode expected)
        {
            var cgram = new SnesCgram();
            palette.ApplyPhaseTwoInitial(cgram);
            for (int color = 0; color < MotherBrainRoomColorRomData.PhaseTwoColors; color++)
            {
                AssertEqual(Word(expected["phaseTwoAttack"]![color]!),
                    cgram.Colors[MotherBrainRoomColorRomData.PhaseTwoAttackColor + color], "independent attack color preserved");
                AssertEqual(Word(expected["phaseTwoRearLeg"]![color]!),
                    cgram.Colors[MotherBrainRoomColorRomData.PhaseTwoRearLegColor + color], "paired rear palette remains independent");
            }
        }
    }
    private static void VerifyStream3FinalRoomPaints(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom);
        var document = System.Text.Json.JsonSerializer.Deserialize<MotherBrainRoomColorDocument>(imported, MapPresentationFormat.JsonOptions)!;
        MotherBrainRoomColorPresentation Load() => MotherBrainRoomColorPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        var stock = Load();
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object finalRoom = typeof(MotherBrainRoomColorPresentation).GetField("finalRoom", fields)!.GetValue(stock)!;
        AssertTrue(finalRoom.GetType().GetField("supplied", fields)!.GetValue(finalRoom) is null, "final-room stock has no supplied output array");
        AssertEqual(0, finalRoom.GetType().GetFields(fields).Count(field => field.FieldType == typeof(ushort)), "final-room paint choices live only in reviewed catalog");
        for (int color = 0; color < document.FinalRoom.Length; color++)
        {
            int address = MotherBrainRoomColorRomData.SourceBank + MotherBrainRoomPaletteProgramDefinitions.FinalPalette + color * sizeof(ushort);
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
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
                AssertEqual(Word(document.InitialGlassShard![color]), colors.Colors[0xb1 + color], "final-room edit cannot alter supplied glass");
                AssertEqual(Word(document.InitialTubeProjectile![color]), colors.Colors[0xf1 + color], "final-room edit cannot alter supplied tube");
            }
            for (int frame = 0; frame < document.RecoveryLights!.Length; frame++)
            {
                palette.ApplyRecoveryLights(colors, frame);
                for (int color = 0; color < 28; color++)
                    AssertEqual(Word(document.RecoveryLights[frame][color]), colors.Colors[color < 14 ? 0x31 + color : 0x51 + color - 14], "final-room edit cannot alter supplied recovery");
            }
            for (int frame = 0; frame < document.Flash.Length; frame++)
            {
                palette.ApplyFlash(colors, checked((ushort)(MotherBrainRoomPaletteProgramDefinitions.FlashStart + frame * 4)));
                for (int color = 0; color < 24; color++)
                    AssertEqual(Word(document.Flash[frame][color]), colors.Colors[color < 12 ? 0x34 + color : 0x53 + color - 12], "final-room edit cannot alter supplied flash");
            }
        }
    }

    private static void VerifyStream3RoomEntryPalettes(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.MotherBrainRoomColorExtractor.Extract(rom);
        MotherBrainRoomColorDocument Read() => System.Text.Json.JsonSerializer.Deserialize<MotherBrainRoomColorDocument>(
            imported, MapPresentationFormat.JsonOptions)!;
        MotherBrainRoomColorPresentation Load(MotherBrainRoomColorDocument document, MotherBrainRoomColorPresentation? current = null) =>
            MotherBrainRoomColorPresentation.Load(new MemoryStream(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(
                document, MapPresentationFormat.JsonOptions)), current);
        var document = Read();
        var stock = Load(document);
        for (int color = 0; color < MotherBrainRoomColorRomData.InitialColors; color++)
        {
            int glassAddress = MotherBrainRoomColorRomData.InitialGlassShardSource + color * sizeof(ushort);
            int tubeAddress = MotherBrainRoomColorRomData.InitialTubeProjectileSource + color * sizeof(ushort);
            AssertEqual((ushort)(rom.ReadByte(glassAddress) | rom.ReadByte(glassAddress + 1) << 8), Word(document.InitialGlassShard![color]), "glass direct native color");
            AssertEqual((ushort)(rom.ReadByte(tubeAddress) | rom.ReadByte(tubeAddress + 1) << 8), Word(document.InitialTubeProjectile![color]), "tube direct native color");
        }
        Confirm(stock, document);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        foreach (string name in new[] { "finalRoom", "phaseTwoAttack", "initialGlassShard", "initialTubeProjectile" })
        {
            object palette = typeof(MotherBrainRoomColorPresentation).GetField(name, fields)!.GetValue(stock)!;
            AssertTrue(palette.GetType().GetField("supplied", fields)!.GetValue(palette) is null,
                "stream 3 room initial palette calculates dependent stock shades: " + name);
        }
        object finalRoom = typeof(MotherBrainRoomColorPresentation).GetField("finalRoom", fields)!.GetValue(stock)!;
        AssertEqual(0, finalRoom.GetType().GetFields(fields).Count(field => field.FieldType == typeof(ushort)),
            "final room calculates from reviewed catalog choices without per-instance paint words");
        object attack = typeof(MotherBrainRoomColorPresentation).GetField("phaseTwoAttack", fields)!.GetValue(stock)!;
        AssertTrue(!attack.GetType().GetFields(fields).Any(field => field.FieldType == typeof(ushort)),
            "stock attack keeps no duplicated scalar paint endpoints");
        object glass = typeof(MotherBrainRoomColorPresentation).GetField("initialGlassShard", fields)!.GetValue(stock)!;
        AssertTrue(!glass.GetType().GetFields(fields).Any(field => field.FieldType == typeof(ushort)),
            "stock glass keeps no duplicated scalar paint endpoint");
        foreach (PaletteRgb5[] palette in new[] { document.InitialGlassShard!, document.InitialTubeProjectile! })
        for (int color = 0; color < palette.Length; color++)
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
            PaletteRgb5 original = document.RecoveryLights![6][14];
            document.RecoveryLights[6][14] = Change(original, component);
            Confirm(Load(document), document);
            document.RecoveryLights[6][14] = original;
        }
        foreach (int version in new[] { MotherBrainRoomColorFormat.PreRoomEntryVersion, MotherBrainRoomColorFormat.PreRecoveryLightsVersion })
        {
            var legacy = document with { Version = version, RecoveryLights = null };
            if (version == MotherBrainRoomColorFormat.PreRoomEntryVersion)
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
                AssertEqual(Word(expected.InitialGlassShard![color]), entry.Colors[0xb1 + color], "room glass exact native/edited shade");
                AssertEqual(Word(expected.InitialTubeProjectile![color]), entry.Colors[0xf1 + color], "room tube exact native/edited shade");
            }
        }
    }
    private static void VerifyStream3BabyInitialPaints(ISnesAddressSpace rom)
    {
        var babyDocument = System.Text.Json.Nodes.JsonNode.Parse(SuperMetroid.AssetExtraction.BabyMetroidCutsceneColorExtractor.Extract(rom))!;
        var liveDocument = System.Text.Json.Nodes.JsonNode.Parse(SuperMetroid.AssetExtraction.ShitroidColorExtractor.Extract(rom))!;
        var stock = LoadBaby(babyDocument);
        var liveStock = LoadLive(liveDocument);
        const System.Reflection.BindingFlags fields = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        object initial = typeof(BabyMetroidCutsceneColorCatalog).GetField("initial", fields)!.GetValue(stock)!;
        AssertTrue(initial.GetType().GetField("supplied", fields)!.GetValue(initial) is null, "Baby stock initial has no stored color array");
        AssertEqual(0, initial.GetType().GetFields(fields).Count(field => field.FieldType == typeof(ushort)), "Baby initial paint choices reside only in dedicated catalog");
        for (int color = 0; color < BabyMetroidCutsceneColorRomData.InitialColorCount; color++)
        {
            int address = BabyMetroidCutsceneColorRomData.InitialSource + color * sizeof(ushort);
            ushort native = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(native, stock.InitialColor(color), "Baby initial exact native color");
            AssertEqual(native, liveStock.TargetColor(ShitroidColorTarget.Shitroid, color + 1), "Baby live initial alias");
        }
        Check(stock, babyDocument);
        for (int color = 0; color < BabyMetroidCutsceneColorRomData.InitialColorCount; color++)
        foreach (string component in new[] { "red", "green", "blue" })
        {
            var editedBabyDocument = babyDocument.DeepClone();
            var rgb = editedBabyDocument["initial"]![color]!;
            rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
            Check(LoadBaby(editedBabyDocument), editedBabyDocument);
            var editedLiveDocument = liveDocument.DeepClone();
            rgb = editedLiveDocument["shitroid"]![color + 1]!;
            rgb[component] = rgb[component]!.GetValue<int>() ^ 1;
            var editedLive = LoadLive(editedLiveDocument);
            for (int index = 0; index < BabyMetroidCutsceneColorRomData.InitialColorCount; index++)
            {
                AssertEqual(Word(editedLiveDocument["shitroid"]![index + 1]!), editedLive.TargetColor(ShitroidColorTarget.Shitroid, index + 1), "independent live alias edit");
                AssertEqual(Word(babyDocument["initial"]![index]!), stock.InitialColor(index), "live edit cannot mutate Baby instance");
                AssertEqual(Word(liveDocument["shitroid"]![index + 1]!), liveStock.TargetColor(ShitroidColorTarget.Shitroid, index + 1), "Baby edit cannot mutate live instance");
            }
        }
        Check(stock, babyDocument);
        foreach (int invalid in new[] { -1, BabyMetroidCutsceneColorRomData.InitialColorCount, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.InitialColor(invalid), "Baby initial index domain preserved");
        static BabyMetroidCutsceneColorCatalog LoadBaby(System.Text.Json.Nodes.JsonNode document) =>
            BabyMetroidCutsceneColorCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        static ShitroidColorCatalog LoadLive(System.Text.Json.Nodes.JsonNode document) =>
            ShitroidColorCatalog.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(document.ToJsonString())));
        static ushort Word(System.Text.Json.Nodes.JsonNode rgb) => (ushort)(rgb["red"]!.GetValue<int>() |
            rgb["green"]!.GetValue<int>() << 5 | rgb["blue"]!.GetValue<int>() << 10);
        static void Check(BabyMetroidCutsceneColorCatalog catalog, System.Text.Json.Nodes.JsonNode document)
        {
            ushort[] colors = document["initial"]!.AsArray().Select(rgb => Word(rgb!)).ToArray();
            ushort[][] fade = document["fade"]!.AsArray().Select(row => row!.AsArray().Select(rgb => Word(rgb!)).ToArray()).ToArray();
            for (int color = 0; color < colors.Length; color++) AssertEqual(colors[color], catalog.InitialColor(color), "Baby independently edited initial colors");
            string identity = SelectedPresentationHash.Create("BabyMetroidCutsceneColorCatalog-v1", content =>
            {
                content.AppendWords("initial", colors);
                content.AppendWordFrames("fade", fade);
            });
            AssertEqual(identity, catalog.ContentIdentity, "Baby initial canonical content identity preserved");
        }
    }
    private static void VerifyStream3InitialNarrationMap()
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp", "initial-narration-" + Guid.NewGuid().ToString("N")));
        try
        {
            string source = Path.GetFullPath("Super Metroid.smc");
            var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
            byte[] native = SuperMetroid.Core.Rom.RomDataReader.Decompress(rom,
                IntroCinematicRomData.Assets.FirstNarrationTilemap, maximumOutputBytes: 2048);
            var installation = SuperMetroid.AssetExtraction.GameAssetInstaller.Install(source, root);
            IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var pageConstructor = typeof(RoomBackgroundTilemapAtlas).GetConstructors(flags).Single();
            RoomBackgroundTilemapAtlas Page(byte[] bytes) => (RoomBackgroundTilemapAtlas)pageConstructor.Invoke([bytes]);
            RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(stock.BackgroundPages.Slice(index * 2048, 2048).ToArray())).ToArray();
            IntroCinematicArtworkCatalog Copy(byte[] narration) => new(stock.BackgroundCharacters,
                stock.IntroObjectCharacters, stock.CinematicObjectCharacters, pages, Page(stock.PortraitTilemap.ToArray()),
                Page(narration), stock.FinalLine, stock.EyeFrames, stock.CaretSprites, stock.MotherBrainSprites,
                stock.MotherBrainExplosionSprites, stock.RinkaSprites, stock.EggEffectSprites,
                stock.DiscoveryActorSprites, stock.ScientistSprites, stock.Palette, stock.CeresFlight, stock.CeresDestruction);
            AssertTrue(stock.InitialNarrationTilemap.Span.SequenceEqual(native), "all 1024 initial narration words match native");
            var field = typeof(IntroCinematicArtworkCatalog).GetField("suppliedInitialNarration", flags)!;
            AssertTrue(field.GetValue(stock) is null, "stock initial narration stores no page array");
            var font = IntroFontAtlas.Load(new MemoryStream(SuperMetroid.AssetExtraction.IntroFontAtlasExtractor.Extract(rom)));
            var intro = new IntroCinematicState(new TestAddressSpace(), introFont: font, characterArtwork: stock,
                beamArtwork: installation.LoadProjectiles().BeamTiles);
            byte[] nativeFont = SuperMetroid.Core.Rom.RomDataReader.Decompress(rom,
                IntroCinematicRomData.Assets.FontOne, maximumOutputBytes: IntroFontAtlasFormat.ByteCount);
            AssertTrue(intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                IntroCinematicRomData.Vram.FontOneDestinationByte, nativeFont.Length).SequenceEqual(nativeFont),
                "actual opening constructor transfers all calculated font bytes");
            AssertTrue(intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                IntroCinematicRomData.Vram.NarrationTilemapDestinationByte, native.Length).SequenceEqual(native),
                "actual opening constructor transfers calculated narration to native VRAM destination");
            typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!.Invoke(intro, null);
            var subtitleUpload = intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                IntroCinematicRomData.Vram.NarrationTilemapDestinationByte + IntroCinematicRomData.Text.FinalLineDestinationStart * 2,
                IntroFinalLineTilemapFormat.CellCount * 2);
            for (int index = 0; index < IntroFinalLineTilemapFormat.CellCount; index++)
                AssertEqual(ReadVerificationWord(rom, IntroCinematicRomData.Assets.FinalTextLine + index * 2),
                    (ushort)(subtitleUpload[index * 2] | subtitleUpload[index * 2 + 1] << 8), "actual Japanese subtitle staging upload");
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
    private static void VerifyStream3FileSelectSprites(ISnesAddressSpace rom)
    {
        byte[] imported = SuperMetroid.AssetExtraction.FileSelectPresentationExtractor.Extract(rom);
        Suite(nameof(VerifyStream3FileSelectBorders), () => VerifyStream3FileSelectBorders(rom, imported));
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        FileSelectPresentationDocument Read() => System.Text.Json.JsonSerializer.Deserialize<FileSelectPresentationDocument>(imported, MapPresentationFormat.JsonOptions)!;
        FileSelectPresentation Load(FileSelectPresentationDocument document) => FileSelectPresentation.Load(new MemoryStream(
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions)));
        SpriteComposition Composition(FileSelectPresentation owner, string name) =>
            ((Dictionary<string, SpriteComposition>)typeof(FileSelectPresentation).GetField("sprites", flags)!.GetValue(owner)!)[name];
        var original = Read();
        var stock = Load(original);
        for (int frame = 0; frame < 8; frame++)
        {
            string name = FileSelectPresentationDefinitions.HelmetFrameName(frame);
            var composition = Composition(stock, name);
            AssertTrue(typeof(SpriteComposition).GetField("parts", flags)!.GetValue(composition) is FileSelectHelmetParts,
                "stock helmet retains semantic frame only");
            Confirm(stock, original, frame);
            int pointer = ReadVerificationWord(rom, 0x82c569 + FileSelectHelmetAnimation.SpritemapId(frame) * 2);
            int address = 0x820000 | pointer;
            AssertEqual((int)ReadVerificationWord(rom, address), composition.PartCount, "exact native helmet part count");
            for (int part = 0; part < composition.PartCount; part++)
            {
                int entry = address + 2 + part * 5;
                var nativeX = new SnesSpritemapXWord(ReadVerificationWord(rom, entry));
                AssertEqual(nativeX.SignedOffset, composition.Part(part).X.SignedOffset, "native helmet X");
                AssertEqual(nativeX.IsLarge, composition.Part(part).X.IsLarge, "native helmet size");
                AssertEqual(rom.ReadByte(entry + 2), composition.Part(part).Y, "native helmet Y");
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
            string name = FileSelectPresentationDefinitions.HelmetFrameName(frame);
            var expectedParts = MenuSpriteCompiler.Compile(expected.Sprites[name], name);
            var selected = Composition(actual, name);
            AssertEqual(expectedParts.PartCount, selected.PartCount, "helmet part count");
            for (int part = 0; part < expectedParts.PartCount; part++)
                AssertEqual(expectedParts.Part(part), selected.Part(part), "exact selected helmet part");
            var actualOam = new OamBuffer();
            var expectedOam = new OamBuffer();
            actual.DrawHelmet(actualOam, frame, 0);
            var anchor = expected.HelmetAnchors[0];
            expectedParts.DrawOnScreen(expectedOam, (ushort)anchor.X, (ushort)anchor.Y, (ushort)(expected.ObjectPalette << 9));
            AssertTrue(actualOam.LowTable.SequenceEqual(expectedOam.LowTable) && actualOam.HighTable.SequenceEqual(expectedOam.HighTable),
                "actual helmet draw preserves native and independent edited ordered OAM");
        }
    }
    private static void VerifyStream3IntroFont()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        byte[] native = SuperMetroid.Core.Rom.RomDataReader.Decompress(rom,
            IntroCinematicRomData.Assets.FontOne, maximumOutputBytes: IntroFontAtlasFormat.ByteCount);
        var stock = IntroFontAtlas.Load(new MemoryStream(SuperMetroid.AssetExtraction.IntroFontAtlasExtractor.Extract(rom)));
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        var constructor = typeof(IntroFontAtlas).GetConstructors(flags).Single();
        var field = typeof(IntroFontAtlas).GetField("suppliedTransfer", flags)!;
        AssertTrue(stock.Transfer.Span.SequenceEqual(native), "144 calculated font tiles match native 2304 bytes");
        AssertTrue(field.GetValue(stock) is null, "native font retains no planar output array");
        for (int index = 0; index < native.Length; index++)
        {
            byte[] edit = native.ToArray();
            edit[index] ^= 0xff;
            var selected = (IntroFontAtlas)constructor.Invoke([edit.ToArray()]);
            AssertTrue(selected.Transfer.Span.SequenceEqual(edit), "independent font planar-byte edit survives");
        }
        AssertTrue(stock.Transfer.Span.SequenceEqual(native), "edited glyphs do not cross-couple stock or aliases");
        byte[] output = stock.Transfer.ToArray();
        output[0] ^= 1;
        AssertTrue(stock.Transfer.Span.SequenceEqual(native), "font output is freshly calculated");
        var type = typeof(IntroFontAtlas).Assembly.GetType("SuperMetroid.Core.Assets.IntroFontGlyphDefinitions")!;
        const System.Reflection.BindingFlags methods = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
        var add = type.GetMethod("AddedContour", methods)!;
        var remove = type.GetMethod("RemovedContour", methods)!;
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
                if ((bool)add.Invoke(null, [source, gx, y + dy])!) additions++;
                if ((bool)remove.Invoke(null, [source, gx, y + dy])!) removals++;
            }
        }
        AssertEqual(4, additions, "only four authored outline additions");
        AssertEqual(91, removals, "only 91 authored corner removals including derived period alias");
        for (int y = 0; y < 8; y++)
        for (int plane = 0; plane < 2; plane++)
            AssertEqual((byte)(native[0x24 * 16 + y * 2 + plane] >> 1), native[0x26 * 16 + y * 2 + plane], "whole period drawing shifts right one pixel");
        Console.WriteLine("Intro font: 144 native tiles/2304 bytes, 2304 independent edits, four outline additions/91 removals, exact period alias and no stock output storage pass.");
    }

    private static void VerifyStream3BackgroundMaps()
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp", "background-maps-" + Guid.NewGuid().ToString("N")));
        try
        {
            string source = Path.GetFullPath("Super Metroid.smc");
            var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
            byte[] native = SuperMetroid.Core.Rom.RomDataReader.Decompress(rom,
                IntroCinematicRomData.Assets.BackgroundPageTilemaps, maximumOutputBytes: 8192);
            var installation = SuperMetroid.AssetExtraction.GameAssetInstaller.Install(source, root);
            IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var pageConstructor = typeof(RoomBackgroundTilemapAtlas).GetConstructors(flags).Single();
            RoomBackgroundTilemapAtlas Page(byte[] bytes) => (RoomBackgroundTilemapAtlas)pageConstructor.Invoke([bytes]);
            IntroCinematicArtworkCatalog Copy(byte[] background)
            {
                RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(background.AsSpan(index * 2048, 2048).ToArray())).ToArray();
                return new(stock.BackgroundCharacters, stock.IntroObjectCharacters, stock.CinematicObjectCharacters,
                    pages, Page(stock.PortraitTilemap.ToArray()), Page(stock.InitialNarrationTilemap.ToArray()),
                    stock.FinalLine, stock.EyeFrames, stock.CaretSprites, stock.MotherBrainSprites,
                    stock.MotherBrainExplosionSprites, stock.RinkaSprites, stock.EggEffectSprites,
                    stock.DiscoveryActorSprites, stock.ScientistSprites, stock.Palette,
                    stock.CeresFlight, stock.CeresDestruction);
            }
            var generator = typeof(IntroCinematicArtworkCatalog).Assembly.GetType("SuperMetroid.Core.Assets.IntroBackgroundTilemapDefinitions")!;
            byte[] calculated = (byte[])generator.GetMethod("Compile", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.Invoke(null, null)!;
            var mismatches = new List<string>();
            for (int cell = 0; cell < native.Length / 2; cell++)
            {
                ushort expected = BitConverter.ToUInt16(native, cell * 2), actual = BitConverter.ToUInt16(calculated, cell * 2);
                if (expected != actual) mismatches.Add($"page{cell / 1024} ({cell % 32},{cell % 1024 / 32}) expected{expected:X4}/actual{actual:X4}");
            }
            AssertTrue(mismatches.Count == 0, "all 4096 background words match native: " + string.Join("; ", mismatches.Take(32)));
            AssertTrue(typeof(IntroCinematicArtworkCatalog).GetField("suppliedBackgroundPages", flags)!.GetValue(stock) is null,
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
            var font = IntroFontAtlas.Load(new MemoryStream(SuperMetroid.AssetExtraction.IntroFontAtlasExtractor.Extract(rom)));
            var intro = new IntroCinematicState(new TestAddressSpace(), introFont: font, characterArtwork: stock,
                beamArtwork: installation.LoadProjectiles().BeamTiles);
            AssertTrue(intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                IntroCinematicRomData.Vram.BackgroundPagesDestinationByte, native.Length).SequenceEqual(native),
                "actual opening constructor uploads all four calculated background pages");
            Console.WriteLine("Intro backgrounds: 4096 native words, 4096 independent edits, owned fallback, canonical hash and actual four-page VRAM upload pass.");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }


    private static void VerifyStream3WorkRobotRegistry()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        EnemySpritemapDefinition[] frames = WorkRobotVisualDefinitions.Frames().ToArray();
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

    private static void VerifyStream3ShaktoolRegistry()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        EnemySpritemapDefinition[] frames = ShaktoolVisualDefinitions.Frames().ToArray();
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

    private static void VerifyStream3NintendoFadeEntries()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var entries = NintendoLogoFadePaletteFxProgramMechanicsDefinitionsTooling.All;
        AssertEqual(2, entries.Count, "both semantic Nintendo fade entries");
        foreach (var entry in new[]
        {
            (DefinitionPointer: NintendoLogoFadePaletteFxProgramMechanicsDefinitions.BootLogoDefinitionPointer,
                ProgramStart: NintendoLogoFadePaletteFxProgramMechanicsDefinitions.BootLogoEntry,
                ColorByteIndex: NintendoLogoFadePaletteFxProgramMechanicsDefinitions.BootLogoColorByteIndex,
                BranchesToSharedBody: false),
            (DefinitionPointer: NintendoLogoFadePaletteFxProgramMechanicsDefinitions.CopyrightDefinitionPointer,
                ProgramStart: NintendoLogoFadePaletteFxProgramMechanicsDefinitions.CopyrightEntry,
                ColorByteIndex: NintendoLogoFadePaletteFxProgramMechanicsDefinitions.CopyrightColorByteIndex,
                BranchesToSharedBody: true),
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
                AssertEqual(NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FirstFramePointer,
                    ReadVerificationWord(rom, 0x8d0000 | (entry.ProgramStart + 6)), "copyright branch target");
            }
            else AssertEqual(NintendoLogoFadePaletteFxProgramMechanicsDefinitions.FirstFramePointer,
                (ushort)(entry.ProgramStart + 4), "boot actually falls through");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => _ = entries[-1], "fade entry lower bound");
        AssertThrows<ArgumentOutOfRangeException>(() => _ = entries[2], "fade entry upper bound");
        AssertTrue(entries.SequenceEqual(new[] { entries[0], entries[1] }), "fade enumeration and indexing agree");
        Console.WriteLine("Nintendo fade entries: both native definition/slot/fallthrough-or-branch contracts, order and bounds pass; timing/colors unchanged.");
    }

    private static void VerifyStream3PortraitMap()
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp", "portrait-map-" + Guid.NewGuid().ToString("N")));
        try
        {
            string source = Path.GetFullPath("Super Metroid.smc");
            var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(source);
            byte[] native = SuperMetroid.Core.Rom.RomDataReader.Decompress(rom,
                IntroCinematicRomData.Assets.SamusHeadTilemap, maximumOutputBytes: 2048);
            var installation = SuperMetroid.AssetExtraction.GameAssetInstaller.Install(source, root);
            IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var pageConstructor = typeof(RoomBackgroundTilemapAtlas).GetConstructors(flags).Single();
            RoomBackgroundTilemapAtlas Page(byte[] bytes) => (RoomBackgroundTilemapAtlas)pageConstructor.Invoke([bytes]);
            RoomBackgroundTilemapAtlas[] pages = Enumerable.Range(0, 4).Select(index => Page(stock.BackgroundPages.Slice(index * 2048, 2048).ToArray())).ToArray();
            IntroCinematicArtworkCatalog Copy(byte[] portrait) => new(stock.BackgroundCharacters,
                stock.IntroObjectCharacters, stock.CinematicObjectCharacters, pages, Page(portrait),
                Page(stock.InitialNarrationTilemap.ToArray()), stock.FinalLine, stock.EyeFrames, stock.CaretSprites,
                stock.MotherBrainSprites, stock.MotherBrainExplosionSprites, stock.RinkaSprites,
                stock.EggEffectSprites, stock.DiscoveryActorSprites, stock.ScientistSprites, stock.Palette,
                stock.CeresFlight, stock.CeresDestruction);
            AssertTrue(stock.PortraitTilemap.Span.SequenceEqual(native), "all 1024 calculated portrait words match native");
            AssertTrue(typeof(IntroCinematicArtworkCatalog).GetField("suppliedPortrait", flags)!.GetValue(stock) is null,
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
            var font = IntroFontAtlas.Load(new MemoryStream(SuperMetroid.AssetExtraction.IntroFontAtlasExtractor.Extract(rom)));
            var intro = new IntroCinematicState(new TestAddressSpace(), introFont: font, characterArtwork: stock,
                beamArtwork: installation.LoadProjectiles().BeamTiles);
            AssertTrue(intro.CaptureTranslatedRenderSnapshot().Memory.Vram.Slice(
                IntroCinematicRomData.Vram.SamusHeadTilemapDestinationByte, native.Length).SequenceEqual(native),
                "actual opening constructor uploads calculated portrait map");
            Console.WriteLine("Portrait map: 1024 native words, 1024 independent edits, owned fallback, canonical hash and actual VRAM upload pass.");
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

}
