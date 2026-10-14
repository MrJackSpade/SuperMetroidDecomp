namespace SuperMetroid.ResourceAudit;

/// <summary>Finite arithmetic index set, optionally admitting a separate inclusive range for a reviewed provider parameter.</summary>
internal readonly record struct ProviderIndexDomain(int First, int Count, int Stride = 1, int? Additional = null, int? AdditionalMaximum = null)
{
    /// <summary>Checks stride alignment and count, or membership in the optional extra range.</summary>
    internal bool Contains(int value) => (Additional is int extra && value >= extra && value <= (AdditionalMaximum ?? extra)) || value >= First && (value - First) % Stride == 0 && (value - First) / Stride < Count;
    /// <summary>Formats the admitted arithmetic progression and optional extra range for audit findings.</summary>
    internal string Description => $"{First}..{First + (Count - 1) * Stride}, stride {Stride}" + (Additional is int extra ? $" or {extra}..{AdditionalMaximum ?? extra}" : "");
}
