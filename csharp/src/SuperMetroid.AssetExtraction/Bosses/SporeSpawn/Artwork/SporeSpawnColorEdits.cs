using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Compares Spore Spawn's supplied spore, health and death colours with the shared band and calculated fades and keeps
/// the endpoints and only the samples that cannot be reconstructed from them, the selection the game resolves from.
/// </summary>
internal static class SporeSpawnColorEdits
{
    /// <summary>Inks 1 through 8 remain painted separately in each intermediate health band.</summary>
    private const int LastPaintedColor = 8;

    /// <summary>Selects the palette endpoints and the supplied samples that differ from their calculation.</summary>
    internal static SporeSpawnColorSelection Select(RgbColor[] spores, RgbColor[][] health,
        RgbColor[][] deathSprite, RgbColor[][] deathLevel, RgbColor[][] deathBackground) =>
        new(SporeEdits(spores, health), HealthBands(health), DeathSpriteEdits(deathSprite, health),
            DeathLevelEdits(deathLevel), DeathBackgroundEdits(deathBackground));

    /// <summary>
    /// Splits four supplied health bands into the healthy and critical endpoints, the painted middle inks and the
    /// middle-band calculated inks whose supplied value differs from the interpolation.
    /// </summary>
    private static SporeSpawnHealthBands HealthBands(RgbColor[][] supplied)
    {
        int last = SporeSpawnColorDefinitions.HealthFrameCount - 1;
        var intermediatePaint = new RgbColor[last - 1][];
        for (int frame = 1; frame < last; frame++)
            intermediatePaint[frame - 1] = supplied[frame].AsSpan(1, LastPaintedColor).ToArray();
        var calculated = new SporeSpawnHealthBands(supplied[0], supplied[last], intermediatePaint, new Dictionary<ColourCell, RgbColor>());
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int frame = 1; frame < last; frame++)
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
            if (SporeSpawnHealthBands.IsCalculated(color) && supplied[frame][color] != calculated.Resolve(frame, color))
                edits.Add(new ColourCell(frame, color), supplied[frame][color]);
        return new SporeSpawnHealthBands(supplied[0], supplied[last], intermediatePaint, edits);
    }

    /// <summary>Keeps the spore colours that differ from the supplied healthy band.</summary>
    private static Dictionary<ColourCell, RgbColor> SporeEdits(RgbColor[] spores, RgbColor[][] health)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
            if (spores[color] != health[0][color]) edits[new ColourCell(0, color)] = spores[color];
        return edits;
    }

    /// <summary>Keeps the death sprite's final fade step, its first step where it differs from critical health, and edited middle samples.</summary>
    private static Dictionary<ColourCell, RgbColor> DeathSpriteEdits(RgbColor[][] deathSprite, RgbColor[][] health)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        int last = SporeSpawnColorDefinitions.DeathSpriteFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            if (deathSprite[0][color] != health[SporeSpawnColorDefinitions.HealthFrameCount - 1][color])
                edits[new ColourCell(0, color)] = deathSprite[0][color];
            edits[new ColourCell(last, color)] = deathSprite[last][color];
            for (int frame = 1; frame < last; frame++)
                if (deathSprite[frame][color] != SporeSpawnColors.CalculateDeathSpriteColor(deathSprite[0][color], deathSprite[last][color], frame))
                    edits[new ColourCell(frame, color)] = deathSprite[frame][color];
        }
        return edits;
    }

    /// <summary>Keeps the level fade's final step and every earlier colour that is not the calculated fade.</summary>
    private static Dictionary<ColourCell, RgbColor> DeathLevelEdits(RgbColor[][] deathLevel)
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

    /// <summary>Keeps the background fade's final step and every earlier colour that is not the calculated fade.</summary>
    private static Dictionary<ColourCell, RgbColor> DeathBackgroundEdits(RgbColor[][] deathBackground)
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
}
