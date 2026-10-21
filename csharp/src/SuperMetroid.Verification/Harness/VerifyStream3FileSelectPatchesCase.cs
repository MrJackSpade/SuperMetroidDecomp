using SuperMetroid.Core.Frontend.FileSelect.Artwork;

namespace SuperMetroid.Verification.Harness;

/// <summary>The patch, address and count of one verify stream3 file select patches case.</summary>
/// <param name="Patch">The patch.</param>
/// <param name="Address">The address.</param>
/// <param name="Count">The count.</param>
internal readonly record struct VerifyStream3FileSelectPatchesCase(FileSelectPatch Patch, int Address, int Count);
