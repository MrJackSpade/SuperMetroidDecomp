using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Grapple visual offsets, including adjacent-row results for nibble directions, not physical hand origins.</summary>
public static class GrappleFlarePlacementExtractor
{
    /// <summary>Exports Grapple's standing and running visual flare offsets for all sixteen low-nibble direction selectors.</summary>
    /// <param name="bus">Import-capable cartridge source for $9B:C14A/C15E and $9B:C19A/C1AE, including bounded reads into adjacent rows for selectors 10..15.</param>
    /// <returns>A new UTF-8 JSON buffer with thirty-two movement-mode/direction keys and signed pixel X/Y offsets from Samus's drawing origin.</returns>
    /// <remarks>Uses the shared charge-flare placement schema, but reads Grapple's tables rather than beam-charge origins; physical hand/beam origins are not exported.</remarks>
    /// <exception cref="ArgumentException"><paramref name="bus"/> does not provide cartridge import access, including null.</exception>
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
                X = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), x + direction * 2)),
                Y = unchecked((short)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), y + direction * 2)),
            });
        }
        return ChargeFlarePlacementCatalog.Write(new() { Version = ChargeFlarePlacementDefinitions.Version, Offsets = offsets });
    }
}
