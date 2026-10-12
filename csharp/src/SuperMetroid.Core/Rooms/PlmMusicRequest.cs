using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Rooms;

/// <summary>One delayed music request made by a room PLM instruction.</summary>
public readonly record struct PlmMusicRequest(
    MusicCommand Command,
    MusicCommandDelay Delay);
