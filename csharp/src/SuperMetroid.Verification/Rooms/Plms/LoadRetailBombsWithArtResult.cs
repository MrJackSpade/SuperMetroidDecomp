using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The vram, tile word and item of one load retail bombs with art result.</summary>
/// <param name="Vram">The vram.</param>
/// <param name="TileWord">The tile word.</param>
/// <param name="Item">The item.</param>
internal readonly record struct LoadRetailBombsWithArtResult(byte[] Vram, ushort TileWord, CollectiblePlmSnapshot Item);
