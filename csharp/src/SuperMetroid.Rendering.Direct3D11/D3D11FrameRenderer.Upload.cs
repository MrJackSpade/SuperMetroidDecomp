using System.Diagnostics;
using Vortice.Direct3D11;

namespace SuperMetroid.Rendering.Direct3D11;

public sealed partial class D3D11FrameRenderer
{
    /// <summary>Accumulates submitted byte counts, upload-call counts, and owner-thread CPU ticks across renderer updates.</summary>
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

    /// <summary>Submits one buffer update on the renderer's owner context and records its CPU cost and byte count.</summary>
    /// <param name="buffer">D3D11 destination buffer updated by the immediate context.</param>
    /// <param name="source">Pointer to the source bytes for the update.</param>
    /// <param name="bytes">Number of source bytes included in cumulative upload statistics.</param>
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
/// <param name="Bytes">Total number of bytes submitted through buffer updates.</param>
/// <param name="Calls">Number of buffer-update calls submitted.</param>
/// <param name="CpuMilliseconds">Owner-thread CPU time spent submitting updates, in milliseconds.</param>
public readonly record struct RenderUploadStatistics(long Bytes, long Calls, double CpuMilliseconds);
