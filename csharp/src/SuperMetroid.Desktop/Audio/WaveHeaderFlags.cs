namespace SuperMetroid.Desktop.Audio;

/// <summary>The WinMM <c>WAVEHDR.dwFlags</c> bits the wave output reads (mmsystem.h <c>WHDR_*</c>).</summary>
[Flags]
internal enum WaveHeaderFlags : uint
{
    /// <summary><c>WHDR_DONE</c>: Windows has finished playing the buffer.</summary>
    Done = 0x0000_0001,
}
