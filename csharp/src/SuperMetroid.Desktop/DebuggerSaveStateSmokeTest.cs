using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Desktop;

public readonly record struct DebuggerSaveStateSmokeTestResult(
    ushort SavedFrame,
    int ContinuationFrames,
    long StateFileBytes,
    bool WrongRomRejected,
    bool EmptySlotReported);

/// <summary>Headless save/advance/load deterministic-continuation audit.</summary>
public static class DebuggerSaveStateSmokeTest
{
    public static DebuggerSaveStateSmokeTestResult Run(string romPath)
    {
        string fullRomPath = Path.GetFullPath(romPath);
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        var game = new SuperMetroidGame(bus, new SuperMetroidGameOptions());
        using var audio = new SpcAudioEngine();
        for (int frame = 0; frame < 90; frame++)
        {
            FrontendFrame current = game.Step(0);
            audio.RenderFrame(current.AudioCommands);
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
        }

        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"SuperMetroid-state-audit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            var store = new DebuggerSaveStateStore(fullRomPath, bus.Rom, temporaryDirectory);
            bool emptySlotReported = !store.TryLoad(9, out _);
            if (!emptySlotReported)
                throw new InvalidDataException("An empty debugger slot was reported as occupied.");
            DebuggerSaveStateMetadata saved = store.Save(0, bus, game, audio.Player);
            long stateBytes = new FileInfo(saved.Path).Length;
            const int continuationFrames = 45;
            var expected = new FrontendFrame[continuationFrames];
            var expectedPcm = new short[continuationFrames][];
            var expectedAcknowledgements = new CartridgeAudioAcknowledgements[continuationFrames];
            for (int frame = 0; frame < continuationFrames; frame++)
            {
                expected[frame] = game.Step(0);
                expectedPcm[frame] = audio.RenderFrame(expected[frame].AudioCommands).ToArray();
                expectedAcknowledgements[frame] = audio.ReadAcknowledgements();
                game.SetAudioAcknowledgements(expectedAcknowledgements[frame]);
            }

            DebuggerSaveStateLoadResult loaded = store.Load(0);
            using var restoredAudio = new SpcAudioEngine(
                ExtractedAudioAssetCatalog.Load(ExtractedAudioAssetLocator.FindAudioDirectory()),
                loaded.AudioPlayer ?? throw new InvalidDataException("Restored state omitted managed audio."));
            if (loaded.Game.FrameNumber != saved.FrameNumber)
            {
                throw new InvalidDataException(
                    $"Restored frame {loaded.Game.FrameNumber} != saved frame {saved.FrameNumber}.");
            }
            for (int frame = 0; frame < continuationFrames; frame++)
            {
                FrontendFrame actual = loaded.Game.Step(0);
                short[] actualPcm = restoredAudio.RenderFrame(actual.AudioCommands).ToArray();
                CartridgeAudioAcknowledgements actualAcknowledgements =
                    restoredAudio.ReadAcknowledgements();
                loaded.Game.SetAudioAcknowledgements(actualAcknowledgements);
                FrontendFrame wanted = expected[frame];
                if (actual.GameState != wanted.GameState ||
                    actual.FrameNumber != wanted.FrameNumber ||
                    actual.Phase != wanted.Phase ||
                    !actual.Pixels.AsSpan().SequenceEqual(wanted.Pixels) ||
                    !actual.AudioCommands.SequenceEqual(wanted.AudioCommands) ||
                    !actualPcm.AsSpan().SequenceEqual(expectedPcm[frame]) ||
                    actualAcknowledgements != expectedAcknowledgements[frame])
                {
                    throw new InvalidDataException(
                        $"Debugger-state continuation diverged on relative frame {frame} " +
                        $"(actual {actual.GameState}/{actual.FrameNumber}/{actual.Phase}, " +
                        $"expected {wanted.GameState}/{wanted.FrameNumber}/{wanted.Phase}).");
                }
            }

            byte[] wrongRom = bus.Rom.ToArray();
            wrongRom[0] ^= 1;
            var wrongStore = new DebuggerSaveStateStore(
                fullRomPath,
                wrongRom,
                temporaryDirectory);
            bool wrongRomRejected = false;
            try
            {
                wrongStore.Load(0);
            }
            catch (InvalidDataException error) when (
                error.Message.Contains("different ROM", StringComparison.Ordinal))
            {
                wrongRomRejected = true;
            }
            if (!wrongRomRejected)
                throw new InvalidDataException("Debugger state did not reject a different ROM digest.");

            return new DebuggerSaveStateSmokeTestResult(
                saved.FrameNumber,
                continuationFrames,
                stateBytes,
                wrongRomRejected,
                emptySlotReported);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }
}
