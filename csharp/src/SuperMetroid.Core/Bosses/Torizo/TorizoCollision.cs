using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Finds the collision components of the Torizo frame shown, from <see cref="TorizoCollisionDefinitions"/>.</summary>
internal static class TorizoCollision
{
    /// <summary>The collision components of Torizo frame <paramref name="frame"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="frame"/> is not one of Torizo's frames.</exception>
    internal static ReadOnlySpan<GoldenTorizoCollisionComponent> ComponentsAt(EnemyFrameId? frame) =>
        frame is { } shown && TorizoCollisionDefinitions.FrameComponents.TryGetValue(shown, out GoldenTorizoCollisionComponent[]? components)
            ? components
            : throw new InvalidDataException($"Torizo frame {frame} is not compiled.");

    /// <summary>The <paramref name="index"/>th left-facing frame in mirror order.</summary>
    internal static GoldenTorizoCollisionComponent[] LeftFacing(int index) => TorizoCollisionDefinitions.LeftFacingFrames[index];
}
