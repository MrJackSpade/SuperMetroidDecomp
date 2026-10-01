namespace SuperMetroid.Core.Rooms;

/// <summary>Complete immutable visual-source identity domain selected by compiled retail room states.</summary>
public static class RoomVisualLayoutSourceDefinitions
{
    /// <summary>
    /// Distinct compressed level identities, including alternate event/boss states.
    /// These are installation keys, not ROM-reading capabilities or editable collision data.
    /// </summary>
    public static IReadOnlyList<int> All { get; } = Array.AsReadOnly(RoomStateDefinitions.All
        .Select(state => state.CompressedLevelDataAddress).Distinct().Order().ToArray());
}
