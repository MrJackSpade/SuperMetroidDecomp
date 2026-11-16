using SuperMetroid.Core.Enemies.FuneNamihe;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>The cue and operands of one fune namihe native list addresses instruction.</summary>
/// <param name="Cue">The cue it becomes; a fire routine is the species' own natively, which the port takes from the spitter.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct FuneNamiheNativeListAddressesInstruction(FuneNamiheCue Cue, string Operands);
