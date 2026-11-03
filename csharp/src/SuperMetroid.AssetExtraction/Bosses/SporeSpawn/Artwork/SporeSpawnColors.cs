using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Spore Spawn's installed colours: how each spore, health, death-sprite, level and background colour resolves from its
/// edit or its calculated fade, and the identity of all of them.
/// </summary>
internal static class SporeSpawnColors
{
    /// <summary>A spore colour: its edit, or the healthy band's.</summary>
    internal static RgbColor Spore(SporeSpawnColorCatalog colors, int color)
    {
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return colors.selection.Spores.TryGetValue(new ColourCell(0, color), out RgbColor selected) ? selected : colors.selection.Health.Resolve(0, color);
    }

    /// <summary>A health band's colour.</summary>
    internal static RgbColor Health(SporeSpawnHealthBands health, int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.HealthFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return health.Resolve(frame, color);
    }

    /// <summary>A death-sprite phase's colour: its edit, or the calculated fade from critical health to the final fade step.</summary>
    internal static RgbColor DeathSprite(SporeSpawnColorCatalog colors, int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.DeathSpriteFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (colors.selection.DeathSprite.TryGetValue(new ColourCell(frame, color), out RgbColor selected))
            return selected;
        RgbColor first = colors.selection.DeathSprite.TryGetValue(new ColourCell(0, color), out RgbColor suppliedFirst)
            ? suppliedFirst : colors.selection.Health.Resolve(SporeSpawnColorDefinitions.HealthFrameCount - 1, color);
        return CalculateDeathSpriteColor(first, colors.selection.DeathSprite[new ColourCell((SporeSpawnColorDefinitions.DeathSpriteFrameCount - 1), color)], frame);
    }

    /// <summary>
    /// Palette_SporeSpawn_DeathSequence_0..7: interpolate each RGB5
    /// channel between the first and last fade steps, rounding to the nearest integer over
    /// seven intervals. The first endpoint repeats critical health band 3 (..E3F8);
    /// its sixteen colors remain required under health. Sixteen final colors also remain required.
    /// </summary>
    internal static RgbColor CalculateDeathSpriteColor(RgbColor first, RgbColor last, int frame)
    {
        int intervals = SporeSpawnColorDefinitions.DeathSpriteFrameCount - 1;
        return first.Zip(last, (_, start, end) => (start * (intervals - frame) + end * frame + intervals / 2) / intervals);
    }

    /// <summary>A level death phase's colour: its edit, or the calculated fade.</summary>
    internal static RgbColor DeathLevel(Dictionary<ColourCell, RgbColor> deathLevel, int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.DeathSceneFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (deathLevel.TryGetValue(new ColourCell(frame, color), out RgbColor selected))
            return selected;
        RgbColor finalColor = deathLevel[new ColourCell((SporeSpawnColorDefinitions.DeathSceneFrameCount - 1), color)];
        if (SporeSpawnDeathFade.TryLevelColor(finalColor, frame, color, out RgbColor calculated))
            return calculated;
        throw new InvalidOperationException("Required Spore Spawn level color is absent.");
    }

    /// <summary>A background death phase's colour: its edit, or the calculated fade.</summary>
    internal static RgbColor DeathBackground(Dictionary<ColourCell, RgbColor> deathBackground, int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.DeathSceneFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (deathBackground.TryGetValue(new ColourCell(frame, color), out RgbColor selected))
            return selected;
        RgbColor finalColor = deathBackground[new ColourCell((SporeSpawnColorDefinitions.DeathSceneFrameCount - 1), color)];
        return SporeSpawnDeathFade.BackgroundColor(finalColor, frame, color);
    }

    /// <summary>Resolves one death-script layer's packed RGB5 ink without writing a palette.</summary>
    internal static RgbColor Death(SporeSpawnColorCatalog colors, SporeSpawnDeathPaletteLayer layer, int frame, int color) =>
        layer switch
        {
            SporeSpawnDeathPaletteLayer.Sprite => colors.ResolveDeathSprite(frame, color),
            SporeSpawnDeathPaletteLayer.Level => colors.ResolveDeathLevel(frame, color),
            SporeSpawnDeathPaletteLayer.Background => colors.ResolveDeathBackground(frame, color),
            _ => throw new ArgumentOutOfRangeException(nameof(layer)),
        };

    /// <summary>Checks a palette phase before its packed frame/color key is used.</summary>
    private static int CheckFrame(int frame, int count) =>
        (uint)frame < count ? frame : throw new ArgumentOutOfRangeException(nameof(frame));
}
