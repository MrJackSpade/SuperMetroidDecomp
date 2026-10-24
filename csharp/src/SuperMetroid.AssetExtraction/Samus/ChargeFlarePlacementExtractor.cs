using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts actual native visual offsets, retaining adjacent-row results for unnamed directions.</summary>
[CpuBusBoundary("codec: ROM charge-flare visual offset tables → charge flare placements")]
public static class ChargeFlarePlacementExtractor
{
    /// <summary>Reads signed native charge-flare X/Y offsets for every default and running direction row, including unnamed adjacent-direction results.</summary>
    /// <param name="bus">Supported-cartridge address space containing both charge-flare placement tables.</param>
    /// <returns>UTF-8 JSON bytes keyed by running state and direction.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var offsets = new Dictionary<string, ChargeFlareOffset>();
        foreach (bool running in new[] { false, true })
        for (int direction = 0; direction < ChargeFlarePlacementDefinitions.DirectionCount; direction++)
        {
            int x = running ? SamusSourceDefinitions.ChargeFlareRunningXOffsets.Value : SamusSourceDefinitions.ChargeFlareDefaultXOffsets.Value;
            int y = running ? SamusSourceDefinitions.ChargeFlareRunningYOffsets.Value : SamusSourceDefinitions.ChargeFlareDefaultYOffsets.Value;
            offsets.Add(ChargeFlarePlacementDefinitions.Key(running, direction), new()
            {
                X = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), x + direction * 2)),
                Y = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), y + direction * 2)),
            });
        }
        return ChargeFlarePlacementCatalog.Write(new() { Version = ChargeFlarePlacementDefinitions.Version, Offsets = offsets });
    }
}
