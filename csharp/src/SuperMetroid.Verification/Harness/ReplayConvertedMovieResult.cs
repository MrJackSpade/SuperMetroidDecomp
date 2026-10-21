using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Verification.Harness;

/// <summary>The game, memory and uncheckpointed updates of one replay converted movie result.</summary>
/// <param name="Game">The game.</param>
/// <param name="Memory">The memory.</param>
/// <param name="UncheckpointedUpdates">The uncheckpointed updates.</param>
internal readonly record struct ReplayConvertedMovieResult(SuperMetroidGame Game, NativeCheckpointMemory Memory, int UncheckpointedUpdates);
