using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// One copy a step of an encounter's artwork sequence makes over the cartridge's sprite characters (#1513): the
/// characters and the sprite character they land from. Only the import knows it; it composes the step's pictures from it.
/// </summary>
/// <param name="FirstCharacter">The sprite character the copy lands from.</param>
/// <param name="Characters">The bytes it copies.</param>
internal sealed record EncounterArtworkUpload(int FirstCharacter, ReadOnlyMemory<byte> Characters)
{
    /// <summary>The sprite character the copy lands from.</summary>
    internal int FirstCharacter { get; } = Ensure.BetweenInclusive(0, SpriteCharacterTableLayout.CharacterCount - 1, FirstCharacter);

    /// <summary>The bytes it copies.</summary>
    internal ReadOnlyMemory<byte> Characters { get; } = Characters.Length > 0 ? Characters
        : throw new InvalidDataException("An encounter's artwork copy copies no bytes.");

    /// <summary>The table byte the copy lands from.</summary>
    internal int FirstByte => FirstCharacter * SpriteCharacterTableLayout.CharacterByteCount;

    /// <summary>Copies the characters over <paramref name="table"/>, as far as the table reaches.</summary>
    internal void ApplyTo(byte[] table)
    {
        Ensure.NotNull(table);
        int shown = Math.Min(Characters.Length, table.Length - FirstByte);
        if (shown > 0)
            Characters.Span[..shown].CopyTo(table.AsSpan(FirstByte));
    }
}
