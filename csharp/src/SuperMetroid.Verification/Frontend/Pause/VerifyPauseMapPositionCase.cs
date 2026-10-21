using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The first, second, third, fourth and fifth of one verify pause map position case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
/// <param name="Fifth">The fifth.</param>
internal readonly record struct VerifyPauseMapPositionCase(AreaId First, int Second, int Third, int Fourth, int Fifth);
