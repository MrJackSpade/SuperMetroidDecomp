using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One step of a Torizo's walk facing one way on one leg (#2411): the foot the step stands on, the moving leg's five poses,
/// and how far forward the body moves after each. Each walk is written once over a stride.
/// </summary>
/// <param name="Foot">The foot the step stands on, which its step cue records.</param>
/// <param name="Lifting">The leg lifting.</param>
/// <param name="Swinging">The leg swinging forward.</param>
/// <param name="Reaching">The leg reaching out.</param>
/// <param name="Planting">The leg planting.</param>
/// <param name="Planted">The leg planted.</param>
/// <param name="Paces">How far forward the body moves after each pose.</param>
internal sealed record TorizoStride(
    HorizontalDirection Foot, EnemyFrameId Lifting, EnemyFrameId Swinging, EnemyFrameId Reaching, EnemyFrameId Planting, EnemyFrameId Planted,
    TorizoStridePaces Paces);
