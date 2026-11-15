using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Verification's native view of <see cref="TorizoCollisionDefinitions"/>: frames by their bank-$AA address.</summary>
internal static class TorizoCollisionDefinitionsAccess
{
    /// <summary>Bank $AA, holding the Torizos' frames.</summary>
    private const byte NativeBank = 0xaa;

    /// <summary>The Torizo frames by their native addresses.</summary>
    extension(TorizoCollisionDefinitions)
    {
        /// <summary>Bank $AA, holding the Torizos' frames.</summary>
        internal static byte Bank => NativeBank;

        /// <summary>The number of Torizo frames with compiled hitboxes.</summary>
        internal static int FrameCount => TorizoCollisionDefinitions.FrameHitboxes.Count;

        /// <summary>The bank-$AA address of every compiled Torizo frame.</summary>
        internal static IEnumerable<ushort> FramePointers =>
            TorizoCollisionDefinitions.FrameHitboxes.Keys.Select(frame => EnemyFrameCartridgeLookupTooling.Word(frame));
    }

    /// <summary>Reads Torizo collision by native address.</summary>
    extension(TorizoCollision)
    {
        /// <summary>The body hitboxes of the frame at native address <paramref name="frame"/>.</summary>
        internal static TorizoHitboxList HitboxesAt(ushort frame) =>
            TorizoCollision.HitboxesAt(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, frame));
    }
}
