namespace BuildPolicy;

/// <summary>Compliant IDE0032 fixture using an auto property for its scalar state.</summary>
internal sealed class Ide0032
{
    /// <summary>Allows callers to observe and replace the fixture's integer state without a manual backing field.</summary>
    internal int Value { get; set; }
}
