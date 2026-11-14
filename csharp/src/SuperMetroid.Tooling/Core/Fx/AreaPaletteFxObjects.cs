using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Fx.RoomPaletteFxDefinitions;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Selects the palette-FX object an area's effect bit enables (<see cref="RoomPaletteFxDefinitions"/>). The import's
/// decode knowledge: the record codec resolves each record's bits through it, and the decoded record names the
/// objects (#1526).
/// </summary>
internal static class AreaPaletteFxObjects
{
    /// <summary>
    /// Selects the palette-FX object enabled by one area's effect bit, or null for Tourian's third bit, which selects a copy
    /// of Tourian's glow that no room's record sets and that is therefore no effect of the game.
    /// </summary>
    /// <remarks>Direct semantic area/bit dispatch for all eight areas and eight bits.
    /// Unused selections (including Ceres/debug) select the empty object.
    /// Validate area before bit, preserving both rejection domains and precedence.
    /// All 64 selections are independently verified; no selection matrix remains.</remarks>
    internal static ColourEffectId? GetAreaDefinition(int areaIndex, int bitIndex)
    {
        if ((uint)areaIndex >= AreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        if ((uint)bitIndex >= DefinitionsPerArea)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        // The eighth area (beyond the retail areas) selects no object for any bit.
        if (areaIndex >= SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count)
            return ColourEffectId.Nothing;
        return new AreaFxBit(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Decode(areaIndex, "Palette-FX area"), bitIndex) switch
        {
            (AreaId.Crateria, 0) => ColourEffectId.CrateriaLightning,
            (AreaId.Crateria, 1) => ColourEffectId.UpperCrateriaRedFlashing,
            (AreaId.Crateria, 2) => ColourEffectId.CrateriaEscapeYellowLightning,
            (AreaId.Crateria, 3) => ColourEffectId.OldTourianRedFlashing,
            (AreaId.Crateria, 4) => ColourEffectId.OldTourianBackgroundRailingsFlashYellow,
            (AreaId.Crateria, 5) => ColourEffectId.OldTourianBackgroundPanelsFlashYellow,
            (AreaId.Crateria, 6) => ColourEffectId.ParlorEscapeSkyFlicker,
            (AreaId.Crateria, 7) => ColourEffectId.BeaconFlashing,
            (AreaId.Brinstar, 0) => ColourEffectId.BrinstarBlueBackgroundSpores,
            (AreaId.Brinstar, 1) => ColourEffectId.BrinstarRedBackgroundGlow,
            (AreaId.Brinstar, 2) => ColourEffectId.BeaconFlashing,
            (AreaId.Brinstar, 3) => ColourEffectId.SporeSpawnBlueBackgroundSpores,
            (AreaId.Norfair, 0) => ColourEffectId.SamusInHeat,
            (AreaId.Norfair, 1) => ColourEffectId.NorfairForegroundAndHeatPhase,
            (AreaId.Norfair, 2) => ColourEffectId.NorfairGreenForegroundHeatGlow,
            (AreaId.Norfair, 3) => ColourEffectId.NorfairPurpleForegroundHeatGlow,
            (AreaId.Norfair, 4) => ColourEffectId.NorfairDarkForegroundHeatGlow,
            (AreaId.WreckedShip, 0) => ColourEffectId.WreckedShipGreenLights,
            (AreaId.Maridia, 0) => ColourEffectId.MaridiaSandPits,
            (AreaId.Maridia, 1) => ColourEffectId.MaridiaSandFalls,
            (AreaId.Maridia, 2) => ColourEffectId.MaridiaBackgroundWaterfalls,
            (AreaId.Tourian, 0) => ColourEffectId.SamusInHeat,
            (AreaId.Tourian, 1) => ColourEffectId.TourianGlowingArkanoidBlocksAndRedOrbs,
            (AreaId.Tourian, 2) => null,
            (AreaId.Tourian, 3) => ColourEffectId.TourianShutterRedFlashing,
            (AreaId.Tourian, 4) => ColourEffectId.TourianBackgroundRedFlashing,
            (AreaId.Tourian, 5) => ColourEffectId.TourianGeneralLevelRedFlashing,
            (AreaId.Tourian, 6) => ColourEffectId.TourianRedFlashingArkanoidBlocksAndRedOrbs,
            _ => ColourEffectId.Nothing,
        };
    }

    /// <summary>Returns the object selected by one retail area/bit pair, or null for the glow copy no room sets.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not a retail area, or <paramref name="bit"/> is not 0 through 7.</exception>
    internal static ColourEffectId? Read(AreaId area, int bit) => GetAreaDefinition(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area), bit);
}
