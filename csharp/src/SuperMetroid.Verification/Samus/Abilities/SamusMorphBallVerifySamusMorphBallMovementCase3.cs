using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The samus x, expected direction and expected word of one samus morph ball verify samus morph ball movement case3.</summary>
/// <param name="SamusX">The samus x.</param>
/// <param name="ExpectedDirection">The expected direction.</param>
/// <param name="ExpectedWord">The expected word.</param>
internal readonly record struct SamusMorphBallVerifySamusMorphBallMovementCase3(ushort SamusX, SamusBombJumpDirection ExpectedDirection, byte ExpectedWord);
