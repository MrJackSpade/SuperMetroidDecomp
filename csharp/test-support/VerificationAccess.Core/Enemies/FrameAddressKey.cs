namespace SuperMetroid.Core.Enemies;

/// <summary>The bank and word that identify an enemy frame.</summary>
/// <param name="Bank">The bank.</param>
/// <param name="Word">The word.</param>
internal readonly record struct FrameAddressKey(byte Bank, ushort Word);
