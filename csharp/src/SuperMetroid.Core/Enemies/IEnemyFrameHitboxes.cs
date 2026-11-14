namespace SuperMetroid.Core.Enemies;

/// <summary>An enemy whose frame-hitbox collision geometry is engine-owned rather than read from installed frames.</summary>
internal interface IEnemyFrameHitboxes : IEnemyEntity
{
    /// <summary>
    /// The reaction of the first of the occupant's hitboxes the probe overlaps: true with the reaction, false when none
    /// overlaps, or null when the species' geometry does not cover the current frame.
    /// </summary>
    /// <param name="probe">The colliding edges and walker.</param>
    /// <param name="reaction">The selected hitbox's touch or shot reaction.</param>
    bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction);

    /// <summary>
    /// The species' own touch reaction, as its hitboxes name it, that the occupant takes where its definition names no
    /// shared one: the reaction a touch selects when it meets the occupant by its radius. Null when it has none.
    /// </summary>
    EnemyReaction? SpeciesTouch { get; }

    /// <summary>
    /// The species' own shot reaction, as its hitboxes name it, that the occupant takes where its definition names no
    /// shared one: the reaction a shot or bomb selects when it meets the occupant by its radius. Null when it has none.
    /// </summary>
    EnemyReaction? SpeciesShot { get; }
}
