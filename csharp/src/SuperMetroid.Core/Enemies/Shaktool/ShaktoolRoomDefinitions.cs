namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The Shaktool room's cleared-path rule: where Samus must pass. The room's controller writes every screen of the room, one
/// row of four.
/// </summary>
internal static class ShaktoolRoomDefinitions
{
    /// <summary>Samus beyond this room X, in pixels, marks the path cleared.</summary>
    public const int ClearedPathXBoundary = 840;
}
