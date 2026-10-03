namespace SuperMetroid.Core.Rooms;

/// <summary>Derives the complete visual-source identity domain from compiled retail room states.</summary>
public static class RoomVisualLayoutSourceDefinitions
{
    /// <summary>
    /// Distinct compressed level identities, including alternate event/boss states.
    /// These are installation keys, not ROM-reading capabilities or editable collision data.
    /// </summary>
    /// Enumerating projects each state's 24-bit source, removes duplicates and sorts
    /// ascending. No persistent source-key lookup or generated cache is retained.
    public static IEnumerable<int> All => RoomStateDefinitions.All
        .Select(state => state.CompressedLevelDataAddress).Distinct().Order();
}
