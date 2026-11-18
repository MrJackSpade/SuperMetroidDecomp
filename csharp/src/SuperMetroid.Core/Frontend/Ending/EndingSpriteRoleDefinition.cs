using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What every ending actor of one role draws from, which sprite layer it shows on, and how it moves (#2489).</summary>
/// <param name="Pictures">The sprite family whose pictures a picture actor of the role draws.</param>
/// <param name="Layer">The sprite layer the role's actors show on (#1514).</param>
/// <param name="Motion">
/// Creates the motion an actor of the role starts with, which keeps that actor's own motion state, or null for a role whose
/// actors have no motion of their own.
/// </param>
internal sealed record EndingSpriteRoleDefinition(CinematicSpriteFamily Pictures, EndingSpriteLayer Layer, Func<EndingActorMotion>? Motion);
