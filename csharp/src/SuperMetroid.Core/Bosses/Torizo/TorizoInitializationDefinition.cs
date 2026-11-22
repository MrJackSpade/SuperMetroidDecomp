namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo's initial placement, animation, the processing it adds and collision radii (#1931).
/// </summary>
/// <param name="Position">Where the Torizo starts.</param>
/// <param name="Animation">The animation it starts with.</param>
/// <param name="InitialFrame">
/// The frame it shows from its spawn until that animation's first step (#2240), a frame without hitboxes: Bomb Torizo's
/// dormant statue, and for Golden Torizo one it is never drawn in, since its dormant animation's first step replaces it
/// before any draw.
/// </param>
/// <param name="Processing">The processing it adds to its population's.</param>
/// <param name="Radius">Its collision radii.</param>
internal readonly partial record struct TorizoInitializationDefinition(
    Position16<RoomPixels> Position, TorizoAnimation Animation, SuperMetroid.Core.Enemies.Artwork.EnemyFrameId InitialFrame, SuperMetroid.Core.Enemies.EnemyProcessing Processing, Extent16<RoomPixels> Radius);
