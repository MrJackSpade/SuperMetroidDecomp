using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Runtime.Attract;

/// <summary>Checks captured attract-demo frames against legacy rendering, audio, and state transitions.</summary>
internal static class AttractCaptureTestsVerification
{
    /// <summary>The final-image hold frame at which the cancelled case presses Start.</summary>
    private const int CancelHoldFrame = 80;

    /// <summary>Compares demo playback, held final image, cancellation, and return-to-title packets with the live frontend.</summary>
    internal static void VerifyAttractCapture()
    {
        AreaMapPresentation presentation = RetailPresentationFixture();
        // Scene setup is compiled now. Bound only the live verification instance after
        // its real first scene has loaded; ROM table patches no longer affect admission.
        var legacy = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
        var captured = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore());
        legacy.BindArtwork(legacy.Artwork with
        {
            MapPresentation = presentation,
        });
        captured.BindArtwork(captured.Artwork with
        {
            MapPresentation = presentation,
        });
        // The completed and the cancelled case receive the same input on every frame before the final-image hold reaches
        // the cancellation frame, so they publish the same frames until then. That shared prefix is played and checked
        // once; the cancelled case continues from an exact debugger round trip of both games at the cancellation frame,
        // with everything the completed case had seen by then.
        var completed = new AttractRun(legacy, captured);
        AttractRun? cancelled = null;
        while (!completed.Finished)
        {
            if (cancelled is null && completed.AtCancelFrame)
                cancelled = completed.Branch();
            completed.Step(cancel: false);
        }
        completed.AssertCovered();
        AssertTrue(cancelled is not null, "the completed demo's final-image hold reaches the cancellation frame");
        AttractRun cancelledRun = cancelled ?? throw new InvalidOperationException("The cancelled case never branched.");
        while (!cancelledRun.Finished)
            cancelledRun.Step(cancel: true);
        cancelledRun.AssertCovered();
        Console.WriteLine($"  Attract capture: {completed.Samples + cancelledRun.Samples} sampled frames cover load, playback, retained hold, completion and cancellation.");
    }

    /// <summary>One attract case: a legacy game and a captured twin stepped together, with what the case has seen.</summary>
    private sealed class AttractRun(SuperMetroidGame legacy, SuperMetroidGame captured)
    {
        /// <summary>The frames the case plays at most before it must have returned to the title.</summary>
        private const int TickLimit = 8000;

        /// <summary>The game states the legacy game has shown.</summary>
        private HashSet<SuperMetroidGameState> states = [];

        /// <summary>Whether the demo's final image has been held.</summary>
        private bool sawHold;

        /// <summary>Whether the case has returned to the title.</summary>
        private bool returned;

        /// <summary>Whether the second demo transition's scene and length have been bounded.</summary>
        private bool bounded;

        /// <summary>The first held final image.</summary>
        private DisplayPacket? held;

        /// <summary>The next frame's number.</summary>
        private int tick;

        /// <summary>The captured frames compared with the legacy game's.</summary>
        internal int Samples { get; private set; }

        /// <summary>Whether the case has returned to the title or played every frame it may.</summary>
        internal bool Finished => returned || tick >= TickLimit;

        /// <summary>Whether the next frame is the one the cancelled case presses Start on.</summary>
        internal bool AtCancelFrame => legacy.AttractDemoHoldFramesRemaining == CancelHoldFrame;

        /// <summary>
        /// A case continuing from this one's next frame: both games restored from exact debugger round trips, and what this
        /// case has seen so far. The frames compared so far are this case's.
        /// </summary>
        internal AttractRun Branch()
        {
            var branch = new AttractRun(RoundTrip(legacy), RoundTrip(captured));
            branch.states = [.. states];
            branch.sawHold = sawHold;
            branch.returned = returned;
            branch.bounded = bounded;
            branch.held = held;
            branch.tick = tick;
            return branch;
        }

        /// <summary>Steps both games one frame, pressing Start at the cancellation frame when <paramref name="cancel"/>.</summary>
        internal void Step(bool cancel)
        {
            if (!bounded && legacy.GameState == SuperMetroidGameState.StartingDemo)
            {
                foreach (var game in new[] { legacy, captured })
                {
                    game.Session.Demo.FramesRemaining = 16;
                    game.Session.Demo.Scene = 6;
                }
                bounded = true;
            }
            ControllerButton input = cancel && AtCancelFrame ? ControllerButton.Start : 0;
            FrontendFrame expected = legacy.Step(input);
            CapturedFrontendFrame actual = captured.StepCaptured(input);
            states.Add(expected.GameState);
            AssertEqual(expected.GameState, actual.Frame.GameState, "demo capture state");
            AssertSequenceEqual(expected.Audio.Events, actual.Frame.Audio.Events, "demo capture audio event order");
            bool demo = expected.GameState is SuperMetroidGameState.LoadingDemo or SuperMetroidGameState.StartingDemo
                or SuperMetroidGameState.PlayingDemo or SuperMetroidGameState.UnloadingDemo or SuperMetroidGameState.RoutingAfterDemo;
            if (demo || tick % 31 == 0)
            {
                AssertTrue(ScenePixels.Same(legacy.GetRetainedDisplay(), actual.Snapshot),
                    $"demo frame {tick} pixel parity");
                Samples++;
            }
            if (legacy.AttractDemoHoldFramesRemaining > 0 && expected.GameState == SuperMetroidGameState.PlayingDemo)
            {
                sawHold = true;
                held ??= legacy.GetRetainedDisplay();
                AssertTrue(ScenePixels.Same(held, legacy.GetRetainedDisplay()), "demo final-image hold stays pixel-identical");
            }
            if (states.Contains(SuperMetroidGameState.RoutingAfterDemo) && expected.GameState == SuperMetroidGameState.OpeningCinematic)
            {
                AssertTrue(ScenePixels.Same(legacy.GetRetainedDisplay(), actual.Snapshot), "demo return title frame");
                returned = true;
            }
            tick++;
        }

        /// <summary>Checks the case held the demo's final image, passed both black transitions and returned to the title.</summary>
        internal void AssertCovered() =>
            AssertTrue(sawHold && returned && states.Contains(SuperMetroidGameState.StartingDemo)
                && states.Contains(SuperMetroidGameState.UnloadingDemo), "demo fixture exercises both black transitions and returns to title");
    }
}
