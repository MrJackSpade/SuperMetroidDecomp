namespace SuperMetroid.Core.Game;

/// <summary>One frame of the save station's electricity: its position in the eight-frame cycle.</summary>
/// <param name="Phase">The frame's position in the cycle, from zero.</param>
internal readonly record struct SaveStationElectricityFrame(int Phase);