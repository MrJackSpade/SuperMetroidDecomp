using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Detached diagnostic identity of one live resident scroll trigger; it does not expose mutable slot or scroll-program state.</summary>
/// <param name="BlockIndex">Zero-based row-major 16x16 room-block index, not the native doubled block-byte offset.</param>
public readonly record struct ScrollPlmSnapshot(
    int BlockIndex);
