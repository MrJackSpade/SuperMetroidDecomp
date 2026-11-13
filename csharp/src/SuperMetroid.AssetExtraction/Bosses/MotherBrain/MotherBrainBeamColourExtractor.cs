using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>
/// Installs the colours of Mother Brain's rainbow beam backdrop by purpose (#1637): its first colour and the colours of its
/// hue cycle, without the cycle's terminator or the table they were kept in.
/// </summary>
internal static class MotherBrainBeamColourExtractor
{
    /// <summary>Imports the beam backdrop's colours, checking the cycle's terminator.</summary>
    /// <param name="bus">Cartridge import source for the beam's colours.</param>
    /// <returns>The installed document's bytes.</returns>
    /// <exception cref="InvalidDataException">The cycle ends early or lacks its terminator.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        var cycle = new RgbColor[MotherBrainRainbowPaletteGeometry.BeamCycleColorCount];
        for (int index = 0; index < cycle.Length; index++)
        {
            int word = cartridge.ReadCartridgeWord(MotherBrainSourceDefinitions.Beam.ColorTable +
                index * MotherBrainSourceDefinitions.Beam.ColorEntryBytes);
            if (!RgbColor.IsColorWord(checked((ushort)word)))
                throw new InvalidDataException($"Mother Brain beam cycle ends before color {index}.");
            cycle[index] = RgbColor.FromWord(checked((ushort)word));
        }
        if (cartridge.ReadCartridgeWord(MotherBrainSourceDefinitions.Beam.ColorTable +
            cycle.Length * MotherBrainSourceDefinitions.Beam.ColorEntryBytes) != ushort.MaxValue)
            throw new InvalidDataException("Mother Brain beam cycle lacks its native signed terminator.");
        return MotherBrainBeamColourFile.Write(new MotherBrainRainbowPalettePresentation(MotherBrainSourceDefinitions.Beam.InitialColor, cycle));
    }
}
