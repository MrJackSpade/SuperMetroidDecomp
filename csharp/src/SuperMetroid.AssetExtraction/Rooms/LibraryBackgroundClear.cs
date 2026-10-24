using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>A library-background clear command: one fill of the work-RAM tilemap staging, then its transfer to VRAM.</summary>
/// <param name="WorkRamSourceAddress">The full bank-$7E address the transfer reads.</param>
/// <param name="WorkRamDestination">The bank-$7E offset the fill starts at.</param>
/// <param name="ByteCount">The bytes filled and transferred.</param>
/// <param name="FillValue">The tilemap word the fill repeats.</param>
/// <param name="VramDestination">Where in VRAM the transfer starts.</param>
internal readonly record struct LibraryBackgroundClear(
    int WorkRamSourceAddress,
    int WorkRamDestination,
    int ByteCount,
    SnesBgTilemapWord FillValue,
    VramDmaDestination VramDestination);
