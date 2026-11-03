using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Gameplay's starting colours by owner (#1464, #1512): the background colours, the sprite colours, Samus's suit's and
/// beams', and the enemy projectiles' that every room load restores. The import pipeline decodes them from the installed
/// starting image, whose colour order is its own knowledge, and pre-colours sheets from them; Core never reads them.
/// </summary>
public sealed class GameplayStartingColours
{
    /// <summary>The background colours, in the cartridge's order.</summary>
    internal readonly RgbColor[] backgroundColours;

    /// <summary>The sprite colours, in the cartridge's order.</summary>
    private readonly RgbColor[] spriteColours;

    /// <summary>Samus's suit colours.</summary>
    private readonly RgbColor[] samusSuit;

    /// <summary>Samus's beam colours.</summary>
    private readonly RgbColor[] samusBeams;

    /// <summary>The enemy projectiles' colours.</summary>
    private readonly RgbColor[] enemyProjectiles;

    /// <summary>Keeps the decoded colours, which nothing else holds.</summary>
    /// <param name="backgroundColours">The background colours, in the cartridge's order.</param>
    /// <param name="spriteColours">The sprite colours, in the cartridge's order.</param>
    /// <param name="samusSuit">Samus's suit colours.</param>
    /// <param name="samusBeams">Samus's beam colours.</param>
    /// <param name="enemyProjectiles">The enemy projectiles' colours.</param>
    /// <exception cref="ArgumentException">A part does not hold its owner's number of colours.</exception>
    internal GameplayStartingColours(RgbColor[] backgroundColours, RgbColor[] spriteColours, RgbColor[] samusSuit,
        RgbColor[] samusBeams, RgbColor[] enemyProjectiles)
    {
        Ensure.LengthEqual<RgbColor>(backgroundColours, GameplayColourPositionDefinitions.SetsColours);
        Ensure.LengthEqual<RgbColor>(spriteColours, GameplayColourPositionDefinitions.SetsColours);
        Ensure.LengthEqual<RgbColor>(samusSuit, GameplayColourPositionDefinitions.SetColours);
        Ensure.LengthEqual<RgbColor>(samusBeams, GameplayColourPositionDefinitions.SetColours);
        Ensure.LengthEqual<RgbColor>(enemyProjectiles, GameplayColourPositionDefinitions.SetColours);
        this.backgroundColours = backgroundColours;
        this.spriteColours = spriteColours;
        this.samusSuit = samusSuit;
        this.samusBeams = samusBeams;
        this.enemyProjectiles = enemyProjectiles;
    }

    /// <summary>The sprite colours, in the cartridge's order, those Samus's draws show over among them.</summary>
    public ReadOnlySpan<RgbColor> SpriteColours => spriteColours;

    /// <summary>Samus's suit colours.</summary>
    public ReadOnlySpan<RgbColor> SamusSuit => samusSuit;

    /// <summary>Samus's beam colours.</summary>
    public ReadOnlySpan<RgbColor> SamusBeams => samusBeams;

    /// <summary>The enemy projectiles' colours.</summary>
    public ReadOnlySpan<RgbColor> EnemyProjectiles => enemyProjectiles;
}
