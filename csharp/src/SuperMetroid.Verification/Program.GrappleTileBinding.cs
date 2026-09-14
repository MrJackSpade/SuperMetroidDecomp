using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static void VerifyGrappleTileBinding(SuperMetroidAddressSpace bus, GrappleTileAtlas stock, GrappleTileAtlas edited)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.RunNmi(0, true);
        var samus = runtime.Samus!;
        samus.Pose = 1; samus.XPosition = (ushort)(runtime.Camera!.XPosition + 100); samus.YPosition = (ushort)(runtime.Camera.YPosition + 100);
        samus.InitializeAnimation(bus); samus.PrimeGraphics(bus);
        samus.Grapple.Phase = GrapplePhase.ConnectedLocked;
        samus.Grapple.PointAnimationFrame = 1; samus.Grapple.PointAnimationTimer = 4;
        samus.Grapple.BeamStartX = samus.XPosition; samus.Grapple.BeamStartY = samus.YPosition;
        samus.Grapple.Angle = SnesAngle.FromRaw(0x6000);
        var game = new SuperMetroidGame(bus);
        var runtimeField = typeof(SuperMetroidGame).GetField("runtime", flags)!;
        runtimeField.SetValue(game, runtime);
        Draw(runtime);
        AssertEqual(2, runtime.VramWrites.Entries.Count, "Actual actor fixture queues endpoint/rope transfers");
        AssertTrue(runtime.VramWrites.Entries.All(entry => entry.AssetId == VramAssetId.None), "Legacy fixture stores native sources before binding");
        byte[] legacy = SaveGrappleFixture(game);
        game.BindGrappleArtwork(stock);
        AssertTrue(runtime.VramWrites.Entries.All(entry => entry.AssetId != VramAssetId.None), "Binding converts known legacy transfers in place");
        byte[] stockGraph = SaveGrappleFixture(game);
        game.BindGrappleArtwork(edited);
        AssertTrue(stockGraph.SequenceEqual(SaveGrappleFixture(game)), "Changing Grapple PNG does not embed different image data in saved state");
        game.BindGrappleArtwork(stock);
        runtime.RunNmi(0, true);
        byte[] nativeVram = runtime.Vram.Bytes.ToArray();
        using var stream = new MemoryStream(legacy);
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(stream);
        var restoredRuntime = (SuperMetroidRuntime)runtimeField.GetValue(restored)!;
        AssertTrue(restoredRuntime.GrappleArtwork is null, "Restored state requires current host Grapple artwork");
        byte[] retained = restoredRuntime.Vram.Bytes.ToArray();
        restored.BindGrappleArtwork(edited);
        restoredRuntime.RunNmi(0, false);
        AssertTrue(retained.AsSpan().SequenceEqual(restoredRuntime.Vram.Bytes), "Grapple rebind preserves retained lag-frame VRAM");
        restoredRuntime.RunNmi(0, true);
        nativeVram[GrappleTileDefinitions.PointDestination * 2] ^= 128;
        for (int tile = 0; tile < 4; tile++) nativeVram[GrappleTileDefinitions.SegmentDestination * 2 + tile * 32] ^= 128;
        AssertTrue(nativeVram.AsSpan().SequenceEqual(restoredRuntime.Vram.Bytes), "Legacy restored NMI resolves current Grapple PNG with exact edited-bit isolation");
        Draw(restoredRuntime);
        AssertTrue(restoredRuntime.VramWrites.Entries.All(entry => entry.AssetId != VramAssetId.None), "Actual rebound actor queues typed Grapple resources");
        using var typedState = new MemoryStream(SaveGrappleFixture(restored));
        var typedRestored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(typedState);
        typedRestored.BindGrappleArtwork(stock);
        var typedRuntime = (SuperMetroidRuntime)runtimeField.GetValue(typedRestored)!;
        typedRuntime.RunNmi(0, true);
        AssertTrue(stock.Resolve(GrappleTileDefinitions.PointAssetFor(typedRuntime.Samus!.Grapple.PointAnimationFrame)).Span.SequenceEqual(
            typedRuntime.Vram.Bytes.Slice(GrappleTileDefinitions.PointDestination * 2, 32)), "Typed pending state also resolves newly selected endpoint artwork");
        AssertTrue(stock.Resolve(GrappleTileDefinitions.SegmentAssetFor(typedRuntime.Samus.Grapple.Angle.RawValue)).Span.SequenceEqual(
            typedRuntime.Vram.Bytes.Slice(GrappleTileDefinitions.SegmentDestination * 2, 128)), "Typed pending state also resolves newly selected rope artwork");
        static void Draw(SuperMetroidRuntime target)
        {
            target.Oam.BeginFrame();
            typeof(SuperMetroidRuntime).GetMethod("DrawGameplayActors", flags)!.Invoke(target, new object?[] { false, null, null, false });
        }
    }
}
