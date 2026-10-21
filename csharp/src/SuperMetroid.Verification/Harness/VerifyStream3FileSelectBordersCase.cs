using SuperMetroid.Core.Frontend.FileSelect.Artwork;

namespace SuperMetroid.Verification.Harness;

/// <summary>The border, address and count of one verify stream3 file select borders case.</summary>
/// <param name="Border">The border.</param>
/// <param name="Address">The address.</param>
/// <param name="Count">The count.</param>
internal readonly record struct VerifyStream3FileSelectBordersCase(FileSelectBorder Border, int Address, int Count);
