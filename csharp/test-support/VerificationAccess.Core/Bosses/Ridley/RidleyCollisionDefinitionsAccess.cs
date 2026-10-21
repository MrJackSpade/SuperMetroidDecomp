using System.Collections.Immutable;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Verification's native view of <see cref="RidleyCollisionDefinitions"/>: body frames by their bank-$A6 address.</summary>
internal static class RidleyCollisionDefinitionsAccess
{
    /// <summary>$A6, the native bank of both Ridley encounters' body maps.</summary>
    private const byte NativeBank = 0xa6;
    /// <summary>$A6:E983, ExtendedSpritemap_Ridley_FacingLeft, the first body frame.</summary>
    private const ushort FirstBodyFrame = 0xe983;
    /// <summary>Each body frame's two-byte count and four eight-byte component records.</summary>
    private const int BodyFrameBytes = 2 + 4 * 8;

    /// <summary>Native views of the catalog.</summary>
    extension(RidleyCollisionDefinitions)
    {
        /// <summary>$A6, the native bank of both Ridley encounters' body maps.</summary>
        internal static byte Bank => NativeBank;

        /// <summary>Each body frame's two-byte count and four eight-byte component records.</summary>
        internal static int SideFrameBytes => BodyFrameBytes;

        /// <summary>The eleven body frames, `$22` bytes apart from the leftmost.</summary>
        internal static ushort[] FramePointers =>
            [.. Enumerable.Range(0, 11).Select(index => (ushort)(FirstBodyFrame + index * BodyFrameBytes))];

        /// <summary>Whether the frame at native address <paramref name="frame"/> is one of Ridley's body frames.</summary>
        internal static bool HasFrame(ushort frame) =>
            RidleyCollisionDefinitions.HasFrame(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, frame));

        /// <summary>The body frame stored at native address <paramref name="frame"/>, or null when none is.</summary>
        internal static RidleyCollisionDefinitions.BodyFrame? BodyFrameAt(ushort frame) =>
            RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, frame) is { } shown &&
                Artwork.RidleyBodyFrameDefinitions.ShownFrames.TryFind(shown, out RidleyCollisionDefinitions.BodyFrame body)
                ? body
                : null;

        /// <summary>The components of the body frame at native address <paramref name="frame"/>.</summary>
        internal static ImmutableArray<RidleyCollisionComponent> ComponentsAt(ushort frame) =>
            RidleyCollisionDefinitions.ComponentsAt(RoomEnemySlotFrameAccess.TryFrameAt(NativeBank, frame));
    }
}
