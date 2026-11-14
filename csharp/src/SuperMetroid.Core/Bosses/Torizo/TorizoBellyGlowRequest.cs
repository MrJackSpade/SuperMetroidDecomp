using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The belly glow a Torizo's fight start asks the room's colour effects for, and the Torizo it lights (#1700).</summary>
/// <param name="Glow">The Bomb or Golden Torizo's belly glow.</param>
/// <param name="Torizo">The Torizo whose belly the glow lights, which the glow watches for its defeat.</param>
public sealed record TorizoBellyGlowRequest(ColourEffectId Glow, ILitTorizo Torizo);
