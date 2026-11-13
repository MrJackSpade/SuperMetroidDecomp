using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The header and source of one verify downward gate header definitions case.</summary>
/// <param name="Header">The header.</param>
/// <param name="Source">The source.</param>
internal readonly record struct VerifyDownwardGateHeaderDefinitionsCase(RoomObjectKind Header, int Source);
