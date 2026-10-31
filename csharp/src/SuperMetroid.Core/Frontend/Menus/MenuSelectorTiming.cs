namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>Shared item-selector and map-arrow dwell rule from $82:C137.</summary>
internal static class MenuSelectorTiming
{
    /// <summary>The initial phase dwells for15 ticks; subsequent phases for2.</summary>
    /// <remarks>Callers validate and normalize their document-owned phase domain.
    /// The native program has fourteen phases followed by an FF loop marker.</remarks>
    internal static int Duration(int phase) => phase == 0 ? 15 : 2;
}
