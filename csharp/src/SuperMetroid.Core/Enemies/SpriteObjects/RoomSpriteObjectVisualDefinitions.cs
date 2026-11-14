namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// Which room sprite objects are pictured on Samus's sheets rather than an enemy sheet. Each animation names the pictures
/// it shows (<see cref="RoomSpriteObjectPicture"/>, #1817).
/// </summary>
internal static class RoomSpriteObjectVisualDefinitions
{
    /// <summary>
    /// The room sprite objects that are Samus's weapon effects (#1713): the burst her hit shows at an enemy it hurts. They take
    /// the room's sprite-object places like every other, but are pictured on her projectile sheets, in her beams' colouring,
    /// and on no enemy sheet.
    /// </summary>
    internal static IReadOnlySet<RoomSpriteObjectKind> SamusHitBursts { get; } = new HashSet<RoomSpriteObjectKind>
    {
        RoomSpriteObjectKind.EnemyHit,
    };
}
