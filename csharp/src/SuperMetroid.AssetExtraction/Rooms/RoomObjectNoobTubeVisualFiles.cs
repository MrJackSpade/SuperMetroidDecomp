using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge-checked, strictly visual-only installed n00b-tube artwork.</summary>
public static class RoomObjectNoobTubeVisualFiles
{
    /// <summary>Editable n00b-tube visual JSON filename.</summary>
    public const string VisualFileName = "noob-tube.json";

    /// <summary>Verifies every native n00b-tube draw and creates stock JSON plus the shared provenance manifest.</summary>
    /// <param name="bus">Validated supported-cartridge source used only for extraction-time bank-$84 comparisons.</param>
    /// <param name="directory">Destination directory; output files must not already exist.</param>
    /// <param name="sourceCartridgeSha256">Source-cartridge identity recorded in the manifest.</param>
    public static void Extract(ISnesAddressSpace bus, string directory,
        string sourceCartridgeSha256) =>
        RoomObjectDoorVisualFileCodec.Extract(bus, directory, sourceCartridgeSha256,
            VisualFileName, "n00b-tube", All,
            VisualId);

    /// <summary>Validates the installed n00b-tube manifest's provenance and loads the installed n00b-tube JSON as the player left it, without cartridge access.</summary>
    /// <param name="directory">Installed directory containing the shared manifest and n00b-tube JSON.</param>
    /// <returns>The immutable n00b-tube visual catalog.</returns>
    public static InstalledRoomObjectNoobTubeVisuals Load(string directory) =>
        RoomObjectDoorVisualFileCodec.Load(directory,
            new RoomObjectVisualFamily<InstalledRoomObjectNoobTubeVisuals>(VisualFileName, "n00b-tube", entries => Create(entries.Select(entry =>
                new RoomObjectVisualEntry(entry.Id, entry.Blocks)))));

    /// <summary>Validates all seven n00b-tube layouts and builds the visuals from every frame's picture as installed, without changing power-bomb gating, debris, event timing, water physics, or physical draw geometry.</summary>
    /// <param name="entries">One visual-only entry per compiled layout, retaining the full flattened count of its single-block or twelve-block runs.</param>
    /// <exception cref="ArgumentNullException">The entry sequence is null.</exception>
    /// <exception cref="InvalidDataException">An entry is invalid, repeats an identity, changes a draw shape, or leaves compiled coverage incomplete.</exception>
    internal static InstalledRoomObjectNoobTubeVisuals Create(IEnumerable<RoomObjectVisualEntry> entries) =>
        new(RoomObjectFramePictureReader.Flattened(entries, "N00b tube", TryGetByVisualId, All.Count()));

    /// <summary>Enumerates tube states in intact-to-cleared and broken presentation order.</summary>
    private static IEnumerable<NoobTubeDraw> Draws()
    {
        yield return NoobTubeDraw.Intact;
        yield return NoobTubeDraw.Damaged;
        yield return NoobTubeDraw.Opened;
        yield return NoobTubeDraw.Cleared;
        yield return NoobTubeDraw.BrokenLate;
        yield return NoobTubeDraw.OpenedRows;
        yield return NoobTubeDraw.BrokenFull;
    }

    /// <summary>Builds the draw list of a tube layout from its computed cells.</summary>
    internal static bool TryGet(RoomObjectDraw request, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        bool found = NoobTubeObjectDraws.TryDescribe(request, out NoobTubeObjectDraws.Draw draw);
        list = found
            ? RoomObjectDrawListExport.Strips(request, draw.RunCount, draw.BlockCount, draw.BlockAt, draw.Start)
            : default;
        return found;
    }

    /// <summary>Converts a validated tube draw to its artwork catalog ID.</summary>
    internal static string VisualId(RoomObjectDraw request) =>
        VisualId(RoomObjectDraw.FrameOf<NoobTubeDraw>(request, "n00b-tube draw with a visual ID"));

    /// <summary>Names the typed intact, damaged, opened, cleared, or broken presentation state.</summary>
    internal static string VisualId(NoobTubeDraw frame) => frame switch
    {
        NoobTubeDraw.Intact => "intact",
        NoobTubeDraw.Damaged => "damaged",
        NoobTubeDraw.Opened => "opened",
        NoobTubeDraw.Cleared => "cleared",
        NoobTubeDraw.BrokenLate => "broken-late",
        NoobTubeDraw.OpenedRows => "opened-rows",
        NoobTubeDraw.BrokenFull => "broken-full",
        _ => throw new InvalidOperationException($"Undefined NoobTubeDraw {frame}."),
    };

    /// <summary>Finds and builds the tube layout corresponding to an artwork catalog ID.</summary>
    internal static bool TryGetByVisualId(string id, out RoomObjectShotBlockDrawDefinitions.DrawList list)
    {
        foreach (NoobTubeDraw frame in Draws())
            if (string.Equals(id, VisualId(frame), StringComparison.Ordinal)) return TryGet(RoomObjectDraw.Of(frame), out list);
        list = default;
        return false;
    }

    /// <summary>Builds each known tube layout's draw list.</summary>
    internal static IEnumerable<RoomObjectShotBlockDrawDefinitions.DrawList> All
    {
        get
        {
            foreach (NoobTubeDraw frame in Draws())
            {
                TryGet(RoomObjectDraw.Of(frame), out var list);
                yield return list;
            }
        }
    }
}
