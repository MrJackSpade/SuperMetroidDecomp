namespace SuperMetroid.Core.Escape;

/// <summary>
/// The Zebes escape rooms' shaking and explosions (#1415): the earthquake each escape room shakes with, how often a
/// horizontal shake turns diagonal and for how long, and the tile outdoor explosions skip.
/// </summary>
public static class ZebesEscapeDefinitions
{
    /// <summary>The updates a temporary diagonal shake lasts before the room returns to its horizontal shake.</summary>
    public const ushort DiagonalFrames = 42;

    /// <summary>Light horizontal shaking, enemies included.</summary>
    public const ushort LightHorizontal = 18;

    /// <summary>Medium horizontal shaking, enemies included.</summary>
    public const ushort MediumHorizontal = 21;

    /// <summary>The medium diagonal shake a lightly shaking room turns to.</summary>
    public const ushort MediumDiagonal = 23;

    /// <summary>The strong diagonal shake a medium shaking room turns to.</summary>
    public const ushort StrongDiagonal = 26;

    /// <summary>A lightly shaking room turns diagonal on an update whose random number is below this.</summary>
    public const ushort LightChance = 512;

    /// <summary>A medium shaking room turns diagonal on an update whose random number is below this.</summary>
    public const ushort MediumChance = 384;

    /// <summary>The blank foreground tile outdoor explosions skip.</summary>
    public const ushort BlankTile = 255;

    /// <summary>The strong horizontal shake, enemies included, Mainstreet's escape room shakes with.</summary>
    public const ushort MainstreetQuake = 24;

    /// <summary>The strong horizontal background shake the Landing Site shakes with during the escape.</summary>
    public const ushort LandingQuake = 6;
}
