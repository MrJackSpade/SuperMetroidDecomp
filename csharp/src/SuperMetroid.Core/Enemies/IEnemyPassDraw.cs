using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Sprites an encounter draws at one of the depths shared drawing gives encounters (#2571): during the enemy pass, before
/// the queued enemies' layers, or over every ordinary enemy layer. The enemy phase and the actor drawing draw every such
/// encounter the room's species compose at its depth, in their order.
/// </summary>
internal interface IEnemyPassDraw
{
    /// <summary>Draws what the encounter shows during the enemy pass, before the queued enemies' layers.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="camera">The camera the frame draws from.</param>
    void DrawDuringEnemyPass(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);

    /// <summary>Draws what the encounter shows over every ordinary enemy layer.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="camera">The camera the frame draws from.</param>
    void DrawOverEnemies(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);
}
