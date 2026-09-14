using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Grapple visual offsets, including adjacent-row results for nibble directions, not physical hand origins.</summary>
public static class GrappleFlarePlacementExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var offsets = new Dictionary<string, ChargeFlareOffset>();
        foreach (bool running in new[] { false, true })
        for (int direction = 0; direction < ChargeFlarePlacementDefinitions.DirectionCount; direction++)
        {
            int x = running ? SamusGrappleRomData.Firing.RunningFlareX : SamusGrappleRomData.Firing.DefaultFlareX;
            int y = running ? SamusGrappleRomData.Firing.RunningFlareY : SamusGrappleRomData.Firing.DefaultFlareY;
            offsets.Add(ChargeFlarePlacementDefinitions.Key(running, direction), new()
            {
                X = unchecked((short)RomDataReader.ReadWordFixedBank(bus, x + direction * 2)),
                Y = unchecked((short)RomDataReader.ReadWordFixedBank(bus, y + direction * 2)),
            });
        }
        return ChargeFlarePlacementCatalog.Write(new() { Version = ChargeFlarePlacementDefinitions.Version, Offsets = offsets });
    }
}
