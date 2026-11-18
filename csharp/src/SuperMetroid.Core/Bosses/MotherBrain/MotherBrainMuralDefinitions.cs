namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The mural behind Mother Brain's tank (#2211), which her fake death's ascent reveals two rows an update from the top,
/// each row revealed where it lies (#2258, <see cref="MotherBrainMuralRow"/>).
/// </summary>
internal static class MotherBrainMuralDefinitions
{
    /// <summary>The mural's rows, from its top border row to its bottom border row.</summary>
    internal const int RowCount = 12;

    /// <summary>The rows the reveal draws each update.</summary>
    internal const int RowsEachUpdate = 2;

    /// <summary>The block column every row is drawn from.</summary>
    internal const int Column = 2;

    /// <summary>The block row the mural's top row is drawn at; each later row lies one below.</summary>
    internal const int TopBlockRow = 2;
}
