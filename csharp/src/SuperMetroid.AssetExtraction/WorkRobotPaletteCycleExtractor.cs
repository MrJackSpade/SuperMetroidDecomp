using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the four RGB5 words preceding each compiled Work Robot timer.</summary>
public static class WorkRobotPaletteCycleExtractor
{
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
                ushort native = RomDataReader.ReadWordFixedBank(bus, source);
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
