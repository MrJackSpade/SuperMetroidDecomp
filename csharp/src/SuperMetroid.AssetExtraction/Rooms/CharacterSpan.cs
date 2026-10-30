using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>A run of characters in one background character set: what an appearance shows over (#1466).</summary>
/// <param name="Set">The character set the run lies in.</param>
/// <param name="First">The run's first character.</param>
/// <param name="Count">The characters in the run.</param>
internal readonly record struct CharacterSpan(BgCharacterSet Set, int First, int Count);
