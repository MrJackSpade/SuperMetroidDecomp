namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose frame hitboxes are the ones the game defines rather than read from installed frames.</summary>
internal interface IEnemyFrameHitboxes
{
    /// <summary>
    /// The reaction of the first of the occupant's hitboxes the probe overlaps: true with the reaction, false when none
    /// overlaps, or null when the species' geometry does not cover the current frame.
    /// </summary>
    /// <param name="probe">The colliding projectile's or Samus's edges, and whether the test selects the shot or the touch reaction.</param>
    /// <param name="reaction">The selected hitbox's touch or shot reaction.</param>
    bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction);

    /// <summary>
    /// The species' own touch reaction, as its hitboxes name it, that the occupant takes where its definition names no
    /// shared one: the reaction a touch selects when it meets the occupant by its radius. Null when it has none: the
    /// absence of a reaction of its own, which leaves the touch to the reactions its parts' definitions name (#2558).
    /// </summary>
    EnemyReaction? SpeciesTouch { get; }

    /// <summary>
    /// The species' own shot reaction, as its hitboxes name it, that the occupant takes where its definition names no
    /// shared one: the reaction a shot or bomb selects when it meets the occupant by its radius. Null when it has none: the
    /// absence of a reaction of its own, which leaves the shot to the reactions its parts' definitions name (#2558).
    /// </summary>
    EnemyReaction? SpeciesShot { get; }
}
