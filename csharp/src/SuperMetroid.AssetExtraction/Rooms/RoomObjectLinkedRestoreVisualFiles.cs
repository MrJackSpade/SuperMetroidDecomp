using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Installed, replaceable bomb and contact-crumble restore visuals, and the editable identities that name them.</summary>
public static class RoomObjectLinkedRestoreVisualFiles
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
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "Linked restore", All, VisualId);

    /// <summary>Validates the installed Linked restore manifest's provenance and loads the installed Linked restore JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and Linked restore JSON.</param>
    /// <returns>A ROM-independent appearance catalog; distinct bomb/contact collision classes and restoration programs remain unchanged.</returns>
    /// <exception cref="InvalidDataException">Manifest provenance, JSON version, coverage, identity, shape, or appearance are invalid.</exception>
    /// <exception cref="IOException">The installed manifest or Linked restore JSON cannot be read.</exception>
    public static InstalledRoomObjectLinkedRestoreVisuals Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<InstalledRoomObjectLinkedRestoreVisuals>(VisualFileName, "Linked restore", entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all six linked restoration layouts and builds the visuals from every frame's picture as installed, without changing linked parent/child collision or restoration programs.</summary>
    /// <param name="entries">One visual-only entry per compiled layout with two appearances per run and complete identity coverage.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static InstalledRoomObjectLinkedRestoreVisuals Create(IEnumerable<RoomObjectVisualEntry> entries) =>
        new(RoomObjectFramePictureReader.Flattened(entries, "Linked restoration", TryGetByVisualId, All.Count()));

    /// <summary>The three bomb-block and three contact-crumble restoration layouts, bomb blocks first.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All =>
        LinkedRestoreObjectFamily.Families.SelectMany(AllOf);

    /// <summary>The horizontal, vertical and square restoration layouts of <paramref name="family"/>, in that order.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> AllOf(LinkedRestoreObjectFamily family)
    {
        foreach (RoomObjectDraw candidate in RestorationDraws())
        {
            if (family.TryDescribe(candidate, out LinkedRestoreObjectFamily.Draw draw))
                yield return Export(candidate, draw);
        }
    }

    /// <summary>Builds the draw list of the restoration <paramref name="request"/>, whose shape is <paramref name="draw"/>.</summary>
    internal static RoomObjectShotBlockDrawDefinitions.DrawList Export(RoomObjectDraw request, LinkedRestoreObjectFamily.Draw draw) =>
        RoomObjectDrawListExport.Runs(request, draw.RunCount, static _ => 2, draw.BlockAt,
            _ => draw.Vertical ? RoomObjectRunDirection.Vertical : RoomObjectRunDirection.Horizontal,
            RoomObjectDrawListExport.Stacked);

    /// <summary>Maps a restoration draw to its bomb or crumble artwork key and rejects other draws.</summary>
    internal static string VisualId(RoomObjectDraw draw) => draw switch
    {
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Horizontal } => "bomb-horizontal",
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Vertical } => "bomb-vertical",
        RoomObjectFrameDraw<BombBlockRestoreDraw> { Frame: BombBlockRestoreDraw.Square } => "bomb-square",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Horizontal } => "crumble-horizontal",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Vertical } => "crumble-vertical",
        RoomObjectFrameDraw<ContactCrumbleRestoreDraw> { Frame: ContactCrumbleRestoreDraw.Square } => "crumble-square",
        _ => throw new InvalidDataException($"{draw} is not a linked restoration draw."),
    };

    /// <summary>Finds the compiled restoration draw whose stable visual key equals the supplied identifier.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list) =>
        RoomObjectDrawListExport.FindByVisualId(All, VisualId, id, out list);

    /// <summary>Every bomb-block and contact-crumble restoration draw, each family's in horizontal, vertical, square order.</summary>
    private static IEnumerable<RoomObjectDraw> RestorationDraws() =>
        Enum.GetValues<BombBlockRestoreDraw>().Select(draw => (RoomObjectDraw)RoomObjectDraw.Of(draw))
            .Concat(Enum.GetValues<ContactCrumbleRestoreDraw>().Select(draw => (RoomObjectDraw)RoomObjectDraw.Of(draw)));
}
