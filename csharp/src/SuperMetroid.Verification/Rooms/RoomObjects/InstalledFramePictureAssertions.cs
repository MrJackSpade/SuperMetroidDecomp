using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Verification.Areas.Escape;
using static SuperMetroid.Verification.Rooms.RoomObjects.SamusEaterObjectDefinitionsVerification;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Checks a room object's installed frame pictures (#1557) against the cartridge's draw lists, which are walked here
/// independently of the import: each run's header gives its length and direction, and the word after it where the next
/// run starts.
/// </summary>
internal static class InstalledFramePictureAssertions
{
    /// <summary>The picture the cartridge's draw list at <paramref name="pointer"/> draws, and how many cells it writes.</summary>
    internal static Dictionary<Offset16<RoomBlocks>, RoomBlockAppearance> NativePicture(CartridgeImportAddressSpace rom, int pointer, out int written) =>
        NativeTerrain(rom, pointer, out written).ToDictionary(cell => cell.Key, cell => cell.Value.Appearance);

    /// <summary>
    /// The terrain the cartridge's draw list at <paramref name="pointer"/> leaves at each cell from the object's origin,
    /// and how many cells it writes; a cell written twice keeps its last write.
    /// </summary>
    internal static Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock> NativeTerrain(CartridgeImportAddressSpace rom, int pointer, out int written)
    {
        var cells = new Dictionary<Offset16<RoomBlocks>, RoomTerrainBlock>();
        Offset16<RoomBlocks> start = Offset16<RoomBlocks>.Zero;
        int cursor = pointer;
        written = 0;
        for (int run = 0; ; run++)
        {
            AssertTrue(run < 8, "a native draw list ends within eight runs");
            int header = ReadSamusEaterObjectWord(rom, 0x840000 | cursor);
            int count = header & 0x7fff;
            bool vertical = (header & 0x8000) != 0;
            cursor += 2;
            for (int cell = 0; cell < count; cell++, cursor += 2)
                cells[start + (vertical ? new Offset16<RoomBlocks>(0, cell) : new Offset16<RoomBlocks>(cell, 0))] =
                    RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | cursor));
            written += count;
            int x = unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | cursor));
            int y = unchecked((sbyte)rom.ReadCartridgeByte(0x840000 | (cursor + 1)));
            cursor += 2;
            if (x == 0 && y == 0) return cells;
            start = new Offset16<RoomBlocks>(x, y);
        }
    }

    /// <summary>
    /// The number of cells the geometry of every frame of <paramref name="family"/> writes, a cell written twice counted
    /// twice: the total the cartridge's draw lists must write.
    /// </summary>
    internal static int WrittenCells<TFrame>(RoomObjectFrameFamily<TFrame> family) where TFrame : notnull
    {
        int cells = 0;
        foreach (RoomObjectFrameLayout layout in family.Layouts.Values)
            foreach (RoomObjectShotBlockDrawDefinitions.Run run in layout.Runs.Span)
                cells += run.Count;
        return cells;
    }

    /// <summary>
    /// The frame of <paramref name="family"/> published as <paramref name="frame"/>'s name, which the game must draw from
    /// <paramref name="frame"/>'s cartridge address: the oracle's address and name pin the frame's identity.
    /// </summary>
    internal static TFrame Identity<TFrame>(RoomObjectFrameFamily<TFrame> family, LookupEscapeGateStockFrame frame) where TFrame : notnull
    {
        TFrame[] named = family.Ids.Where(pair => pair.Value == frame.Id).Select(pair => pair.Key).ToArray();
        AssertEqual(1, named.Length, family.Name + " publishes frame " + frame.Id + " once");
        AssertEqual(frame.Pointer, RoomObjectDrawAddressCodec.Of(family.Layouts[named[0]].Draw), family.Name + " frame " + frame.Id + "'s address");
        return named[0];
    }

    /// <summary>
    /// Requires the geometry the game draws each of <paramref name="frames"/> by, as the import pictures it, to be the
    /// cartridge's: the frame's address and published name, each run's header, the start of the run after it and, when
    /// <paramref name="collision"/> is set, how each cell collides.
    /// </summary>
    internal static void AssertLayoutsAsNative<TFrame>(CartridgeImportAddressSpace rom, RoomObjectFrameFamily<TFrame> family,
        IReadOnlyList<LookupEscapeGateStockFrame> frames, bool collision) where TFrame : notnull
    {
        AssertEqual(family.Ids.Count, family.Layouts.Count, family.Name + " names every frame it lays out");
        if (typeof(TFrame).IsEnum)
            AssertEqual(Enum.GetValues(typeof(TFrame)).Length, family.Layouts.Count, family.Name + " lays out every frame");
        foreach (LookupEscapeGateStockFrame frame in frames)
        {
            TFrame identity = Identity(family, frame);
            RoomObjectFrameLayout layout = family.Layouts[identity];
            int cursor = frame.Pointer;
            for (int run = 0; run < layout.Runs.Length; run++)
            {
                RoomObjectShotBlockDrawDefinitions.Run part = layout.Runs.Span[run];
                AssertEqual(RoomObjectRunHeaderCodec.Encode(part.Direction, part.Count), ReadSamusEaterObjectWord(rom, 0x840000 | cursor),
                    family.Name + " frame " + frame.Id + " run header");
                if (collision)
                    for (int cell = 0; cell < part.Count; cell++)
                        AssertEqual(RoomLevelWordCodec.Decode(ReadSamusEaterObjectWord(rom, 0x840000 | (cursor + 2 + cell * 2))).Collision,
                            part.Collisions.Span[cell], family.Name + " frame " + frame.Id + " cell collision");
                AssertEqual(RoomObjectRunContinuationCodec.Encode(layout, run), ReadSamusEaterObjectWord(rom, 0x840000 | (cursor + 2 + part.Count * 2)),
                    family.Name + " frame " + frame.Id + " run continuation");
                cursor += 4 + part.Count * 2;
            }
        }
    }

    /// <summary>
    /// Requires every frame of <paramref name="family"/> to be pictured exactly as the cartridge draws it: the same cells,
    /// each showing the cartridge's block and mirroring, under the frame's published name, and nothing outside them.
    /// </summary>
    /// <param name="rom">The cartridge.</param>
    /// <param name="images">The installed pictures.</param>
    /// <param name="family">The object's frames as the import pictures them.</param>
    /// <param name="frames">Each frame's draw-list address and published name.</param>
    /// <param name="writtenCells">The number of cells the frames' draw lists write in all.</param>
    internal static void AssertPicturedAsNative<TFrame>(CartridgeImportAddressSpace rom, RoomObjectFrameImages<TFrame> images,
        RoomObjectFrameFamily<TFrame> family, IReadOnlyList<LookupEscapeGateStockFrame> frames, int writtenCells) where TFrame : notnull
    {
        AssertEqual(frames.Count, images.All.Count, family.Name + " pictures every frame");
        AssertEqual(frames.Count, family.Layouts.Count, family.Name + " names every frame's address");
        int written = 0;
        foreach (LookupEscapeGateStockFrame frame in frames)
        {
            TFrame identity = Identity(family, frame);
            Dictionary<Offset16<RoomBlocks>, RoomBlockAppearance> native = NativePicture(rom, frame.Pointer, out int frameWritten);
            written += frameWritten;
            RoomObjectFrameImage image = images.Image(identity);
            AssertEqual(native.Count, image.Cells.Count, family.Name + " frame " + frame.Id + " covers the cartridge's cells");
            foreach (KeyValuePair<Offset16<RoomBlocks>, RoomBlockAppearance> cell in native)
                AssertEqual(cell.Value, image.At(cell.Key), family.Name + " frame " + frame.Id + " shows the cartridge's block");
            AssertThrows<InvalidDataException>(() => image.At(new Offset16<RoomBlocks>(64, 64)), family.Name + " frame " + frame.Id + " covers no other cell");
            AssertEqual(native.Count, family.Layouts[identity].Cells().Count, family.Name + " frame " + frame.Id + " geometry covers the cartridge's cells");
            foreach (Offset16<RoomBlocks> cell in native.Keys)
                AssertTrue(family.Layouts[identity].Cells().Contains(cell), family.Name + " frame " + frame.Id + " geometry covers each cartridge cell");
        }
        AssertEqual(writtenCells, written, family.Name + " native cell count");
    }

    /// <summary>
    /// Requires <paramref name="shown"/>, the picture a frame's second identity shows, to be what the cartridge's draw list
    /// at <paramref name="pointer"/> draws: the same cells, each showing the cartridge's block and mirroring.
    /// </summary>
    internal static void AssertAliasPicturedAsNative(CartridgeImportAddressSpace rom, RoomObjectFrameImage shown, int pointer, string label)
    {
        Dictionary<Offset16<RoomBlocks>, RoomBlockAppearance> native = NativePicture(rom, pointer, out _);
        AssertEqual(native.Count, shown.Cells.Count, label + " covers the cartridge's cells");
        foreach (KeyValuePair<Offset16<RoomBlocks>, RoomBlockAppearance> cell in native)
            AssertEqual(cell.Value, shown.At(cell.Key), label + " shows the cartridge's block");
    }

    /// <summary>
    /// Requires the importer's pictures of every frame, read from <paramref name="rom"/>, to equal the installed ones, and
    /// the installed file to be rejected when a frame is missing, repeated, misnamed or misshapen.
    /// </summary>
    /// <param name="rom">The cartridge the importer pictures the frames from.</param>
    /// <param name="installed">The family's installed pictures.</param>
    /// <param name="family">The family's frames and installed file.</param>
    /// <param name="installedDirectory">The repository installation's directory of the family's pictures.</param>
    internal static void AssertImportAndRejections<TFrame>(CartridgeImportAddressSpace rom, RoomObjectFrameImages<TFrame> installed,
        RoomObjectFrameFamily<TFrame> family, string installedDirectory) where TFrame : notnull
    {
        foreach (TFrame frame in family.Layouts.Keys)
        {
            RoomObjectFrameImage imported = RoomObjectFrameImageFiles.Picture(rom, family.Name, family.Layouts[frame]);
            RoomObjectFrameImage image = installed.Image(frame);
            AssertEqual(image.Cells.Count, imported.Cells.Count, family.Name + " import covers the installed cells");
            foreach (KeyValuePair<Offset16<RoomBlocks>, RoomBlockAppearance> cell in imported.Cells)
                AssertEqual(cell.Value, image.At(cell.Key), family.Name + " import pictures what is installed");
        }
        string directory = Path.Combine(Path.GetTempPath(), "sm-1557-" + Guid.NewGuid().ToString("N"));
        try
        {
            // The edited copies are written beside each other in a scratch directory; the stock file is the installation's.
            Directory.CreateDirectory(directory);
            string stock = File.ReadAllText(Path.Combine(installedDirectory, family.FileName));
            string path = Path.Combine(directory, family.FileName);
            AssertEqual(installed.All.Count, RoomObjectFrameImageFiles.Load(installedDirectory, family).All.Count, family.Name + " installed file loads");
            var document = System.Text.Json.Nodes.JsonNode.Parse(stock)!.AsObject();
            var frames = document["frames"]!.AsArray();

            void Rejected(string label, Action<System.Text.Json.Nodes.JsonArray> edit)
            {
                var copy = System.Text.Json.Nodes.JsonNode.Parse(stock)!.AsObject();
                edit(copy["frames"]!.AsArray());
                File.WriteAllText(path, copy.ToJsonString());
                AssertThrows<InvalidDataException>(() => RoomObjectFrameImageFiles.Load(directory, family), family.Name + " rejects " + label);
            }

            Rejected("a missing frame", array => array.RemoveAt(0));
            Rejected("a repeated frame", array => array.Add(System.Text.Json.Nodes.JsonNode.Parse(array[0]!.ToJsonString())));
            Rejected("an unknown frame name", array => array[0]!["id"] = array[0]!["id"]!.GetValue<string>().ToUpperInvariant());
            Rejected("a missing cell", array => array[0]!["cells"]!.AsArray().RemoveAt(0));
            Rejected("a cell outside the frame", array => array[0]!["cells"]![0]!["x"] = 64);
            Rejected("an invalid block", array => array[0]!["cells"]![0]!["block"] = RoomBlockAppearance.LastBlock + 1);
            AssertEqual(frames.Count, installed.All.Count, family.Name + " extracted file names every frame");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }
}
