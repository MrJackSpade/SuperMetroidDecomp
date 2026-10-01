using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>
    /// Exercises the statically identified missing Mother Brain body draw path with
    /// a RAM-only fixture. This is not a gameplay search for cartridge accesses.
    /// </summary>
    private static void VerifyMotherBrainBodyPresentation()
    {
        var author = new EnemyIdentityFixture();
        EnemyExtendedFrameDocument stockOam = BodyOam(edited: false), editedOam = BodyOam(edited: true);
        EnemyBg2FrameDocument stockBg2 = BodyBg2(edited: false), editedBg2 = BodyBg2(edited: true);
        var baseline = new MotherBrainPresentationFixture(Build(stockOam, stockBg2));
        var mixed = MotherBrainBodyVisualDefinitions.Frames.ToArray()
            .Where(frame => MotherBrainBodyVisualDefinitions.HasBg2(frame.Pointer)).ToArray();
        var physicalFrames = MotherBrainBodyInstructionProgramDefinitions.AllWords
            .Where(word => word.Word < 0x8000)
            .Select(word => MotherBrainBodyInstructionProgramDefinitions.ReadVisualSelector(
                checked((ushort)(word.Address + 2)))).ToHashSet();
        physicalFrames.UnionWith(MotherBrainHandBeamBodyInstructionDefinitions.PresentationOperands.ToArray()
            .Select(MotherBrainHandBeamBodyInstructionDefinitions.ReadVisualSelector));
        AssertTrue(physicalFrames.SetEquals(MotherBrainBodyVisualDefinitions.Frames.ToArray()
            .Select(frame => frame.Pointer)), "installed body inventory covers every compiled program selector, not only exercised paths");
        // Remap the whole mixed pose, not only its limbs. The native selector must not change.
        for (int index = 0; index < mixed.Length; index++)
            editedOam.DisplayFrames![mixed[index].Name] = mixed[(index + 1) % mixed.Length].Name;
        var replacement = new MotherBrainPresentationFixture(Build(editedOam, editedBg2));
        foreach (EnemyExtendedFrameDefinition frame in MotherBrainBodyVisualDefinitions.Frames)
        {
            baseline.SetFrame(frame.Pointer, fresh: true);
            replacement.SetFrame(frame.Pointer, fresh: true);
            baseline.ClearBg2(); replacement.ClearBg2();
            OamBuffer original = baseline.Draw(), changed = replacement.Draw();
            AssertBodyOam(original, stockOam.Frames[frame.Name], baseline.Body);
            string selected = editedOam.DisplayFrames![frame.Name];
            AssertBodyOam(changed, editedOam.Frames[selected], replacement.Body);
            AssertBodyBg2(baseline.Inner.Vram, stockBg2, frame.Name);
            AssertBodyBg2(replacement.Inner.Vram, editedBg2, selected);
            AssertEnemyAnimationMechanics(baseline.Inner, replacement.Inner, frame.Pointer);
            AssertEqual(frame.Pointer, replacement.Body.SpritemapPointer, "display remap retains physical frame");

            replacement.SetFrame(frame.Pointer, fresh: false);
            replacement.Inner.Vram.ExecuteWordTransfer(new ushort[] { 0xbeef }, EnemyBg2FrameLayout.VramBase, 1);
            byte[] retained = replacement.Inner.Vram.Bytes.ToArray();
            AssertBodyOam(replacement.Draw(), editedOam.Frames[selected], replacement.Body);
            AssertTrue(retained.AsSpan().SequenceEqual(replacement.Inner.Vram.Bytes),
                "repeat OAM draws do not rewrite BG2 without the native new-frame bit");
        }
        int steps = 0;
        foreach (ushort program in new[] { MotherBrainBodyInstructionLists.WalkForwardsMedium,
                     MotherBrainBodyInstructionLists.WalkBackwardsFast, MotherBrainBodyInstructionLists.CrouchSlow })
        {
            baseline.SetProgram(program); replacement.SetProgram(program);
            for (int frame = 0; frame < 100; frame++, steps++)
            {
                baseline.Step(frame); replacement.Step(frame);
                baseline.Draw(); replacement.Draw();
                AssertEnemyAnimationMechanics(baseline.Inner, replacement.Inner, steps);
                AssertAnimationValues(baseline.State, replacement.State, "Mother Brain instruction state");
                AssertTrue(baseline.ScrollCalls.SequenceEqual(replacement.ScrollCalls),
                    "editable art preserves each body/BG2 counter-scroll callback");
            }
        }
        AssertTrue(baseline.ScrollCalls.Count > 0, "production body bytecode actually moves and counter-scrolls");
        var missing = new MotherBrainPresentationFixture(EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort, RoomCharacterAtlas>(), new Dictionary<ushort, EnemyPaletteSheet>(),
            extendedFrames: EnemyExtendedFrameCatalog.Load(author.Json(stockOam))));
        missing.SetFrame(mixed[0].Pointer, fresh: true);
        AssertThrows<InvalidDataException>(() => missing.Draw(), "missing mixed-frame BG2 art fails explicitly");
        var fields = (FieldInfo[])typeof(DebuggerObjectGraphSerializer).GetMethod("GetSerializableFields",
            BindingFlags.Static | BindingFlags.NonPublic)!.Invoke(null, [typeof(EnemyTileArtworkCatalog)])!;
        FieldInfo[] legacy = fields.Where(field => field.Name != "<MotherBrainBodyBg2Frames>k__BackingField").ToArray();
        AssertTrue(DebuggerStateFieldMigrations.SelectSerializedFields(typeof(EnemyTileArtworkCatalog),
            fields, legacy.Length).SequenceEqual(legacy), "historical enemy catalogs omit only the new visual owner");
        using var state = new MemoryStream();
        EnemyTileArtworkCatalog bundle = Build(stockOam, stockBg2);
        DebuggerObjectGraphSerializer.Serialize(state, bundle); state.Position = 0;
        AssertEqual(bundle.ContentIdentity, DebuggerObjectGraphSerializer.Deserialize<EnemyTileArtworkCatalog>(state).ContentIdentity,
            "new Mother Brain visual owner round-trips in debugger states");
        Console.WriteLine($"PASS Mother Brain: all 17 exact OAM/BG2 poses, mixed remaps, new-frame gating, " +
            $"{steps} production instruction steps with unchanged movement/timing/scroll/physics, missing-art rejection and state compatibility. RAM-only.");

        EnemyTileArtworkCatalog Build(EnemyExtendedFrameDocument oam, EnemyBg2FrameDocument bg2) => author.Build(
            extendedFrames: EnemyExtendedFrameCatalog.Load(author.Json(oam)),
            motherBrainBodyBg2Frames: MotherBrainBodyBg2FrameCatalog.Load(author.Json(bg2)));

        EnemyExtendedFrameDocument BodyOam(bool edited)
        {
            EnemyExtendedFrameDocument document = author.ExtendedDocument();
            int index = 0;
            foreach (EnemyExtendedFrameDefinition frame in MotherBrainBodyVisualDefinitions.Frames)
            {
                SpriteVisualPart part = document.Frames[frame.Name][0].Parts[0] with
                { OffsetX = 2, OffsetY = 3, Size = 16, TileColumn = index % 16, TileRow = index / 16,
                    Palette = 1, Priority = 2, FlipX = edited, FlipY = edited };
                document.Frames[frame.Name] =
                [ new() { OffsetX = edited ? 5 : 0, OffsetY = edited ? -4 : 0, Parts = [part] },
                  new() { OffsetX = edited ? -7 : 6, OffsetY = edited ? 8 : 5, Parts = [part with { Size = 8 }] } ];
                index++;
            }
            return document;
        }

        static EnemyBg2FrameDocument BodyBg2(bool edited) => new()
        {
            Version = MotherBrainBodyVisualDefinitions.Bg2Version,
            Frames = MotherBrainBodyVisualDefinitions.Bg2Frames.ToArray().Select((frame, index) =>
                KeyValuePair.Create(frame.Name, new EnemyBg2WriteDocument[]
                { new() { X = edited ? 3 : 0, Y = edited ? 2 : 0, Tiles = [100 + index, 200 + index] },
                  new() { X = edited ? 4 : 1, Y = edited ? 3 : 1, Tiles = [300 + index, edited ? 600 + index : 400 + index] } })).ToDictionary(),
        };
    }

    private static void AssertBodyOam(OamBuffer actual, EnemyExtendedVisualComponent[] authored, RoomEnemySlot body)
    {
        var expected = new byte[authored.Length * 4];
        byte high = 0;
        for (int index = 0; index < authored.Length; index++)
        {
            EnemyExtendedVisualComponent component = authored[index];
            SpriteVisualPart part = component.Parts.Single();
            int x = body.XPosition + component.OffsetX + part.OffsetX;
            int y = body.YPosition + component.OffsetY + part.OffsetY;
            int palette = part.Palette ?? throw new InvalidDataException("Body OAM fixture requires an explicit palette.");
            ushort tile = checked((ushort)(part.TileRow * 16 + part.TileColumn | palette << 9 |
                part.Priority << 12 | (part.FlipX ? 0x4000 : 0) | (part.FlipY ? 0x8000 : 0)));
            expected[index * 4] = unchecked((byte)x); expected[index * 4 + 1] = unchecked((byte)y);
            expected[index * 4 + 2] = (byte)tile; expected[index * 4 + 3] = (byte)(tile >> 8);
            high |= (byte)(((x >> 8 & 1) | (part.Size == 16 ? 2 : 0)) << (index * 2));
        }
        AssertEqual(expected.Length, actual.NextByteOffset, "body emits exactly the authored limb count");
        AssertTrue(expected.AsSpan().SequenceEqual(actual.LowTable[..expected.Length]), "body limb position/tile/palette/flip/order match authored art");
        AssertEqual(high, actual.HighTable[0], "body limb sizes and high X bits match authored art");
    }

    private static void AssertBodyBg2(SnesVram actual, EnemyBg2FrameDocument authored, string oamName)
    {
        var expected = new ushort[EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight];
        if (authored.Frames.TryGetValue(oamName.Replace("_oam_", "_bg2_", StringComparison.Ordinal), out var writes))
            foreach (EnemyBg2WriteDocument write in writes)
                for (int tile = 0; tile < write.Tiles.Length; tile++)
                    expected[write.Y * EnemyBg2FrameLayout.TilemapWidth + write.X + tile] = checked((ushort)write.Tiles[tile]);
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index], actual.ReadWord(EnemyBg2FrameLayout.VramBase + index), "body BG2 exact ordered runs and destinations");
    }

    /// <summary>Invokes import-only parsers for the 17 statically inventoried body roots.</summary>
    private static void VerifyMotherBrainBodyStockPresentation()
    {
        var source = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        MotherBrainBodyBg2FrameCatalog bg2 = MotherBrainBodyBg2FrameCatalog.Load(new MemoryStream(
            MotherBrainBodyBg2FrameFiles.Extract(source), writable: false));
        EnemyExtendedFrameCatalog oam = EnemyExtendedFrameCatalog.Load(new MemoryStream(
            EnemyExtendedFrameFiles.Extract(source), writable: false));
        int limbs = 0, sprites = 0, runs = 0, tiles = 0;
        foreach (EnemyExtendedFrameDefinition frame in MotherBrainBodyVisualDefinitions.Frames)
        {
            AssertTrue(oam.TryGet(frame.Bank, frame.Pointer, out var parts), frame.Name + " imports its OAM half");
            limbs += parts.Length;
            foreach (EnemyExtendedDrawComponent component in parts.Span) sprites += component.Parts.Length;
            if (!MotherBrainBodyVisualDefinitions.HasBg2(frame.Pointer)) continue;
            AssertTrue(bg2.TryGet(frame.Pointer, out var writes), frame.Name + " imports its BG2 half");
            runs += writes.Length;
            foreach (EnemyBg2TilemapWrite write in writes.Span) tiles += write.Tiles.Length;
        }
        Console.WriteLine($"PASS stock import: 17 Mother Brain roots, {limbs} OAM limbs/{sprites} sprites, " +
            $"16 BG2 frames/{runs} ordered runs/{tiles} tile words. Production runtime receives no cartridge source.");
        using var temporary = new MapCatalogTestDirectory();
        string stockPath = Path.Combine(temporary.Root, "stock"), overridePath = Path.Combine(temporary.Root, "overrides");
        EnemyTileArtworkFiles.Extract(source, stockPath, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(stockPath);
        EnemyTileArtworkCatalog installed = EnemyTileArtworkFiles.Load(stockPath, null);
        AssertEqual(bg2.ContentIdentity, installed.MotherBrainBodyBg2Frames!.ContentIdentity,
            "manifest loads the exact imported Mother Brain body catalog");
        AssertEqual(oam.ContentIdentity, installed.ExtendedFrames!.ContentIdentity,
            "manifest loads the complete current multipart catalog");
        string file = Path.Combine(stockPath, MotherBrainBodyVisualDefinitions.Bg2FileName);
        var author = new EnemyIdentityFixture();
        using var parsed = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(file));
        var document = System.Text.Json.JsonSerializer.Deserialize<EnemyBg2FrameDocument>(parsed,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase })!;
        EnemyBg2WriteDocument[] first = document.Frames.Values.First();
        first[0].Tiles[0] ^= 1;
        Directory.CreateDirectory(overridePath);
        string selectedFile = Path.Combine(overridePath, MotherBrainBodyVisualDefinitions.Bg2FileName);
        using (var json = author.Json(document)) File.WriteAllBytes(selectedFile, json.ToArray());
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockPath, overridePath);
        AssertTrue(installed.ContentIdentity != edited.ContentIdentity, "selected Mother Brain BG2 edit changes installed bundle identity");
        AssertEqual(installed.ExtendedFrames.ContentIdentity, edited.ExtendedFrames!.ContentIdentity,
            "a BG2-only override does not alter limb compositions");
        using (var json = author.Json(document with { Version = document.Version + 1 }))
            File.WriteAllBytes(selectedFile, json.ToArray());
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockPath, overridePath),
            "malformed Mother Brain override fails at installation loading");
        using (var json = author.Json(document)) File.WriteAllBytes(file, json.ToArray());
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.ValidateStock(stockPath),
            "modified Mother Brain stock fails its manifest hash");
        Console.WriteLine("PASS Mother Brain installation: stock hashes, selected overrides/identity and malformed/tampered-data rejection.");
    }
}
