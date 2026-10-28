using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>The definition and first of one verify treadmill artwork sources case.</summary>
/// <param name="Definition">The definition.</param>
/// <param name="First">The first.</param>
internal readonly record struct VerifyTreadmillArtworkSourcesCase(WreckedShipTreadmillLoop Definition, int First);
