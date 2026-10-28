using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Spore Spawn's installed colours: which supplied spore, death-sprite, level and background colours are edits of
/// their calculated fades, how each resolves, and the identity of all of them.
/// </summary>
internal static class SporeSpawnColors
{
    /// <summary>Hashes the spores, then the health, death-sprite, death-level and death-background rows.</summary>
    internal static string ContentIdentity(SporeSpawnColorCatalog colors) =>
        SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
        {
            content.AppendColors("spores", Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame).Select(colors.ResolveSpore).ToArray());
            content.AppendColorFrames("health", Enumerable.Range(0, SporeSpawnColorDefinitions.HealthFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame)
                    .Select(color => colors.selection.Health.Resolve(frame, color)).ToArray()).ToArray());
            content.AppendColorFrames("deathSprite", Enumerable.Range(0, SporeSpawnColorDefinitions.DeathSpriteFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame)
                    .Select(color => colors.ResolveDeathSprite(frame, color)).ToArray()).ToArray());
            content.AppendColorFrames("deathLevel", Enumerable.Range(0, SporeSpawnColorDefinitions.DeathSceneFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame)
                    .Select(color => colors.ResolveDeathLevel(frame, color)).ToArray()).ToArray());
            content.AppendColorFrames("deathBackground", Enumerable.Range(0, SporeSpawnColorDefinitions.DeathSceneFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame)
                    .Select(color => colors.ResolveDeathBackground(frame, color)).ToArray()).ToArray());
        });

    /// <summary>Keeps the spore colours that differ from the supplied healthy boss row.</summary>
    internal static Dictionary<ColourCell, RgbColor> SporeEdits(RgbColor[] spores, RgbColor[][] health)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
            if (spores[color] != health[0][color]) edits[new ColourCell(0, color)] = spores[color];
        return edits;
    }

    /// <summary>Keeps the death sprite's final row, its first row where it differs from critical health, and edited middle samples.</summary>
    internal static Dictionary<ColourCell, RgbColor> DeathSpriteEdits(RgbColor[][] deathSprite, RgbColor[][] health)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        int last = SporeSpawnColorDefinitions.DeathSpriteFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            if (deathSprite[0][color] != health[SporeSpawnColorDefinitions.HealthFrameCount - 1][color])
                edits[new ColourCell(0, color)] = deathSprite[0][color];
            edits[new ColourCell(last, color)] = deathSprite[last][color];
            for (int frame = 1; frame < last; frame++)
                if (deathSprite[frame][color] != CalculateDeathSpriteColor(deathSprite[0][color], deathSprite[last][color], frame))
                    edits[new ColourCell(frame, color)] = deathSprite[frame][color];
        }
        return edits;
    }

    /// <summary>Keeps the level fade's final row and every earlier colour that is not the calculated fade.</summary>
    internal static Dictionary<ColourCell, RgbColor> DeathLevelEdits(RgbColor[][] deathLevel)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        int levelLast = SporeSpawnColorDefinitions.DeathSceneFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            edits[new ColourCell(levelLast, color)] = deathLevel[levelLast][color];
            for (int frame = 0; frame < levelLast; frame++)
                if (!SporeSpawnDeathFade.TryLevelColor(deathLevel[levelLast][color], frame, color, out RgbColor calculated)
                    || deathLevel[frame][color] != calculated)
                    edits[new ColourCell(frame, color)] = deathLevel[frame][color];
        }
        return edits;
    }

    /// <summary>Keeps the background fade's final row and every earlier colour that is not the calculated fade.</summary>
    internal static Dictionary<ColourCell, RgbColor> DeathBackgroundEdits(RgbColor[][] deathBackground)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        int backgroundLast = SporeSpawnColorDefinitions.DeathSceneFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            edits[new ColourCell(backgroundLast, color)] = deathBackground[backgroundLast][color];
            for (int frame = 0; frame < backgroundLast; frame++)
                if (deathBackground[frame][color] != SporeSpawnDeathFade.BackgroundColor(deathBackground[backgroundLast][color], frame, color))
                    edits[new ColourCell(frame, color)] = deathBackground[frame][color];
        }
        return edits;
    }

    /// <summary>A spore colour: its edit, or the healthy boss row's.</summary>
    internal static RgbColor Spore(SporeSpawnColorCatalog colors, int color)
    {
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return colors.selection.Spores.TryGetValue(new ColourCell(0, color), out RgbColor selected) ? selected : colors.selection.Health.Resolve(0, color);
    }

    /// <summary>A health row's colour.</summary>
    internal static RgbColor Health(SporeSpawnHealthRows health, int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.HealthFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return health.Resolve(frame, color);
    }

    /// <summary>A death-sprite phase's colour: its edit, or the calculated fade from critical health to the final row.</summary>
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
    /// $A5:E3F9..E4F8, Palette_SporeSpawn_DeathSequence_0..7: interpolate each RGB5
    /// channel between the first and last rows, rounding to the nearest integer over
    /// seven intervals. The first endpoint repeats critical-health row3 (..E3F8);
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
