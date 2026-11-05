using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable bomb and contact-crumble restore visuals, and the editable identities that name them.</summary>
public static class RoomPlmLinkedRestoreVisualFiles
{
    /// <summary>Family-relative JSON filename for the six bomb/contact-crumble horizontal, vertical, and square restoration layouts.</summary>
    public const string VisualFileName = "linked-restores.json";

    /// <summary>Verifies the native bank-$84 linked-restore draw data and exports visible block/flip words while keeping bomb and contact-crumble mechanics compiled.</summary>
    /// <param name="bus">Cartridge source for all three bomb and three contact-crumble restoration draws.</param>
    /// <param name="directory">Family directory, created if absent; receives new <see cref="VisualFileName"/> and <c>manifest.json</c> files.</param>
    /// <param name="sourceCartridgeSha256">Nonblank provenance hash supplied by the importer, not recomputed here; stock loading requires the supported cartridge identity.</param>
    /// <remarks>Visual words are flattened in native run/cell order. Files use create-new semantics, and a partial JSON/manifest write is not rolled back.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException">The directory or source hash is null, empty, or whitespace.</exception>
    /// <exception cref="InvalidDataException">Native run shapes, terrain, or offsets do not match compiled definitions.</exception>
    /// <exception cref="IOException">An output exists or filesystem access fails.</exception>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomPlmDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Linked restore", All, VisualId);

    /// <summary>Validates the installed Linked restore manifest's provenance and loads the installed Linked restore JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Linked restore JSON.</param>
    /// <returns>A ROM-independent appearance catalog; distinct bomb/contact collision classes and restoration programs remain unchanged.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance, JSON version, coverage, identity, shape, or appearance are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or Linked restore JSON cannot be read.</exception>
    public static InstalledRoomPlmLinkedRestoreVisuals Load(string directory) =>
        RoomPlmDoorVisualFileCodec.Load(directory,
            new RoomPlmVisualFamily<InstalledRoomPlmLinkedRestoreVisuals>(VisualFileName, "Linked restore", All, VisualId, entries => Create(entries.Select(entry =>
                new RoomPlmVisualEntry(entry.Id, entry.Blocks))), (catalog, draw, run, block) => catalog.GetAppearance(draw, run, block)));

    /// <summary>Validates all six linked restoration layouts and builds the visuals from the appearances that differ from the compiled layouts, without changing linked parent/child collision or restoration programs.</summary>
    /// <param name="entries">One visual-only entry per compiled layout with two appearances per run and complete identity coverage.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static InstalledRoomPlmLinkedRestoreVisuals Create(IEnumerable<RoomPlmVisualEntry> entries) =>
        new(RoomPlmVisualOverrideReader.Flattened(entries, "Linked restoration", TryGetByVisualId, All.Count()));

    /// <summary>The three bomb-block and three contact-crumble restoration layouts, bomb blocks first.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> All =>
        LinkedRestorePlmFamily.Families.SelectMany(AllOf);

    /// <summary>The horizontal, vertical and square restoration layouts of <paramref name="family"/>, in that order.</summary>
    internal static IEnumerable<RoomPlmShotBlockDrawDefinitions.DrawList> AllOf(LinkedRestorePlmFamily family)
    {
        foreach (PlmDraw candidate in RestorationDraws())
        {
            if (family.TryDescribe(candidate, out LinkedRestorePlmFamily.Draw draw))
                yield return Export(candidate, draw);
        }
    }

    /// <summary>Builds the draw list of the restoration <paramref name="request"/>, whose shape is <paramref name="draw"/>.</summary>
    internal static RoomPlmShotBlockDrawDefinitions.DrawList Export(PlmDraw request, LinkedRestorePlmFamily.Draw draw) =>
        RoomPlmDrawListExport.Runs(request, draw.RunCount, static _ => 2, draw.BlockAt,
            _ => draw.Vertical ? PlmRunDirection.Vertical : PlmRunDirection.Horizontal,
            static _ => 0, run => run + 1 < draw.RunCount ? 1 : 0);

    /// <summary>Maps a restoration draw to its bomb or crumble artwork key and rejects other draws.</summary>
    internal static string VisualId(PlmDraw draw) => draw switch
    {
        PlmFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Horizontal } => "bomb-horizontal",
        PlmFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Vertical } => "bomb-vertical",
        PlmFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Square } => "bomb-square",
        PlmFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Horizontal } => "crumble-horizontal",
        PlmFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Vertical } => "crumble-vertical",
        PlmFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Square } => "crumble-square",
        _ => throw new InvalidDataException($"{draw} is not a linked restoration draw."),
    };

    /// <summary>Finds the compiled restoration draw whose stable visual key equals the supplied identifier.</summary>
    internal static bool TryGetByVisualId(string id, out RoomPlmShotBlockDrawDefinitions.DrawList list) =>
        RoomPlmDrawListExport.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Every bomb-block and contact-crumble restoration draw, each family's in horizontal, vertical, square order.</summary>
    private static IEnumerable<PlmDraw> RestorationDraws() =>
        Enum.GetValues<BombBlockRestoreDraw>().Select(draw => (PlmDraw)PlmDraw.Of(draw))
            .Concat(Enum.GetValues<ContactCrumbleRestoreDraw>().Select(draw => (PlmDraw)PlmDraw.Of(draw)));
}
