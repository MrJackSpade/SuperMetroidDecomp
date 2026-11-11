using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.AssetExtraction.Bosses.SporeSpawn.Artwork;

/// <summary>
/// Spore Spawn's installed RGB5 images, each kept as installed. Health thresholds, death timing, and target/current
/// palette ownership remain compiled behavior.
/// </summary>
public sealed class SporeSpawnColorCatalog
{
    /// <summary>The installed spore colours.</summary>
    private readonly InstalledColourRows spores;
    /// <summary>The installed health bands.</summary>
    private readonly InstalledColourRows health;
    /// <summary>The installed sprite-death phases.</summary>
    private readonly InstalledColourRows deathSprite;
    /// <summary>The installed level-graphics death phases.</summary>
    private readonly InstalledColourRows deathLevel;
    /// <summary>The installed background death phases.</summary>
    private readonly InstalledColourRows deathBackground;

    /// <summary>Keeps every complete installed colour set.</summary>
    /// <param name="spores">The spores' <see cref="SporeSpawnColorFormat.ColorsPerFrame"/> colours.</param>
    /// <param name="health">The health bands' colours.</param>
    /// <param name="deathSprite">The sprite-death phases' colours.</param>
    /// <param name="deathLevel">The level-graphics death phases' colours.</param>
    /// <param name="deathBackground">The background death phases' colours.</param>
    /// <exception cref="InvalidDataException">A set is missing a frame or a colour.</exception>
    internal SporeSpawnColorCatalog(RgbColor[] spores, RgbColor[][] health, RgbColor[][] deathSprite, RgbColor[][] deathLevel,
        RgbColor[][] deathBackground)
    {
        this.spores = new InstalledColourRows(spores, SporeSpawnColorFormat.ColorsPerFrame, "Spore Spawn spores");
        this.health = new InstalledColourRows(health, SporeSpawnColorDefinitions.HealthFrameCount,
            SporeSpawnColorFormat.ColorsPerFrame, "Spore Spawn health");
        this.deathSprite = new InstalledColourRows(deathSprite, SporeSpawnColorFormat.DeathSpriteFrameCount,
            SporeSpawnColorFormat.ColorsPerFrame, "Spore Spawn death sprite");
        this.deathLevel = new InstalledColourRows(deathLevel, SporeSpawnColorFormat.DeathSceneFrameCount,
            SporeSpawnColorFormat.ColorsPerFrame, "Spore Spawn death level");
        this.deathBackground = new InstalledColourRows(deathBackground, SporeSpawnColorFormat.DeathSceneFrameCount,
            SporeSpawnColorFormat.ColorsPerFrame, "Spore Spawn death background");
    }

    /// <summary>Gets installed spore ink 0–15, shown in sprite palette 7.</summary>
    public RgbColor ResolveSpore(int color) => spores.Colour(color);
    /// <summary>Gets installed ink 0–15 of health band 0–3, selected by boss health and copied to sprite palette 1.</summary>
    public RgbColor ResolveHealth(int frame, int color) => health.Colour(frame, color);
    /// <summary>Gets installed ink 0–15 of sprite-death phase 0–7.</summary>
    public RgbColor ResolveDeathSprite(int frame, int color) => deathSprite.Colour(frame, color);
    /// <summary>Gets installed ink 0–15 of level-graphics death phase 0–6, assigned to background palette 4 by the death-script owner.</summary>
    public RgbColor ResolveDeathLevel(int frame, int color) => deathLevel.Colour(frame, color);
    /// <summary>Gets installed ink 0–15 of background death phase 0–6, assigned to background palette 7 by the death-script owner.</summary>
    public RgbColor ResolveDeathBackground(int frame, int color) => deathBackground.Colour(frame, color);
}
