using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// A room actor that stands in front of Samus (#2571): the actor drawing draws every such actor the room holds after the
/// bomb and projectile explosions and before Samus.
/// </summary>
internal interface IActorInFrontOfSamus
{
    /// <summary>Draws the actor in front of Samus.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="presentation">How the room's enemies look, whose artwork the actor draws with.</param>
    /// <param name="camera">The camera the frame draws from.</param>
    void DrawInFrontOfSamus(SpriteFrameBuffer sprites, EnemyPresentation presentation, Position16<RoomPixels> camera);
}
