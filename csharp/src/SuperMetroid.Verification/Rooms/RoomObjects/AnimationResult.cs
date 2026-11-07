namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The frame and timer of one animation result.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Timer">The timer.</param>
internal readonly record struct AnimationResult(int Frame, ushort Timer);
