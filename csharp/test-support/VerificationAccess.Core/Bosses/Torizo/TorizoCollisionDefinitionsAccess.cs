
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoCollisionDefinitions"/> members production does not use.</summary>
internal static class TorizoCollisionDefinitionsAccess
{
    /// <summary>The native Torizo hitbox list identities.</summary>
    private static readonly ushort[] ListPointers =
    [
        0x87c7, 0x87e8, 0x87f6, 0x8804, 0x8812, 0x8820, 0x882e, 0x883c, 0x884a, 0x8858, 0x885a, 0x886a, 0x887a, 0x888a, 0x889a, 0x88aa, 0x88ba, 0x88bc, 0x88cc, 0x88dc, 0x88ec, 0x88fc, 0x890c, 0x891c, 0x892a, 0x8946, 0x8954, 0x8962, 0x8970, 0x897e, 0x898c, 0x899a, 0x89a8, 0x89b6, 0x89b8, 0x89c8, 0x89d8, 0x89e8, 0x89f8, 0x8a08, 0x8a18, 0x8a1a, 0x8a2a, 0x8a3a, 0x8a4a, 0x8a5a, 0x8a6a, 0x8a7a, 0x8a88,
    ];

    /// <summary>Enumerates Torizo's empty, facing, awakening, and jump-back collision frames and fixed hitbox roots.</summary>
    extension(TorizoCollisionDefinitions)
    {
        /// <summary>Empty, mirrored facing, and jump-back frame totals.</summary>
        internal static int FrameCount => 2 + TorizoCollisionDefinitions.MirroredCount * 2 + TorizoCollisionDefinitions.JumpBackFrames.Length;

        /// <summary>Every frame pointer, derived from the four runs.</summary>
        internal static IEnumerable<ushort> FramePointers
        {
            get
            {
                foreach (TorizoCollisionDefinitions.EmptyFrame emptyFrame in Enum.GetValues<TorizoCollisionDefinitions.EmptyFrame>())
                    yield return (ushort)emptyFrame;
                ushort cursor = TorizoCollisionDefinitions.LeftStart;
                foreach (GoldenTorizoCollisionComponent[] frame in TorizoCollisionDefinitions.LeftFrames)
                {
                    yield return cursor;
                    cursor = ((ushort)(TorizoCollisionDefinitions.Next((ushort)(cursor), (int)(frame.Length))));
                }
                cursor = TorizoCollisionDefinitions.AwakeningStart;
                foreach (GoldenTorizoCollisionComponent[] frame in TorizoCollisionDefinitions.AwakeningFrames)
                {
                    yield return cursor;
                    cursor = ((ushort)(TorizoCollisionDefinitions.Next((ushort)(cursor), (int)(frame.Length))));
                }
                cursor = TorizoCollisionDefinitions.RightStart;
                for (int index = 0; index < TorizoCollisionDefinitions.MirroredCount; index++)
                {
                    yield return cursor;
                    cursor = ((ushort)(TorizoCollisionDefinitions.Next((ushort)(cursor), (int)(((GoldenTorizoCollisionComponent[])(TorizoCollisionDefinitions.LeftFacing((int)(index)))).Length))));
                }
                foreach (GoldenTorizoCollisionComponent[] frame in TorizoCollisionDefinitions.JumpBackFrames)
                {
                    yield return cursor;
                    cursor = ((ushort)(TorizoCollisionDefinitions.Next((ushort)(cursor), (int)(frame.Length))));
                }
            }
        }

        /// <summary>Native roots of the fixed Torizo hitbox-list roster.</summary>
        internal static IEnumerable<ushort> HitboxPointers => ListPointers;

        /// <summary>Checks membership in the compiled Torizo component-frame set.</summary>
        internal static bool HasFrame(ushort frame) => TorizoCollisionDefinitions.TryGetComponents(frame, out _);

        /// <summary>Finds components only after a pointer matches a compiled native frame root.</summary>
        internal static bool TryGetComponents(ushort frame, out TorizoCollisionComponents components)
        {
            foreach (ushort pointer in TorizoCollisionDefinitions.FramePointers)
            {
                if (pointer != frame) continue;
                components = TorizoCollisionDefinitions.ComponentsAt(frame);
                return true;
            }
            components = default;
            return false;
        }
    }
}
