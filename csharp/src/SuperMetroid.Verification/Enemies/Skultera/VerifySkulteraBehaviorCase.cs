using SuperMetroid.Core.Enemies.Skultera;

namespace SuperMetroid.Verification.Enemies.Skultera;

/// <summary>The starts left, swim, loop, layer and turn of one verify skultera behavior case.</summary>
/// <param name="StartsLeft">The starts left.</param>
/// <param name="Swim">The swim.</param>
/// <param name="Loop">The loop.</param>
/// <param name="Layer">The layer.</param>
/// <param name="Turn">The turn.</param>
internal readonly record struct VerifySkulteraBehaviorCase(bool StartsLeft, SkulteraProgram Swim, SkulteraProgram Loop, ushort Layer, SkulteraProgram Turn);
