namespace SuperMetroid.Core.Escape;

/// <summary>
/// The Zebes escape rooms' shaking and explosions (#1415): the earthquake each escape room shakes with, how often a
/// horizontal shake turns diagonal and for how long, and the tile outdoor explosions skip.
/// </summary>
public static class ZebesEscapeDefinitions
{
    /// <summary>The updates a temporary diagonal shake lasts before the room returns to its horizontal shake.</summary>
    public const int DiagonalFrames = 42;

    /// <summary>Light horizontal shaking, enemies included.</summary>
    public const int LightHorizontal = 18;

    /// <summary>Medium horizontal shaking, enemies included.</summary>
    public const int MediumHorizontal = 21;

    /// <summary>The medium diagonal shake a lightly shaking room turns to.</summary>
    public const int MediumDiagonal = 23;

    /// <summary>The strong diagonal shake a medium shaking room turns to.</summary>
    public const int StrongDiagonal = 26;

    /// <summary>A lightly shaking room turns diagonal on an update whose random number is below this.</summary>
    public const int LightChance = 512;

    /// <summary>A medium shaking room turns diagonal on an update whose random number is below this.</summary>
    public const int MediumChance = 384;

    /// <summary>The blank foreground tile outdoor explosions skip.</summary>
    public const int BlankTile = 255;

    /// <summary>The strong horizontal shake, enemies included, Mainstreet's escape room shakes with.</summary>
    public const int MainstreetQuake = 24;

    /// <summary>The strong horizontal background shake the Landing Site shakes with during the escape.</summary>
    public const int LandingQuake = 6;
}
