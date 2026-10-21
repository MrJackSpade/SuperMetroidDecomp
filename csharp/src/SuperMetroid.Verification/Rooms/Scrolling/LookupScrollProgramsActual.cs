using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Verification.Rooms.Scrolling;

/// <summary>The index and state of one lookup scroll programs actual.</summary>
/// <param name="Index">The index.</param>
/// <param name="State">The state.</param>
internal readonly record struct LookupScrollProgramsActual(int Index, RoomScrollState State);
