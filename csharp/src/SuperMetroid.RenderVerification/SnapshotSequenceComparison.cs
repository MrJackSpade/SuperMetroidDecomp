using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Checks captured frames in order on a persistent GPU renderer, including first-use transitions.</summary>
internal static class SnapshotSequenceComparison
{
    public static void Run(string directory)
    {
        // Lexically ordered, zero-padded capture names preserve the original frame
        // order. Keep one renderer alive: recreating it per frame hides stale state.
        string[] files = Directory.GetFiles(directory, "frame-*.smframe");
        Array.Sort(files, StringComparer.Ordinal);
        if (files.Length == 0) throw new InvalidDataException("No frame-*.smframe captures found.");
        foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
        {
            using var device = new D3D11RenderDevice(kind);
            using var renderer = new D3D11FrameRenderer(device);
            foreach (string file in files)
            {
                var packet = RenderFrameSnapshotCodec.Deserialize(File.ReadAllBytes(file));
                PixelComparison.Verify(packet, SoftwareFrameSnapshotRenderer.Render(packet),
                    renderer.RenderForReadback(packet), $"{kind}: {Path.GetFileName(file)}");
            }
            Console.WriteLine($"{kind}: {device.AdapterDescription}; {files.Length} sequential frames match software exactly.");
        }
    }
}
