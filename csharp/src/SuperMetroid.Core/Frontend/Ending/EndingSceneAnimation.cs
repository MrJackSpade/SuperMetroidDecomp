namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The animations of the ending scenes' picture actors (#2197): the escape's clouds, the exploding planet, the clear time's
/// digits, the animals' escape pod and the reward Samus at rest.
/// </summary>
internal enum EndingSceneAnimation
{
    /// <summary>The pattern of the upper clouds the gunship escapes between.</summary>
    TopCloudPattern,
    /// <summary>The edge of the upper clouds the gunship escapes between.</summary>
    TopCloudEdge,
    /// <summary>The edge of the lower clouds the gunship escapes between.</summary>
    BottomCloudEdge,
    /// <summary>The pattern of the lower clouds the gunship escapes between.</summary>
    BottomCloudPattern,
    /// <summary>A cloud passing on the right of the escaping gunship.</summary>
    RightSideCloud,
    /// <summary>A cloud passing on the left of the escaping gunship.</summary>
    LeftSideCloud,
    /// <summary>
    /// The exploding planet: its rumble, played over, then its breaking apart; as a segment, the breaking apart, which
    /// fades the explosion's colours in, cracks the planet open and spawns its silhouette.
    /// </summary>
    ExplodingPlanet,
    /// <summary>
    /// A segment of the exploding planet (#2358): the planet rumbling once, from its deep shadow to its brightest light, before
    /// it breaks apart.
    /// </summary>
    PlanetRumbling,
    /// <summary>The explosion's purple glow.</summary>
    ExplosionGlow,
    /// <summary>The explosion's stars.</summary>
    ExplosionStars,
    /// <summary>
    /// The planet's lava, which shows after a delay and then glows on; as a segment, its glow, dark then bright, played over
    /// and over.
    /// </summary>
    ExplosionLava,
    /// <summary>A segment of the planet's lava (#2358): the delay before it shows, while the planet still covers it.</summary>
    ExplosionLavaHidden,
    /// <summary>The explosion silhouette, which starts the explosion.</summary>
    ExplosionSilhouette,
    /// <summary>The right stars, which start the finale, end the explosion and wait for the flyaway.</summary>
    ExplosionRightStars,
    /// <summary>The left stars.</summary>
    ExplosionLeftStars,
    /// <summary>The planet's afterglow.</summary>
    ExplosionAfterglow,
    /// <summary>The clear time digit 0.</summary>
    ClearTimeDigit0,
    /// <summary>The clear time digit 1.</summary>
    ClearTimeDigit1,
    /// <summary>The clear time digit 2.</summary>
    ClearTimeDigit2,
    /// <summary>The clear time digit 3.</summary>
    ClearTimeDigit3,
    /// <summary>The clear time digit 4.</summary>
    ClearTimeDigit4,
    /// <summary>The clear time digit 5.</summary>
    ClearTimeDigit5,
    /// <summary>The clear time digit 6.</summary>
    ClearTimeDigit6,
    /// <summary>The clear time digit 7.</summary>
    ClearTimeDigit7,
    /// <summary>The clear time digit 8.</summary>
    ClearTimeDigit8,
    /// <summary>The clear time digit 9.</summary>
    ClearTimeDigit9,
    /// <summary>The clear time's colon.</summary>
    ClearTimeColon,
    /// <summary>The animals escaping Zebes.</summary>
    AnimalEscape,
    /// <summary>The suitless Samus at rest.</summary>
    SuitlessIdle,
    /// <summary>The suitless Samus's legs at rest.</summary>
    SuitlessLegsIdle,
    /// <summary>The suited Samus at rest.</summary>
    SuitedIdle,
    /// <summary>The helmet at rest.</summary>
    HelmetIdle,
    /// <summary>The helmetless head at rest.</summary>
    HeadIdle,
}
