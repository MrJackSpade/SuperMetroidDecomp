
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoCollisionDefinitions"/> members production does not use.</summary>
internal static class TorizoCollisionDefinitionsAccess
{
    /// <summary>Bank $AA, holding the Torizos' frames.</summary>
    private const byte NativeBank = 0xaa;
    /// <summary>$AA:87D0 and $AA:A4F0: the native blank frame and the shared facing-screen turning frame.</summary>
    private static readonly ushort[] EmptyFrames = [0x87d0, 0xa4f0];
    /// <summary>$AA:A4FA, first left-facing frame after the turning frame.</summary>
    private const ushort NativeLeftStart = 0xa4fa;
    /// <summary>$AA:AA12, first awakening frame.</summary>
    private const ushort NativeAwakeningStart = 0xaa12;
    /// <summary>$AA:AA98, first right-facing frame.</summary>
    private const ushort NativeRightStart = 0xaa98;

    /// <summary>Enumerates Torizo's empty, facing, awakening, and jump-back collision frames.</summary>
    extension(TorizoCollisionDefinitions)
    {
        /// <summary>Bank $AA, holding the Torizos' frames.</summary>
        internal static byte Bank => NativeBank;
        /// <summary>$AA:A4FA, first left-facing frame after the turning frame.</summary>
        internal static ushort LeftStart => NativeLeftStart;
        /// <summary>$AA:AA12, first awakening frame.</summary>
        internal static ushort AwakeningStart => NativeAwakeningStart;
        /// <summary>$AA:AA98, first right-facing frame.</summary>
        internal static ushort RightStart => NativeRightStart;
        /// <summary>Advances past a native count word and all eight-byte component records.</summary>
        internal static ushort Next(ushort frame, int components) => (ushort)(frame + 2 + 8 * components);
        /// <summary>Empty, mirrored facing, and jump-back frame totals.</summary>
        internal static int FrameCount => 2 + TorizoCollisionDefinitions.MirroredCount * 2 + TorizoCollisionDefinitions.JumpBackFrames.Length;

        /// <summary>Every frame pointer, derived from the four runs.</summary>
        internal static IEnumerable<ushort> FramePointers
        {
            get
            {
                foreach (ushort emptyFrame in EmptyFrames)
                    yield return emptyFrame;
                ushort cursor = TorizoCollisionDefinitions.LeftStart;
                foreach (GoldenTorizoCollisionComponent[] frame in TorizoCollisionDefinitions.LeftFrames)
                {
                    yield return cursor;
                    cursor = TorizoCollisionDefinitions.Next(cursor, frame.Length);
                }
                cursor = TorizoCollisionDefinitions.AwakeningStart;
                foreach (GoldenTorizoCollisionComponent[] frame in TorizoCollisionDefinitions.AwakeningFrames)
                {
                    yield return cursor;
                    cursor = TorizoCollisionDefinitions.Next(cursor, frame.Length);
                }
                cursor = TorizoCollisionDefinitions.RightStart;
                for (int index = 0; index < TorizoCollisionDefinitions.MirroredCount; index++)
                {
                    yield return cursor;
                    cursor = TorizoCollisionDefinitions.Next(cursor, TorizoCollisionConstants.LeftFacing(index).Length);
                }
                foreach (GoldenTorizoCollisionComponent[] frame in TorizoCollisionDefinitions.JumpBackFrames)
                {
                    yield return cursor;
                    cursor = TorizoCollisionDefinitions.Next(cursor, frame.Length);
                }
            }
        }

        /// <summary>Checks membership in the compiled Torizo component-frame set.</summary>
        internal static bool HasFrame(ushort frame) => TorizoCollisionDefinitions.TryGetComponents(frame, out _);

        /// <summary>Finds components only after a pointer matches a compiled native frame root.</summary>
        internal static bool TryGetComponents(ushort frame, out ReadOnlySpan<GoldenTorizoCollisionComponent> components)
        {
            foreach (ushort pointer in TorizoCollisionDefinitions.FramePointers)
            {
                if (pointer != frame) continue;
                components = TorizoCollision.ComponentsAt(frame);
                return true;
            }
            components = default;
            return false;
        }
    }

    /// <summary>Reads Torizo collision by native address.</summary>
    extension(TorizoCollision)
    {
        /// <summary>The components of the frame at native address <paramref name="frame"/>.</summary>
        internal static ReadOnlySpan<GoldenTorizoCollisionComponent> ComponentsAt(ushort frame) =>
            TorizoCollision.ComponentsAt(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, frame));
    }
}
