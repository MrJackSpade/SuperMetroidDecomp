namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>
/// The Zeb, Zebbo, Gamet and Geega Pipe Bugs' rising, shooting, flying and arcing loops as typed programs (#641), one per bank-$B3 list a spawn, branch or resumed sleep enters, in authored order.
/// Generated from the verified native lists; see <see cref="PipeBugAnimations"/>.
/// </summary>
internal enum PipeBugProgram
{
    /// <summary><c>InstList_Zeb_FacingLeft_Rising</c> at $B3:87AB.</summary>
    ZebRisingLeft,
    /// <summary><c>InstList_Zeb_FacingLeft_Shooting</c> at $B3:87CF.</summary>
    ZebShootingLeft,
    /// <summary><c>InstList_Zeb_FacingRight_Rising</c> at $B3:87EB.</summary>
    ZebRisingRight,
    /// <summary><c>InstList_Zeb_FacingRight_Shooting</c> at $B3:880F.</summary>
    ZebShootingRight,
    /// <summary><c>InstList_Zebbo_FacingLeft_Rising</c> at $B3:8A1D.</summary>
    ZebboRisingLeft,
    /// <summary><c>InstList_Zebbo_FacingLeft_Shooting</c> at $B3:8A31.</summary>
    ZebboShootingLeft,
    /// <summary><c>InstList_Zebbo_FacingRight_Rising</c> at $B3:8A45.</summary>
    ZebboRisingRight,
    /// <summary><c>InstList_Zebbo_FacingRight_Shooting</c> at $B3:8A59.</summary>
    ZebboShootingRight,
    /// <summary>The native list at $B3:8AE1.</summary>
    GametRisingLeft,
    /// <summary>The native list at $B3:8B05.</summary>
    GametFlyingLeft,
    /// <summary>The native list at $B3:8B21.</summary>
    GametRisingRight,
    /// <summary>The native list at $B3:8B45.</summary>
    GametFlyingRight,
    /// <summary>The native list at $B3:8EFC.</summary>
    GeegaFlyingLeft,
    /// <summary>The native list at $B3:8F10.</summary>
    GeegaArcingLeft,
    /// <summary>The native list at $B3:8F24.</summary>
    GeegaFlyingRight,
    /// <summary>The native list at $B3:8F38.</summary>
    GeegaArcingRight,
}
