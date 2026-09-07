using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyColorWindowSnapshots()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var cgram = new SnesCgram();
        cgram.SetColor(0, 0x396b);
        var oam = new OamBuffer(); oam.BeginFrame(); oam.FinalizeFrame();
        var memory = PpuMemorySnapshot.Capture(new SnesVram(), cgram, oam);
        Rgba32[] baseline = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        int samples = 0;
        foreach (bool dead in new[] { false, true })
        {
            Rgba32[] expected = baseline.ToArray();
            SnesGameplayFrameRenderer.ApplyCeresHaze(expected, dead);
            Compare(expected, SnesGameplayFrameRenderer.CaptureCeresHaze(dead));
        }

        foreach (SamusSuitPickupKind kind in new[] { SamusSuitPickupKind.Varia, SamusSuitPickupKind.Gravity })
        {
            var pickup = new SamusSuitPickupState();
            AssertTrue(SamusSuitPickupRenderer.Capture(pickup) is null, "inactive suit emits no operation");
            var samus = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose };
            pickup.Begin(bus, samus, 0, 0, kind);
            var phases = new HashSet<byte>();
            for (int tick = 0; tick < 2000 && pickup.IsActive; tick++)
            {
                bool sample = phases.Add(pickup.Substate) || tick % 7 == 0;
                ScanlineColorAddRenderLayer? retained = null;
                Rgba32[]? expected = null;
                if (sample)
                {
                    expected = baseline.ToArray();
                    SamusSuitPickupRenderer.Composite(expected, pickup);
                    retained = SamusSuitPickupRenderer.Capture(pickup)!;
                    Compare(expected, retained);
                }
                pickup.Step(bus, samus, new SnesCgram());
                if (retained is not null) Compare(expected!, retained);
            }
            AssertTrue(!pickup.IsActive, "suit window fixture completes");
        }

        foreach (ushort centerX in new ushort[] { 128, 65500, 290 })
        {
            var explosion = new SamusPowerBombExplosionState();
            AssertTrue(SnesGameplayFrameRenderer.CapturePowerBombColorMath(bus, explosion, 0, 0) is null,
                "inactive Power Bomb emits no operation");
            explosion.Arm(); explosion.Spawn(centerX, 120);
            var phases = new HashSet<PowerBombExplosionPhase>();
            for (int tick = 0; tick < 2000 && explosion.IsActive; tick++)
            {
                bool sample = phases.Add(explosion.RenderedPhase) || tick % 13 == 0;
                ScanlineColorAddRenderLayer? retained = null;
                Rgba32[]? expected = null;
                if (sample)
                {
                    expected = baseline.ToArray();
                    SnesGameplayFrameRenderer.ApplyPowerBombColorMath(expected, bus, explosion, 0, 0);
                    retained = SnesGameplayFrameRenderer.CapturePowerBombColorMath(bus, explosion, 0, 0)!;
                    Compare(expected, retained);
                }
                explosion.StepFrame(bus);
                if (retained is not null) Compare(expected!, retained);
            }
            AssertTrue(!explosion.IsActive && phases.Contains(PowerBombExplosionPhase.Afterglow),
                "Power Bomb fixture reaches afterglow and completes");
        }

        var windows = new ColorAddWindow[224];
        Array.Fill(windows, new ColorAddWindow(0, 255, 200, 10, 255));
        var owned = new ScanlineColorAddRenderLayer(windows);
        Array.Fill(windows, ColorAddWindow.Empty);
        Rgba32[] alpha = Enumerable.Repeat(new Rgba32(100, 40, 10, 17), 256 * 224).ToArray();
        SoftwareScanlineColorRenderer.Composite(alpha, owned);
        AssertTrue(alpha.All(p => p == new Rgba32(255, 50, 255, 17)), "windows own input, saturate bytes and preserve alpha");
        AssertThrows<ArgumentException>(() => new ScanlineColorAddRenderLayer(new ColorAddWindow[223]),
            "window operation rejects incomplete scanline table");
        var packet = new RenderFrameSnapshot(new(1, 1, 0), new LayeredRenderSnapshot(memory, new RenderLayer[] { owned }, 3, 15));
        byte[] bytes = RenderFrameSnapshotCodec.Serialize(packet);
        AssertThrows<IOException>(() => RenderFrameSnapshotCodec.Deserialize(bytes[..^1]), "truncated window table rejected");
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(RenderPacketFormat.Signature.Length),
            RenderPacketFormat.OrdinaryGameplayLayerVersion);
        AssertThrows<InvalidDataException>(() => RenderFrameSnapshotCodec.Deserialize(bytes), "old format rejects color windows");
        Console.WriteLine($"  Color window snapshots: {samples} suit/haze/Power Bomb comparisons, clipping, lifetime and codec agree.");

        void Compare(Rgba32[] expected, ScanlineColorAddRenderLayer layer)
        {
            var frame = new RenderFrameSnapshot(new(++samples, 1, (ushort)samples),
                new LayeredRenderSnapshot(memory, new RenderLayer[] { layer }, 3, 15));
            AssertTrue(expected.AsSpan().SequenceEqual(SoftwareFrameSnapshotRenderer.Render(RoundTripRenderPacket(frame))),
                $"color window snapshot sample {samples}");
        }
    }
}
