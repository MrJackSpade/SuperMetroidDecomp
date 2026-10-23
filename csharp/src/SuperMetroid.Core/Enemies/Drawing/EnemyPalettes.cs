namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Typed view of the enemy and boss CGRAM ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: enemy and boss CGRAM ranges")]
public readonly struct EnemyPalettes(IPaletteColors cgram)
{
    /// <summary>The escape typewriter's source colours.</summary>
    public CgramPalette EscapeTypewriterSource => new(cgram, EnemyPaletteLayout.EscapeTypewriterSource);
    /// <summary>The Wrecked Ship power-on room colours.</summary>
    public CgramPalette WreckedShipPowerOn => new(cgram, EnemyPaletteLayout.WreckedShipPowerOn);
}
