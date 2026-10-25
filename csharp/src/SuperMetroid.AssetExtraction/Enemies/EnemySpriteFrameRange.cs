using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>A run of a composition's parts and the tile base and palette row they draw with.</summary>
/// <param name="Start">The first part.</param>
/// <param name="Count">How many parts.</param>
/// <param name="Base">The packed tile base added to every part's attribute word.</param>
/// <param name="Palette">The palette row ORed into every part's attribute word.</param>
[EncodingOwner("enemy sprite frame tile base and palette row")]
internal readonly record struct EnemySpriteFrameRange(int Start, int Count, int Base, int Palette);
