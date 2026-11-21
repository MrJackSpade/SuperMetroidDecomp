using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The strips a room object's frame is written in, row by row: where each strip starts from the object's block and how many
/// blocks long it is, in the order the frame writes them.
/// </summary>
/// <param name="Starts">Where each strip starts, from the object's block.</param>
/// <param name="Counts">How many blocks long each strip is.</param>
internal sealed record RoomObjectStripRuns(Offset16<RoomBlocks>[] Starts, int[] Counts);
