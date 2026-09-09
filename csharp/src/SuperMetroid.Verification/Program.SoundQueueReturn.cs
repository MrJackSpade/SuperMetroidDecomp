using System.Globalization;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySoundQueueAccumulator(SuperMetroidAddressSpace bus)
    {
        string fixture = Path.GetFullPath("csharp/test-fixtures/movement-release/soundqueue-return.csv");
        string[] rows = File.ReadAllLines(fixture);
        AssertEqual(65, rows.Length, "native sound-queue capture dimensions");
        foreach (string row in rows.Skip(1))
        {
            string[] fields = row.Split(',');
            int occupancy = int.Parse(fields[0], CultureInfo.InvariantCulture);
            bool suppressed = fields[1] != "0";
            ushort expectedAccumulator = ushort.Parse(fields[2], NumberStyles.HexNumber);
            int expectedCount = int.Parse(fields[3], CultureInfo.InvariantCulture);
            int expectedNewSound = int.Parse(fields[4], CultureInfo.InvariantCulture);
            var queue = new CartridgeAudioState();
            queue.AdvanceFrame(bus, default);
            for (int index = 0; index < occupancy; index++)
                queue.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x22), 15);
            ushort actual = queue.QueueSoundAndGetAccumulator(
                SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 3), 6, suppressed);
            AssertEqual(expectedAccumulator, actual, $"native sound return {row}");

            // Observe queue mutation through real dequeue/acknowledge port writes,
            // independently of the newly exposed accumulator calculation.
            var sounds = new List<byte>();
            byte acknowledgement = 0;
            for (int frame = 0; frame < 256; frame++)
            {
                foreach (var command in queue.AdvanceFrame(bus,
                    new CartridgeAudioAcknowledgements(0, 0, 0, acknowledgement)))
                {
                    if (command == CartridgeAudioCommand.WritePort(3, 0)) acknowledgement = 0;
                    else if (command == CartridgeAudioCommand.WritePort(3, 0x22))
                    {
                        acknowledgement = 0x22;
                        sounds.Add(0x22);
                    }
                    else if (command == CartridgeAudioCommand.WritePort(3, 3))
                    {
                        acknowledgement = 3;
                        sounds.Add(3);
                    }
                    else throw new InvalidDataException($"Unexpected sound-queue port command: {command}");
                }
            }
            AssertEqual(expectedCount, sounds.Count, $"native queue write count {row}");
            AssertEqual(expectedNewSound == 3 ? 1 : 0, sounds.Count(sound => sound == 3),
                $"native queue accepted sound {row}");
        }
    }
}
