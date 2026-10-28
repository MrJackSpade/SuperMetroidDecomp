using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The vram, slot artwork and item of one load retail bombs with art result.</summary>
/// <param name="Vram">The vram.</param>
/// <param name="SlotArtwork">The artwork bound over the item's slot characters, which names its blocks' pictures (#1431).</param>
/// <param name="Item">The item.</param>
internal readonly record struct LoadRetailBombsWithArtResult(byte[] Vram, BgArtworkIdentity SlotArtwork, CollectiblePlmSnapshot Item);
