using System.Reflection;
using SuperMetroid.Core.Audio;

/// <summary>Compares sequencer register state at original-SPC loop boundaries, not PCM.</summary>
internal static class SpcTickComparisonAudit
{
    public static int Run(string directory, int musicAddress, string tracePath, int expectedTicks = 3000)
    {
        var assets = ExtractedAudioAssetCatalog.Load(directory);
        var player = new ManagedSpcPlayer();
        player.Upload(assets.GetUpload(AudioUploadAddresses.SpcEngine).Span);
        player.Upload(assets.GetUpload(musicAddress).Span);
        player.SetSampleBank(assets.GetSampleBank(musicAddress));
        // The CPU probe loads RAM before running reset, without an upload handshake.
        // Undo constructor/upload scalar state before executing the same reset. In
        // particular a second reset with lastWrittenEchoDelay still set would skip
        // the initial EDL write after DSP.Reset and create a fixture-only mismatch.
        foreach (var field in typeof(ManagedSpcPlayer).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (field.FieldType.IsValueType && !field.IsInitOnly)
                field.SetValue(player, Activator.CreateInstance(field.FieldType));
        }
        foreach (string ports in new[] { "inputPorts", "portsToSnes" })
            Array.Clear((byte[])(typeof(ManagedSpcPlayer).GetField(ports,
                BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(player)!));
        // Diagnostic-only entry into the production driver loop; no new runtime API
        // or replacement timing policy is installed in the application.
        MethodInfo Method(string name) => typeof(ManagedSpcPlayer).GetMethod(name,
            BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new MissingMethodException(name);
        var initialize = Method("InitializeDriver");
        var partOne = Method("LoopPartOne");
        var partTwo = Method("LoopPartTwo");
        int ticks = 0, mismatchedTicks = 0;
        byte[] latchedInputs = new byte[4];
        byte[] deliveredInputs = new byte[4];
        foreach (string line in File.ReadLines(tracePath))
        {
            if (!line.StartsWith("T ", StringComparison.Ordinal)) continue;
            string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int tick = int.Parse(fields[1]);
            if (tick != ticks) throw new InvalidDataException("SPC trace has a missing or repeated tick.");
            // Original RAM retains the latched port value; the managed command API
            // consumes a write event. Reposting the same byte each tick would restart
            // SFX initialization indefinitely and produce a fixture-only divergence.
            for (int port = 0; port < 4; port++)
                if (latchedInputs[port] != deliveredInputs[port])
                {
                    player.WritePort(port, latchedInputs[port]);
                    deliveredInputs[port] = latchedInputs[port];
                }
            if (tick == 0) initialize.Invoke(player, null);
            else { partTwo.Invoke(player, [byte.Parse(fields[2])]); partOne.Invoke(player, null); }
            byte[] expected = Convert.FromHexString(fields[3]);
            if (expected.Length != 128) throw new InvalidDataException("SPC trace needs the complete register file.");
            var differences = new List<string>();
            for (byte reg = 0; reg < 128; reg++)
            {
                // ENVX/OUTX/ENDX belong to sample execution. This probe compares
                // sequencer writes without pretending that its DSP was cycle-stepped.
                if ((reg & 15) is 8 or 9 || reg == SnesDspRegisterMap.Global.EndFlags) continue;
                byte actual = player.ReadDspRegisterForVerification(reg);
                if (actual != expected[reg]) differences.Add($"{reg:X2}:{actual:X2}!={expected[reg]:X2}");
            }
            if (differences.Count != 0)
            {
                if (mismatchedTicks < 12 || tick == 1800 || tick == 2300 || tick == 2999) Console.WriteLine($"DIFF tick={tick} {string.Join(',', differences)}");
                mismatchedTicks++;
            }
            latchedInputs = Convert.FromHexString(fields[4]);
            if (latchedInputs.Length != 4) throw new InvalidDataException("SPC trace needs four latched input bytes.");
            ticks++;
        }
        Console.WriteLine($"SPC tick comparison: ticks={ticks}, mismatched={mismatchedTicks}. Scope: latched-input-aligned sequencer registers, not PCM or host port timing.");
        return ticks == expectedTicks && mismatchedTicks == 0 ? 0 : 1;
    }
}
