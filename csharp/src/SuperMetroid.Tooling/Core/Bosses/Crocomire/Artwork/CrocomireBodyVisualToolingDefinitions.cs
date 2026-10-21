namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Development-tool members of <see cref="CrocomireBodyVisualDefinitions"/>; never linked by player hosts.</summary>
internal static class CrocomireBodyVisualToolingDefinitions
{
    /// <summary>Native Crocomire body visual bank $A4.</summary>
    internal const byte Bank = 0xa4;
    /// <summary>First pure-OAM transition root at $A4:CA7E.</summary>
    internal const ushort FirstPureOamFrame = 0xca7e;
    /// <summary>First skeleton extended frame at $A4:E1FE.</summary>
    internal const ushort FirstSkeletonFrame = 0xe1fe;
    /// <summary>The first forty-two body roots contain BG2 components alongside OAM parts.</summary>
    internal const int MixedBg2FrameCount = 42;
    /// <summary>$A4:BFC4, twelve selected six-component charge/step-back frames.</summary>
    internal const ushort ChargeStepStart = 0xbfc4;
    /// <summary>$A4:C2EC, six selected seven-component attack frames.</summary>
    internal const ushort AttackStart = 0xc2ec;
    /// <summary>$A4:C448, six selected six-component attack/claw frames.</summary>
    internal const ushort AttackClawStart = 0xc448;
    /// <summary>$A4:C574, three selected seven-component body frames.</summary>
    internal const ushort BodyStart = 0xc574;
    /// <summary>$A4:C6A4, selected seven-component frame before two unused roots.</summary>
    internal const ushort IsolatedBodyFrame = 0xc6a4;
    /// <summary>$A4:C752, fourteen selected seven-component body frames.</summary>
    internal const ushort LaterBodyStart = 0xc752;
    /// <summary>Enumerates native frame roots in ascending order. A frame has a
    /// two-byte count plus eight bytes per component: strides $32, $3A and $0A.
    /// The selected instruction programs omit the gaps between these runs.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= CrocomireBodyVisualDefinitions.BodyFrameCount) throw new ArgumentOutOfRangeException(nameof(index));
        return (ushort)(index < 12 ? ChargeStepStart + 0x32 * index
            : index < 18 ? AttackStart + 0x3a * (index - 12)
            : index < 24 ? AttackClawStart + 0x32 * (index - 18)
            : index < 27 ? BodyStart + 0x3a * (index - 24)
            : index == 27 ? IsolatedBodyFrame
            : index < MixedBg2FrameCount ? LaterBodyStart + 0x3a * (index - 28)
            : FirstPureOamFrame + 10 * (index - MixedBg2FrameCount));
    }
}
