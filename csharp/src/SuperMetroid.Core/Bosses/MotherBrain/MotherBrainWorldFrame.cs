using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One frame Mother Brain draws at a room position.</summary>
/// <param name="Frame">The frame shown.</param>
/// <param name="World">Its room position.</param>
/// <param name="Shading">The shading it draws in.</param>
internal readonly record struct MotherBrainWorldFrame(EnemyFrameId Frame, Position16<RoomPixels> World, PictureShading Shading);
