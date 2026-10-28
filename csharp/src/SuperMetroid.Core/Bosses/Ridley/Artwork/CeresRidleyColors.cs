using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>
/// Ceres Ridley's installed colours at work: building them from their rows, composing and resolving the retreat
/// colours, and installing each family at its native CGRAM destination.
/// </summary>
internal static class CeresRidleyColors
{
    /// <summary>Builds the selected Ceres Ridley colours from their rows; a missing alarm or Baby family, which older overrides predate, is taken from <paramref name="stock"/>.</summary>
    /// <param name="rows">The selected colour rows.</param>
    /// <param name="stock">The current stock colours a missing family is inherited from.</param>
    internal static CeresRidleyColorCatalog Create(CeresRidleyColorRows rows, CeresRidleyColorCatalog? stock)
    {
        Ensure.NotNull(rows);
        var compiledBody = new CeresRidleyFade(CeresRidleyFadeKind.Body, rows.BodyFade);
        return new(new CeresRidleyColorParts
        {
            Start = rows.Start,
            EyeFade = new CeresRidleyFade(CeresRidleyFadeKind.Eyes, rows.EyeFade),
            BodyFade = compiledBody,
            Health = rows.Health,
            Alarm = rows.Alarm is null ? Ensure.NotNull(stock).alarm : new CeresRidleyAlarmColors(rows.Alarm),
            RetreatBg = rows.RetreatBg,
            RetreatShared = rows.RetreatShared,
            Baby = rows.Baby is null ? Ensure.NotNull(stock).baby : new CeresBabyPaint(rows.Baby, compiledBody),
        });
    }

    /// <summary>Keeps the supplied retreat BG colours that differ from their body, eye and neutral composition.</summary>
    internal static Dictionary<ColourCell, RgbColor> RetreatBgEdits(CeresRidleyColorParts parts)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < parts.RetreatBg.Length; color++)
            if (parts.RetreatBg[color] != CalculateRetreatBg(parts.BodyFade, parts.EyeFade, color))
                edits.Add(new ColourCell(0, color), parts.RetreatBg[color]);
        return edits;
    }

    /// <summary>
    /// The retreat copies the normal door/container paints into both drawing domains; keeps only independent supplied
    /// differences from the existing startup owner.
    /// </summary>
    internal static Dictionary<ColourCell, RgbColor> RetreatSharedEdits(RgbColor[] retreatShared, CeresRidleyStartColors start)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < retreatShared.Length; color++)
            if (retreatShared[color] != start.Resolve(color + 1))
                edits.Add(new ColourCell(0, color), retreatShared[color]);
        return edits;
    }

    /// <summary>Returns one BGR555 retreat BG color, index 0..14, preserving supplied edits over the composition.</summary>
    internal static RgbColor RetreatBg(CeresRidleyColorCatalog colors, int color)
    {
        if ((uint)color >= CeresRidleyPaletteDefinitions.RetreatBgColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return colors.retreatBg.TryGetValue(new ColourCell(0, color), out RgbColor edited)
            ? edited
            : CalculateRetreatBg(colors.bodyFade, colors.eyeFade, color);
    }

    /// <summary>Composes the native retreat background from body-fade row one, eye-fade row fourteen, and its neutral final color.</summary>
    private static RgbColor CalculateRetreatBg(CeresRidleyFade bodyFade, CeresRidleyFade eyeFade, int color) =>
        color < 11 ? bodyFade.Resolve(1, color)
            : color < 14 ? eyeFade.Resolve(14, color - 11) : CeresRidleyFloorPaint.RetreatNeutral;

    /// <summary>Returns one BGR555 retreat shared color, index 0..7; unedited values reuse startup colors 1..8.</summary>
    internal static RgbColor RetreatShared(CeresRidleyColorCatalog colors, int color)
    {
        if ((uint)color >= CeresRidleyPaletteDefinitions.RetreatSharedColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return colors.retreatShared.TryGetValue(new ColourCell(0, color), out RgbColor edited) ? edited : colors.start.Resolve(color + 1);
    }

    /// <summary>Copies the 32 startup colors into CGRAM entries 160..191.</summary>
    internal static void ApplyStart(CeresRidleyColorCatalog colors, GameplayColours colours)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.StartColorCount; color++)
            new RidleyColours(colours).CeresStart.Write(color, colors.start.Resolve(color));
    }

    /// <summary>Copies one eye-fade row, index 0..15, to CGRAM entries 252..254.</summary>
    internal static void ApplyEyeFade(CeresRidleyColorCatalog colors, GameplayColours colours, int row)
    {
        colors.eyeFade.ValidateRow(row);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.EyeFadeColorCount; color++)
            new RidleyColours(colours).CeresEyeFade.Write(color, colors.eyeFade.Resolve(row, color));
    }

    /// <summary>Copies one eleven-color body-fade row, index 0..15, to both BG CGRAM entries 145..155 and OBJ entries 241..251.</summary>
    internal static void ApplyBodyFade(CeresRidleyColorCatalog colors, GameplayColours colours, int row)
    {
        colors.bodyFade.ValidateRow(row);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.BodyFadeColorCount; color++)
            new RidleyColours(colours).CeresBodyFadeShadow.Write(color, colors.bodyFade.Resolve(row, color));
        for (int color = 0; color < CeresRidleyPaletteDefinitions.BodyFadeColorCount; color++)
            new RidleyColours(colours).CeresBodyFade.Write(color, colors.bodyFade.Resolve(row, color));
    }

    /// <summary>Copies one fourteen-color health row, index 0..2, to CGRAM entries 241..254.</summary>
    internal static void ApplyHealth(CeresRidleyColorCatalog colors, GameplayColours colours, int row)
    {
        _ = colors.health.Resolve(row, 0);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.HealthColorCount; color++)
            new RidleyColours(colours).Health.Write(color, colors.health.Resolve(row, color));
    }

    /// <summary>Copies one self-destruct EMERGENCY-text phase, index 0..15, to CGRAM entries 97..99.</summary>
    internal static void ApplyAlarm(CeresRidleyColorCatalog colors, GameplayColours colours, int row)
    {
        _ = CeresRidleyAlarmColors.SourceRow(row);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.AlarmColorCount; color++)
            new RidleyColours(colours).CeresAlarm.Write(color, colors.alarm.Resolve(row, color));
    }

    /// <summary>Installs the retreat's fifteen BG colors and eight shared colors.</summary>
    internal static void ApplyRetreat(CeresRidleyColorCatalog colors, GameplayColours colours)
    {
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.RetreatBgColorCount; color++)
            new RidleyColours(colours).CeresRetreatBackground.Write(color, colors.ResolveRetreatBg(color));
        for (int color = 0; color < CeresRidleyPaletteDefinitions.RetreatSharedColorCount; color++)
        {
            RgbColor selected = colors.ResolveRetreatShared(color);
            new RidleyColours(colours).CeresRetreatSharedBackground.Write(color, selected);
            new RidleyColours(colours).CeresRetreatSharedObject.Write(color, selected);
        }
    }

    /// <summary>Copies one private Baby/container draw shade, index 0..3, to OBJ palette-three entries 177..191.</summary>
    internal static void ApplyBaby(CeresRidleyColorCatalog colors, GameplayColours colours, int row)
    {
        _ = colors.baby.Resolve(row, 0);
        Ensure.NotNull(colours);
        for (int color = 0; color < CeresRidleyPaletteDefinitions.BabyColorCount; color++)
            new RidleyColours(colours).CeresBaby.Write(color, colors.baby.Resolve(row, color));
    }
}
