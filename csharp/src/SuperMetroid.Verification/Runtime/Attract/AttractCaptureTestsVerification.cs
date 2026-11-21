using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime.Attract;
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
        InstalledScreenContent presentation = RetailPresentationFixture();
        // Scene setup is compiled now. Bound only the live verification instance after
        // its real first scene has loaded; ROM table patches no longer affect admission.
        var legacy = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps));
        var captured = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps));
        legacy.BindArtwork(legacy.Artwork with
        {
            ScreenContent = presentation,
        });
        captured.BindArtwork(captured.Artwork with
        {
            ScreenContent = presentation,
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

        /// <summary>The names of the attract-demo steps the legacy game has run.</summary>
        private HashSet<string> states = [];

        /// <summary>Whether the first scene's route to the set's next scene has been turned to the title.</summary>
        private bool routed;

        /// <summary>Whether the demo's final image has been held.</summary>
        private bool sawHold;

        /// <summary>The updates the legacy game's final-image hold has counted since it began; none while no image holds.</summary>
        private int heldUpdates;

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
        internal bool AtCancelFrame =>
            legacy.IsAt(AttractDemoStep.Holding) && AttractDemoDefinitions.FinalImageHoldFrames - heldUpdates == CancelHoldFrame;

        /// <summary>
        /// A case continuing from this one's next frame: both games restored from exact debugger round trips, and what this
        /// case has seen so far. The frames compared so far are this case's.
        /// </summary>
        internal AttractRun Branch()
        {
            var branch = new AttractRun(RoundTrip(legacy), RoundTrip(captured));
            branch.states = [.. states];
            branch.sawHold = sawHold;
            branch.heldUpdates = heldUpdates;
            branch.returned = returned;
            branch.bounded = bounded;
            branch.routed = routed;
            branch.held = held;
            branch.tick = tick;
            return branch;
        }

        /// <summary>Steps both games one frame, pressing Start at the cancellation frame when <paramref name="cancel"/>.</summary>
        internal void Step(bool cancel)
        {
            // The first scene plays sixteen scripted updates, and after it the title returns from its beginning, as after
            // a set's last scene: the game enters each step as its own run would, with a shorter script and that route.
            if (!bounded && legacy.IsAtAttractStep<AttractDemoStep.Starting>())
            {
                foreach (var game in new[] { legacy, captured })
<<<<<<< ours
                {
                    game.Session.Enter(new AttractDemoGameSequence.Starting(game.Session, duration: 16));
                    PrivateState.Set(game.Session.Demo, nameof(AttractDemoSchedule.ScenesBegun), game.Session.Demo.Set.Scenes.Count);
                }
||||||| base
                {
                    PrivateState.Set(game.Session.Demo, "Scene", new AttractDemoSchedule.SceneState.Playing(UpdateCountdown.Lasting(16)));
                    PrivateState.Set(game.Session.Demo, nameof(AttractDemoSchedule.ScenesBegun), game.Session.Demo.Set.Scenes.Count);
                }
=======
                    game.Session.Enter(new AttractDemoStep.Starting(UpdateCountdown.Lasting(16)));
>>>>>>> theirs
                bounded = true;
            }
            if (!routed && legacy.Session.GameState == SuperMetroidGameState.AttractDemo &&
                legacy.Session.AttractDemoStep is AttractDemoStep.Routing { Route: AttractDemoRoute.NextScene })
            {
                foreach (var game in new[] { legacy, captured })
                    game.Session.Enter(new AttractDemoStep.Routing(new AttractDemoRoute.TitleFromBeginning()));
                routed = true;
            }
            ControllerButton input = cancel && AtCancelFrame ? ControllerButton.Start : 0;
            bool wasHolding = legacy.IsAt(AttractDemoStep.Holding);
            FrontendFrame expected = legacy.Step(input);
            heldUpdates = !legacy.IsAt(AttractDemoStep.Holding) ? 0 : wasHolding ? heldUpdates + 1 : 0;
            CapturedFrontendFrame actual = captured.StepCaptured(input);
            if (legacy.GameState == SuperMetroidGameState.AttractDemo)
                states.Add(legacy.Session.AttractDemoStep.Name);
            AssertEqual(expected.GameState, actual.Frame.GameState, "demo capture state");
            AssertSequenceEqual(expected.Audio.Events, actual.Frame.Audio.Events, "demo capture audio event order");
            bool demo = expected.GameState == SuperMetroidGameState.AttractDemo;
            if (demo || tick % 31 == 0)
            {
                AssertTrue(ScenePixels.Same(legacy.GetRetainedDisplay(), actual.Snapshot),
                    $"demo frame {tick} pixel parity");
                Samples++;
            }
<<<<<<< ours
            if (legacy.IsAt(AttractDemoStep.Holding))
||||||| base
            if (legacy.AttractDemoHoldFramesRemaining > 0 && legacy.IsAt(AttractDemoStep.Playing))
=======
            if (legacy.AttractDemoHoldFramesRemaining > 0)
>>>>>>> theirs
            {
                sawHold = true;
                held ??= legacy.GetRetainedDisplay();
                AssertTrue(ScenePixels.Same(held, legacy.GetRetainedDisplay()), "demo final-image hold stays pixel-identical");
            }
            if (states.Contains(nameof(AttractDemoStep.Routing)) && legacy.IsAt(TitleStep.Showing))
            {
                AssertTrue(ScenePixels.Same(legacy.GetRetainedDisplay(), actual.Snapshot), "demo return title frame");
                returned = true;
            }
            tick++;
        }

        /// <summary>Checks the case held the demo's final image, passed both black transitions and returned to the title.</summary>
        internal void AssertCovered() =>
            AssertTrue(sawHold && returned && states.Contains(nameof(AttractDemoStep.Starting))
                && states.Contains(nameof(AttractDemoStep.Unloading)), "demo fixture exercises both black transitions and returns to title");
    }
}
