namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>
/// One of the straight legs an Sbug's placement gives it (#2450): its spawn heading forward, or an eighth of a turn either
/// side of it for a zigzag, with the displacement it moves each update and the facing it shows.
/// </summary>
/// <param name="XVelocity">The horizontal displacement in pixels per AI update.</param>
/// <param name="YVelocity">The vertical displacement in pixels per AI update.</param>
/// <param name="Facing">The facing whose loop the leg shows.</param>
internal readonly partial record struct SbugLeg(SignedSixteenSixteen XVelocity, SignedSixteenSixteen YVelocity, CompassDirection Facing);
