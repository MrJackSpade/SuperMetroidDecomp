namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The half of its throw a ninja pirate's thrown claw is in (#2512). Its values are arbitrary.</summary>
internal enum NinjaPirateClawFlight
{
    /// <summary>Flying out along its throw, slowing to rest.</summary>
    FlyingOut,

    /// <summary>Coming back the other way, speeding up, once its outward speed has run out.</summary>
    ComingBack,
}
