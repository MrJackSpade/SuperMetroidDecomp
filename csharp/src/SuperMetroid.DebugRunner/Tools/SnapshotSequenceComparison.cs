using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;

/// <summary>Checks captured frames in order on a persistent GPU renderer, including first-use transitions.</summary>
internal static class SnapshotSequenceComparison
{
    /// <summary>Omits packets, not emulation steps, to model a slower presentation consumer.</summary>
    public static void RunSparse(string directory)
    {
        string[] files = Directory.GetFiles(directory, "frame-*.smframe");
        Array.Sort(files, StringComparer.Ordinal);
        if (files.Length < 2) throw new InvalidDataException("Sparse comparison needs an ordinary frame and a subsequent sequence.");
        var packets = files.Select(file => RenderFrameSnapshotCodec.Deserialize(File.ReadAllBytes(file))).ToArray();
        var expected = packets.Select(packet => SoftwareFrameSnapshotRenderer.Render(packet)).ToArray();
        foreach (D3D11DeviceKind kind in Enum.GetValues<D3D11DeviceKind>())
        {
            using var device = new D3D11RenderDevice(kind);
            int checkedFrames = 0, schedules = 0;
            // Every phase of each stride is important: otherwise all runs could
            // accidentally retain the one setup frame that initializes a resource.
            for (int stride = 2; stride <= 8; stride++)
            for (int phase = 0; phase < stride; phase++)
            {
                using var renderer = new D3D11FrameRenderer(device);
                Verify(0);
                for (int frame = 1 + phase; frame < files.Length; frame += stride) Verify(frame);
                schedules++;
                void Verify(int index)
                {
                    PixelComparison.Verify(packets[index], expected[index], renderer.RenderForReadback(packets[index]),
                        $"{kind}: stride {stride}, phase {phase}, {Path.GetFileName(files[index])}");
                    checkedFrames++;
                }
            }
            Console.WriteLine($"{kind}: {device.AdapterDescription}; {schedules} sparse schedules, {checkedFrames} retained frames match software exactly.");
        }
    }

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
