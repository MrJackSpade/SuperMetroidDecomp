using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The operands and cue of one head cue op.</summary>
/// <param name="Operands">The operands.</param>
/// <param name="Cue">The cue.</param>
internal readonly record struct HeadCueOp(int Operands, Func<ushort[], MotherBrainHeadCue> Cue);
