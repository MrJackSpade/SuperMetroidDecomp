using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Verification's native view of <see cref="KraidFootCollisionDefinitions"/>: frames by their bank-$A7 address.</summary>
internal static class KraidFootCollisionDefinitionsAccess
{
    /// <summary>Bank $A7, holding Kraid's foot frames.</summary>
    private const byte NativeBank = 0xa7;
    /// <summary>First authored Kraid-foot extended frame at $A7:8CE3.</summary>
    private const ushort NativeFirstFrame = 0x8ce3;
    /// <summary>Small-Kraid initial foot frame at $A7:A565; physically the first walk frame.</summary>
    private const ushort NativeInitialFrame = 0xa565;
    /// <summary>Each two-component frame occupies 18 bytes in bank $A7.</summary>
    private const int NativeFrameByteCount = 18;

    /// <summary>Native views of the catalog.</summary>
    extension(KraidFootCollisionDefinitions)
    {
        /// <summary>Bank $A7, holding Kraid's foot frames.</summary>
        internal static byte Bank => NativeBank;
        /// <summary>First authored Kraid-foot extended frame at $A7:8CE3.</summary>
        internal static ushort FirstFrame => NativeFirstFrame;
        /// <summary>Small-Kraid initial foot frame at $A7:A565.</summary>
        internal static ushort InitialFrame => NativeInitialFrame;
        /// <summary>Each two-component frame occupies 18 bytes in bank $A7.</summary>
        internal static int FrameByteCount => NativeFrameByteCount;

        /// <summary>Computes a foot collision frame root from the fixed native record stride.</summary>
        internal static ushort FramePointer(int index) => checked((ushort)(NativeFirstFrame + index * NativeFrameByteCount));

        /// <summary>The components of the foot frame at native address <paramref name="pointer"/>, when it is one.</summary>
        internal static bool TryGetComponents(ushort pointer, out ImmutableArray<KraidCollisionComponent> components) =>
            KraidFootCollisionDefinitions.TryGetComponents(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, pointer), out components);
    }
}
