namespace SuperMetroid.Core.Bosses.Ridley.Artwork;

/// <summary>The selected Ceres Ridley colour rows a catalog is built from; a family an older override predates is null.</summary>
internal sealed record CeresRidleyColorRows
{
    /// <summary>The startup colours.</summary>
    public required Bgr555[] Start { get; init; }

    /// <summary>The eye fade rows.</summary>
    public required Bgr555[][] EyeFade { get; init; }

    /// <summary>The body fade rows.</summary>
    public required Bgr555[][] BodyFade { get; init; }

    /// <summary>The health rows.</summary>
    public required Bgr555[][] Health { get; init; }

    /// <summary>The alarm rows, or null when they are inherited from stock.</summary>
    public required Bgr555[][]? Alarm { get; init; }

    /// <summary>The retreat background colours.</summary>
    public required Bgr555[] RetreatBg { get; init; }

    /// <summary>The retreat colours shared with the startup door paints.</summary>
    public required Bgr555[] RetreatShared { get; init; }

    /// <summary>The Baby Metroid rows, or null when they are inherited from stock.</summary>
    public required Bgr555[][]? Baby { get; init; }
}
