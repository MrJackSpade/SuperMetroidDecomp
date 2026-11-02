using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts the native visual offsets of the ten named aiming directions and the standing turning placements.</summary>
public static class ChargeFlarePlacementExtractor
{
    /// <summary>JSON resource filename for beam-charge flare drawing offsets; editing these signed pixel placements does not change physical projectile origins or charge timing.</summary>
    public const string FileName = "charge-flare-placement.json";

    /// <summary>Reads signed native charge-flare X/Y offsets for every named direction of the default and running rows and the default rows' turning placements.</summary>
    /// <param name="bus">Supported-cartridge address space containing both charge-flare placement tables.</param>
    /// <returns>UTF-8 JSON bytes keyed by running state and direction.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var offsets = new Dictionary<string, ChargeFlareOffset>();
        foreach (bool running in new[] { false, true })
        for (int direction = 0; direction < (running ? ChargeFlarePlacementDefinitions.DirectionCount : ChargeFlarePlacementDefinitions.StandingDirectionCount); direction++)
        {
            int x = running ? SamusSourceDefinitions.ChargeFlareRunningXOffsets.Value : SamusSourceDefinitions.ChargeFlareDefaultXOffsets.Value;
            int y = running ? SamusSourceDefinitions.ChargeFlareRunningYOffsets.Value : SamusSourceDefinitions.ChargeFlareDefaultYOffsets.Value;
            offsets.Add(ChargeFlarePlacementFile.Key(running, direction), new()
            {
                X = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), x + direction * 2)),
                Y = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), y + direction * 2)),
            });
        }
        return ChargeFlarePlacementFile.Write(new() { Version = ChargeFlarePlacementFile.Version, Offsets = offsets });
    }
}
