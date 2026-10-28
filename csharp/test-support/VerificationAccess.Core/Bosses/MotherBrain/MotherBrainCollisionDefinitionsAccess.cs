using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification's native view of <see cref="MotherBrainCollision"/>: frames by their bank-$A9 address.</summary>
internal static class MotherBrainCollisionDefinitionsAccess
{
    /// <summary>Bank $A9, holding Mother Brain's frames.</summary>
    private const byte NativeBank = 0xa9;

    /// <summary>Native views of the catalog.</summary>
    extension(MotherBrainCollision)
    {
        /// <summary>Bank $A9, holding Mother Brain's frames.</summary>
        internal static byte Bank => NativeBank;

        /// <summary>The components of the body frame at native address <paramref name="frame"/>.</summary>
        internal static ReadOnlySpan<MotherBrainCollisionComponent> ComponentsAt(ushort frame) =>
            MotherBrainCollision.ComponentsAt(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, frame));
    }
}
