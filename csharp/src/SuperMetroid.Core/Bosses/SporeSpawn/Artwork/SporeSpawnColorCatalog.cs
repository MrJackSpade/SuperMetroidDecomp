using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Editable Spore Spawn RGB5 images. Health thresholds, death timing, and target/current
/// palette ownership remain compiled behavior.
/// </summary>
public sealed class SporeSpawnColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered health bands and death fade steps, independent of JSON encoding.</summary>
    public string ContentIdentity => SporeSpawnColors.ContentIdentity(this);

    /// <summary>The selected spore, health and death colours.</summary>
    internal readonly SporeSpawnColorSelection selection;

    /// <summary>Retains palette endpoints and only the supplied intermediate samples that cannot be reconstructed from them.</summary>
    internal SporeSpawnColorCatalog(RgbColor[] spores, RgbColor[][] health,
        RgbColor[][] deathSprite, RgbColor[][] deathLevel, RgbColor[][] deathBackground) =>
        selection = new(SporeSpawnColors.SporeEdits(spores, health), new SporeSpawnHealthBands(health),
            SporeSpawnColors.DeathSpriteEdits(deathSprite, health), SporeSpawnColors.DeathLevelEdits(deathLevel),
            SporeSpawnColors.DeathBackgroundEdits(deathBackground));

    /// <summary>
    /// Spore Spawn's palette repeats the healthy body/spawner health band in sprite
    /// palette7. The chosen healthy colors remain required under the health payload.
    /// Independently supplied spore colors override this original shared coloring.
    /// </summary>
    public RgbColor ResolveSpore(int color) => SporeSpawnColors.Spore(this, color);
    /// <summary>Gets packed RGB5 ink 0–15 from health band 0–3, selected by boss health and copied to sprite palette 1.</summary>
    public RgbColor ResolveHealth(int frame, int color) => SporeSpawnColors.Health(selection.Health, frame, color);
    /// <summary>Gets packed RGB5 ink 0–15 from sprite-death phase 0–7; independently supplied samples override the calculated fade from critical-health colors to the final fade step.</summary>
    public RgbColor ResolveDeathSprite(int frame, int color) => SporeSpawnColors.DeathSprite(this, frame, color);
    /// <summary>Gets packed RGB5 ink 0–15 from level-graphics death phase 0–6, assigned to background palette 4 by the death-script owner.</summary>
    public RgbColor ResolveDeathLevel(int frame, int color) => SporeSpawnColors.DeathLevel(selection.DeathLevel, frame, color);
    /// <summary>Gets packed RGB5 ink 0–15 from background death phase 0–6, assigned to background palette 7 by the death-script owner.</summary>
    public RgbColor ResolveDeathBackground(int frame, int color) => SporeSpawnColors.DeathBackground(selection.DeathBackground, frame, color);

    /// <summary>Resolves one death-script layer's packed RGB5 ink without writing a palette; the caller chooses current colours or target-image ownership.</summary>
    /// <param name="layer">Mutually exclusive sprite, level, or background colour set.</param>
    /// <param name="frame">Zero-based phase, not a byte offset: 0–7 for sprite, 0–6 for level/background.</param>
    /// <param name="color">Ink index 0–15, including the colour set's retained transparent-slot word.</param>
    public RgbColor ResolveDeath(SporeSpawnDeathPaletteLayer layer, int frame, int color) =>
        SporeSpawnColors.Death(this, layer, frame, color);
}
