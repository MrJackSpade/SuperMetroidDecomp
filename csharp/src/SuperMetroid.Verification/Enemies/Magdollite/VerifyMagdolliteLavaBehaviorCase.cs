using SuperMetroid.Core.Enemies.Magdollite;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>The direction and program of one verify magdollite lava behavior case.</summary>
/// <param name="Direction">The direction.</param>
/// <param name="Program">The program.</param>
internal readonly record struct VerifyMagdolliteLavaBehaviorCase(ushort Direction, MagdolliteLavaProgram Program);
