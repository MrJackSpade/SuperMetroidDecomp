using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.Verification.Fx;

/// <summary>The read and shift of one verify fx pair storage case.</summary>
/// <param name="Read">The read.</param>
/// <param name="Shift">The shift.</param>
internal readonly record struct VerifyFxPairStorageCase(Func<RoomFxPairColor, int?> Read, int Shift);
