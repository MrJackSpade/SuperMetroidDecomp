namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What every ending actor of one role draws from, which sprite layer it shows on, and how it moves (#2489).</summary>
/// <param name="Pictures">The sprite family whose pictures a picture actor of the role draws.</param>
/// <param name="Layer">The sprite layer the role's actors show on (#1514).</param>
/// <param name="Motion">
/// Creates the motion an actor of the role starts with, which keeps that actor's own motion state; a role whose actors have
/// no motion of their own creates a still one (<see cref="EndingStillMotion"/>, #2541).
/// </param>
internal sealed record EndingSpriteRoleDefinition(EndingSpriteFamily Pictures, EndingSpriteLayer Layer, Func<EndingActorMotion> Motion);
