using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The first and second of one verify ridley fireball damage case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyRidleyFireballDamageCase(AreaId First, ushort Second);
