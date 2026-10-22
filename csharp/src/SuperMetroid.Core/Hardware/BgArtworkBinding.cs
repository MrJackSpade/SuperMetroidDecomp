namespace SuperMetroid.Core.Hardware;

/// <summary>One background artwork binding: owned character bytes shown from a VRAM word onward.</summary>
/// <param name="Identity">The artwork shown.</param>
/// <param name="FirstWord">The first VRAM word the bytes stand for.</param>
/// <param name="Bytes">The bytes, low byte first, two per word.</param>
/// <param name="Stamp">When the binding was made, on the same clock as <see cref="SnesVram"/>'s writes.</param>
public sealed record BgArtworkBinding(BgArtworkIdentity Identity, int FirstWord, ReadOnlyMemory<byte> Bytes, uint Stamp)
{
    /// <summary>The VRAM words the binding covers.</summary>
    public int WordCount => Bytes.Length / sizeof(ushort);
}
