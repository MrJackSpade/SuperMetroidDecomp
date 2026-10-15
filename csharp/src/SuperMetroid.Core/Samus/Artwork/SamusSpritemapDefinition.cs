namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A native bank-$92 spritemap record and its editable five-byte OAM parts.</summary>
public sealed record SamusSpritemapDefinition(ushort Pointer, SamusSpritePart[] Parts);
