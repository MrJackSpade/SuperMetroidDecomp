using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Sprites an encounter draws after every queued enemy has drawn (#2571). A late draw may read the room's random numbers as
/// it draws (Mother Brain's brain), so the draw list runs every late draw the room's species compose once each draw pass, in
/// their order, after the queued enemies.
/// </summary>
internal interface IEnemyLateDraw
{
    /// <summary>Draws the encounter's late sprites after every queued enemy.</summary>
    /// <param name="sprites">The frame's sprites.</param>
    /// <param name="camera">The camera the frame draws from.</param>
    /// <param name="samus">Samus, whom a late draw may follow.</param>
    void DrawAfterEnemies(SpriteFrameBuffer sprites, Position16<RoomPixels> camera, SamusState samus);
}
