using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using static DoorDefinitionsVerification;
using static MapPresentationVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>Checks editable save-marker coordinates against all six native area tables.</summary>
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
                points.Add($"{(AreaId)area}.Save.{station}", new(checked((ushort)x), ReadVerificationWord(rom, address + 2)));
            }
        }
        AssertEqual(34, points.Count, "complete original marker coordinate corpus");
        var original = new MapSaveMarkerDocument { Version = 1, Markers = points };
        byte[] extracted = MapSaveMarkerExtractor.Extract(rom);
        using var serialized = new MemoryStream();
        MapSaveMarkerLayout.Write(serialized, original);
        AssertTrue(extracted.SequenceEqual(serialized.ToArray()), "original marker JSON names, order and values preserved");
        Suite(nameof(VerifyMapSaveMarkerX), () => VerifyMapSaveMarkerX(original, extracted));
        Suite(nameof(VerifyMapSaveMarkerY), () => VerifyMapSaveMarkerY(original, extracted));
    }

    /// <summary>Exercises independent X edits within the map's 512-pixel width.</summary>
    private static void VerifyMapSaveMarkerX(MapSaveMarkerDocument original, byte[] extracted) =>
        Suite(nameof(VerifyMapSaveMarkerCoordinateField), () => VerifyMapSaveMarkerCoordinateField(original, extracted, vertical: false));
    /// <summary>Exercises independent Y edits within the map's 256-pixel height.</summary>
    private static void VerifyMapSaveMarkerY(MapSaveMarkerDocument original, byte[] extracted) =>
        Suite(nameof(VerifyMapSaveMarkerCoordinateField), () => VerifyMapSaveMarkerCoordinateField(original, extracted, vertical: true));

    /// <summary>Round trips single-axis and full-corpus edits while rejecting out-of-range positions.</summary>
    private static void VerifyMapSaveMarkerCoordinateField(MapSaveMarkerDocument original, byte[] extracted, bool vertical)
    {
        MapSaveMarkerLayout stock = MapSaveMarkerLayout.Load(new MemoryStream(extracted, writable: false));
        AssertEqual(0, stock.StoredCoordinateComponentCount, "stock marker coordinates are calculated, not cached");
        Check(original, stock);
        foreach (string id in original.Markers.Keys)
        foreach (int coordinate in new[] { 0, vertical ? 255 : 511 })
        {
            Position16<MapPixels> point = original.Markers[id];
            var editedPoints = new Dictionary<string, Position16<MapPixels>>(original.Markers);
            editedPoints[id] = vertical ? point with { Y = checked((ushort)coordinate) } : point with { X = checked((ushort)coordinate) };
            var editedDocument = original with { Markers = editedPoints };
            MapSaveMarkerLayout edited = RoundTrip(editedDocument);
            AssertEqual(1, edited.StoredCoordinateComponentCount, "one edited axis stores one independent component");
            Check(editedDocument, edited);
        }
        var allEdited = original with { Markers = original.Markers.ToDictionary(pair => pair.Key,
            pair => new Position16<MapPixels>(checked((ushort)(511 - pair.Value.X)), checked((ushort)(255 - pair.Value.Y)))) };
        MapSaveMarkerLayout allCustom = RoundTrip(allEdited);
        AssertEqual(68, allCustom.StoredCoordinateComponentCount, "all independent custom axes are preserved");
        Check(allEdited, allCustom);
        foreach (ushort invalid in new ushort[] { ushort.MaxValue, vertical ? (ushort)256 : (ushort)512 })
        {
            string id = original.Markers.Keys.First();
            var invalidPoints = new Dictionary<string, Position16<MapPixels>>(original.Markers);
            invalidPoints[id] = vertical ? invalidPoints[id] with { Y = invalid }
                : invalidPoints[id] with { X = invalid };
            AssertThrows<InvalidDataException>(() => RoundTrip(original with { Markers = invalidPoints }),
                "coordinate schema bounds remain enforced");
        }
        AssertThrows<InvalidDataException>(() => stock.Get(AreaId.Crateria, 2), "unused marker remains rejected");
        AssertThrows<ArgumentOutOfRangeException>(() => stock.Get(AreaId.Ceres, 0), "unsupported marker area");
        foreach (int invalid in new[] { int.MinValue, -1, 16, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => stock.Get(AreaId.Crateria, invalid), "unsupported marker index");

        void Check(MapSaveMarkerDocument expected, MapSaveMarkerLayout actual)
        {
            for (int area = 0; area < 6; area++)
            for (int station = 0; station < 16; station++)
            {
                string id = $"{(AreaId)area}.Save.{station}";
                if (!expected.Markers.TryGetValue(id, out var point)) continue;
                Position16<MapPixels> result = actual.Get((AreaId)area, station);
                AssertEqual(vertical ? point.Y : point.X, vertical ? result.Y : result.X,
                    $"original/custom marker coordinate {id} vertical={vertical}");
                // The other field must remain independent when this one is edited.
                AssertEqual(point, result, "marker edits preserve the complete supplied point");
                var marker = new FileSelectStationMarker(NewForbiddenMapBus(), (AreaId)area, station, actual);
                AssertEqual(point, marker.MapPosition, "actual marker binds supplied position without ROM access");
            }
        }
        static MapSaveMarkerLayout RoundTrip(MapSaveMarkerDocument document)
        {
            using var stream = new MemoryStream();
            MapSaveMarkerLayout.Write(stream, document);
            stream.Position = 0;
            return MapSaveMarkerLayout.Load(stream);
        }
    }
}
