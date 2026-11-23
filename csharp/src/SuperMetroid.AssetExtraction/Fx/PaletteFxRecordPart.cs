using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>One run of a colour record: colours written in sequence, or colour entries skipped.</summary>
/// <param name="Skip">True for skipped colour entries, false for written colours.</param>
/// <param name="Count">How many entries.</param>
internal readonly partial record struct PaletteFxRecordPart(bool Skip, int Count);
