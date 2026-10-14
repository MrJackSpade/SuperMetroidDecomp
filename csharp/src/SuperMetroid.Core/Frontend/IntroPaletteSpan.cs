namespace SuperMetroid.Core.Frontend;

/// <summary>One byte-indexed CGRAM span used by the intro palette fader.</summary>
public readonly record struct IntroPaletteSpan(ushort ByteOffset, ushort ByteCount);
