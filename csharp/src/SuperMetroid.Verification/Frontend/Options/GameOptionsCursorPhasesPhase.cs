using SuperMetroid.Core.Frontend.Options;

namespace SuperMetroid.Verification.Frontend.Options;

/// <summary>The phase and native of one game options cursor phases phase.</summary>
/// <param name="Phase">The phase.</param>
/// <param name="Native">The native.</param>
internal readonly record struct GameOptionsCursorPhasesPhase(GameOptionsPhase Phase, int Native);
