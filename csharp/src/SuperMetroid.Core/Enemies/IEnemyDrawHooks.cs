using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose drawing adds to the common enemy drawing.</summary>
internal interface IEnemyDrawHooks : IEnemyEntity
{
    /// <summary>
    /// The shading the common enemy drawing draws the occupant in, hit and freeze flashes included, when its AI owns it;
    /// otherwise null (#1512).
    /// </summary>
    PictureShading? CommonDrawShading();

    /// <summary>Publishes supplemental sprites before the common enemy drawing draws the occupant's body.</summary>
    /// <param name="sprites">The frame's sprite description.</param>
    /// <param name="camera">The camera position.</param>
    void DrawBeforeBody(SpriteFrameBuffer sprites, Position16<RoomPixels> camera);
}
