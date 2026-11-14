using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// A falling tube piece's pose (#1747), which the tube compositions of <see cref="MotherBrainVisualDefinitions"/> show.
/// Its values are arbitrary.
/// </summary>
public enum MotherBrainTubeFrame
{
    /// <summary>The bottom-left outer piece's pose.</summary>
    BottomLeft,
    /// <summary>The bottom-right outer piece's pose.</summary>
    BottomRight,
    /// <summary>The bottom-middle-left inner piece's pose.</summary>
    BottomMiddleLeft,
    /// <summary>The bottom-middle-right inner piece's pose.</summary>
    BottomMiddleRight,
    /// <summary>The main tube's pose.</summary>
    Main,
}
