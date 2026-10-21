using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>The first, second, third and fourth of one verify ending mode7 role selection case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
internal readonly record struct VerifyEndingMode7RoleSelectionCase(EndingMode7SceneId First, int Second, int Third, string Fourth);
