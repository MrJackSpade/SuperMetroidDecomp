namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>The mutually exclusive suit palette selected by the Samus-in-heat pre-instruction.</summary>
public enum PaletteFxHeatSuit
{
    /// <summary>Power Suit, selected when neither Varia nor Gravity is equipped.</summary>
    Power,

    /// <summary>Varia Suit, selected when Varia is equipped without Gravity.</summary>
    Varia,

    /// <summary>Gravity Suit, which has native priority when both suit bits are equipped.</summary>
    Gravity,
}
