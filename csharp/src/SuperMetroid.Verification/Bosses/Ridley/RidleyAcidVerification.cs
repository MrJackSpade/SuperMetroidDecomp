using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
namespace SuperMetroid.Verification.Bosses.Ridley;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.Core.Assets;

/// <summary>Checks Norfair Ridley's shared FX acid rise and death drain through room rendering.</summary>
internal static class RidleyAcidVerification
{
    /// <summary>Runs native palette reveal and death phases, checking acid targets and visible lower-arena pixels.</summary>
    internal static void VerifyRidleyAcid()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Ridley);
        // The room load's queued uploads, the acid's characters among them, are shown once its first frame drains them.
        runtime.FrameBoundary.BeginAcceptedFrame(0);
        var fx = runtime.RoomFxOverlay;
        AssertEqual(RoomFxType.Acid, fx.Type, "final Ridley room authors acid, not lava");
        AssertEqual((ushort)528, fx.NamedBaseY.Whole, "acid starts below the arena");
        var state = runtime.Enemies.RidleyEncounter.Ridley;
        // The shared Ridley state has no Ceres Baby program in Norfair. The
        // global actor-draw seam must not invoke Ceres's private graphics hook.
        var expectedEmptyOam = new OamBuffer();
        expectedEmptyOam.BeginFrame();
        expectedEmptyOam.FinalizeFrame();
        var ridleyOam = new OamBuffer();
        ridleyOam.BeginFrame();
        ridleyOam.AppendDrawn(frame => runtime.Enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyImmediateBabyAndDoor(frame, new(0, 256)));
        ridleyOam.AppendDrawn(frame => runtime.Enemies.CeresRidleyBehavior.BabyScene.DrawCeresRidleyBabyAndDoorOverEnemies(frame, new(0, 256)));
        ridleyOam.FinalizeFrame();
        AssertTrue(ridleyOam.ImageEquals(expectedEmptyOam),
            "Norfair Ridley does not draw the Ceres Baby or arena-door overlay");
        AssertEqual(0, (state ?? throw new InvalidOperationException("Verification requires state.")).BabyPoseDuration,
            "Norfair Ridley never advances an uninitialized Ceres Baby program");
        // Enter the native palette-reveal phase, then let its real ROM table reach
        // the terminator. This does not inject a liquid command into the fixture.
        state.Function = RidleyAiFunction.WaitBeforeLiftoff;
        state.FunctionTimer = 0;
        state.FadeStep = 0;
        for (int frame = 0; frame < 200 && state.Function == RidleyAiFunction.WaitBeforeLiftoff; frame++)
            runtime.Enemies.StepFrame(new RoomEnemyFrame { FrameCounter = runtime.Enemies.StandaloneFrameCounter, Camera = new(0, 256), TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData, SharedProjectiles = runtime.BombProjectiles });
        AssertEqual(440, fx.NamedTargetSurface, "native reveal writes acid rise target to shared FX");
        AssertEqual(unchecked((ushort)-96), fx.YVelocity.RawValue, "native reveal writes rising velocity");
        AssertEqual((ushort)32, fx.Timer.RawValue, "native reveal writes rise delay once");
        for (int frame = 0; frame < 400; frame++) fx.Step(runtime.Room.LoadedScope.ArtworkFrames, new RoomFxOverlayFrame { Camera = new Position16<RoomPixels>(0, 256), TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
        AssertEqual((ushort)440, fx.NamedBaseY.Whole, "acid reaches native visible battle height");
        AssertAcidVisible(true);
        state.Function = RidleyAiFunction.NorfairDeathExplosions;
        state.FunctionTimer = 0;
        runtime.Enemies.StepFrame(new RoomEnemyFrame { FrameCounter = runtime.Enemies.StandaloneFrameCounter, Camera = new(0, 256), TimeIsFrozen = false, Samus = runtime.Player.Samus, Level = runtime.Room.LevelData, SharedProjectiles = runtime.BombProjectiles });
        AssertEqual(528, fx.NamedTargetSurface, "native death roar writes acid drain target");
        AssertEqual((ushort)64, fx.YVelocity.RawValue, "native death writes draining velocity");
        AssertEqual((ushort)1, fx.Timer.RawValue, "native death writes drain delay once");
        for (int frame = 0; frame < 400; frame++) fx.Step(runtime.Room.LoadedScope.ArtworkFrames, new RoomFxOverlayFrame { Camera = new Position16<RoomPixels>(0, 256), TimeIsFrozen = false, EarthquakeSoundDelay = SuperMetroid.Core.Compatibility.PowerOnEarthquakeSoundDelay.OneFrame });
        AssertEqual((ushort)528, fx.NamedBaseY.Whole, "acid drains below arena after death");
        AssertAcidVisible(false);
        Console.WriteLine("Ridley acid: retail reveal raises 528 -> 440; death roar drains 440 -> 528 through shared FX.");

        void AssertAcidVisible(bool expectedVisible)
        {
            // The background characters are the installed artwork the room's uploads name, as the displayed frame captures them.
            // The colours the frame's pictures show, composed from their published shading and the import's colours (#1512),
            // in a gameplay frame whose groups show their pictures' own colours, which the acid page is painted in.
            var memory = FramePaint.FromNativeGameplay(GameplayShownColours.Of(runtime, InstalledFixtures.SamusShadeSources), runtime.DisplayedSprites.Draws);
            // The room's effect page: the acid page the room effect chose (#1514).
            var effect = GameplayEffectCapture.CaptureRoomFxOverlay((fx.CaptureForDisplay() ?? throw new InvalidOperationException("Verification requires fx.CaptureForDisplay().")),
                FxOverlayFixtures.PageOf(fx.EffectPlane, InstalledFixtures.ImportMaps.RoomFxPages, runtime.ComposedVram(),
                    GameplayShownColours.Of(runtime, InstalledFixtures.SamusShadeSources)));
            var baseline = DisplayRendering.Render(new LayeredScene(memory, [], 15));
            var actual = DisplayRendering.Render(new LayeredScene(memory,
                effect is null ? [] : new DisplayLayer[] { effect }, 15));
            AssertEqual(expectedVisible, !actual.SequenceEqual(baseline), "acid visibility at lower-arena camera");
            AssertTrue(actual.Take(32 * 256).SequenceEqual(baseline.Take(32 * 256)), "acid does not overwrite HUD");
        }
    }

    /// <summary>Runs the focused Ridley acid FX reproduction.</summary>
    [VerificationCommand("--ridley-acid")]
    internal static int RidleyAcidCommand()
    {
        Suite(nameof(VerifyRidleyAcid), () => VerifyRidleyAcid());
        return 0;
    }
}
