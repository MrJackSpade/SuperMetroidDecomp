using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>The health, samus and row of one ridley attack choices case.</summary>
/// <param name="Health">The health.</param>
/// <param name="Samus">The samus.</param>
/// <param name="Row">The row.</param>
internal readonly record struct RidleyAttackChoicesCase(ushort Health, SamusState? Samus, int Row);
