using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Options;

/// <summary>The route and after press of one verify game options start game case.</summary>
/// <param name="Route">The route.</param>
/// <param name="AfterPress">The after press.</param>
internal readonly record struct VerifyGameOptionsStartGameCase(SaveLoadingGameState Route, GameOptionsPhase AfterPress);
