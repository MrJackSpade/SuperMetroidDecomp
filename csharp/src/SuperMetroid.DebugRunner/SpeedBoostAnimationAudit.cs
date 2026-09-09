using System.Globalization;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Native queue-dependent stage-transition gate, with an explicitly matched audio queue.</summary>
internal static class SpeedBoostAnimationAudit
{
    public static int Run(string romPath, string capturePath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        string[] rows = File.ReadAllLines(capturePath);
        if (rows.Length != 65)
            throw new InvalidDataException("Expected all 64 native queue/animation contexts.");
        int failures = 0;
        foreach (string row in rows.Skip(1))
        {
            string[] fields = row.Split(',');
            if (fields.Length != 7)
                throw new InvalidDataException("Changed native animation capture schema.");
            ushort expectedCounter = ushort.Parse(fields[2], NumberStyles.HexNumber);
            ushort expectedFrame = ushort.Parse(fields[3], NumberStyles.HexNumber);
            ushort expectedTimer = ushort.Parse(fields[4], NumberStyles.HexNumber);
            int occupancy = int.Parse(fields[0], CultureInfo.InvariantCulture);
            var audio = new CartridgeAudioState();
            for (int index = 0; index < occupancy; index++)
                audio.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x22), 15);
            var speed = new SamusHorizontalSpeedState
            {
                HasRunningMomentum = true,
                SpeedBoostCounter = 0x0301,
            };
            ushort frame = 10;
            bool changed = speed.TryAdvanceSpeedBoosterAnimationStage(bus,
                SamusMovementType.Running, (ushort)SnesButton.B, 0, ref frame, out ushort timer,
                () => audio.QueueSoundAndGetAccumulator(
                    SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 3), 6,
                    soundSuppressed: fields[1] != "0"));
            // Use the real queue operation; no expected timer/register is fed back
            // into the routine. The frontend still needs this synchronous handoff.
            if (speed.ConsumeEchoSoundRequest())
                throw new InvalidDataException("Synchronous sound call also emitted a duplicate deferred request.");
            if (!changed || speed.SpeedBoostCounter != expectedCounter ||
                frame != expectedFrame || timer != expectedTimer)
            {
                failures++;
                if (failures <= 8)
                    Console.WriteLine($"occupancy={fields[0]} suppression={fields[1]}: " +
                        $"{speed.SpeedBoostCounter:X4}/{frame:X4}/{timer:X4} != " +
                        $"{expectedCounter:X4}/{expectedFrame:X4}/{expectedTimer:X4}");
            }
        }
        Console.WriteLine($"Speed Booster animation: 64 native cases, {failures} mismatches.");
        return failures == 0 ? 0 : 1;
    }
}
