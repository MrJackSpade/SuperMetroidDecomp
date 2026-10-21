using SuperMetroid.Core.Enemies.FuneNamihe;

namespace SuperMetroid.Verification.Enemies.FuneNamihe;

/// <summary>The kind and operands of one fune namihe native list addresses instruction.</summary>
/// <param name="Kind">The kind.</param>
/// <param name="Operands">The operands.</param>
internal readonly record struct FuneNamiheNativeListAddressesInstruction(FuneNamiheCueKind Kind, string Operands);
