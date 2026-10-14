using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static GraphDigestVerification;
using static RetailGameFixtureVerification;
using static RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Artwork;

/// <summary>Checks Grapple VRAM transfer rebinding across legacy and typed debugger snapshots.</summary>
internal static class GrappleTileBindingVerification
{
    /// <summary>Confirms queued endpoint and rope transfers resolve current artwork after restore without embedding image bytes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyGrappleTileBinding(SuperMetroidAddressSpace bus, GrappleTileAtlas stock, GrappleTileAtlas edited)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug); runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.NmiHandler.RunNmi(0, true);
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.FacingRightNormalPose; samus.Position = samus.Position.WithWhole(new((ushort)(runtime.Room.LoadedCamera.PixelPosition.X + 100), (ushort)(runtime.Room.LoadedCamera.PixelPosition.Y + 100)));
        samus.Presentation.InitializeAnimation(bus); samus.Presentation.PrimeGraphics(bus);
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.PointAnimationFrame = 1; samus.Grapple.PointAnimationTimer = NativeCountdown.Start(4);
        samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithX(samus.Position.X.Whole); samus.Grapple.BeamStart = samus.Grapple.BeamStart.WithY(samus.Position.Y.Whole);
        samus.Grapple.Angle = SnesAngle.FromRaw(0x6000);
        var game = CreateRetailGameFixture(bus);
        game.runtime = runtime;
        Draw(runtime);
        AssertEqual(2, runtime.VramWrites.Entries.Count, "Actual actor fixture queues endpoint/rope transfers");
        AssertTrue(runtime.VramWrites.Entries.All(entry => entry.AssetId != VramAssetId.None), "Installed actor queues typed endpoint/rope transfers");
        // Reconstruct the native records an older snapshot stores, preserving queue order,
        // byte counts and destinations from the actual actor draw.
        var entries = runtime.VramWrites._entries;
        for (int i = 0; i < entries.Count; i++)
            entries[i] = entries[i] with
            {
                SourceAddress = GrappleTileDefinitions.TransferFor(entries[i].AssetId).SourceAddress,
                AssetId = VramAssetId.None,
            };
        AssertTrue(runtime.VramWrites.Entries.All(entry => entry.AssetId == VramAssetId.None), "Legacy fixture stores native sources before binding");
        byte[] legacy = SerializeGraph(game);
        game.BindArtwork(game.Artwork with
        {
            GrappleArtwork = stock,
        });
        AssertTrue(runtime.VramWrites.Entries.All(entry => entry.AssetId != VramAssetId.None), "Binding converts known legacy transfers in place");
        byte[] stockGraph = GraphDigest(game);
        game.BindArtwork(game.Artwork with
        {
            GrappleArtwork = edited,
        });
        AssertTrue(stockGraph.SequenceEqual(GraphDigest(game)), "Changing Grapple PNG does not embed different image data in saved state");
        game.BindArtwork(game.Artwork with
        {
            GrappleArtwork = stock,
        });
        runtime.NmiHandler.RunNmi(0, true);
        byte[] nativeVram = runtime.Vram.Bytes.ToArray();
        using var stream = new MemoryStream(legacy);
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        var restoredRuntime = restored.GameplayRuntime;
        AssertTrue(restoredRuntime.Artwork.GrappleArtwork is null, "Restored state requires current host Grapple artwork");
        byte[] retained = restoredRuntime.Vram.Bytes.ToArray();
        BindRetailRuntimeFixture(restoredRuntime);
        restored.BindArtwork(restored.Artwork with
        {
            GrappleArtwork = edited,
        });
        restoredRuntime.NmiHandler.RunNmi(0, false);
        AssertTrue(retained.SequenceEqual(restoredRuntime.Vram.Bytes), "Grapple rebind preserves retained lag-frame VRAM");
        restoredRuntime.NmiHandler.RunNmi(0, true);
        nativeVram[GrappleTileDefinitions.PointDestination * 2] ^= 128;
        for (int tile = 0; tile < 4; tile++) nativeVram[GrappleTileDefinitions.SegmentDestination * 2 + tile * 32] ^= 128;
        AssertTrue(nativeVram.SequenceEqual(restoredRuntime.Vram.Bytes), "Legacy restored NMI resolves current Grapple PNG with exact edited-bit isolation");
        Draw(restoredRuntime);
        AssertTrue(restoredRuntime.VramWrites.Entries.All(entry => entry.AssetId != VramAssetId.None), "Actual rebound actor queues typed Grapple resources");
        using var typedState = new MemoryStream(SerializeGraph(restored));
        var typedRestored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(typedState);
        typedRestored.BindArtwork(typedRestored.Artwork with
        {
            GrappleArtwork = stock,
        });
        var typedRuntime = typedRestored.GameplayRuntime;
        BindRetailRuntimeFixture(typedRuntime);
        typedRestored.BindArtwork(typedRestored.Artwork with
        {
            GrappleArtwork = stock,
        });
        typedRuntime.NmiHandler.RunNmi(0, true);
        AssertTrue(stock.Resolve(GrappleTileDefinitions.PointAssetFor(typedRuntime.Player.IntroducedSamus.Grapple.PointAnimationFrame)).Span.SequenceEqual(
            typedRuntime.Vram.Bytes.Slice(GrappleTileDefinitions.PointDestination * 2, 32)), "Typed pending state also resolves newly selected endpoint artwork");
        AssertTrue(stock.Resolve(GrappleTileDefinitions.SegmentAssetFor(typedRuntime.Player.IntroducedSamus.Grapple.Angle.RawValue)).Span.SequenceEqual(
            typedRuntime.Vram.Bytes.Slice(GrappleTileDefinitions.SegmentDestination * 2, 128)), "Typed pending state also resolves newly selected rope artwork");
        static void Draw(SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            target.ActorDrawing.DrawGameplayActors(false, false);
        }
    }
}
