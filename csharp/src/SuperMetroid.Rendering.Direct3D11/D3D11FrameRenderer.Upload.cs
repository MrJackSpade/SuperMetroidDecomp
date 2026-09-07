using System.Diagnostics;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    private long uploadBytes, uploadCalls, uploadTicks;

    /// <summary>Owner-thread cumulative CPU upload submission, including reusable child scenes.
    /// This measures UpdateSubresource calls, not GPU transfer completion or memory packing.</summary>
    public RenderUploadStatistics UploadStatistics
    {
        get
        {
            owner.VerifyOwner();
            var child = childRenderer?.UploadStatistics ?? default;
            return new(uploadBytes + child.Bytes, uploadCalls + child.Calls,
                uploadTicks * 1000.0 / Stopwatch.Frequency + child.CpuMilliseconds);
        }
    }

    private void UploadBuffer(ID3D11Buffer buffer, nint source, int bytes)
    {
        long started = Stopwatch.GetTimestamp();
        owner.Context.UpdateSubresource(buffer, 0, null, source, 0, 0);
        uploadTicks += Stopwatch.GetTimestamp() - started;
        uploadBytes += bytes;
        uploadCalls++;
    }
}

/// <summary>Cumulative bytes submitted and CPU call time; not asynchronous GPU transfer duration.</summary>
public readonly record struct RenderUploadStatistics(long Bytes, long Calls, double CpuMilliseconds);
