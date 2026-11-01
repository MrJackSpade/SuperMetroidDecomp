using SuperMetroid.Core.Bosses.Ridley;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>The cue and operands of one body instruction.</summary>
/// <param name="Cue">The cue.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct BodyInstruction(NativeCueShape<RidleyBodyCue> Cue, int Operands);
