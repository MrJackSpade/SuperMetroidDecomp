using System.Collections.Immutable;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;
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
    private static readonly ImmutableDictionary<ColourEffectColours, int> FirstInks = new Dictionary<ColourEffectColours, int>
    {
        [ColourEffectColours.DraygonStatue] = 96,
        [ColourEffectColours.KraidStatue] = 112,
        [ColourEffectColours.WreckedShipGreenLights] = 76,
        [ColourEffectColours.LandingSiteLightning] = 84,
        [ColourEffectColours.BrinstarBlueSpores] = 113,
        [ColourEffectColours.BrinstarRedGlow] = 100,
        [ColourEffectColours.Beacon] = 113,
        [ColourEffectColours.NorfairHeatForeground] = 53,
        [ColourEffectColours.NorfairGreenForeground] = 65,
        [ColourEffectColours.NorfairPurpleForeground] = 81,
        [ColourEffectColours.NorfairDarkForeground] = 97,
        [ColourEffectColours.MaridiaSandPits] = 36,
        [ColourEffectColours.MaridiaSandFalls] = 40,
        [ColourEffectColours.MaridiaBackgroundWaterfalls] = 52,
        [ColourEffectColours.TourianArkanoidBlocksAndOrbs] = 116,
        [ColourEffectColours.TourianBackground] = 56,
        [ColourEffectColours.TourianLevel] = 84,
        [ColourEffectColours.OldTourianLevel] = 81,
        [ColourEffectColours.OldTourianRailings] = 105,
        [ColourEffectColours.OldTourianPanels] = 85,
        [ColourEffectColours.UpperCrateriaLevel] = 65,
        [ColourEffectColours.CrateriaEscapeLightning] = 81,
        [ColourEffectColours.ParlorEscapeSky] = 87,
    }.ToImmutableDictionary();

    /// <summary>The objects Tourian's statue room spawns as each boss's statue greys out, which no FX record names.</summary>
    private static readonly ImmutableArray<ColourEffectId> StatueGreyOuts =
    [
        ColourEffectId.GreyOutTourianStatueDraygon, ColourEffectId.GreyOutTourianStatueKraid,
        ColourEffectId.GreyOutTourianStatueRidley, ColourEffectId.GreyOutTourianStatuePhantoon,
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
        var objects = new HashSet<ColourEffectId>();
        foreach (SelectedRoom room in rooms)
        {
            foreach (RoomFxRecordDefinition record in room.State.Fx?.Records ?? [])
                objects.UnionWith(record.PaletteEffects);
            if (room.AreaIndex == AreaId.Tourian)
                objects.UnionWith(StatueGreyOuts);
        }
        return [.. PaletteFxRecordShades.ShownBy(objects).Where(entry => FirstInks.ContainsKey(entry.Colours))
            .Select(entry => Shade(entry.Colours, entry.Frame, colours))];
    }

    /// <summary>The first ink of a room picture the room's colour group <paramref name="group"/> recolours.</summary>
    /// <exception cref="InvalidDataException">The group recolours no room picture.</exception>
    internal static int FirstInk(ColourEffectColours group) => FirstInks.TryGetValue(group, out int ink) ? ink
        : throw new InvalidDataException($"Palette-FX colours {group} recolour no room picture.");

    /// <summary>The shade of the room's <paramref name="group"/> showing <paramref name="frame"/>, coloured from <paramref name="colours"/>.</summary>
    /// <exception cref="InvalidDataException">A colour is not installed, or the frame's record writes past its group's colours.</exception>
    internal static SheetShade Shade(ColourEffectColours group, ColourEffectFrame frame, IPaletteFxColorSource colours)
    {
        int first = FirstInk(group);
        return PaletteFxRecordShades.Shade(group, frame, new InkRange(first, (first / GroupInks + 1) * GroupInks), colours);
    }
}
