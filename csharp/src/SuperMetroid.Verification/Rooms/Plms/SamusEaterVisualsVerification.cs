using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Rooms.Plms.PlmRunHeaderAssertions;
using SuperMetroid.Verification.Areas.Escape;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using static SuperMetroid.Verification.Rooms.Plms.SamusEaterPlmDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;

/// <summary>Checks Samus Eater PLM draw geometry, editable visual blocks and physical collision isolation against retail data.</summary>
internal static class SamusEaterVisualsVerification
{
    /// <summary>Extracts all eight native draws, checks their compiled geometry and live floor/ceiling redraws, and rejects collision-changing overrides.</summary>
    internal static void VerifySamusEaterVisuals()
    {
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)),
            "Samus Eater native oracle revision");
        Suite(nameof(VerifySamusEaterDrawGeometry), () => VerifySamusEaterDrawGeometry(rom));
        Suite(nameof(VerifySamusEaterDrawWords), () => VerifySamusEaterDrawWords(rom));
        using var testRootScratch = new TestScratchDirectory("samus-eater-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmSamusEaterVisualFiles.Extract(rom,
                installation.RoomPlmSamusEaterVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmSamusEaterVisualFiles.ValidateStock(
                installation.RoomPlmSamusEaterVisualDirectory);
            VerifySamusEaterStockVisualMapping(rom, installation.LoadRoomPlmSamusEaterVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmSamusEaterVisualDirectory,
                RoomPlmSamusEaterVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted Samus Eater JSON is empty.");
            JsonArray entries = document.Required("entries").AsArray();
            JsonNode floor = entries.RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "floor-chew-2");
            JsonNode ceiling = entries.RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "ceiling-chew-2");
            floor.Required("blocks")[0] = PlmVisualJson.Appearance(0x058);
            ceiling.Required("blocks")[0] = PlmVisualJson.Appearance(0x059);
            Directory.CreateDirectory(
                installation.RoomPlmSamusEaterVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmSamusEaterVisualOverrideDirectory,
                RoomPlmSamusEaterVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            InstalledRoomPlmSamusEaterVisuals edited =
                installation.LoadRoomPlmSamusEaterVisuals();
            VerifySamusEaterLiveDraw(rom, edited,
                PlmDrawAddressCodec.Of(PlmDraw.Of(SamusEaterDraw.FloorChew2)), RoomTerrainBlock.Of(0x1a5, BlockMirroring.Horizontal, RoomCollisionType.Air), RoomBlockAppearance.Of(0x058));
            VerifySamusEaterLiveDraw(rom, edited,
                PlmDrawAddressCodec.Of(PlmDraw.Of(SamusEaterDraw.CeilingChew2)), RoomTerrainBlock.Of(0x1a5, BlockMirroring.Horizontal | BlockMirroring.Vertical, RoomCollisionType.Air), RoomBlockAppearance.Of(0x059));

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmSamusEaterVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual(RoomBlockAppearance.Of(0x058),
                RoomPlmSamusEaterVisualFiles.Load(refreshed,
                    installation.RoomPlmSamusEaterVisualOverrideDirectory)
                    .GetAppearance(PlmDraw.Of(SamusEaterDraw.FloorChew2), 0, 0),
                "Samus Eater override survives stock refresh");
            floor.Required("blocks")[0] = 0xf058;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmSamusEaterVisuals(),
                "Samus Eater override cannot change collision nibble");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => RoomPlmSamusEaterVisualFiles.ValidateStock(
                    installation.RoomPlmSamusEaterVisualDirectory),
                "tampered Samus Eater stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
        Console.WriteLine("Samus Eater visuals: eight exact native draw lists, editable live floor/ceiling art, physical isolation, stock integrity and repair pass.");
    }

    /// <summary>Matches each installed visual frame to its native draw-list words while checking clone and ID validation rules.</summary>
    private static void VerifySamusEaterStockVisualMapping(SuperMetroidAddressSpace rom,
        InstalledRoomPlmSamusEaterVisuals installed)
    {
        LookupEscapeGateStockFrame[] frames = [new LookupEscapeGateStockFrame(0x9e0d,"floor-idle"), new LookupEscapeGateStockFrame(0x9e45,"floor-chew-1"),
            new LookupEscapeGateStockFrame(0x9e61,"floor-chew-2"), new LookupEscapeGateStockFrame(0x9e7d,"floor-chew-3"), new LookupEscapeGateStockFrame(0x9e99,"ceiling-idle"),
            new LookupEscapeGateStockFrame(0x9ed1,"ceiling-chew-1"), new LookupEscapeGateStockFrame(0x9eed,"ceiling-chew-2"), new LookupEscapeGateStockFrame(0x9f09,"ceiling-chew-3")];
        var native = new Dictionary<ushort, RoomBlockAppearance[]>();
        var counts = new Dictionary<ushort, int[]>();
        foreach (var frame in frames)
        {
            var words = new List<ushort>();
            var runs = new List<int>();
            int cursor = 0x840000 | frame.Pointer;
            while (true)
            {
                int count = ReadSamusEaterPlmWord(rom, cursor) & 0x7fff;
                runs.Add(count);
                for (int block = 0; block < count; block++)
                    words.Add(RoomLevelWordCodec.DecodeAppearance(ReadSamusEaterPlmWord(rom, cursor + 2 + block * 2)));
                cursor += 2 + count * 2;
                ushort offset = ReadSamusEaterPlmWord(rom, cursor);
                cursor += 2;
                if (offset == 0) break;
            }
            native.Add(frame.Pointer, words.ToArray());
            counts.Add(frame.Pointer, runs.ToArray());
        }
        var entries = frames.Select(frame => new RoomPlmVisualEntry(frame.Id, native[frame.Pointer].ToArray())).ToArray();
        var stock = InstalledRoomPlmSamusEaterVisuals.Stock();
        var imported = new InstalledRoomPlmSamusEaterVisuals(entries.Reverse());
        string Hash(Dictionary<ushort, RoomBlockAppearance[]> words) => SuperMetroid.Core.Assets.SelectedPresentationHash.FromAppearanceFrames(
            nameof(InstalledRoomPlmSamusEaterVisuals), words.ToDictionary(pair => SamusEaterPlmDraws.VisualId(NativePlmDraws.At<SamusEaterDraw>(pair.Key)), pair => pair.Value));
        foreach (var catalog in new[] {stock, imported, installed})
        {
            AssertEqual(Hash(native), catalog.ContentIdentity, "plant native flattened stock hash");
            foreach (var frame in frames)
            {
                int index = 0;
                for (int run = 0; run < counts[frame.Pointer].Length; run++)
                {
                    int count = counts[frame.Pointer][run];
                    for (int block = 0; block < count; block++)
                        AssertEqual(native[frame.Pointer][index++], catalog.GetAppearance(NativePlmDraws.At<SamusEaterDraw>(frame.Pointer), run, block), "plant native stock appearance");
                    foreach (int bad in new[] {int.MinValue, -1, count, int.MaxValue})
                        AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<SamusEaterDraw>(frame.Pointer),run,bad), "plant stock block bounds");
                }
                foreach (int bad in new[] {int.MinValue, -1, counts[frame.Pointer].Length, int.MaxValue})
                    AssertThrows<ArgumentOutOfRangeException>(() => catalog.GetAppearance(NativePlmDraws.At<SamusEaterDraw>(frame.Pointer),bad,0), "plant stock run bounds");
            }
        }
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
            if (!native.ContainsKey((ushort)pointer))
                AssertThrows<InvalidDataException>(() => stock.GetAppearance(NativePlmDraws.At<SamusEaterDraw>((ushort)pointer),0,0), "plant stock rejects unknown pointer");
        entries[0].Blocks[7] = RoomBlockAppearance.Of(0x058, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        entries[6].Blocks[0] = RoomBlockAppearance.Of(0x059);
        var mixed = new InstalledRoomPlmSamusEaterVisuals(entries);
        var expected = frames.ToDictionary(frame => frame.Pointer, frame => entries.Single(entry => entry.Id == frame.Id).Blocks.ToArray());
        AssertEqual(Hash(expected), mixed.ContentIdentity, "plant mixed custom and stock hash");
        entries[0].Blocks[7] = RoomBlockAppearance.Of(0x05a);
        entries[1].Blocks[0] = RoomBlockAppearance.Of(0x05b);
        foreach (var frame in frames)
        {
            int index = 0;
            for (int run = 0; run < counts[frame.Pointer].Length; run++)
            for (int block = 0; block < counts[frame.Pointer][run]; block++)
                AssertEqual(expected[frame.Pointer][index++], mixed.GetAppearance(NativePlmDraws.At<SamusEaterDraw>(frame.Pointer),run,block), "plant custom cloning and calculated stock isolation");
        }
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmSamusEaterVisuals(entries[..7]), "plant missing frame rejected");
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmSamusEaterVisuals(entries.Append(entries[0])), "plant duplicate frame rejected");
        var badEntries = entries.ToArray();
        badEntries[0] = new("FLOOR-IDLE", entries[0].Blocks);
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmSamusEaterVisuals(badEntries), "plant IDs remain ordinal");
        badEntries[0] = new(entries[0].Id, new RoomBlockAppearance[7]);
        AssertThrows<InvalidDataException>(() => new InstalledRoomPlmSamusEaterVisuals(badEntries), "plant wrong shape rejected");
    }

    /// <summary>Checks the run sizes and signed offsets of each compiled Samus Eater draw.</summary>
    private static void VerifySamusEaterDrawGeometry(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterDrawField), () => VerifySamusEaterDrawField(rom, false));

    /// <summary>Checks every compiled physical level word against its cartridge draw list.</summary>
    private static void VerifySamusEaterDrawWords(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifySamusEaterDrawField), () => VerifySamusEaterDrawField(rom, true));

    /// <summary>Scans pointer ownership and compares either draw geometry or level words for all eight native lists.</summary>
    private static void VerifySamusEaterDrawField(SuperMetroidAddressSpace rom, bool words)
    {
        ushort[] pointers = [0x9e0d, 0x9e45, 0x9e61, 0x9e7d, 0x9e99, 0x9ed1, 0x9eed, 0x9f09];
        var exported = SamusEaterPlmDraws.All.ToArray();
        AssertEqual(pointers.Length, exported.Length, "plant draw export coverage");
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            ushort pointer = (ushort)raw;
            int frame = Array.IndexOf(pointers, pointer);
            AssertEqual(frame >= 0, SamusEaterPlmDraws.TryDescribe(NativePlmDraws.At<SamusEaterDraw>(pointer), out var draw),
                "plant draw pointer ownership includes only supported records");
            AssertEqual(frame >= 0, SamusEaterPlmDraws.TryGet(NativePlmDraws.At<SamusEaterDraw>(pointer), out var dto),
                "plant draw DTO pointer ownership");
            if (frame < 0)
            {
                AssertEqual(default(SamusEaterPlmDraws.Draw), draw, "missing plant descriptor");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), dto, "missing plant DTO");
                continue;
            }
            AssertEqual(pointer, PlmDrawAddressCodec.Of(exported[frame].Draw), "plant export native order");
            AssertEqual(3, dto.Runs.Length, "plant run count");
            int cursor = 0x840000 | pointer;
            for (int run = 0; run < 3; run++)
            {
                ushort count = ReadSamusEaterPlmWord(rom, cursor);
                var exportedRun = exported[frame].Runs.Span[run];
                var dtoRun = dto.Runs.Span[run];
                if (words)
                {
                    for (int block = 0; block < count; block++)
                    {
                        RoomTerrainBlock native = RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, cursor + 2 + block * 2));
                        AssertEqual(native, draw.BlockAt(run, block), "calculated plant physical word");
                        AssertEqual(native, dtoRun.Blocks.Span[block], "direct plant DTO word");
                        AssertEqual(native, exportedRun.Blocks.Span[block], "exported plant word");
                    }
                    foreach (int invalid in new[] { int.MinValue, -1, count, 256, int.MaxValue })
                        AssertThrows<ArgumentOutOfRangeException>(() => draw.BlockAt(run, invalid), "plant block bounds");
                }
                else
                {
                    AssertEqual(count, SamusEaterPlmDraws.Draw.Count(run), "plant native horizontal count");
                    AssertRunHeader(count, dtoRun, "plant DTO direction/count");
                    AssertRunHeader(count, exportedRun, "plant export direction/count");
                    ushort offset = ReadSamusEaterPlmWord(rom, cursor + 2 + count * 2);
                    sbyte x = unchecked((sbyte)offset), y = unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
                    AssertEqual(x, SamusEaterPlmDraws.Draw.NextX(run), "plant signed horizontal offset");
                    AssertEqual(y, draw.NextY(run), "plant signed vertical offset");
                    AssertEqual(x, dtoRun.Next.X, "plant DTO horizontal offset");
                    AssertEqual(y, dtoRun.Next.Y, "plant DTO vertical offset");
                    AssertEqual(x, exportedRun.Next.X, "plant export horizontal offset");
                    AssertEqual(y, exportedRun.Next.Y, "plant export vertical offset");
                }
                cursor += 4 + count * 2;
            }
            foreach (int invalid in new[] { int.MinValue, -1, 3, 256, int.MaxValue })
            {
                AssertThrows<ArgumentOutOfRangeException>(() => SamusEaterPlmDraws.Draw.Count(invalid), "plant run count bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => SamusEaterPlmDraws.Draw.NextX(invalid), "plant offset X bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => draw.NextY(invalid), "plant offset Y bounds");
                AssertThrows<ArgumentOutOfRangeException>(() => draw.BlockAt(invalid, 0), "plant word run bounds");
            }
        }
    }

    /// <summary>Draws an edited PLM into a room and checks that the rendered block changes while native collision words and placement remain intact.</summary>
    private static void VerifySamusEaterLiveDraw(
        SuperMetroidAddressSpace rom, InstalledRoomPlmSamusEaterVisuals visuals, ushort pointer,
        RoomTerrainBlock physicalWord, RoomBlockAppearance visualWord)
    {
        const int width = 16;
        const int height = 16;
        var level = CreateRoom(new(width, height), new RoomTerrainBlock[width * height],
            new RoomBlockBehavior[width * height]);
        var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmSamusEaterVisuals = visuals });
        int origin = 8 * width + 8;
        plms.Drawing.Draw(level, origin, NativePlmDraws.At<SamusEaterDraw>(pointer));
        AssertEqual(physicalWord,
            level.GetCollisionBlockByIndex(origin).Terrain,
            "Samus Eater edited draw retains native collision word");
        AssertEqual(visualWord, level.DisplayedAppearance(origin),
            "Samus Eater edited block reaches the real redraw path");
        int nativeCursor = 0x840000 | pointer;
        int expectedX = 8, expectedY = 8;
        for (int run = 0; run < 3; run++)
        {
            int count = ReadSamusEaterPlmWord(rom, nativeCursor);
            for (int block = 0; block < count; block++)
                AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterPlmWord(rom, nativeCursor + 2 + block * 2)),
                    level.GetCollisionBlockByIndex(expectedY * width + expectedX + block).Terrain,
                    "plant live placement follows native signed run offsets");
            ushort offset = ReadSamusEaterPlmWord(rom, nativeCursor + 2 + count * 2);
            expectedX = 8 + unchecked((sbyte)offset);
            expectedY = 8 + unchecked((sbyte)(LittleEndianBytes.HighByte(offset)));
            nativeCursor += 4 + count * 2;
        }
    }

    /// <summary>Runs the Samus Eater visual and physical draw checks from the verification command.</summary>
    [VerificationCommand("--samus-eater-visuals")]
    internal static int SamusEaterVisualsCommand()
    {
        Suite(nameof(VerifySamusEaterVisuals), () => VerifySamusEaterVisuals());
        return 0;
    }
}
