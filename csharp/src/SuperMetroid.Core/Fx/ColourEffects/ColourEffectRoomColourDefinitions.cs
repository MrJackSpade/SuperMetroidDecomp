using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// The colour effects' colour groups the room's own pictures show (#1512): its glows, lights, sand, waterfalls, statues and
/// escape flashes. The room's pictures are shaded with the records these groups last showed; which inks each group
/// recolours is the import's.
/// </summary>
internal static class ColourEffectRoomColourDefinitions
{
    /// <summary>The groups the room's pictures show.</summary>
    internal static ImmutableHashSet<ColourEffectColours> RoomColours { get; } =
    [
        ColourEffectColours.DraygonStatue,
        ColourEffectColours.KraidStatue,
        ColourEffectColours.WreckedShipGreenLights,
        ColourEffectColours.LandingSiteLightning,
        ColourEffectColours.BrinstarBlueSpores,
        ColourEffectColours.BrinstarRedGlow,
        ColourEffectColours.Beacon,
        ColourEffectColours.NorfairHeatForeground,
        ColourEffectColours.NorfairGreenForeground,
        ColourEffectColours.NorfairPurpleForeground,
        ColourEffectColours.NorfairDarkForeground,
        ColourEffectColours.MaridiaSandPits,
        ColourEffectColours.MaridiaSandFalls,
        ColourEffectColours.MaridiaBackgroundWaterfalls,
        ColourEffectColours.TourianArkanoidBlocksAndOrbs,
        ColourEffectColours.TourianBackground,
        ColourEffectColours.TourianLevel,
        ColourEffectColours.OldTourianLevel,
        ColourEffectColours.OldTourianRailings,
        ColourEffectColours.OldTourianPanels,
        ColourEffectColours.UpperCrateriaLevel,
        ColourEffectColours.CrateriaEscapeLightning,
        ColourEffectColours.ParlorEscapeSky,
    ];
}
