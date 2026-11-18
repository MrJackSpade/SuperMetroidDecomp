using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A linked bomb-block restoration (#2484): a bomb block of one arrangement growing back, its children linked to its
/// bombable parent. What it shows is its installed picture (#1557).
/// </summary>
/// <param name="Layout">The blocks' arrangement, never a single block.</param>
internal readonly record struct BombBlockRestoreDraw(BlockArrangement Layout)
{
    /// <summary>The blocks the draw rebuilds around its bombable parent.</summary>
    internal RoomBlockShape Blocks => LinkedRestoreObjectFamily.BombBlockRestored[Layout];
}
