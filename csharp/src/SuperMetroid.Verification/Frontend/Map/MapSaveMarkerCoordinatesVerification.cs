using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>Checks installed save-marker coordinates against all six native area tables.</summary>
internal static class MapSaveMarkerCoordinatesVerification
{
    /// <summary>Builds the independent 34-marker cartridge corpus and compares its serialized presentation.</summary>
    internal static void VerifyMapSaveMarkerCoordinates(ISnesAddressSpace rom)
    {
        var points = new Dictionary<string, Position16<MapPixels>>();
        for (int area = 0; area < 6; area++)
        {
            int pointer = ReadVerificationWord(rom, 0x82c80b + area * 2);
            for (int station = 0; station < 16; station++)
            {
                int address = 0x820000 | (pointer + station * 4);
                int x = ReadVerificationWord(rom, address);
                AssertTrue(x != ushort.MaxValue, "native marker view has sixteen slots");
                if (x == ushort.MaxValue - 1) continue;
                LoadStation marked = SuperMetroid.AssetExtraction.Progression.LoadStationNumberCodec.Find(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(area), station)
                    ?? throw new InvalidDataException($"Native marker slot {area}/{station} numbers no load station.");
                points.Add(marked.Name, new(checked((ushort)x), ReadVerificationWord(rom, address + 2)));
            }
        }
        AssertEqual(34, points.Count, "complete original marker coordinate corpus");
        var original = new MapSaveMarkerDocument { Version = MapSaveMarkerFormat.Version, Markers = points };
        byte[] extracted = MapSaveMarkerExtractor.Extract(rom);
        using var serialized = new MemoryStream();
        MapSaveMarkerFile.Write(serialized, original);
        AssertTrue(extracted.SequenceEqual(serialized.ToArray()), "original marker JSON names, order and values preserved");
        Suite(nameof(VerifyMapSaveMarkerX), () => VerifyMapSaveMarkerX(original, extracted));
        Suite(nameof(VerifyMapSaveMarkerY), () => VerifyMapSaveMarkerY(original, extracted));
    }

    /// <summary>Checks the stock X coordinates and the map's 512-pixel width bound.</summary>
    private static void VerifyMapSaveMarkerX(MapSaveMarkerDocument original, byte[] extracted) =>
        Suite(nameof(VerifyMapSaveMarkerCoordinateField), () => VerifyMapSaveMarkerCoordinateField(original, extracted, vertical: false));
    /// <summary>Checks the stock Y coordinates and the map's 256-pixel height bound.</summary>
    private static void VerifyMapSaveMarkerY(MapSaveMarkerDocument original, byte[] extracted) =>
        Suite(nameof(VerifyMapSaveMarkerCoordinateField), () => VerifyMapSaveMarkerCoordinateField(original, extracted, vertical: true));

    /// <summary>Checks the stock coordinates and markers while rejecting out-of-range positions.</summary>
    private static void VerifyMapSaveMarkerCoordinateField(MapSaveMarkerDocument original, byte[] extracted, bool vertical)
    {
        MapSaveMarkerPositions stock = MapSaveMarkerFile.Read(new MemoryStream(extracted, writable: false));
        Check(original, stock);
        foreach (ushort invalid in new ushort[] { ushort.MaxValue, vertical ? (ushort)256 : (ushort)512 })
        {
            string id = original.Markers.Keys.First();
            var invalidPoints = new Dictionary<string, Position16<MapPixels>>(original.Markers);
            invalidPoints[id] = vertical ? invalidPoints[id] with { Y = invalid }
                : invalidPoints[id] with { X = invalid };
            AssertThrows<InvalidDataException>(() => RoundTrip(original with { Markers = invalidPoints }),
                "coordinate schema bounds remain enforced");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Get(LoadStationDefinitions.ZebesArrival), "a station with no marker is rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Get(LoadStationDefinitions.CeresElevator), "Ceres has no marker");

        void Check(MapSaveMarkerDocument expected, MapSaveMarkerPositions actual)
        {
            foreach (LoadStation station in LoadStationDefinitions.All)
            {
                string id = station.Name;
                if (!expected.Markers.TryGetValue(id, out var point)) continue;
                Position16<MapPixels> result = actual.Get(station);
                AssertEqual(vertical ? point.Y : point.X, vertical ? result.Y : result.X,
                    $"original marker coordinate {id} vertical={vertical}");
                AssertEqual(point, result, "stock marker keeps its complete point");
                var marker = new FileSelectStationMarker(station, actual);
                AssertEqual(point, marker.MapPosition, "actual marker binds supplied position without ROM access");
            }
        }
        static MapSaveMarkerPositions RoundTrip(MapSaveMarkerDocument document)
        {
            using var stream = new MemoryStream();
            MapSaveMarkerFile.Write(stream, document);
            stream.Position = 0;
            return MapSaveMarkerFile.Read(stream);
        }
    }
}
