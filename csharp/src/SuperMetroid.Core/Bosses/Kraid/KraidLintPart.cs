namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's three lint parts, valued by their enemy slot index.</summary>
internal enum KraidLintPart
{
    /// <summary>$A7:AE2F selects native enemy offset $0080: the top lint.</summary>
    Top = 2,
    /// <summary>$A7:AE38 selects native enemy offset $00C0: the middle lint.</summary>
    Middle = 3,
    /// <summary>$A7:AE41 selects native enemy offset $0100: the bottom lint.</summary>
    Bottom = 4,
}
