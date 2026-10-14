namespace SuperMetroid.Core.Assets;

/// <summary>One Samus body tile allocation: the upper or lower half, its tile set, and the position within that set (#633).</summary>
internal readonly record struct SamusBodyTileSlot(bool Upper, int Set, int Position);
