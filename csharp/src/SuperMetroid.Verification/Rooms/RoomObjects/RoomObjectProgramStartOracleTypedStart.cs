using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The start and entry of one roomObject program start oracle typed start.</summary>
/// <param name="Start">The start.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct RoomObjectProgramStartOracleTypedStart(RoomObjectProgramStart Start, ushort Entry);
