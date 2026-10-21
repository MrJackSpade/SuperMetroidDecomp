using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>The first and second of one configuration.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct Configuration(RoomFxType First, LayerBlendingConfiguration Second);
