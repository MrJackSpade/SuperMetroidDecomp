
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Two poses that mirror each other.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct SamusPoseMirrorPair(SamusPoseId First, SamusPoseId Second);
