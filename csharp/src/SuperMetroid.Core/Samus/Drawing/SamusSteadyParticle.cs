namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>
/// A kind of Samus's particles that drifts by the same pixels on each update it shows, whichever way Samus faces (#2554):
/// splashes, bubbles and dust (<see cref="SamusParticleKindDefinitions"/>). Lava spray, which drifts to the side its spawn
/// gives, is not one; it starts through <see cref="SamusParticles.SpawnLavaSpray"/>.
/// </summary>
/// <param name="Kind">What the particle shows.</param>
/// <param name="Drift">The pixels the particle moves by on each update it shows.</param>
internal sealed record SamusSteadyParticle(SamusParticleKind Kind, Offset16<RoomPixels> Drift);
