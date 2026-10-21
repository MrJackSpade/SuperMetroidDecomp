using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// All nine extended frames selected by Crocomire's independently scheduled
/// tongue actor. The four fight frames refer to the empty $A4:CBB3 hitbox
/// list; the five melting frames refer to the empty $A4:CC3B list. Their OAM
/// pointers are deliberately absent so presentation can be edited separately.
/// </summary>
internal static class CrocomireTongueCollisionDefinitions
{

    /// <summary>Whether <paramref name="frame"/> shows one of the tongue actor's nine frames.</summary>
    internal static bool HasFrame(EnemyFrameId? frame) =>
        frame is { } shown && CrocomireTongueFrameDefinitions.ShownFrames.TryFind(shown, out _);

    /// <summary>Fight frames share a single physical component. Melting frame
    /// identities select their pose-specific offsets and common empty hitbox list.
    /// OAM/artwork identity remains separate from this physical geometry.</summary>
    internal static CrocomireTongueCollisionComponent ComponentAt(EnemyFrameId? frame) =>
        frame is { } shown && CrocomireTongueFrameDefinitions.ShownFrames.TryFind(shown, out CrocomireTongueFrame tongue)
            ? ComponentAt(tongue)
            : throw new InvalidDataException($"Crocomire tongue frame {frame} has no compiled collision.");

    /// <summary>Maps fight and melting frame families to their respective empty hitbox lists.</summary>
    internal static CrocomireTongueCollisionComponent ComponentAt(CrocomireTongueFrame frame) => frame switch
    {
        CrocomireTongueFrame.Fight0 or CrocomireTongueFrame.Fight1 or
            CrocomireTongueFrame.Fight2 or CrocomireTongueFrame.Fight3 => new(CrocomireTongueHitboxList.Fight),
        CrocomireTongueFrame.MeltingPose0 or CrocomireTongueFrame.MeltingPose1 or CrocomireTongueFrame.MeltingPose2 or
            CrocomireTongueFrame.MeltingPose3 or CrocomireTongueFrame.MeltingPose4 => new(CrocomireTongueHitboxList.Melting),
        _ => throw new InvalidOperationException($"Undefined CrocomireTongueFrame {frame}."),
    };

    /// <summary>Confirms both native tongue hitbox lists contain no collision boxes.</summary>
    internal static int HitboxCountAt(CrocomireTongueHitboxList list) => list switch
    {
        CrocomireTongueHitboxList.Fight or CrocomireTongueHitboxList.Melting => 0,
        _ => throw new InvalidOperationException($"Undefined CrocomireTongueHitboxList {list}."),
    };
}
