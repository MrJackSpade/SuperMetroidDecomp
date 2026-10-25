using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose drawing adds to the common writer's.</summary>
internal interface IEnemyDrawHooks : IEnemyEntity
{
    /// <summary>The palette row the common writer uses for the occupant, when its AI owns it; otherwise null.</summary>
    int? CommonDrawPalette();

    /// <summary>Publishes supplemental sprites before the common writer publishes the occupant's body.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">The camera position.</param>
    void DrawBeforeBody(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);
}
