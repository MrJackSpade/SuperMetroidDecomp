namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>What a cue of the Torizos' explosions does that only the explosion behaviour runs: return to where it burst, or turn to smoke.</summary>
internal enum TorizoExplosionCueKind
{
    /// <summary>Returns the projectile to where it burst or struck.</summary>
    ReturnToOrigin,
    /// <summary>A death explosion has shown itself (#2167): one time in four, by the room's random draw, it turns into the death smoke instead of a scattered blast.</summary>
    MayTurnToSmoke,
}
