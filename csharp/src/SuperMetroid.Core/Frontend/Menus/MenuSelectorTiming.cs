namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Shared item-selector and map-arrow dwell rule.</summary>
internal static class MenuSelectorTiming
{
    /// <summary>The initial phase dwells for15 ticks; subsequent phases for2.</summary>
    /// <remarks>Callers validate and normalize their document-owned phase domain.
    /// The animation has fourteen phases followed by a loop marker.</remarks>
    internal static int Duration(int phase) => phase == 0 ? 15 : 2;
}
