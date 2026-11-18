namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A linked contact-crumble restoration (#2484): a crumble block of one arrangement growing back, its children linked to
/// its special-block parent. What it shows is its installed picture (#1557).
/// </summary>
/// <param name="Layout">The blocks' arrangement, never a single block.</param>
internal readonly record struct ContactCrumbleRestoreDraw(BlockArrangement Layout)
{
    /// <summary>The blocks the draw rebuilds around its special-block parent.</summary>
    internal RoomBlockShape Blocks => LinkedRestoreObjectFamily.ContactCrumbleRestored[Layout];
}
