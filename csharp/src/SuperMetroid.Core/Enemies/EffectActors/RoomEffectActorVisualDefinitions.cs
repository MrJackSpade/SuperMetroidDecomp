namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// Which room effect actors are pictured on Samus's sheets rather than an enemy sheet. Each animation names the pictures
/// it shows (<see cref="RoomEffectActorPicture"/>, #1817).
/// </summary>
internal static class RoomEffectActorVisualDefinitions
{
    /// <summary>
    /// The room effect actors that are Samus's weapon effects (#1713): the burst her hit shows at an enemy it hurts. They take
    /// the room's effect-actor places like every other, but are pictured on her projectile sheets, in her beams' colouring,
    /// and on no enemy sheet.
    /// </summary>
    internal static IReadOnlySet<RoomEffectActorKind> SamusHitBursts { get; } = new HashSet<RoomEffectActorKind>
    {
        RoomEffectActorKind.EnemyHit,
    };
}
