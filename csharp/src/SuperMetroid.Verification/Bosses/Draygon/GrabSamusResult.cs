using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>The runtime, samus and draygon of one grab samus result.</summary>
/// <param name="Runtime">The runtime.</param>
/// <param name="Samus">The samus.</param>
/// <param name="Draygon">The draygon.</param>
internal readonly record struct GrabSamusResult(SuperMetroidRuntime Runtime, SamusState Samus, DraygonEnemyState Draygon);
