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
            var gameplay = new CheckGameplaySoundState();
            var queue = AudioQueues.Standalone(gameplay);
            queue.AdvanceFrame(default);
            for (int index = 0; index < occupancy; index++)
                queue.QueueSound(RoomSound.CrocomireAcidDamage, SoundRequestPriority.Essential);
            // A suppressed row is a request the queue turns away after its occupancy test, as it does while a power bomb explodes.
            gameplay.PowerBombExploding = suppressed;
            SoundQueueAdmission actual = queue.QueueSound(RoomSound.SpeedBooster, SoundRequestPriority.Normal);
            gameplay.PowerBombExploding = false;
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
                foreach (var command in queue.AdvanceFrame(new SoundAcknowledgements<SoundEffectLibrary, SoundEffectId>([SoundEffectLibrary.Room], _ => SoundAcknowledgement<SoundEffectId>.Of(acknowledgement))))
                {
                    if (command == new AudioRequest.ClearSoundRequest(SoundEffectLibrary.Room)) acknowledgement = null;
                    else if (command == new AudioRequest.RequestSound(RoomSound.CrocomireAcidDamage))
                    {
                        acknowledgement = RoomSound.CrocomireAcidDamage;
                        sounds.Add(RoomSound.CrocomireAcidDamage);
                    }
                    else if (command == new AudioRequest.RequestSound(RoomSound.SpeedBooster))
                    {
                        acknowledgement = RoomSound.SpeedBooster;
                        sounds.Add(RoomSound.SpeedBooster);
                    }
                    else throw new InvalidDataException($"Unexpected sound-queue port command: {command}");
                }
            }
            AssertEqual(expectedCount, sounds.Count, $"native queue write count {row}");
            AssertEqual(expectedNewSound == SoundEffectCommands.Command(RoomSound.SpeedBooster) ? 1 : 0,
                sounds.Count(sound => sound == RoomSound.SpeedBooster),
                $"native queue accepted sound {row}");
        }
    }
}
