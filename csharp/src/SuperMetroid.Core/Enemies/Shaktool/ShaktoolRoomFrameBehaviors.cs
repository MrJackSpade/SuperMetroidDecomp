using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The Shaktool room's resident controller.</summary>
internal static class ShaktoolRoomFrameBehaviors
{
    /// <summary><c>$84:B8B0 FrameBehavior_PLM_ShaktoolsRoom</c>, which the room's list installs through <c>$84:86C1</c>.</summary>
    internal static PlmFrameBehavior Controller { get; } = new(
        "Shaktool room controller", static (slot, context) =>
            context.Plms.Enemies().Shaktool.RunShaktoolRoomFrameBehavior(slot, context.Frame.Scrolls, context.Frame.PowerBombExplosionActive));
}
