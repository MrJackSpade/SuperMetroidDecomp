using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The start and entry of one plm program start oracle typed start.</summary>
/// <param name="Start">The start.</param>
/// <param name="Entry">The entry.</param>
internal readonly record struct PlmProgramStartOracleTypedStart(PlmProgramStart Start, ushort Entry);
