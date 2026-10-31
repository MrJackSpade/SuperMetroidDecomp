using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>Checks that captured colour windows for the suit pickup and the Power Bomb keep their shapes.</summary>
internal static class ColorWindowSnapshotTestsVerification
{
    /// <summary>
    /// Checks that a captured colour window keeps the window and colour it was captured with while the effect advances
    /// (#1468): each sampled layer equals a capture taken at the same moment, after the effect has stepped on. Also
    /// checks that a window owns its shapes and joins no more than it can draw.
    /// </summary>
    internal static void VerifyColorWindowSnapshots()
    {
        var cgram = new SnesCgram();
        cgram.SetColor(0, RgbColor.FromWord(0x396b));
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = FramePaintConstants.CaptureFrontend(new SnesVram(), cgram, oam, 3);
        int samples = 0;

        foreach (SamusSuitPickupKind kind in new[] { SamusSuitPickupKind.Varia, SamusSuitPickupKind.Gravity })
        {
            var pickup = new SamusSuitPickupState();
            AssertTrue(GameplayEffectCapture.CaptureSuitPickup(pickup.Flash) is null, "inactive suit emits no operation");
            var samus = PrepareRetailSamusFixture(new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FacingRightNormalPose });
            pickup.Begin(samus, new(0, 0), kind);
            var phases = new HashSet<SamusSuitPickupStage>();
            for (int tick = 0; tick < 2000 && pickup.IsActive; tick++)
            {
                bool sample = phases.Add(pickup.Stage) || tick % 7 == 0;
                ColourAddWindowLayer? retained = null;
                ColourAddWindowLayer? copy = null;
                if (sample)
                {
                    retained = GameplayEffectCapture.CaptureSuitPickup(pickup.Flash)
                        ?? throw new InvalidOperationException("An active suit pickup captures its window.");
                    copy = GameplayEffectCapture.CaptureSuitPickup(pickup.Flash);
                }
                pickup.Step(samus, new SamusColourRig().Samus);
                if (retained is not null) Compare(copy, retained);
            }
            AssertTrue(!pickup.IsActive, "suit window fixture completes");
        }

        foreach (ushort centerX in new ushort[] { 128, 65500, 290 })
        {
            var explosion = new SamusPowerBombExplosionState { PresentationColors = RetailPresentationFixture().PowerBombFixedColors };
            AssertTrue(GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0)) is null,
                "inactive Power Bomb emits no operation");
            explosion.Arm(); explosion.Spawn(new(centerX, 120));
            var phases = new HashSet<PowerBombExplosionPhase>();
            for (int tick = 0; tick < 2000 && explosion.IsActive; tick++)
            {
                bool sample = phases.Add(explosion.RenderedPhase) || tick % 13 == 0;
                ColourAddWindowLayer? retained = null;
                ColourAddWindowLayer? copy = null;
                if (sample)
                {
                    retained = GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0))
                        ?? throw new InvalidOperationException("An active Power Bomb captures its window.");
                    copy = GameplayEffectCapture.CapturePowerBombGlow(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0));
                }
                explosion.StepFrame();
                if (retained is not null) Compare(copy, retained);
            }
            AssertTrue(!explosion.IsActive && phases.Contains(PowerBombExplosionPhase.Afterglow),
                "Power Bomb fixture reaches afterglow and completes");
        }

        WindowShape[] shapes = [WindowRectangle.Spanning(0, 0, 255, 223)];
        var owned = new ScreenWindow(shapes);
        shapes[0] = WindowRectangle.Spanning(1, 1, 2, 2);
        AssertEqual(WindowRectangle.Spanning(0, 0, 255, 223), owned.Shapes[0], "a window owns its shapes");
        AssertThrows<ArgumentException>(() => new ScreenWindow(new WindowShape[ScreenWindow.MaximumShapes + 1]),
            "a window joins no more shapes than the backend draws");
        AssertThrows<ArgumentException>(() => new WindowCone(Offset16<ScreenPixels>.Zero, 0, WindowCone.AngleUnitsPerTurn / 2),
            "a cone is narrower than half a turn");
        _ = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory,
            new DisplayLayer[] { new ColourAddWindowLayer(owned, new RgbColor(25, 1, 31)) }, 15));
        Console.WriteLine($"  Color window snapshots: {samples} suit/Power Bomb retained windows and window ownership agree.");

        void Compare(ColourAddWindowLayer? captured, ColourAddWindowLayer layer)
        {
            samples++;
            AssertEqual(captured, layer, $"color window snapshot sample {samples}");
        }
    }

    /// <summary>Runs the colour-window retention checks.</summary>
    [VerificationCommand("--color-window-fixture")]
    internal static int ColorWindowFixtureCommand()
    {
     VerifyColorWindowSnapshots(); return 0;
    }
}
