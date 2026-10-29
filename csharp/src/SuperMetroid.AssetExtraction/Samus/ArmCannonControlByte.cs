
namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>A control byte of an arm cannon drawing.</summary>
/// <param name="Drawing">Where the drawing's descriptor starts in the drawing window.</param>
/// <param name="ByteOffset">The byte offset.</param>
internal readonly record struct ArmCannonControlByte(int Drawing, int ByteOffset);
