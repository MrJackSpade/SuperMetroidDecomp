namespace SuperMetroid.Core.Assets;

/// <summary>Runtime PPU representation compiled from authored offsets, regions, size and color selection.</summary>
internal readonly record struct CompiledSpritePart(SnesSpritemapXWord X, byte Y, SnesObjAttributeWord Attributes, bool InheritPalette);
