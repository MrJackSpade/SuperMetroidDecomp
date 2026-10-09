using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the four RGB5 words preceding each compiled Work Robot timer.</summary>
public static class WorkRobotPaletteCycleExtractor
{
    /// <summary>Exports the four color operands of each of the six Work Robot palette records at $A8:CCC1, excluding their timer words and terminator.</summary>
    /// <param name="bus">Non-null import-capable cartridge source for six ten-byte color/timer records.</param>
    /// <returns>A new UTF-8 JSON buffer with six ordered four-color RGB5 frames for OBJ-row colors nine through twelve, with channels 0..31.</returns>
    /// <remarks>The other twelve OBJ colors, compiled 64/16/16-update cadence, and wrap at the separate terminator are not imported into this artwork document.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">A source color sets unrepresentable bit 15.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new PaletteRgb5[WorkRobotPaletteTimingDefinitions.RecordCount][];
        for (int frame = 0; frame < frames.Length; frame++)
        {
            frames[frame] = new PaletteRgb5[WorkRobotPaletteRomData.ColorCount];
            for (int color = 0; color < frames[frame].Length; color++)
            {
                int source = EnemyRomTablePointers.WorkRobot.PaletteAnimationRecords +
                    frame * WorkRobotPaletteTimingDefinitions.RecordByteCount +
                    color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source);
                if ((native & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Work Robot color ${source:X6} has an unrepresentable high bit.");
                frames[frame][color] = new PaletteRgb5
                {
                    Red = native & 31,
                    Green = native >> 5 & 31,
                    Blue = native >> 10 & 31,
                };
            }
        }
        return WorkRobotPaletteCycle.Write(new WorkRobotPaletteCycleDocument
        {
            Version = WorkRobotPaletteCycleFormat.Version,
            Frames = frames,
        });
    }
}
