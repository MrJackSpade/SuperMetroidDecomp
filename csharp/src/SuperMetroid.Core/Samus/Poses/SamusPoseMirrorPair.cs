
namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Two poses that mirror each other.</summary>
/// <param name="First">One pose.</param>
/// <param name="Second">The pose that mirrors it.</param>
internal readonly record struct SamusPoseMirrorPair(SamusPoseId First, SamusPoseId Second);
