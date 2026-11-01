using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Bosses.SporeSpawn;

namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>The cue and operands of one spore spawn native list addresses body instruction.</summary>
/// <param name="Cue">The cue.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct SporeSpawnNativeListAddressesBodyInstruction(NativeCueShape<SporeSpawnCue> Cue, int Operands);
