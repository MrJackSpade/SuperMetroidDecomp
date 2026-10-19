
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification access to <see cref="TorizoCollisionDefinitions"/> members production does not use.</summary>
internal static class TorizoCollisionDefinitionsAccess
{
    /// <summary>Enumerates Torizo's empty, facing, awakening, and jump-back collision frames.</summary>
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
