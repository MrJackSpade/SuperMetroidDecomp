namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The shape of Kraid's body layer (#1466): four pages, two across and two down, of which the left two show his upper
/// and lower body; the right two show his blank.
/// </summary>
internal static class KraidBodyLayout
{
    /// <summary>The rows of Kraid's body layer: four pages of thirty-two.</summary>
    internal const int LayerRows = 128;

    /// <summary>The first row of the bottom-left page, where his lower body is drawn.</summary>
    internal const int LowerPageTopRow = 64;
}
