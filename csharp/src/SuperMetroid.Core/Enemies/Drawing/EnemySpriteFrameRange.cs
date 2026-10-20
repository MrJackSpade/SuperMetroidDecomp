namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>A run of an enemy's sprite frames and the tile base and palette they draw with.</summary>
/// <param name="Start">The first frame.</param>
/// <param name="Count">How many frames.</param>
/// <param name="Base">The packed tile base added to every part's attribute word.</param>
/// <param name="Palette">The palette bits ORed into every part's attribute word.</param>
[SnesEncodingOwner]
internal readonly record struct EnemySpriteFrameRange(int Start, int Count, ushort Base, ushort Palette);
