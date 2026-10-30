using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Exposes the compiled two-stage collision-kind and behaviour reveal lookup.</summary>
public static class XrayRevealTable
{
    /// <summary>Returns null when the cartridge leaves this block's copied BG1 art unchanged.</summary>
    public static XrayRevealDefinition? Find(RoomCollisionType type, RoomBlockBehavior behavior) =>
        XrayReveals.Find(type, behavior);
}
