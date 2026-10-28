using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification's native view of <see cref="PhantoonCollisionDefinitions"/>: frames by their bank-$A7 address.</summary>
internal static class PhantoonCollisionDefinitionsAccess
{
    /// <summary>Bank $A7, holding Phantoon's frames.</summary>
    private const byte NativeBank = 0xa7;

    /// <summary>Native views of the catalog.</summary>
    extension(PhantoonCollision)
    {
        /// <summary>The components of the frame at native address <paramref name="frame"/>.</summary>
        internal static ImmutableArray<PhantoonCollisionComponent> ComponentsAt(ushort frame) =>
            PhantoonCollision.ComponentsAt(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, frame));
    }
}
