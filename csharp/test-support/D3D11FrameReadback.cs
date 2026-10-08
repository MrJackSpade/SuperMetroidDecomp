using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace SuperMetroid.Rendering.Direct3D11;

/// <summary>
/// CPU readback of a renderer's last completed frame, for the debug runner and render verification;
/// player hosts present on the GPU and never read frames back.
/// </summary>
internal static class D3D11FrameReadback
{
    extension(D3D11FrameRenderer self)
    {
        /// <summary>Diagnostic convenience; production presentation must use Render without readback.</summary>
        public Rgba32[] RenderForReadback(RenderFrameSnapshot packet)
        {
            self.Render(packet);
            return self.Readback();
        }
        /// <summary>Reads the last completed submission for diagnostics; never advances simulation.</summary>
        public unsafe Rgba32[] Readback()
        {
            self.owner.VerifyOwner(); ObjectDisposedException.ThrowIf(self.disposed, self);
            if (self.renderedIdentity is null) throw new InvalidOperationException("No successful frame submission is available for readback.");
            const int width = SnesPpuLayout.ScreenWidthPixels, height = SnesPpuLayout.ScreenHeightPixels;
            using ID3D11Texture2D staging = self.owner.Device.CreateTexture2D(new Texture2DDescription(Format.R32_UInt,
                width, height, 1, 1, BindFlags.None, ResourceUsage.Staging, CpuAccessFlags.Read));
            self.owner.Context.CSSetUnorderedAccessView(0, null);
            self.owner.Context.CSSetUnorderedAccessView(1, null);
            self.owner.Context.CopyResource(staging, self.output);
            MappedSubresource mapped = self.owner.Context.Map(staging, 0, MapMode.Read);
            try
            {
                var pixels = new Rgba32[width * height];
                for (int y = 0; y < height; y++)
                {
                    uint* row = (uint*)((byte*)mapped.DataPointer + y * mapped.RowPitch);
                    for (int x = 0; x < width; x++)
                    {
                        uint packed = row[x];
                        pixels[y * width + x] = new((byte)packed, (byte)(packed >> 8), (byte)(packed >> 16), (byte)(packed >> 24));
                    }
                }
                return pixels;
            }
            finally { self.owner.Context.Unmap(staging, 0); }
        }
    }
}
