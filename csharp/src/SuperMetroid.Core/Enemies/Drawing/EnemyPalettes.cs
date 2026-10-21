namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Typed view of the enemy and boss CGRAM ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: enemy and boss CGRAM ranges")]
public readonly struct EnemyPalettes(IPaletteColors cgram)
{
    /// <summary>The ninja Space Pirate's gold palette row.</summary>
    public CgramPalette NinjaSpacePirate => new(cgram, EnemyPaletteLayout.NinjaSpacePirate);
    /// <summary>The dead sidehopper's palette.</summary>
    public CgramPalette DeadSidehopper => new(cgram, EnemyPaletteLayout.DeadSidehopper);
    /// <summary>The escape typewriter's source colours.</summary>
    public CgramPalette EscapeTypewriterSource => new(cgram, EnemyPaletteLayout.EscapeTypewriterSource);
    /// <summary>The escape typewriter's text colours.</summary>
    public CgramPalette EscapeTypewriterText => new(cgram, EnemyPaletteLayout.EscapeTypewriterText);
    /// <summary>The Wrecked Ship power-on room colours.</summary>
    public CgramPalette WreckedShipPowerOn => new(cgram, EnemyPaletteLayout.WreckedShipPowerOn);

    /// <summary>Chozo statue / glass tube palettes.</summary>
    public CgramPalette ChozoAndTube => new(cgram, EnemyPaletteLayout.ChozoAndTube);
}
