namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// How the equipment page's reserve-arrow tiles are coloured (#1467): as the page template leaves them until the arrow is
/// first set, then in the arrow's lit or dim colour variant, which stays until the arrow is set again.
/// </summary>
internal enum ShownReserveArrow
{
    /// <summary>The template's own colour variant: the arrow has not been set since the page was built.</summary>
    Unpainted,
    /// <summary>The lit colour variant: the arrow shows solid or pulsing.</summary>
    Lit,
    /// <summary>The dim colour variant: the arrow is hidden.</summary>
    Dim,
}
