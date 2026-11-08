using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// A picture fixed in the room around a perspective floor (#1545), drawn behind the floor and the actors: the getaway
/// chamber's walls stand still while Ceres Ridley's floor turns in front of them.
/// </summary>
/// <param name="Picture">The installed picture, whose frame origin lands on <paramref name="Point"/>.</param>
/// <param name="Point">The room point the picture is placed at.</param>
/// <param name="Shading">The shades and colour effects the picture shows.</param>
public sealed record PerspectiveFloorScenery(SpriteSheetFrame Picture, Position16<RoomPixels> Point, PictureShading Shading);
