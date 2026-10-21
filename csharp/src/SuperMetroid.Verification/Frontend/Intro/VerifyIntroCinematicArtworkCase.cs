using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Verification.Frontend.Intro;

/// <summary>The first, second and third of one verify intro cinematic artwork case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
internal readonly record struct VerifyIntroCinematicArtworkCase(string First, int Second, Func<IntroCinematicArtworkCatalog, ReadOnlyMemory<byte>> Third);
