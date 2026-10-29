using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Rendering.Direct3D11.Telemetry;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.RenderVerification.Device;
using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks the D3D11 display scaler, GPU timing boundaries, and compute reuse after display.</summary>
internal static class DisplayPassTests
{
    /// <summary>Compares scaled display pixels against the legacy desktop layout oracle across window sizes and verifies subsequent compute rendering.</summary>
    internal static unsafe void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        // Seeded tile memory gives every display size a full-screen image with no flat runs.
        var random = new Random(32111);
        var vram = new byte[SnesVram.ByteCount];
        random.NextBytes(vram);
        RgbColor[] palette = Enumerable.Range(0, SnesCgram.ColorCount)
            .Select(_ => RgbColor.FromWord((ushort)random.Next(32768))).ToArray();
        var packet = new DisplayPacket(new(1, 1, 0), new LayeredScene(FramePaint.FromNative(palette, []),
            [new ScreenTilesLayer(VramTileFixtures.Cells(vram, palette, 0x4000, 0x6000, TileInkDepth.SixteenColours, new Extent16<TilemapCells>(32, 32)),
                new Position16<TilePlanePixels>(3, 11), null)], 15));
        var native = DisplayRendering.Render(packet);
        int count = 0;
        using var timer = new D3D11GpuTimer(device);
        foreach (var size in new[] { new DisplaySize(256,224), new DisplaySize(512,448), new DisplaySize(800,600), new DisplaySize(1920,1080), new DisplaySize(319,601), new DisplaySize(1,1), new DisplaySize(1,224), new DisplaySize(256,1) })
        {
            int width = size.Width, height = size.Height;
            using var target = device.Device.CreateTexture2D(new Texture2DDescription(Format.B8G8R8A8_UNorm,
                (uint)width, (uint)height, 1, 1, BindFlags.RenderTarget));
            using var view = device.Device.CreateRenderTargetView(target);
            using var staging = device.Device.CreateTexture2D(new Texture2DDescription(Format.B8G8R8A8_UNorm,
                (uint)width, (uint)height, 1, 1, BindFlags.None, ResourceUsage.Staging, CpuAccessFlags.Read));
            if (!timer.TryBegin(packet.Identity)) throw new InvalidOperationException("Display timing ring unexpectedly full.");
            renderer.Render(packet);
            timer.MarkCompositionFinished();
            renderer.DrawDisplay(view, new Extent16<DisplayPixels>((ushort)width, (ushort)height));
            timer.End();
            device.Context.CopyResource(staging, target);
            var mapped = device.Context.Map(staging, 0, MapMode.Read);
            try
            {
                // Independent legacy desktop layout oracle, not the shared helper.
                int scale = Math.Max(1, Math.Min(width / 299, height / 224));
                int w = 299 * scale, h = 224 * scale;
                int left = (width - w) / 2, top = (height - h) / 2;
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    RgbaPixel expected = x < left || x >= left + w || y < top || y >= top + h
                        ? new(0,0,0) : native[((y - top) * 224 / h) * 256 + (x - left) * 256 / w];
                    uint actual = *(uint*)((byte*)mapped.DataPointer + y * mapped.RowPitch + x * 4);
                    uint packed = System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian([expected.B, expected.G, expected.R, 255]);
                    if (actual != packed) throw new InvalidOperationException(
                        $"{device.Kind}: display {width}x{height} at ({x},{y}): expected {packed:X8}, actual {actual:X8}.");
                }
            }
            finally { device.Context.Unmap(staging, 0); }
            if (!timer.TryRead(out var timing) || !timing.Valid || timing.CompositionMilliseconds > timing.Milliseconds)
                throw new InvalidOperationException("GPU display timing did not preserve ordered composition/display boundaries.");
            // Display SRV binding must not poison subsequent compute UAV use.
            PixelComparison.Verify(packet, native, renderer.RenderForReadback(packet), $"{device.Kind}: compute after display {size}");
            count++;
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} scaled display targets and subsequent compute frames match.");
    }
}
