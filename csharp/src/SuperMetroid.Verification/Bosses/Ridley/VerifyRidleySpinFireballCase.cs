using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The first, second, third and fourth of one verify ridley spin fireball case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
internal readonly record struct VerifyRidleySpinFireballCase(int First, bool Second, RidleyFacing Third, bool Fourth);
