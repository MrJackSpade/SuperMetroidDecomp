using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The colours an auxiliary family's derived frames are calculated from when they are not the progression's own stock
/// paint: the endpoints every frame interpolates between, the sidehopper's separate corpse frame, and the face block's
/// active-glow accent.
/// </summary>
/// <param name="First">Initial per-color interpolation endpoints.</param>
/// <param name="Last">Final per-color interpolation endpoints.</param>
/// <param name="Corpse">The sidehopper's terminal corpse frame, or null for the other families.</param>
/// <param name="GlowAccentStart">The face-block accent colour the active glow starts from; black for other families.</param>
internal sealed record EnemyAuxiliaryColourEndpoints(RgbColor[] First, RgbColor[] Last, RgbColor[]? Corpse, RgbColor GlowAccentStart);
