using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The game, runtime and samus of one create with xray selected result.</summary>
/// <param name="Game">The game.</param>
/// <param name="Runtime">The runtime.</param>
/// <param name="Samus">The samus.</param>
internal readonly record struct CreateWithXraySelectedResult(SuperMetroidGame Game, SuperMetroidRuntime Runtime, SamusState Samus);
