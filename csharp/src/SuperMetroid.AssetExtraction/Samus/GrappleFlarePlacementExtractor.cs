using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Extracts Grapple visual offsets for the ten aiming directions, not physical hand origins.</summary>
public static class GrappleFlarePlacementExtractor
{
    /// <summary>Exports Grapple's standing and running visual flare offsets for the ten aiming directions.</summary>
    /// <param name="bus">Import-capable cartridge source for $9B:C14A/C15E and $9B:C19A/C1AE.</param>
    /// <returns>A new UTF-8 JSON buffer with twenty movement-mode/direction keys and signed pixel X/Y offsets from Samus's drawing origin.</returns>
    /// <remarks>Uses the shared charge-flare placement schema, but reads Grapple's tables rather than beam-charge origins; physical hand/beam origins are not exported.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var offsets = new Dictionary<string, ChargeFlareOffset>();
        foreach (ChargeFlarePlacement placement in ChargeFlarePlacements.Placements(ChargeFlareSource.Grapple))
        {
            int x = placement.Running ? SamusSourceDefinitions.GrappleRunningFlareX.Value : SamusSourceDefinitions.GrappleDefaultFlareX.Value;
            int y = placement.Running ? SamusSourceDefinitions.GrappleRunningFlareY.Value : SamusSourceDefinitions.GrappleDefaultFlareY.Value;
            int row = SamusProjectileDirectionCodec.FlarePlacementRow(placement.Aim);
            offsets.Add(ChargeFlarePlacementFile.Key(placement), new()
            {
                X = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), x + row * 2)),
                Y = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), y + row * 2)),
            });
        }
        return ChargeFlarePlacementFile.WriteGrapple(new() { Version = ChargeFlarePlacementFile.Version, Offsets = offsets });
    }
}
