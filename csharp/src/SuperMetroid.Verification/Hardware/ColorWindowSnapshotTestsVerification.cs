using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;

/// <summary>Checks retained scanline color windows for suit pickup and Power Bomb effects.</summary>
internal static class ColorWindowSnapshotTestsVerification
{
    /// <summary>Checks captured window layers keep their windows through later phases and own their input.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
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
            var samus = PrepareRetailSamusFixture(new SamusState { Pose = SamusPoseId.FacingRightNormalPose });
            pickup.Begin(samus, new(0, 0), kind);
            var phases = new HashSet<SamusSuitPickupStage>();
            for (int tick = 0; tick < 2000 && pickup.IsActive; tick++)
            {
                bool sample = phases.Add(pickup.Stage) || tick % 7 == 0;
                ScanlineColorAddRenderLayer? retained = null;
                ColorAddWindow[]? captured = null;
                if (sample)
                {
                    retained = GameplayEffectCapture.CaptureSuitPickup(pickup.Flash)
                        ?? throw new InvalidOperationException("An active suit pickup captures its windows.");
                    captured = retained.Windows.ToArray();
                }
                pickup.Step(samus, new SamusColourRig().Samus);
                if (retained is not null) Compare(captured ?? throw new InvalidOperationException("Verification requires captured."), retained);
            }
            AssertTrue(!pickup.IsActive, "suit window fixture completes");
        }

        foreach (ushort centerX in new ushort[] { 128, 65500, 290 })
        {
            var explosion = new SamusPowerBombExplosionState { PresentationColors = RetailPresentationFixture().PowerBombFixedColors };
            AssertTrue(GameplayEffectCapture.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0)) is null,
                "inactive Power Bomb emits no operation");
            explosion.Arm(); explosion.Spawn(new(centerX, 120));
            var phases = new HashSet<PowerBombExplosionPhase>();
            for (int tick = 0; tick < 2000 && explosion.IsActive; tick++)
            {
                bool sample = phases.Add(explosion.RenderedPhase) || tick % 13 == 0;
                ScanlineColorAddRenderLayer? retained = null;
                ColorAddWindow[]? captured = null;
                if (sample)
                {
                    retained = GameplayEffectCapture.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(explosion), new Position16<RoomPixels>(0, 0))
                        ?? throw new InvalidOperationException("Verification requires retained.");
                    captured = retained.Windows.ToArray();
                }
                explosion.StepFrame();
                if (retained is not null) Compare(captured ?? throw new InvalidOperationException("Verification requires captured."), retained);
            }
            AssertTrue(!explosion.IsActive && phases.Contains(PowerBombExplosionPhase.Afterglow),
                "Power Bomb fixture reaches afterglow and completes");
        }

        var windows = new ColorAddWindow[224];
        Array.Fill(windows, new ColorAddWindow(0, 255, 200, 10, 255));
        var owned = new ScanlineColorAddRenderLayer(windows);
        Array.Fill(windows, ColorAddWindow.Empty);
        AssertTrue(owned.Windows.ToArray().All(window => window == new ColorAddWindow(0, 255, 200, 10, 255)), "windows own input");
        AssertThrows<ArgumentException>(() => new ScanlineColorAddRenderLayer(new ColorAddWindow[223]),
            "window operation rejects incomplete scanline table");
        _ = new DisplayPacket(new(1, 1, 0), new LayeredScene(memory, new RenderLayer[] { owned }, 15));
        Console.WriteLine($"  Color window snapshots: {samples} suit/Power Bomb retained windows and input ownership agree.");

        void Compare(ColorAddWindow[] captured, ScanlineColorAddRenderLayer layer)
        {
            samples++;
            AssertTrue(layer.Windows.SequenceEqual(captured), $"color window snapshot sample {samples}");
        }
    }

    /// <summary>Runs the scanline color-window retention checks.</summary>
    [VerificationCommand("--color-window-fixture")]
    internal static int ColorWindowFixtureCommand()
    {
     VerifyColorWindowSnapshots(); return 0;
    }
}
