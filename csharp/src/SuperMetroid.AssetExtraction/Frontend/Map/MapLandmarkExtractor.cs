using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Imports only landmark positions; validates mixed-record identities against compiled definitions.</summary>
public static class MapLandmarkExtractor
{
    /// <summary>Exports named boss, elevator-destination-label, and Crateria gunship drawing positions from native area-map records.</summary>
    /// <param name="bus">Import-capable cartridge source for bank-$82 boss/elevator lists and the Crateria save-point position list.</param>
    /// <returns>A new UTF-8 JSON buffer mapping compiled landmark identities to area-map pixel anchors before scroll subtraction.</returns>
    /// <remarks>Verifies that each used boss slot shows its named marker's boss, list terminators, and elevator destination spritemaps; discovery, defeated-boss state, and elevator connectivity are not editable output.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    /// <exception cref="InvalidDataException">Required records, unused slots, destination bindings, or list boundaries differ from compiled definitions.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var points = new Dictionary<string, Position16<MapPixels>>();
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            int boss = Pointer(SnesAddress.FromBusAddress(MapSourceDefinitions.Landmarks.BossLists), area);
            var markers = MapLandmarks.BossMarkers(area);
            if (boss == 0 && !markers.IsEmpty) throw new InvalidDataException($"Missing {area} boss list.");
            // Each used slot holds the next named marker, which must show the flag the slot shows; unused slots show none.
            int shown = 0;
            for (int slot = 0; boss != 0 && Read(boss + slot * 4) != ushort.MaxValue; slot++)
            {
                if (slot >= MapSourceDefinitions.Landmarks.BossSlotFlags.Length)
                    throw new InvalidDataException($"Unexpected {area} boss slots.");
                int x = Read(boss + slot * 4);
                if (x == MapSourceDefinitions.Landmarks.UnusedBossSlot)
                    continue;
                if (shown >= markers.Length || markers[shown].Boss != MapSourceDefinitions.Landmarks.BossSlotFlags[slot])
                    throw new InvalidDataException($"{area}'s boss slot {slot} differs from the named boss markers.");
                points.Add(markers[shown].Id, Position16<MapPixels>.Of(x, Read(boss + slot * 4 + 2)));
                shown++;
            }
            if (shown != markers.Length)
                throw new InvalidDataException($"Missing map boss {markers[shown].Id}.");
            if (area == AreaId.Ceres) continue;
            int elevator = Pointer(SnesAddress.FromBusAddress(MapSourceDefinitions.Landmarks.ElevatorLists), area);
            var elevators = MapLandmarks.Elevators(area);
            // The cartridge lists each area's labels in the named labels' drawing order.
            for (int i = 0; i < elevators.Length; i++)
            {
                var label = elevators[i];
                if (Read(elevator + i * 6 + 4) != MapSpriteOrdinals.Spritemaps.Word(MapLandmarks.ElevatorPicture(label.Destination)))
                    throw new InvalidDataException($"Elevator label destination differs from cartridge: {label.Id}.");
                points.Add(label.Id, new(Read(elevator + i * 6), Read(elevator + i * 6 + 2)));
            }
            if (Read(elevator + elevators.Length * 6) != ushort.MaxValue)
                throw new InvalidDataException($"Unexpected {area} elevator labels.");
        }
        int ship = Pointer(FrontendSourceDefinitions.SavePointMapPointers, AreaId.Crateria);
        points.Add(MapLandmarkDefinitions.Gunship, new(Read(ship), Read(ship + 2)));
        using var stream = new MemoryStream();
        MapLandmarkFile.Write(stream, new() { Version = MapLandmarkFormat.Version, Markers = points });
        return stream.ToArray();

        ushort Pointer(SnesAddress table, AreaId area) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), table.AddWithinBank(SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Encode(area) * 2));
        ushort Read(int pointer) => RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), MapSourceDefinitions.Menu.ObjectBank | pointer);
    }
}
