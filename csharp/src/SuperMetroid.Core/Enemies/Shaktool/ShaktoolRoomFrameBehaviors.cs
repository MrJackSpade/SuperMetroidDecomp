using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The Shaktool room's resident controller.</summary>
internal static class ShaktoolRoomFrameBehaviors
{
    /// <summary>The Shaktool room controller's frame behaviour, which the room's animation installs.</summary>
    internal static PlmFrameBehavior Controller { get; } = new(
        "Shaktool room controller", static (slot, context) =>
            context.Plms.Enemies().Shaktool.RunShaktoolRoomFrameBehavior(slot, context.Frame.Scrolls, context.Frame.PowerBombExplosionActive));
}
