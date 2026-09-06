using System.Runtime.InteropServices;
using System.Text;
using SuperMetroid.Core.Audio;

/// <summary>
/// Writes each contiguous stable-pause interval as unmodified stereo PCM for listening
/// independently of waveOut/RDP. Files are created exclusively, never overwritten.
/// </summary>
internal sealed class PausePcmCapture(string directory) : IDisposable
{
    private BinaryWriter? writer;
    private int interval;
    private int firstFrame;
    private int byteCount;

    public void Observe(int frame, bool paused, short[] samples)
    {
        if (!paused)
        {
            Finish(frame);
            return;
        }
        if (writer is null)
        {
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, $"pause-{interval++:D3}.wav");
            writer = new BinaryWriter(new FileStream(path, FileMode.CreateNew, FileAccess.Write), Encoding.ASCII);
            firstFrame = frame;
            byteCount = 0;
            writer.Write("RIFF"u8);
            writer.Write(0);
            writer.Write("WAVEfmt "u8);
            writer.Write(16);
            writer.Write((ushort)1); // RIFF PCM, interleaved stereo, sixteen-bit samples.
            writer.Write((ushort)2);
            writer.Write(SpcDriverData.HostSampleRate);
            writer.Write(SpcDriverData.HostSampleRate * 2 * sizeof(short));
            writer.Write((ushort)(2 * sizeof(short)));
            writer.Write((ushort)16);
            writer.Write("data"u8);
            writer.Write(0);
            Console.WriteLine($"Capturing stable pause from frame {frame}: {Path.GetFullPath(path)}");
        }
        writer.Write(MemoryMarshal.AsBytes(samples.AsSpan()));
        byteCount = checked(byteCount + samples.Length * sizeof(short));
    }

    private void Finish(int? endFrame)
    {
        if (writer is null) return;
        using BinaryWriter completed = writer;
        writer = null;
        completed.Seek(4, SeekOrigin.Begin);
        completed.Write(checked(36 + byteCount));
        completed.Seek(40, SeekOrigin.Begin);
        completed.Write(byteCount);
        Console.WriteLine($"Captured pause [{firstFrame},{endFrame?.ToString() ?? "end"}): {byteCount} PCM bytes.");
    }

    public void Dispose() => Finish(null);
}
