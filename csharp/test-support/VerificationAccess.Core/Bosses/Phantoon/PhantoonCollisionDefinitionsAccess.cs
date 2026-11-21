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
        /// <summary>
        /// The components of the frame at native address <paramref name="frame"/>: the cartridge stored his body's picture
        /// once for each hit area, so the record names the hit area the body has live.
        /// </summary>
        internal static ImmutableArray<EnemyCollisionComponent> ComponentsAt(ushort frame) =>
            PhantoonCollision.ComponentsAt(RoomEnemyPlaceFrameAccess.TryFrameAt(NativeBank, frame), NativeHitArea(frame));

        /// <summary>The hit area the native body record at <paramref name="frame"/> has live; any other record's is unread.</summary>
        internal static PhantoonBodyHitArea NativeHitArea(ushort frame) => (PhantoonCollisionFrame)frame switch
        {
            PhantoonCollisionFrame.BodyFullHitbox => PhantoonBodyHitArea.WholeBody,
            PhantoonCollisionFrame.BodyEyeHitboxOnly => PhantoonBodyHitArea.EyeOnly,
            _ => PhantoonBodyHitArea.SolidifyingCentre,
        };
    }
}
