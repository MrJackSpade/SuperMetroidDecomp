using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification's native view of Crocomire's tongue frames: frames by their bank-$A4 address.</summary>
internal static class CrocomireTongueCollisionDefinitionsAccess
{
    /// <summary>Whether the frame at native address <paramref name="frame"/> is one of the tongue's.</summary>
    internal static bool HasFrame(ushort frame) =>
        RoomEnemySlotFrameAccess.TryFrameAt(CrocomireTongueCollisionDefinitionsConstants.Bank, frame) is { } shown &&
        Artwork.CrocomireTongueFrameDefinitions.ShownFrames.TryFind(shown, out _);
}
