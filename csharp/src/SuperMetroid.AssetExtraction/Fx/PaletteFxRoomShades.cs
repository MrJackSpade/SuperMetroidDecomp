using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The shades of the room's palette-FX colour groups (#1512), which the import pre-colours a tileset's room pictures in:
/// each record a group its rooms run can show, its colours over the inks the group recolours. Which inks those are is the
/// import's: each group starts at one ink of a room picture and runs to the end of that ink's colour group, as
/// <c>PaletteFXObject_Handler</c> writes a record's colours from the object's colour pointer, skipping those it skips.
/// </summary>
internal static class PaletteFxRoomShades
{
    /// <summary>The colours of one of a room picture's colour groups.</summary>
    private const int GroupInks = 16;

    /// <summary>The first ink of a room picture each of the room's colour groups recolours.</summary>
    private static readonly ImmutableDictionary<PaletteFxColours, int> FirstInks = new Dictionary<PaletteFxColours, int>
    {
        [PaletteFxColours.DraygonStatue] = 96,
        [PaletteFxColours.KraidStatue] = 112,
        [PaletteFxColours.WreckedShipGreenLights] = 76,
        [PaletteFxColours.LandingSiteLightning] = 84,
        [PaletteFxColours.BrinstarBlueSpores] = 113,
        [PaletteFxColours.BrinstarRedGlow] = 100,
        [PaletteFxColours.Beacon] = 113,
        [PaletteFxColours.NorfairHeatForeground] = 53,
        [PaletteFxColours.NorfairGreenForeground] = 65,
        [PaletteFxColours.NorfairPurpleForeground] = 81,
        [PaletteFxColours.NorfairDarkForeground] = 97,
        [PaletteFxColours.MaridiaSandPits] = 36,
        [PaletteFxColours.MaridiaSandFalls] = 40,
        [PaletteFxColours.MaridiaBackgroundWaterfalls] = 52,
        [PaletteFxColours.TourianArkanoidBlocksAndOrbs] = 116,
        [PaletteFxColours.TourianBackground] = 56,
        [PaletteFxColours.TourianLevel] = 84,
        [PaletteFxColours.OldTourianLevel] = 81,
        [PaletteFxColours.OldTourianRailings] = 105,
        [PaletteFxColours.OldTourianPanels] = 85,
        [PaletteFxColours.UpperCrateriaLevel] = 65,
        [PaletteFxColours.CrateriaEscapeLightning] = 81,
        [PaletteFxColours.ParlorEscapeSky] = 87,
    }.ToImmutableDictionary();

    /// <summary>The objects Tourian's statue room spawns as each boss's statue greys out, which no FX record names.</summary>
    private static readonly ImmutableArray<PaletteFxObject> StatueGreyOuts =
    [
        PaletteFxObject.GreyOutTourianStatueDraygon, PaletteFxObject.GreyOutTourianStatueKraid,
        PaletteFxObject.GreyOutTourianStatueRidley, PaletteFxObject.GreyOutTourianStatuePhantoon,
    ];

    /// <summary>
    /// The shades of every record of the room's colour groups that <paramref name="rooms"/>' palette effects show, coloured
    /// from <paramref name="colours"/>.
    /// </summary>
    /// <param name="rooms">The rooms whose pictures are shaded, each with the state it loads.</param>
    /// <param name="colours">The records' installed colours.</param>
    /// <exception cref="InvalidDataException">A record has no installed colour, or writes past its group's colours.</exception>
    internal static IReadOnlyList<SheetShade> Of(IEnumerable<SelectedRoom> rooms, IPaletteFxColorSource colours)
    {
        Ensure.NotNull(rooms);
        Ensure.NotNull(colours);
        var objects = new HashSet<PaletteFxObject>();
        foreach (SelectedRoom room in rooms)
        {
            foreach (RoomEffectDefinition record in room.State.Fx?.Effects ?? [])
                objects.UnionWith(record.PaletteEffects);
            if (room.AreaIndex == AreaId.Tourian)
                objects.UnionWith(StatueGreyOuts);
        }
        return [.. PaletteFxRecordShades.ShownBy(objects).Where(entry => FirstInks.ContainsKey(entry.Colours))
            .Select(entry => Shade(entry.Colours, entry.Frame, colours))];
    }

    /// <summary>The first ink of a room picture the room's colour group <paramref name="group"/> recolours.</summary>
    /// <exception cref="InvalidDataException">The group recolours no room picture.</exception>
    internal static int FirstInk(PaletteFxColours group) => FirstInks.TryGetValue(group, out int ink) ? ink
        : throw new InvalidDataException($"Palette-FX colours {group} recolour no room picture.");

    /// <summary>The shade of the room's <paramref name="group"/> showing <paramref name="frame"/>, coloured from <paramref name="colours"/>.</summary>
    /// <exception cref="InvalidDataException">A colour is not installed, or the frame's record writes past its group's colours.</exception>
    internal static SheetShade Shade(PaletteFxColours group, PaletteFxFrame frame, IPaletteFxColorSource colours)
    {
        int first = FirstInk(group);
        return PaletteFxRecordShades.Shade(group, frame, new InkRange(first, (first / GroupInks + 1) * GroupInks), colours);
    }
}
