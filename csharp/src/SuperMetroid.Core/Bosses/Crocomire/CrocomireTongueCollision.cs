using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The collision of Crocomire's tongue actor: which frames are the tongue's and the component each one carries.</summary>
internal static class CrocomireTongueCollision
{
    /// <summary>Whether <paramref name="frame"/> shows one of the tongue actor's nine frames.</summary>
    internal static bool HasFrame(EnemyFrameId? frame) =>
        frame is { } shown && CrocomireTongueFrameDefinitions.ShownFrames.TryFind(shown, out _);

    /// <summary>The physical component of the tongue frame shown.</summary>
    internal static CrocomireTongueCollisionComponent ComponentAt(EnemyFrameId? frame) =>
        frame is { } shown && CrocomireTongueFrameDefinitions.ShownFrames.TryFind(shown, out CrocomireTongueFrame tongue)
            ? ComponentAt(tongue)
            : throw new InvalidDataException($"Crocomire tongue frame {frame} has no compiled collision.");

    /// <summary>Maps fight and melting frame families to their respective empty hitbox lists.</summary>
    internal static CrocomireTongueCollisionComponent ComponentAt(CrocomireTongueFrame frame) =>
        CrocomireTongueCollisionDefinitions.Components.TryGetValue(frame, out CrocomireTongueCollisionComponent component)
            ? component
            : throw new InvalidOperationException($"Undefined CrocomireTongueFrame {frame}.");

    /// <summary>Confirms both native tongue hitbox lists contain no collision boxes.</summary>
    internal static int HitboxCountAt(CrocomireTongueHitboxList list) =>
        CrocomireTongueCollisionDefinitions.HitboxCounts.TryGetValue(list, out int count)
            ? count
            : throw new InvalidOperationException($"Undefined CrocomireTongueHitboxList {list}.");
}
