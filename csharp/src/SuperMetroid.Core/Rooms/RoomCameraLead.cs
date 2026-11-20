namespace SuperMetroid.Core.Rooms;

/// <summary>
/// How far above Samus a room's camera aims its top: one distance while she moves any way but up, leading the view
/// below her as she falls, and one while she moves up, leading the view above her as she rises (#2482).
/// </summary>
/// <param name="FallingOffset">How many pixels above Samus the camera aims its top while she moves any way but up; zero or more.</param>
/// <param name="RisingOffset">How many pixels above Samus the camera aims its top while she moves up; zero or more.</param>
public readonly record struct RoomCameraLead(int FallingOffset, int RisingOffset);
