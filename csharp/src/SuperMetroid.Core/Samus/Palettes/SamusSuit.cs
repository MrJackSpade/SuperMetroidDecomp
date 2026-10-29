namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>The suit whose colours Samus shows: her suit colour sets, cycles and flashes come in one version per suit.</summary>
public enum SamusSuit
{
    /// <summary>The Power Suit, shown while neither Varia nor Gravity is equipped.</summary>
    Power,

    /// <summary>The Varia Suit, shown while Varia is equipped without Gravity.</summary>
    Varia,

    /// <summary>The Gravity Suit, shown whenever Gravity is equipped.</summary>
    Gravity,
}
