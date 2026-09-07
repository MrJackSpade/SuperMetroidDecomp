using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

internal static class ShaderFailureTests
{
    internal static void Run(D3D11RenderDevice device)
    {
        foreach (string missing in new[] { D3D11ShaderLayout.SolidResourceName, D3D11ShaderLayout.TileResourceName,
            D3D11ShaderLayout.DisplayVertexResourceName, D3D11ShaderLayout.DisplayPixelResourceName })
        {
            bool rejected = false;
            try
            {
                using var failed = new D3D11FrameRenderer(device, name => name == missing ? null :
                    typeof(D3D11FrameRenderer).Assembly.GetManifestResourceStream(name));
            }
            catch (InvalidDataException error)
            {
                if (!error.Message.Contains(missing)) throw new InvalidOperationException("Shader diagnostic lost missing resource identity.", error);
                rejected = true;
            }
            if (!rejected) throw new InvalidOperationException($"Missing shader silently accepted: {missing}");
            // Late shader failure occurs after earlier native resources were created.
            // A fresh renderer must still execute correctly on the same owner/device.
            using var recovered = new D3D11FrameRenderer(device);
            var packet = new RenderFrameSnapshot(new(1, 1, 0), new Rgba32(13, 29, 71));
            PixelComparison.Verify(packet, SoftwareFrameSnapshotRenderer.Render(packet), recovered.RenderForReadback(packet),
                $"{device.Kind}: initialization after missing {missing}");
        }
        Console.WriteLine($"{device.Kind}: all four missing shaders fail with resource identity; subsequent initialization renders exactly.");
    }
}
