namespace SuperMetroid.Core.Input;

/// <summary>Mutually exclusive WinMM MMRESULT values consumed by the adapter.</summary>
public enum WindowsMultimediaResult : uint
{
    /// <summary>WinMM success result <c>JOYERR_NOERROR</c>/<c>MMSYSERR_NOERROR</c>; the adapter accepts capabilities and position data only when the native call returns zero.</summary>
    NoError = 0,
}
