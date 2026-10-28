using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Finds the physical components of Crocomire's body for the frame it shows, living or defeated.</summary>
internal static class CrocomireBodyCollision
{
    /// <summary>Accepts a living-body frame or delegates defeated-body frames to the corpse collision.</summary>
    internal static bool HasFrame(EnemyFrameId? frame) => CrocomireBodyCollisionDefinitions.Frames.Contains(frame) ||
        CrocomireCorpseCollision.HasFrame(frame);

    /// <summary>Finds the physical components, each with its own hitbox list, for a living or defeated frame.</summary>
    internal static ReadOnlySpan<CrocomireBodyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        CrocomireBodyCollisionDefinitions.Frames.TryGet(frame, out CrocomireBodyCollisionComponent[] components)
            ? components
            : CrocomireCorpseCollision.ComponentsAt(frame);
}
