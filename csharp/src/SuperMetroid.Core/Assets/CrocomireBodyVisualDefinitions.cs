using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The fifty distinct fight-body extended-frame roots selected by Crocomire's
/// compiled bank-$A4 instruction operands. Forty-two have both ordinary OAM
/// components and BG2 streams; the final eight death-transition roots have
/// only ordinary OAM components. Skeleton poses use a separate later range.
/// </summary>
internal static class CrocomireBodyVisualDefinitions
{
    /// <summary>Native Crocomire body visual bank $A4.</summary>
    internal const byte Bank = 0xa4;
    /// <summary>First pure-OAM transition root at $A4:CA7E.</summary>
    internal const ushort FirstPureOamFrame = 0xca7e;
    /// <summary>First skeleton extended frame at $A4:E1FE.</summary>
    internal const ushort FirstSkeletonFrame = 0xe1fe;
    internal const int BodyFrameCount = 50;
    internal const int MixedBg2FrameCount = 42;

    /// <summary>$A4:BFC4, twelve selected six-component charge/step-back frames.</summary>
    private const ushort ChargeStepStart = 0xbfc4;
    /// <summary>$A4:C2EC, six selected seven-component attack frames.</summary>
    private const ushort AttackStart = 0xc2ec;
    /// <summary>$A4:C448, six selected six-component attack/claw frames.</summary>
    private const ushort AttackClawStart = 0xc448;
    /// <summary>$A4:C574, three selected seven-component body frames.</summary>
    private const ushort BodyStart = 0xc574;
    /// <summary>$A4:C6A4, selected seven-component frame before two unused roots.</summary>
    private const ushort IsolatedBodyFrame = 0xc6a4;
    /// <summary>$A4:C752, fourteen selected seven-component body frames.</summary>
    private const ushort LaterBodyStart = 0xc752;

    internal static CrocomireBodyFrameSequence Frames => new(BodyFrameCount);

    /// <summary>Enumerates native frame roots in ascending order. A frame has a
    /// two-byte count plus eight bytes per component: strides $32, $3A and $0A.
    /// The selected instruction programs omit the gaps between these runs.</summary>
    internal static ushort FramePointer(int index)
    {
        if ((uint)index >= BodyFrameCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 12 ? ChargeStepStart + 0x32 * index
            : index < 18 ? AttackStart + 0x3a * (index - 12)
            : index < 24 ? AttackClawStart + 0x32 * (index - 18)
            : index < 27 ? BodyStart + 0x3a * (index - 24)
            : index == 27 ? IsolatedBodyFrame
            : index < MixedBg2FrameCount ? LaterBodyStart + 0x3a * (index - 28)
            : FirstPureOamFrame + 10 * (index - MixedBg2FrameCount));
    }

    /// <summary>Accepts only the forty-two selected mixed OAM/BG2 roots; gaps,
    /// component interiors, pure-OAM transition frames and skeletons are excluded.</summary>
    internal static bool HasBg2(ushort pointer) =>
        InRun(pointer, ChargeStepStart, 12, 0x32) ||
        InRun(pointer, AttackStart, 6, 0x3a) ||
        InRun(pointer, AttackClawStart, 6, 0x32) ||
        InRun(pointer, BodyStart, 3, 0x3a) || pointer == IsolatedBodyFrame ||
        InRun(pointer, LaterBodyStart, 14, 0x3a);

    private static bool InRun(ushort pointer, ushort start, int count, int stride)
    {
        int offset = pointer - start;
        return offset >= 0 && offset < count * stride && offset % stride == 0;
    }
}

/// <summary>Calculated body-frame enumeration; no cached pointer lookup.</summary>
internal readonly record struct CrocomireBodyFrameSequence(int Length)
{
    internal ushort this[int index] => CrocomireBodyVisualDefinitions.FramePointer(index);
    internal ushort[] ToArray()
    {
        var result = new ushort[Length];
        for (int i = 0; i < result.Length; i++) result[i] = this[i];
        return result;
    }
    public Enumerator GetEnumerator() => new(this);
    internal struct Enumerator(CrocomireBodyFrameSequence sequence)
    {
        private int index = -1;
        public bool MoveNext() => ++index < sequence.Length;
        public ushort Current => sequence[index];
    }
}