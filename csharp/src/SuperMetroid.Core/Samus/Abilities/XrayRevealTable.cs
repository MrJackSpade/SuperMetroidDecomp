using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Exposes the compiled two-stage collision-kind and behaviour reveal shape lookup.</summary>
public static class XrayRevealTable
{
    /// <summary>
    /// The reveal shape for a collision kind and behaviour; null when the game leaves this block's copied foreground art
    /// unchanged. The blocks a copy shape shows are installed (<see cref="Artwork.InstalledXrayReveals"/>).
    /// </summary>
    public static XrayRevealShape? Find(RoomCollisionType type, RoomBlockBehavior behavior) =>
        XrayReveals.Find(type, behavior);
}
