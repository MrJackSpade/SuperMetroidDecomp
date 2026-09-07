using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class RetailFileMapTests
{
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        int samples = 0;
        void Compare(Rgba32[] expected, LayeredRenderSnapshot scene, string context)
        {
            var packet = RenderFrameSnapshotCodec.Deserialize(RenderFrameSnapshotCodec.Serialize(
                new RenderFrameSnapshot(new(++samples, 1, 0), scene)));
            PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), $"{device.Kind}: file map {context}");
        }
        for (int area = 0; area < 6; area++)
        {
            var graphics = new FileSelectAreaMapGraphics(bus, area);
            foreach (bool backdrop in new[] { false, true })
            foreach (ushort used in new ushort[] { 0, 1, ushort.MaxValue })
            {
                ushort[] masks = Enumerable.Repeat(used, 6).ToArray();
                Compare(graphics.Render(masks, backdrop), graphics.CaptureRenderSnapshot(masks, backdrop), $"area={area}, labels/backdrop");
            }
            var room = new FileSelectRoomMapGraphics(bus, new Bank80SystemState(), (AreaId)area);
            Compare(room.RenderFrameOnly(), room.CaptureRenderSnapshot(frameOnly: true), "frame-only");
            foreach (ushort scroll in new ushort[] { 0, 255, 511, ushort.MaxValue })
                Compare(room.RenderBackgrounds(scroll, scroll), room.CaptureRenderSnapshot(scroll, scroll), $"room={area}, scroll={scroll}");
        }
        foreach (bool cancel in new[] { false, true })
        {
            var saves = new SuperMetroidSaveRam(bus);
            var save = new SuperMetroidSaveSnapshot { Area = 4, SaveStation = 0, Health = 99, MaxHealth = 99 };
            save.MapStationBytes[4] = 1; save.UsedSaveStationBytes[8] = 1;
            saves.SaveSlot(0, save);
            var legacy = new FileSelectMapMenuState(bus, new CartridgeAudioState(), saves.ReadSlot(0)!, 0);
            var captured = new FileSelectMapMenuState(bus, new CartridgeAudioState(), saves.ReadSlot(0)!, 0);
            var phases = new HashSet<FileSelectMapNavigationPhase>();
            for (int tick = 0; tick < 300 && !legacy.LoadRequested && !legacy.OptionsRequested; tick++)
            {
                ushort input = tick == 48 ? (ushort)SnesButton.Start
                    : tick == 130 ? (ushort)(cancel ? SnesButton.B : SnesButton.Start)
                    : cancel && tick == 240 ? (ushort)SnesButton.B
                    : tick is >= 110 and <= 119 ? (ushort)SnesButton.Right : (ushort)0;
                legacy.Step(input); captured.Step(input);
                phases.Add(legacy.Phase);
                Compare(legacy.Render(), captured.CaptureRenderSnapshot(), $"phase={legacy.Phase}, tick={tick}, cancel={cancel}");
                if (legacy.Phase != captured.Phase || legacy.LoadRequested != captured.LoadRequested ||
                    legacy.OptionsRequested != captured.OptionsRequested)
                    throw new InvalidOperationException("GPU file-map capture changed navigation signals.");
            }
            if (!phases.Contains(FileSelectMapNavigationPhase.Room) || !phases.Contains(FileSelectMapNavigationPhase.ExpandingWindow) ||
                !(cancel ? legacy.OptionsRequested : legacy.LoadRequested))
                throw new InvalidOperationException("GPU file-map route did not complete its expected coverage.");
        }
        Console.WriteLine($"{device.Kind}: {samples} exact retail file-map frames; all areas, wrapped scroll, zoom, load/cancel routes passed.");
        RetailLoadAppearanceTests.Run(device, renderer);
    }
}
