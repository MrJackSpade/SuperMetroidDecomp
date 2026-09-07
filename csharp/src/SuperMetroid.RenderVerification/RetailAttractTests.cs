using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailAttractTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        byte[] rom = File.ReadAllBytes("Super Metroid.smc");
        var source = new SuperMetroidAddressSpace(rom);
        int list = AttractDemoRomData.RoomBank | RomDataReader.ReadWordFixedBank(source, AttractDemoRomData.RoomSetPointers);
        // Same bounded fixture as portable capture verification: retain retail room,
        // inputs and graphics, but end after sixteen ticks rather than tour more rooms.
        BinaryPrimitives.WriteUInt16LittleEndian(rom.AsSpan(SuperMetroidAddressSpace.ToRomOffset(list + AttractDemoRomData.RoomFields.Duration)), 16);
        BinaryPrimitives.WriteUInt16LittleEndian(rom.AsSpan(SuperMetroidAddressSpace.ToRomOffset(list + AttractDemoRomData.RoomRecordBytes)), AttractDemoRomData.EndOfSet);
        int samples = 0;
        foreach (bool cancel in new[] { false, true })
        {
            var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom));
            var captured = new SuperMetroidGame(new SuperMetroidAddressSpace(rom));
            var states = new HashSet<SuperMetroidGameState>();
            bool sawHold = false, returned = false;
            Rgba32[]? held = null;
            for (int tick = 0; tick < 8000; tick++)
            {
                ushort input = cancel && legacy.AttractDemoHoldFramesRemaining == 80 ? (ushort)SnesButton.Start : (ushort)0;
                var expected = legacy.Step(input);
                var actual = captured.StepCaptured(input, tick + 1, 1);
                states.Add(expected.GameState);
                if (actual.UsedLegacyRaster || expected.GameState != actual.Frame.GameState ||
                    expected.Phase != actual.Frame.Phase || !expected.AudioCommands.SequenceEqual(actual.Frame.AudioCommands))
                    throw new InvalidOperationException($"Attract state/audio/capture divergence at {tick}.");
                bool demo = expected.GameState is SuperMetroidGameState.TransitionToDemoA or SuperMetroidGameState.TransitionToDemoB
                    or SuperMetroidGameState.PlayingDemo or SuperMetroidGameState.TransitionFromDemoA or SuperMetroidGameState.TransitionFromDemoB;
                if (demo || tick % 31 == 0)
                {
                    var packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(actual.Snapshot!));
                    PixelComparison.Verify(packet, expected.Pixels, renderer.RenderForReadback(packet), $"{device.Kind}: attract tick={tick}, cancel={cancel}");
                    samples++;
                }
                if (legacy.AttractDemoHoldFramesRemaining > 0 && expected.GameState == SuperMetroidGameState.PlayingDemo)
                {
                    sawHold = true;
                    held ??= expected.Pixels.ToArray();
                    if (!held.AsSpan().SequenceEqual(expected.Pixels)) throw new InvalidOperationException("Demo held frame changed.");
                }
                if (states.Contains(SuperMetroidGameState.TransitionFromDemoB) && expected.GameState == SuperMetroidGameState.OpeningCinematic)
                {
                    PixelComparison.Verify(actual.Snapshot!, expected.Pixels, renderer.RenderForReadback(actual.Snapshot!), "attract return to title");
                    returned = true; break;
                }
            }
            if (!sawHold || !returned || !states.Contains(SuperMetroidGameState.TransitionToDemoB) || !states.Contains(SuperMetroidGameState.TransitionFromDemoA))
                throw new InvalidOperationException("Attract fixture missed hold, transitions or return to title.");
        }
        Console.WriteLine($"{device.Kind}: {samples} exact attract samples; bounded retail room, hold, completion/cancel and return to title passed.");
    }
}
