using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Finds the physical components of Crocomire's body for the frame it shows, living or defeated.</summary>
internal static class CrocomireBodyCollision
{
    /// <summary>Accepts a living-body frame or delegates defeated-body frames to the corpse collision.</summary>
    internal static bool HasFrame(EnemyFrameId? frame) => Frames.Contains(frame) ||
        CrocomireCorpseCollision.HasFrame(frame);

    /// <summary>
    /// The living body's frames' components, checked once with the corpse's (#2553) to name only the touches and shots
    /// Crocomire answers: the ignoring, claw and ordinary touch, and the ordinary, absorbed, dust and mouth shot.
    /// </summary>
    private static EnemyFrameCollision<EnemyCollisionComponent> Frames { get; } = EnemyHitboxReactionCheck.Checked(
        CrocomireBodyCollisionDefinitions.Frames,
        CrocomireBodyCollisionDefinitions.Frames.SelectMany(frame => frame.Value)
            .Concat(CrocomireCorpseCollisionDefinitions.Frames.Values.SelectMany(components => components))
            .SelectMany(component => component.Hitboxes),
        "Crocomire",
        [EnemyReaction.Ignore, CrocomireReactions.ClawTouch, EnemyReaction.OrdinaryTouch],
        [EnemyReaction.OrdinaryShot, CrocomireReactions.AbsorbedShot, CrocomireReactions.DustShot, CrocomireReactions.MouthShot]);

    /// <summary>Finds the physical components, each with its own hitbox list, for a living or defeated frame.</summary>
    internal static ReadOnlySpan<EnemyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        Frames.TryGet(frame, out EnemyCollisionComponent[] components)
            ? components
            : CrocomireCorpseCollision.ComponentsAt(frame);
}
