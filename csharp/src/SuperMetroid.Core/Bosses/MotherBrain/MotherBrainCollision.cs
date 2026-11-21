using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using static SuperMetroid.Core.Bosses.MotherBrain.MotherBrainCollisionDefinitions;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Finds the collision components of the Mother Brain frame shown.</summary>
internal static class MotherBrainCollision
{
    /// <summary>
    /// Each body frame's components, checked once (#2553) to name only the body's ignoring touch and dud shot or, on the
    /// brain's rectangle the body shows until its first animation (#2220), the brain's touch and shot, so the body's shot
    /// reaction tells the brain's rectangle from the body's by the shot it names. The body's contact uses its definition's
    /// radius, so no frame's touch is read for it.
    /// </summary>
    private static Dictionary<EnemyFrameId, EnemyCollisionComponent[]> Components { get; } = EnemyHitboxReactionCheck.Checked(
        FrameComponents,
        FrameComponents.Values.SelectMany(components => components).SelectMany(component => component.Hitboxes),
        "Mother Brain's body",
        [EnemyReaction.Ignore, MotherBrainReactions.HeadTouch],
        [MotherBrainReactions.BodyShot, MotherBrainReactions.HeadShot]);

    /// <summary>The collision components of Mother Brain body frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Mother Brain's body frames.</exception>
    internal static ReadOnlySpan<EnemyCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && Components.TryGetValue(shown, out EnemyCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException($"Mother Brain has no frame {frame} with hitboxes.");
}
