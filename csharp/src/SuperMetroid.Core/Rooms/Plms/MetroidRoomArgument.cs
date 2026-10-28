namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The entry of the Metroids-cleared table a Tourian PLM selects.</summary>
/// <param name="Entry">The table entry, 0 through 12.</param>
public sealed record MetroidRoomArgument(int Entry) : PlmRoomArgument;
