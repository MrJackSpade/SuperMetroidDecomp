namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The Shaktool room's cleared-path rule: where Samus must pass, and the scroll cells it opens.</summary>
internal static class ShaktoolRoomDefinitions
{
    /// <summary>Samus beyond this room X, in pixels, marks the path cleared.</summary>
    public const int ClearedPathXBoundary = 840;

    /// <summary>The room's first scroll cells the controller writes.</summary>
    public const int ScrollCellCount = 4;
}
