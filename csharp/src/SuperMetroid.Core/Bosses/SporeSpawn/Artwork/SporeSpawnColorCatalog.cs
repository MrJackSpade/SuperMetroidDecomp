using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Editable Spore Spawn RGB5 images. Health thresholds, death timing, and target/current
/// palette ownership remain compiled cartridge behavior.
/// </summary>
public sealed class SporeSpawnColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("SporeSpawnColorCatalog-v1", content =>
        {
            content.AppendColors("spores", Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame).Select(ResolveSpore).ToArray());
            content.AppendColorFrames("health", Enumerable.Range(0, SporeSpawnColorDefinitions.HealthFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame)
                    .Select(color => health.Resolve(frame, color)).ToArray()).ToArray());
            content.AppendColorFrames("deathSprite", Enumerable.Range(0, SporeSpawnColorDefinitions.DeathSpriteFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame)
                    .Select(color => ResolveDeathSprite(frame, color)).ToArray()).ToArray());
            content.AppendColorFrames("deathLevel", Enumerable.Range(0, SporeSpawnColorDefinitions.DeathSceneFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame)
                    .Select(color => ResolveDeathLevel(frame, color)).ToArray()).ToArray());
            content.AppendColorFrames("deathBackground", Enumerable.Range(0, SporeSpawnColorDefinitions.DeathSceneFrameCount)
                .Select(frame => Enumerable.Range(0, SporeSpawnColorDefinitions.ColorsPerFrame)
                    .Select(color => ResolveDeathBackground(frame, color)).ToArray()).ToArray());
        });

    /// <summary>Only spore colors that differ from the healthy boss row; stock art shares that row.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> spores = new();
    /// <summary>Painted health endpoints and intermediate edits used by boss palette selection.</summary>
    internal readonly HealthRows health;
    /// <summary>Death-sprite endpoint colors plus samples that differ from the calculated seven-step fade.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> deathSprite = new();
    /// <summary>Required final BG palette 4 row and independent earlier level-art colors.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> deathLevel = new();
    /// <summary>Required final BG palette 7 row and independent earlier backdrop colors.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> deathBackground = new();

    /// <summary>Retains palette endpoints and only the supplied intermediate samples that cannot be reconstructed from them.</summary>
    internal SporeSpawnColorCatalog(RgbColor[] spores, RgbColor[][] health,
        RgbColor[][] deathSprite, RgbColor[][] deathLevel, RgbColor[][] deathBackground)
    {
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
            if (spores[color] != health[0][color]) this.spores[new ColourCell(0, color)] = spores[color];
        this.health = new HealthRows(health);
        int last = SporeSpawnColorDefinitions.DeathSpriteFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            if (deathSprite[0][color] != health[SporeSpawnColorDefinitions.HealthFrameCount - 1][color])
                this.deathSprite[new ColourCell(0, color)] = deathSprite[0][color];
            this.deathSprite[new ColourCell(last, color)] = deathSprite[last][color];
            for (int frame = 1; frame < last; frame++)
                if (deathSprite[frame][color] != CalculateDeathSpriteColor(deathSprite[0][color], deathSprite[last][color], frame))
                    this.deathSprite[new ColourCell(frame, color)] = deathSprite[frame][color];
        }
        int levelLast = SporeSpawnColorDefinitions.DeathSceneFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            this.deathLevel[new ColourCell(levelLast, color)] = deathLevel[levelLast][color];
            for (int frame = 0; frame < levelLast; frame++)
                if (!SporeSpawnDeathColorDefinitions.TryLevelColor(deathLevel[levelLast][color], frame, color, out RgbColor calculated)
                    || deathLevel[frame][color] != calculated)
                    this.deathLevel[new ColourCell(frame, color)] = deathLevel[frame][color];
        }
        int backgroundLast = SporeSpawnColorDefinitions.DeathSceneFrameCount - 1;
        for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
        {
            this.deathBackground[new ColourCell(backgroundLast, color)] = deathBackground[backgroundLast][color];
            for (int frame = 0; frame < backgroundLast; frame++)
                if (deathBackground[frame][color] != SporeSpawnDeathColorDefinitions.BackgroundColor(deathBackground[backgroundLast][color], frame, color))
                    this.deathBackground[new ColourCell(frame, color)] = deathBackground[frame][color];
        }
    }

    /// <summary>
    /// $A5:E359..E378 repeats the healthy body/spawner row at $E379..E398 in sprite
    /// palette7. The chosen healthy colors remain required under the health payload.
    /// Independently supplied spore colors override this original shared coloring.
    /// </summary>
    public RgbColor ResolveSpore(int color)
    {
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return spores.TryGetValue(new ColourCell(0, color), out RgbColor selected) ? selected : health.Resolve(0, color);
    }
    /// <summary>Gets packed RGB5 ink 0–15 from health row 0–3 at $A5:E379–$E3F8, selected by boss health and copied to OBJ palette 1 at CGRAM 144–159.</summary>
    public RgbColor ResolveHealth(int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.HealthFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return health.Resolve(frame, color);
    }

    /// <summary>
    /// $A5:E379..E3F8, the four health rows. The healthy and critical rows are authored paint.
    /// In the two intermediate rows the shell and glow colors nine to fifteen interpolate those
    /// rows to nearest integer over three intervals and the transparent slot repeats the
    /// critical row's; colors one to eight are painted per row. Only supplied deviations from
    /// the calculation are stored, so edited endpoint rows drive every intermediate step.
    /// </summary>
    internal sealed class HealthRows
    {
        /// <summary>Shell and glow colors from ink 9 onward interpolate between healthy and critical.</summary>
        private const int FirstInterpolatedColor = 9;
        /// <summary>Inks 1 through 8 remain painted separately in each intermediate health row.</summary>
        private const int LastPaintedColor = 8;
        /// <summary>Healthy endpoint supplies the first boss row and interpolation source.</summary>
        private readonly RgbColor[] healthy;
        /// <summary>Critical endpoint supplies the last boss row and transparent ink zero.</summary>
        private readonly RgbColor[] critical;
        /// <summary>Authored inks 1 through 8 from the two middle health rows.</summary>
        private readonly RgbColor[][] intermediatePaint;
        /// <summary>Middle-row calculated inks retained only when the supplied RGB5 value differs.</summary>
        internal readonly Dictionary<ColourCell, RgbColor> intermediateEdits = new();

        /// <summary>Splits four supplied health rows into endpoints, painted middle inks and independent interpolation edits.</summary>
        internal HealthRows(RgbColor[][] supplied)
        {
            int last = SporeSpawnColorDefinitions.HealthFrameCount - 1;
            healthy = supplied[0];
            critical = supplied[last];
            intermediatePaint = new RgbColor[last - 1][];
            for (int frame = 1; frame < last; frame++)
            {
                intermediatePaint[frame - 1] = supplied[frame].AsSpan(1, LastPaintedColor).ToArray();
                for (int color = 0; color < SporeSpawnColorDefinitions.ColorsPerFrame; color++)
                    if (IsCalculated(color) && supplied[frame][color] != Calculate(frame, color))
                        intermediateEdits.Add(new ColourCell(frame, color), supplied[frame][color]);
            }
        }

        /// <summary>Chooses authored paint or reconstructs the requested middle-row ink from editable endpoints.</summary>
        internal RgbColor Resolve(int frame, int color)
        {
            int last = SporeSpawnColorDefinitions.HealthFrameCount - 1;
            if (frame == 0) return healthy[color];
            if (frame == last) return critical[color];
            if (!IsCalculated(color)) return intermediatePaint[frame - 1][color - 1];
            return intermediateEdits.TryGetValue(new ColourCell(frame, color), out RgbColor edit)
                ? edit : Calculate(frame, color);
        }

        /// <summary>Identifies transparent ink zero and shell/glow inks that derive from endpoint rows.</summary>
        private static bool IsCalculated(int color) => color is 0 or >= FirstInterpolatedColor;

        /// <summary>Interpolates an RGB5 shell/glow ink to nearest integer over the three health intervals; ink zero repeats critical.</summary>
        private RgbColor Calculate(int frame, int color)
        {
            if (color == 0) return critical[0];
            int intervals = SporeSpawnColorDefinitions.HealthFrameCount - 1;
            return healthy[color].Zip(critical[color], (_, start, end) =>
                (start * (intervals - frame) + end * frame + intervals / 2) / intervals);
        }
    }
    /// <summary>Gets packed RGB5 ink 0–15 from sprite-death phase 0–7 at $A5:E3F9–$E4F8; independently supplied samples override the calculated fade from critical-health colors to the final row.</summary>
    public RgbColor ResolveDeathSprite(int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.DeathSpriteFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (deathSprite.TryGetValue(new ColourCell(frame, color), out RgbColor selected))
            return selected;
        RgbColor first = deathSprite.TryGetValue(new ColourCell(0, color), out RgbColor suppliedFirst)
            ? suppliedFirst : health.Resolve(SporeSpawnColorDefinitions.HealthFrameCount - 1, color);
        return CalculateDeathSpriteColor(first, deathSprite[new ColourCell((SporeSpawnColorDefinitions.DeathSpriteFrameCount - 1), color)], frame);
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
    /// <summary>Gets packed RGB5 ink 0–15 from level-graphics death phase 0–6 at $A5:E4F9–$E5D8, assigned to BG palette 4 at CGRAM 64–79 by the death-script owner.</summary>
    public RgbColor ResolveDeathLevel(int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.DeathSceneFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (deathLevel.TryGetValue(new ColourCell(frame, color), out RgbColor selected))
            return selected;
        RgbColor finalColor = deathLevel[new ColourCell((SporeSpawnColorDefinitions.DeathSceneFrameCount - 1), color)];
        if (SporeSpawnDeathColorDefinitions.TryLevelColor(finalColor, frame, color, out RgbColor calculated))
            return calculated;
        throw new InvalidOperationException("Required Spore Spawn level color is absent.");
    }
    /// <summary>Gets packed RGB5 ink 0–15 from background death phase 0–6 at $A5:E5D9–$E6B8, assigned to BG palette 7 at CGRAM 112–127 by the death-script owner.</summary>
    public RgbColor ResolveDeathBackground(int frame, int color)
    {
        _ = CheckFrame(frame, SporeSpawnColorDefinitions.DeathSceneFrameCount);
        if ((uint)color >= SporeSpawnColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        if (deathBackground.TryGetValue(new ColourCell(frame, color), out RgbColor selected))
            return selected;
        RgbColor finalColor = deathBackground[new ColourCell((SporeSpawnColorDefinitions.DeathSceneFrameCount - 1), color)];
        return SporeSpawnDeathColorDefinitions.BackgroundColor(finalColor, frame, color);
    }

    /// <summary>Resolves one death-script layer's packed RGB5 ink without writing a palette; the caller chooses current CGRAM or target-image ownership.</summary>
    /// <param name="layer">Mutually exclusive sprite, level, or background palette row.</param>
    /// <param name="frame">Zero-based phase, not a native byte offset: 0–7 for sprite, 0–6 for level/background.</param>
    /// <param name="color">Ink index 0–15, including the row's retained transparent-slot word.</param>
    public RgbColor ResolveDeath(SporeSpawnDeathPaletteLayer layer, int frame, int color) =>
        layer switch
        {
            SporeSpawnDeathPaletteLayer.Sprite => ResolveDeathSprite(frame, color),
            SporeSpawnDeathPaletteLayer.Level => ResolveDeathLevel(frame, color),
            SporeSpawnDeathPaletteLayer.Background => ResolveDeathBackground(frame, color),
            _ => throw new ArgumentOutOfRangeException(nameof(layer)),
        };

    /// <summary>Checks a palette phase before its packed frame/color key is used.</summary>
    private static int CheckFrame(int frame, int count) =>
        (uint)frame < count ? frame : throw new ArgumentOutOfRangeException(nameof(frame));
}
