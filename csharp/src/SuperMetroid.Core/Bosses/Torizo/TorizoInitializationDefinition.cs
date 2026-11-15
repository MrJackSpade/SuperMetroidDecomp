namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo's initial placement, animation, the processing it adds and collision radii, and the animations its walking
/// motion turns with (#1931).
/// </summary>
/// <param name="Position">Where the Torizo starts.</param>
/// <param name="Animation">The animation it starts with.</param>
/// <param name="Processing">The processing it adds to its population's.</param>
/// <param name="Radius">Its collision radii.</param>
/// <param name="TurningLeft">The animation its walking motion turns left with once its turn timer runs out.</param>
/// <param name="TurningRight">The animation its walking motion turns right with once its turn timer runs out.</param>
internal readonly record struct TorizoInitializationDefinition(
    Position16<RoomPixels> Position, TorizoAnimation Animation, SuperMetroid.Core.Enemies.EnemyProcessing Processing, Extent16<RoomPixels> Radius,
    TorizoAnimation TurningLeft, TorizoAnimation TurningRight);
