namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gameplay's starting colours by owner (#1464): the background sets', the sprite sets', Samus's suit's and beams', and
/// the enemy projectiles' that every room load restores. The import pipeline decodes them from the installed starting
/// image, whose colour order is its own knowledge.
/// </summary>
public sealed class GameplayStartingColours
{
    /// <summary>Samus's suit colours.</summary>
    private readonly RgbColor[] samusSuit;

    /// <summary>Samus's beam colours.</summary>
    private readonly RgbColor[] samusBeams;

    /// <summary>The enemy projectiles' colours.</summary>
    private readonly RgbColor[] enemyProjectiles;

    /// <summary>Keeps the decoded colours, which nothing else holds.</summary>
    /// <param name="backgroundSets">Every background set's colours, set after set.</param>
    /// <param name="spriteSets">Every sprite set's colours, set after set.</param>
    /// <param name="samusSuit">Samus's suit colours.</param>
    /// <param name="samusBeams">Samus's beam colours.</param>
    /// <param name="enemyProjectiles">The enemy projectiles' colours.</param>
    /// <exception cref="ArgumentException">A part does not hold its owner's number of colours.</exception>
    internal GameplayStartingColours(RgbColor[] backgroundSets, RgbColor[] spriteSets, RgbColor[] samusSuit,
        RgbColor[] samusBeams, RgbColor[] enemyProjectiles)
    {
        Ensure.LengthEqual<RgbColor>(backgroundSets, ColourSets.SetCount * ColourSets.SetColours);
        Ensure.LengthEqual<RgbColor>(spriteSets, ColourSets.SetCount * ColourSets.SetColours);
        Ensure.LengthEqual<RgbColor>(samusSuit, ColourSets.SetColours);
        Ensure.LengthEqual<RgbColor>(samusBeams, ColourSets.SetColours);
        Ensure.LengthEqual<RgbColor>(enemyProjectiles, ColourSets.SetColours);
        var backgrounds = new BackgroundColourSets();
        backgrounds.All.CopyFrom(backgroundSets);
        Backgrounds = ShownColourSets.Of(backgrounds);
        var sprites = new SpriteColourSets();
        sprites.All.CopyFrom(spriteSets);
        Sprites = ShownColourSets.Of(sprites);
        this.samusSuit = samusSuit;
        this.samusBeams = samusBeams;
        this.enemyProjectiles = enemyProjectiles;
    }

    /// <summary>Samus's suit colours.</summary>
    public ReadOnlySpan<RgbColor> SamusSuit => samusSuit;

    /// <summary>Samus's beam colours.</summary>
    public ReadOnlySpan<RgbColor> SamusBeams => samusBeams;

    /// <summary>The enemy projectiles' colours.</summary>
    public ReadOnlySpan<RgbColor> EnemyProjectiles => enemyProjectiles;

    /// <summary>The background sets' starting colours.</summary>
    public ShownColourSets Backgrounds { get; }

    /// <summary>The sprite sets' starting colours, those Samus's draws show over among them.</summary>
    public ShownColourSets Sprites { get; }
}
