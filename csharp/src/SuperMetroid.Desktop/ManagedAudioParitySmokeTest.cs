using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Desktop;

public readonly record struct ManagedAudioParitySmokeTestResult(
    int MusicFrames,
    int SoundEffectFrames,
    int ComparedPcmSamples,
    int ComparedAcknowledgements,
    int MusicBankScenarios,
    int SoundEffectScenarios);

/// <summary>
/// Temporary migration oracle. It gives the managed port and the pinned native translation
/// identical cartridge commands, then requires every PCM sample and acknowledgement byte to
/// agree. The native runtime dependency can be removed once this audit is green.
/// </summary>
public static class ManagedAudioParitySmokeTest
{
    public static ManagedAudioParitySmokeTestResult Run(
        string romPath,
        int musicFrames = 600,
        int soundEffectFrames = 120)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        int samples = CompareMusic(bus, musicFrames);
        int acknowledgements = CompareSoundEffect(bus, soundEffectFrames, ref samples);
        CompareEveryMusicBank(bus, Math.Min(musicFrames, 120), ref samples, ref acknowledgements);
        CompareAdditionalSoundLibraries(
            bus, soundEffectFrames, ref samples, ref acknowledgements);
        return new ManagedAudioParitySmokeTestResult(
            musicFrames,
            soundEffectFrames,
            samples,
            acknowledgements,
            AudioAssetCatalogData.Music.Count,
            3);
    }

    private static int CompareMusic(ISnesAddressSpace bus, int frameCount)
    {
        var queue = new CartridgeAudioState();
        queue.QueueMusicDelayed8(MusicCommand.LoadData(AudioRomData.MusicBanks.Title));
        queue.QueueMusicDelayed8(MusicCommand.SelectTrack(AudioRomData.MusicTracks.Title));
        using var managed = new SpcAudioEngine();
        using var native = new NativeSpcAudioOracle(bus);
        CartridgeAudioAcknowledgements acknowledgements = default;
        int compared = 0;
        for (int frame = 0; frame < frameCount; frame++)
        {
            IReadOnlyList<CartridgeAudioCommand> commands = queue.AdvanceFrame(bus, acknowledgements);
            CompareFrame(frame, "title music", managed, native, commands, ref compared);
            CartridgeAudioAcknowledgements managedAck = managed.ReadAcknowledgements();
            CartridgeAudioAcknowledgements nativeAck = native.ReadAcknowledgements();
            CompareAcknowledgements(frame, "title music", managedAck, nativeAck);
            acknowledgements = managedAck;
        }
        return compared;
    }

    private static int CompareSoundEffect(ISnesAddressSpace bus, int frameCount, ref int samples)
    {
        var queue = new CartridgeAudioState();
        queue.QueueSound(
            SoundEffectLibrary1Sounds.PowerBeam,
            maximumQueued: 15);
        using var managed = new SpcAudioEngine();
        using var native = new NativeSpcAudioOracle(bus);
        CartridgeAudioAcknowledgements acknowledgements = default;
        int comparedAcknowledgements = 0;
        for (int frame = 0; frame < frameCount; frame++)
        {
            IReadOnlyList<CartridgeAudioCommand> commands = queue.AdvanceFrame(bus, acknowledgements);
            CompareFrame(frame, "power-beam SFX", managed, native, commands, ref samples);
            CartridgeAudioAcknowledgements managedAck = managed.ReadAcknowledgements();
            CartridgeAudioAcknowledgements nativeAck = native.ReadAcknowledgements();
            CompareAcknowledgements(frame, "power-beam SFX", managedAck, nativeAck);
            comparedAcknowledgements += AudioRomData.Apu.PortCount;
            acknowledgements = managedAck;
        }
        return comparedAcknowledgements;
    }

    /// <summary>
    /// Loads every extracted bank and starts its first retail track. This catches a bad
    /// upload-address mapping as well as bank-specific instruments, BRR data, envelopes,
    /// pitch, echo, and sequence opcodes before the native oracle is retired.
    /// </summary>
    private static void CompareEveryMusicBank(
        ISnesAddressSpace bus,
        int frameCount,
        ref int samples,
        ref int acknowledgements)
    {
        foreach (AudioUploadAssetDefinition bank in AudioAssetCatalogData.Music)
        {
            using var managed = new SpcAudioEngine();
            using var native = new NativeSpcAudioOracle(bus);
            for (int frame = 0; frame < frameCount; frame++)
            {
                IReadOnlyList<CartridgeAudioCommand> commands = frame == 0
                    ?
                    [
                        CartridgeAudioCommand.Upload(AudioAssetCatalogData.Common.SnesAddress),
                        CartridgeAudioCommand.Upload(bank.SnesAddress),
                        CartridgeAudioCommand.WritePort(AudioRomData.Apu.MusicPort, 1),
                    ]
                    : Array.Empty<CartridgeAudioCommand>();
                string scenario = $"{bank.Name} track 1";
                CompareFrame(frame, scenario, managed, native, commands, ref samples);
                CompareAcknowledgements(
                    frame, scenario, managed.ReadAcknowledgements(), native.ReadAcknowledgements());
                acknowledgements += AudioRomData.Apu.PortCount;
            }
        }
    }

    /// <summary>Exercises the other two SFX command tables plus overlap and cancellation.</summary>
    private static void CompareAdditionalSoundLibraries(
        ISnesAddressSpace bus,
        int frameCount,
        ref int samples,
        ref int acknowledgements)
    {
        SoundEffectId[] effects =
        [
            SoundEffectLibrary2Sounds.DoorOpening,
            new SoundEffectId(SoundEffectLibrary.Library3, 0x23),
        ];
        foreach (SoundEffectId effect in effects)
        {
            var queue = new CartridgeAudioState();
            queue.QueueSound(effect, maximumQueued: 15);
            CompareQueuedScenario(
                bus, queue, effect.ToString(), frameCount, ref samples, ref acknowledgements);
        }

        var overlapping = new CartridgeAudioState();
        overlapping.QueueSound(SoundEffectLibrary1Sounds.PowerBeam, maximumQueued: 15);
        overlapping.QueueSound(SoundEffectLibrary2Sounds.DoorOpening, maximumQueued: 15);
        overlapping.QueueSound(
            new SoundEffectId(SoundEffectLibrary.Library3, 0x23), maximumQueued: 15);
        CompareQueuedScenario(
            bus, overlapping, "three-library overlap", frameCount, ref samples, ref acknowledgements,
            cancelFrame: frameCount / 2);
    }

    private static void CompareQueuedScenario(
        ISnesAddressSpace bus,
        CartridgeAudioState queue,
        string scenario,
        int frameCount,
        ref int samples,
        ref int acknowledgements,
        int cancelFrame = -1)
    {
        using var managed = new SpcAudioEngine();
        using var native = new NativeSpcAudioOracle(bus);
        CartridgeAudioAcknowledgements previous = default;
        for (int frame = 0; frame < frameCount; frame++)
        {
            if (frame == cancelFrame)
                queue.QueueCancelSoundEffects();
            IReadOnlyList<CartridgeAudioCommand> commands = queue.AdvanceFrame(bus, previous);
            CompareFrame(frame, scenario, managed, native, commands, ref samples);
            CartridgeAudioAcknowledgements managedAck = managed.ReadAcknowledgements();
            CompareAcknowledgements(frame, scenario, managedAck, native.ReadAcknowledgements());
            acknowledgements += AudioRomData.Apu.PortCount;
            previous = managedAck;
        }
    }

    private static void CompareFrame(
        int frame,
        string scenario,
        SpcAudioEngine managed,
        NativeSpcAudioOracle native,
        IReadOnlyList<CartridgeAudioCommand> commands,
        ref int compared)
    {
        managed.BeginDspWriteCapture();
        native.BeginDspWriteCapture();
        ReadOnlySpan<short> managedPcm = managed.RenderFrame(commands);
        ReadOnlySpan<short> nativePcm = native.RenderFrame(commands);
        IReadOnlyList<(byte Address, byte Value)> managedWrites = managed.CapturedDspWrites;
        IReadOnlyList<(byte Address, byte Value)> nativeWrites = native.ReadCapturedDspWrites();
        string stateDifference = DescribeStateDifference(managed, native);
        if (stateDifference != "identical")
            throw new InvalidDataException(
                $"{scenario} frame {frame} driver state differs: {stateDifference}. " +
                $"Music table: {DescribeRam(managed, native, 0x581e, 48)}. " +
                $"First pattern: {DescribeRam(managed, native, 0x5880, 48)}. " +
                $"Managed trace: {string.Join(" | ", managed.CapturedDriverTrace)}");
        string writeDifference = DescribeWriteDifference(managedWrites, nativeWrites);
        if (writeDifference != "identical")
            throw new InvalidDataException($"{scenario} frame {frame} DSP-write history differs: {writeDifference}.");
        string registerDifferences = DescribeRegisterDifferences(managed, native);
        if (registerDifferences != "identical")
        {
            throw new InvalidDataException(
                $"{scenario} frame {frame} ended with different DSP registers: {registerDifferences}.");
        }
        if (managedPcm.Length != nativePcm.Length)
            throw new InvalidDataException($"{scenario} frame {frame} PCM lengths disagree.");
        for (int index = 0; index < managedPcm.Length; index++)
        {
            if (managedPcm[index] != nativePcm[index])
            {
                string sampleWindow = DescribeSampleWindow(managedPcm, nativePcm, index);
                throw new InvalidDataException(
                    $"{scenario} frame {frame} PCM sample {index} differs: " +
                    $"managed {managedPcm[index]}, native {nativePcm[index]}. " +
                    $"PCM window: {sampleWindow}");
            }
            compared++;
        }
    }

    private static string DescribeRam(
        SpcAudioEngine managed,
        NativeSpcAudioOracle native,
        ushort start,
        int count)
    {
        var bytes = new List<string>();
        for (int offset = 0; offset < count; offset++)
        {
            ushort address = unchecked((ushort)(start + offset));
            bytes.Add($"{address:X4}:{managed.ReadApuRam(address):X2}/{native.ReadApuRam(address):X2}");
        }
        return string.Join(",", bytes);
    }

    private static string DescribeStateDifference(
        SpcAudioEngine managed,
        NativeSpcAudioOracle native)
    {
        var differences = new List<string>();
        foreach (SpcAudioDebugValue value in Enum.GetValues<SpcAudioDebugValue>())
        {
            int channelCount = value >= SpcAudioDebugValue.PatternPointer
                ? ManagedSnesDsp.VoiceCount
                : 1;
            for (int channel = 0; channel < channelCount; channel++)
            {
                int managedValue = managed.ReadDebugValue(value, channel);
                int nativeValue = native.ReadDebugValue(value, channel);
                if (managedValue != nativeValue)
                {
                    differences.Add($"{value}[{channel}] managed ${managedValue:X}, native ${nativeValue:X}");
                    if (differences.Count == 24)
                        return string.Join("; ", differences);
                }
            }
        }
        return differences.Count == 0 ? "identical" : string.Join("; ", differences);
    }

    private static string DescribeWriteDifference(
        IReadOnlyList<(byte Address, byte Value)> managed,
        IReadOnlyList<(byte Address, byte Value)> native)
    {
        int common = Math.Min(managed.Count, native.Count);
        for (int index = 0; index < common; index++)
        {
            if (managed[index] != native[index])
            {
                return $"write {index} managed ${managed[index].Address:X2}=${managed[index].Value:X2}, " +
                    $"native ${native[index].Address:X2}=${native[index].Value:X2}; " +
                    $"counts {managed.Count}/{native.Count}; " +
                    $"managed [{FormatWrites(managed)}], native [{FormatWrites(native)}]";
            }
        }
        return managed.Count == native.Count
            ? "identical"
            : $"common prefix {common}, counts {managed.Count}/{native.Count}";
    }

    private static string FormatWrites(IReadOnlyList<(byte Address, byte Value)> writes) =>
        string.Join(",", writes.Take(32).Select(write => $"{write.Address:X2}={write.Value:X2}"));

    private static string DescribeRegisterDifferences(
        SpcAudioEngine managed,
        NativeSpcAudioOracle native)
    {
        var differences = new List<string>();
        for (int address = 0; address < 0x80; address++)
        {
            byte managedValue = managed.ReadDspRegister(unchecked((byte)address));
            byte nativeValue = native.ReadDspRegister(unchecked((byte)address));
            if (managedValue != nativeValue)
                differences.Add($"${address:X2}=${managedValue:X2}/${nativeValue:X2}");
        }
        return differences.Count == 0 ? "identical" : string.Join(",", differences);
    }

    private static string DescribeSampleWindow(
        ReadOnlySpan<short> managed,
        ReadOnlySpan<short> native,
        int mismatch)
    {
        int start = Math.Max(0, mismatch - 8);
        int end = Math.Min(managed.Length, mismatch + 9);
        var pairs = new List<string>();
        for (int index = start; index < end; index++)
            pairs.Add($"{index}:{managed[index]}/{native[index]}");
        return string.Join(",", pairs);
    }

    private static void CompareAcknowledgements(
        int frame,
        string scenario,
        CartridgeAudioAcknowledgements managed,
        CartridgeAudioAcknowledgements native)
    {
        for (int port = 0; port < AudioRomData.Apu.PortCount; port++)
        {
            if (managed[port] != native[port])
            {
                throw new InvalidDataException(
                    $"{scenario} frame {frame} acknowledgement port {port} differs: " +
                    $"managed ${managed[port]:X2}, native ${native[port]:X2}.");
            }
        }
    }
}
