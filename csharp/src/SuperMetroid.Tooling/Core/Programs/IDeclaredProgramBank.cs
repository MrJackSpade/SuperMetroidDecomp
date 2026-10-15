namespace SuperMetroid.Core.Programs;

/// <summary>A catalog whose program bank is declared rather than probed.</summary>
internal interface IDeclaredProgramBank
{
    /// <summary>SNES bank containing the catalog's native instruction program.</summary>
    static abstract int Bank { get; }
}
