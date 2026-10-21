
namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>A control byte of an arm cannon drawing.</summary>
/// <param name="Drawing">The drawing.</param>
/// <param name="ByteOffset">The byte offset.</param>
internal readonly record struct ArmCannonControlByte(SamusArmCannonArtworkFormat.ArmCannonDrawing Drawing, int ByteOffset);
