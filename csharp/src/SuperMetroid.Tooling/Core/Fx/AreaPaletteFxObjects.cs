using SuperMetroid.Core.Fx.PaletteFx;
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
    internal static PaletteFxObject? GetAreaDefinition(int areaIndex, int bitIndex)
    {
        if ((uint)areaIndex >= AreaCount)
            throw new ArgumentOutOfRangeException(nameof(areaIndex));
        if ((uint)bitIndex >= DefinitionsPerArea)
            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        // The eighth area (beyond the retail areas) selects no object for any bit.
        if (areaIndex >= SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count)
            return PaletteFxObject.Nothing;
        return new AreaFxBit(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Decode(areaIndex, "Palette-FX area"), bitIndex) switch
        {
            (AreaId.Crateria, 0) => PaletteFxObject.CrateriaLightning,
            (AreaId.Crateria, 1) => PaletteFxObject.UpperCrateriaRedFlashing,
            (AreaId.Crateria, 2) => PaletteFxObject.CrateriaEscapeYellowLightning,
            (AreaId.Crateria, 3) => PaletteFxObject.OldTourianRedFlashing,
            (AreaId.Crateria, 4) => PaletteFxObject.OldTourianBackgroundRailingsFlashYellow,
            (AreaId.Crateria, 5) => PaletteFxObject.OldTourianBackgroundPanelsFlashYellow,
            (AreaId.Crateria, 6) => PaletteFxObject.ParlorEscapeSkyFlicker,
            (AreaId.Crateria, 7) => PaletteFxObject.BeaconFlashing,
            (AreaId.Brinstar, 0) => PaletteFxObject.BrinstarBlueBackgroundSpores,
            (AreaId.Brinstar, 1) => PaletteFxObject.BrinstarRedBackgroundGlow,
            (AreaId.Brinstar, 2) => PaletteFxObject.BeaconFlashing,
            (AreaId.Brinstar, 3) => PaletteFxObject.SporeSpawnBlueBackgroundSpores,
            (AreaId.Norfair, 0) => PaletteFxObject.SamusInHeat,
            (AreaId.Norfair, 1) => PaletteFxObject.NorfairForegroundAndHeatPhase,
            (AreaId.Norfair, 2) => PaletteFxObject.NorfairGreenForegroundHeatGlow,
            (AreaId.Norfair, 3) => PaletteFxObject.NorfairPurpleForegroundHeatGlow,
            (AreaId.Norfair, 4) => PaletteFxObject.NorfairDarkForegroundHeatGlow,
            (AreaId.WreckedShip, 0) => PaletteFxObject.WreckedShipGreenLights,
            (AreaId.Maridia, 0) => PaletteFxObject.MaridiaSandPits,
            (AreaId.Maridia, 1) => PaletteFxObject.MaridiaSandFalls,
            (AreaId.Maridia, 2) => PaletteFxObject.MaridiaBackgroundWaterfalls,
            (AreaId.Tourian, 0) => PaletteFxObject.SamusInHeat,
            (AreaId.Tourian, 1) => PaletteFxObject.TourianGlowingArkanoidBlocksAndRedOrbs,
            (AreaId.Tourian, 2) => null,
            (AreaId.Tourian, 3) => PaletteFxObject.TourianShutterRedFlashing,
            (AreaId.Tourian, 4) => PaletteFxObject.TourianBackgroundRedFlashing,
            (AreaId.Tourian, 5) => PaletteFxObject.TourianGeneralLevelRedFlashing,
            (AreaId.Tourian, 6) => PaletteFxObject.TourianRedFlashingArkanoidBlocksAndRedOrbs,
            _ => PaletteFxObject.Nothing,
        };
    }

    /// <summary>Returns the object selected by one retail area/bit pair, or null for the glow copy no room sets.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not a retail area, or <paramref name="bit"/> is not 0 through 7.</exception>
    internal static PaletteFxObject? Read(AreaId area, int bit) => GetAreaDefinition(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area), bit);
}
