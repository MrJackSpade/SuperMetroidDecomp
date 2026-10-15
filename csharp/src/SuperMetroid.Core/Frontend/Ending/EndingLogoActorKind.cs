namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Four mutually exclusive native logo roles, in E554..E569 allocation order.</summary>
internal enum EndingLogoActorKind
{
    /// <summary>Upper S half approaches the final logo from its initial off-center position.</summary>
    UpperS,
    /// <summary>Lower S half moves toward the opposite landing point.</summary>
    LowerS,
    /// <summary>Stationary upper circle uses the right-wrap display list.</summary>
    UpperCircle,
    /// <summary>Stationary lower circle uses the left-wrap display list.</summary>
    LowerCircle,
}
