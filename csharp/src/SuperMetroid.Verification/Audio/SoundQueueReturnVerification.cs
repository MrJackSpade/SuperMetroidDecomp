using SuperMetroid.AssetExtraction.Audio;
using System.Globalization;

namespace SuperMetroid.Verification.Audio;

/// <summary>Checks sound-queue accumulator results and observable port writes against native capture.</summary>
internal static class SoundQueueReturnVerification
{
    /// <summary>Replays 64 occupancy and suppression rows through queueing, dequeue, and acknowledgement.</summary>
    internal static void VerifySoundQueueAccumulator()
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
            var queue = new AudioQueues();
            queue.AdvanceFrame(default);
            for (int index = 0; index < occupancy; index++)
                queue.QueueSound(Library3Sound.CrocomireAcidDamage, SoundRequestPriority.Essential);
            SoundQueueAdmission actual = queue.QueueSound(
                Library3Sound.SpeedBooster, SoundRequestPriority.Normal, suppressed);
            // The recorded accumulator holds the occupancy in its high byte and the request in its low byte when the
            // threshold admits the request, and the two swapped when it rejects it; the check now asserts the outcome
            // that word encodes instead of the word itself.
            int high = expectedAccumulator >> 8;
            int low = expectedAccumulator & 0xff;
            AssertTrue(high == occupancy || low == occupancy, $"native sound return {row} carries the occupancy");
            SoundQueueAdmission expected = occupancy < 6 && high == occupancy
                ? SoundQueueAdmission.Admitted(occupancy)
                : SoundQueueAdmission.Rejected(occupancy);
            AssertEqual(expected, actual, $"native sound return {row}");

            // Observe queue mutation through real dequeue/acknowledge port writes,
            // independently of the queue call's outcome.
            var sounds = new List<SoundEffectId>();
            SoundEffectId? acknowledgement = null;
            for (int frame = 0; frame < 256; frame++)
            {
                foreach (var command in queue.AdvanceFrame(SoundAcknowledgements<SoundEffectId>.Of(null, null, acknowledgement)))
                {
                    if (command == new AudioRequest.ClearSoundRequest(SoundEffectLibrary.Library3)) acknowledgement = null;
                    else if (command == new AudioRequest.RequestSound(Library3Sound.CrocomireAcidDamage))
                    {
                        acknowledgement = Library3Sound.CrocomireAcidDamage;
                        sounds.Add(Library3Sound.CrocomireAcidDamage);
                    }
                    else if (command == new AudioRequest.RequestSound(Library3Sound.SpeedBooster))
                    {
                        acknowledgement = Library3Sound.SpeedBooster;
                        sounds.Add(Library3Sound.SpeedBooster);
                    }
                    else throw new InvalidDataException($"Unexpected sound-queue port command: {command}");
                }
            }
            AssertEqual(expectedCount, sounds.Count, $"native queue write count {row}");
            AssertEqual(expectedNewSound == SoundEffectCommands.Command(Library3Sound.SpeedBooster) ? 1 : 0,
                sounds.Count(sound => sound == Library3Sound.SpeedBooster),
                $"native queue accepted sound {row}");
        }
    }
}
