using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Verification.Fx;

/// <summary>The read and shift of one verify fx pair storage and edits case.</summary>
/// <param name="Read">The read.</param>
/// <param name="Shift">The shift.</param>
internal readonly record struct VerifyFxPairStorageAndEditsCase(Func<RoomFxPairColor, int?> Read, int Shift);
