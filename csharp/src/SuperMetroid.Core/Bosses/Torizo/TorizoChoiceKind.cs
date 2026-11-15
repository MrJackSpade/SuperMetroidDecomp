namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Which decision a <see cref="TorizoCue.Choice"/> cue is (#2028): a choice between two body animations.</summary>
internal enum TorizoChoiceKind
{
    /// <summary>A landing walks on faceless when Bomb Torizo's face is blown up, in Golden Torizo's own walk for Golden Torizo, and runs on otherwise.</summary>
    LandFacelessOrGolden,
    /// <summary>Bomb Torizo chooses between its two attacks.</summary>
    BombAttack,
    /// <summary>Golden Torizo chooses between its two attacks.</summary>
    GoldenAttack,
}
