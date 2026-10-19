namespace SuperMetroid.Core.Hardware;

/// <summary>
/// The window selections of two layers that share one selection register: the lower layer (BG1, BG3 or OBJ) and the
/// upper layer (BG2, BG4 or the colour window). Each layer composes its own <see cref="SnesWindowSelection"/> bits.
/// </summary>
[Flags]
public enum SnesWindowSelectionPair : byte
{
}
