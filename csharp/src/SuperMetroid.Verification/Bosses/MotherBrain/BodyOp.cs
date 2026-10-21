using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The operands and cue of one body op.</summary>
/// <param name="Operands">The operands.</param>
/// <param name="Cue">The cue.</param>
internal readonly record struct BodyOp(int Operands, Func<ushort[], MotherBrainBodyCue> Cue);
