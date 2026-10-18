using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The Shaktool room's resident controller.</summary>
internal static class ShaktoolRoomPreInstructions
{
    /// <summary><c>$84:B8B0 PreInstruction_PLM_ShaktoolsRoom</c>, which the room's list installs through <c>$84:86C1</c>.</summary>
    internal static PlmPreInstruction Controller { get; } = new(
        "Shaktool room controller", static (slot, context) =>
            context.Plms.Shaktool.RunShaktoolRoomPreInstruction(slot, context.Frame.Scrolls, context.Frame.PowerBombExplosionStatus));
}
