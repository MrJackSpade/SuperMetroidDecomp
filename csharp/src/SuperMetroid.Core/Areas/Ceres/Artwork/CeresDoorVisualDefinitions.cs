namespace SuperMetroid.Core.Areas.Ceres.Artwork;

/// <summary>
/// The shape of the Ceres door's installed visuals (#1415): how many colours and animation rows the door shows.
/// The elevator platform's strips are pictures of the room's floor (#1467).
/// </summary>
public static class CeresDoorVisualDefinitions
{
    /// <summary>The visible colours of either setup appearance, normal or escape; colour zero stays transparent.</summary>
    public const int SetupColorCount = 15;

    /// <summary>The rows of the door's glow cycle, selected by its animation frame.</summary>
    public const int AnimationRowCount = 8;

    /// <summary>The colours each glow row replaces.</summary>
    public const int AnimationColorCount = 6;
}
