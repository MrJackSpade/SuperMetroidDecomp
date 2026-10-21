using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Verification.Frontend.Pause;

/// <summary>The game and runtime of one unpause reserve blackout create result.</summary>
/// <param name="Game">The game.</param>
/// <param name="Runtime">The runtime.</param>
internal readonly record struct UnpauseReserveBlackoutCreateResult(SuperMetroidGame Game, SuperMetroidRuntime Runtime);
