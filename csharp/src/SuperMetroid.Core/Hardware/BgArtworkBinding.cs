namespace SuperMetroid.Core.Hardware;

/// <summary>One background artwork binding: owned character bytes shown over a set's characters from one onward.</summary>
/// <param name="Identity">The artwork shown.</param>
/// <param name="Set">The characters it shows over.</param>
/// <param name="FirstCharacter">The first character the bytes stand for.</param>
/// <param name="Bytes">The bytes, whole characters of the set.</param>
public sealed record BgArtworkBinding(BgArtworkIdentity Identity, BgCharacterSet Set, int FirstCharacter, ReadOnlyMemory<byte> Bytes)
{
    /// <summary>The characters the binding covers.</summary>
    public int CharacterCount => Bytes.Length / Set.CharacterBytes();
}
