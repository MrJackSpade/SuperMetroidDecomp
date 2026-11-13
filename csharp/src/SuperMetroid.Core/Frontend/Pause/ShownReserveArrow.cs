namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// How the equipment page's reserve-arrow pictures are coloured (#1467, #1597): as the page template colours them while Samus
/// has no reserve capacity, and otherwise in the arrow's lit or dim colour variant as the cursor lights it now.
/// </summary>
internal enum ShownReserveArrow
{
    /// <summary>The template's own colour variant: Samus has no reserve capacity.</summary>
    NoReserves,
    /// <summary>The lit colour variant: the arrow shows solid or pulsing.</summary>
    Lit,
    /// <summary>The dim colour variant: the arrow is hidden.</summary>
    Dim,
}
