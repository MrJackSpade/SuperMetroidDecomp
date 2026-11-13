using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The palette-FX colour groups the room's own pictures show (#1512): its glows, lights, sand, waterfalls, statues and
/// escape flashes. The room's pictures are shaded with the records these groups last showed; which inks each group
/// recolours is the import's.
/// </summary>
internal static class PaletteFxRoomColourDefinitions
{
    /// <summary>The groups the room's pictures show.</summary>
    internal static ImmutableHashSet<PaletteFxColours> RoomColours { get; } =
    [
        PaletteFxColours.DraygonStatue,
        PaletteFxColours.KraidStatue,
        PaletteFxColours.WreckedShipGreenLights,
        PaletteFxColours.LandingSiteLightning,
        PaletteFxColours.BrinstarBlueSpores,
        PaletteFxColours.BrinstarRedGlow,
        PaletteFxColours.Beacon,
        PaletteFxColours.NorfairHeatForeground,
        PaletteFxColours.NorfairGreenForeground,
        PaletteFxColours.NorfairPurpleForeground,
        PaletteFxColours.NorfairDarkForeground,
        PaletteFxColours.MaridiaSandPits,
        PaletteFxColours.MaridiaSandFalls,
        PaletteFxColours.MaridiaBackgroundWaterfalls,
        PaletteFxColours.TourianArkanoidBlocksAndOrbs,
        PaletteFxColours.TourianBackground,
        PaletteFxColours.TourianLevel,
        PaletteFxColours.OldTourianLevel,
        PaletteFxColours.OldTourianRailings,
        PaletteFxColours.OldTourianPanels,
        PaletteFxColours.UpperCrateriaLevel,
        PaletteFxColours.CrateriaEscapeLightning,
        PaletteFxColours.ParlorEscapeSky,
    ];
}
