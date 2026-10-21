using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Verification.Fx.PaletteFx;

/// <summary>The suit and operand of one palette fx instruction codes source.</summary>
/// <param name="Suit">The suit.</param>
/// <param name="Operand">The operand.</param>
internal readonly record struct PaletteFxInstructionCodesSource(PaletteFxHeatSuit Suit, int Operand);
