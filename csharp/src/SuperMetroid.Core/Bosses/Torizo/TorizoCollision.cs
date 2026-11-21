using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Finds the body hitboxes of the Torizo frame shown, from <see cref="TorizoCollisionDefinitions"/>.</summary>
internal static partial class TorizoCollision
{
    /// <summary>The body hitboxes of Torizo frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Torizo's frames.</exception>
    internal static TorizoHitboxList HitboxesAt(EnemyFrameId? frame) =>
        frame is { } shown && TorizoCollisionDefinitions.FrameHitboxes.TryGetValue(shown, out TorizoHitboxList? hitboxes)
            ? hitboxes
            : throw new InvalidDataException($"Torizo frame {frame} has no hitboxes.");
}
