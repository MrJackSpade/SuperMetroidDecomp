using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Verification.Harness;

/// <summary>The address and definition of one lookup stream1 definition.</summary>
/// <param name="Address">The address.</param>
/// <param name="Definition">The definition.</param>
internal readonly record struct LookupStream1Definition(int Address, SamusBodyTileDefinition Definition);
