using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Reflection;

internal static partial class Program
{
    private static void VerifyAttractCapture()
    {
        byte[] rom = File.ReadAllBytes(Path.GetFullPath("Super Metroid.smc"));
        // Scene setup is compiled now. Bound only the live verification instance after
        // its real first scene has loaded; ROM table patches no longer affect admission.
        FieldInfo countdown = typeof(SuperMetroidGame).GetField("demoFramesRemaining", BindingFlags.Instance | BindingFlags.NonPublic)!;
        FieldInfo nextScene = typeof(SuperMetroidGame).GetField("demoScene", BindingFlags.Instance | BindingFlags.NonPublic)!;
        int samples = 0;
        foreach (bool cancel in new[] { false, true })
        {
            var legacy = new SuperMetroidGame(new SuperMetroidAddressSpace(rom));
            var captured = new SuperMetroidGame(new SuperMetroidAddressSpace(rom));
            var states = new HashSet<SuperMetroidGameState>();
            bool sawHold = false, returned = false, bounded = false;
            Rgba32[]? held = null;
            for (int tick = 0; tick < 8000; tick++)
            {
                if (!bounded && legacy.GameState == SuperMetroidGameState.TransitionToDemoB)
                {
                    foreach (var game in new[] { legacy, captured })
                    {
                        countdown.SetValue(game, 16);
                        nextScene.SetValue(game, 6);
                    }
                    bounded = true;
                }
                ushort input = cancel && legacy.AttractDemoHoldFramesRemaining == 80 ? (ushort)SnesButton.Start : (ushort)0;
                FrontendFrame expected = legacy.Step(input);
                CapturedFrontendFrame actual = captured.StepCaptured(input, tick + 1, 1);
                states.Add(expected.GameState);
                AssertTrue(!actual.UsedLegacyRaster, $"demo capture never uses legacy pixels: tick {tick}, {expected.GameState}");
                AssertEqual(expected.GameState, actual.Frame.GameState, "demo capture state");
                AssertSequenceEqual(expected.AudioCommands, actual.Frame.AudioCommands, "demo capture audio order");
                bool demo = expected.GameState is SuperMetroidGameState.TransitionToDemoA or SuperMetroidGameState.TransitionToDemoB
                    or SuperMetroidGameState.PlayingDemo or SuperMetroidGameState.TransitionFromDemoA or SuperMetroidGameState.TransitionFromDemoB;
                if (demo || tick % 31 == 0)
                {
                    AssertTrue(expected.Pixels.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(actual.Snapshot!))),
                        $"demo frame {tick} pixel parity");
                    samples++;
                }
                if (legacy.AttractDemoHoldFramesRemaining > 0 && expected.GameState == SuperMetroidGameState.PlayingDemo)
                {
                    sawHold = true;
                    held ??= expected.Pixels.ToArray();
                    AssertTrue(held.AsSpan().SequenceEqual(expected.Pixels), "demo final-image hold stays pixel-identical");
                }
                if (states.Contains(SuperMetroidGameState.TransitionFromDemoB) && expected.GameState == SuperMetroidGameState.OpeningCinematic)
                {
                    AssertTrue(expected.Pixels.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(actual.Snapshot!)), "demo return title frame");
                    returned = true; break;
                }
            }
            AssertTrue(sawHold && returned && states.Contains(SuperMetroidGameState.TransitionToDemoB)
                && states.Contains(SuperMetroidGameState.TransitionFromDemoA), "demo fixture exercises both black transitions and returns to title");
        }
        Console.WriteLine($"  Attract capture: {samples} sampled frames cover load, playback, retained hold, completion and cancellation.");
    }
}
