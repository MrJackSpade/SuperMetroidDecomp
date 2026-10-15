using SuperMetroid.Core.Hardware;
namespace SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="SamusSlopePhysics"/> members production does not use.</summary>
internal static class SamusSlopePhysicsAccess
{
    /// <summary>Reads the slope alignment height selected by the native collision table.</summary>
    extension(SamusSlopePhysics)
    {
        /// <summary>
        /// Returns the five-bit height sample selected by BTS shape/mirroring and Samus X.
        /// Values may be 0–20; masking with $1F is part of every native consumer.
        /// </summary>
        internal static byte ReadAlignmentHeight(
            ISnesAddressSpace bus,
            byte behavior,
            ushort xPosition)
            => SamusSlopePhysics.ReadAlignmentHeight(bus, new RoomBlockBehavior(behavior), xPosition);
    }
}
