using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Verification's native view of the Shaktool room controller PLM: its bank-$84 program and pre-instruction words.</summary>
internal static class ShaktoolRoomPlmNativeProgram
{
    /// <summary>$84:B8D6 installs the callback, then sleeps.</summary>
    public const ushort AnimationScript = 0xb8d6;
    /// <summary>
    /// <c>$84:B8D6-B8DB InstList_PLM_ShaktoolsRoom</c> is the complete
    /// three-word resident controller. Its scroll/event rules live in the
    /// setup-selected pre-instruction, not in editable room artwork.
    /// </summary>
    internal static bool TryReadInstructionWord(ushort address, out ushort value)
    {
        value = address switch
        {
            AnimationScript => (ushort)RoomPlmInstruction.InstallFrameBehavior,
            AnimationScript + 2 => FrameBehavior,
            AnimationScript + 4 => (ushort)RoomPlmInstruction.Sleep,
            _ => 0,
        };
        return address is AnimationScript or AnimationScript + 2 or AnimationScript + 4;
    }
    /// <summary>$84:B8B0 FrameBehavior_PLM_ShaktoolsRoom.</summary>
    public const ushort FrameBehavior = 0xb8b0;
}
