using SuperMetroid.Core.Enemies.Skultera;

namespace SuperMetroid.Verification.Enemies.Skultera;

/// <summary>The starts left, swim, loop and turn of one verify skultera behavior case.</summary>
/// <param name="StartsLeft">The starts left.</param>
/// <param name="Swim">The swim.</param>
/// <param name="Loop">The loop.</param>
/// <param name="Turn">The turn.</param>
internal readonly record struct VerifySkulteraBehaviorCase(bool StartsLeft, SkulteraSegment Swim, SkulteraSegment Loop, SkulteraSegment Turn);
