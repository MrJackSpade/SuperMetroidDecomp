using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;
using Vortice.Direct3D11;
using Vortice.DXGI;

internal static class DisplayPassTests
{
    internal static unsafe void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var packet = RenderFrameSnapshotCodec.Deserialize(File.ReadAllBytes(
            "csharp/test-fixtures/issue-321-tile-byte-selection/frame.smframe"));
        var native = SoftwareFrameSnapshotRenderer.Render(packet);
        int count = 0;
        foreach (var size in new[] { (256,224), (512,448), (800,600), (1920,1080), (319,601), (1,1), (1,224), (256,1) })
        {
            int width = size.Item1, height = size.Item2;
            using var target = device.Device.CreateTexture2D(new Texture2DDescription(Format.B8G8R8A8_UNorm,
                (uint)width, (uint)height, 1, 1, BindFlags.RenderTarget));
            using var view = device.Device.CreateRenderTargetView(target);
            using var staging = device.Device.CreateTexture2D(new Texture2DDescription(Format.B8G8R8A8_UNorm,
                (uint)width, (uint)height, 1, 1, BindFlags.None, ResourceUsage.Staging, CpuAccessFlags.Read));
            renderer.Render(packet);
            renderer.DrawDisplay(view, width, height);
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
                    Rgba32 expected = x < left || x >= left + w || y < top || y >= top + h
                        ? new(0,0,0) : native[((y - top) * 224 / h) * 256 + (x - left) * 256 / w];
                    uint actual = *(uint*)((byte*)mapped.DataPointer + y * mapped.RowPitch + x * 4);
                    uint packed = (uint)(expected.B | expected.G << 8 | expected.R << 16 | 255 << 24);
                    if (actual != packed) throw new InvalidOperationException(
                        $"{device.Kind}: display {width}x{height} at ({x},{y}): expected {packed:X8}, actual {actual:X8}.");
                }
            }
            finally { device.Context.Unmap(staging, 0); }
            // Display SRV binding must not poison subsequent compute UAV use.
            PixelComparison.Verify(packet, native, renderer.RenderForReadback(packet), $"{device.Kind}: compute after display {size}");
            count++;
        }
        Console.WriteLine($"{device.Kind}: {device.AdapterDescription}; {count} scaled display targets and subsequent compute frames match.");
    }
}
