using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One world-space frame Mother Brain's private writer draws.</summary>
/// <param name="Frame">The frame shown.</param>
/// <param name="World">Its room position.</param>
/// <param name="PaletteIndex">Its packed palette attribute bits.</param>
/// <param name="TileIndexBase">Its packed tile base.</param>
internal readonly record struct MotherBrainWorldFrame(EnemyFrameId Frame, Position16<RoomPixels> World, int PaletteIndex, int TileIndexBase);
