namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive suit whose colours Samus's heat shimmer shows.</summary>
public enum PaletteFxHeatSuit
{
    /// <summary>Power Suit, selected when neither Varia nor Gravity is equipped.</summary>
    Power,

    /// <summary>Varia Suit, selected when Varia is equipped without Gravity.</summary>
    Varia,

    /// <summary>Gravity Suit, which has priority when both suit bits are equipped.</summary>
    Gravity,
}
