namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// The enemy family a run of the extended frames document belongs to (#1623): an extended frame's display binding selects
/// a frame of its own family, and the family decides how the import reads the frame's native header.
/// </summary>
internal enum EnemyExtendedFrameFamily
{
    /// <summary>The walking Space Pirates.</summary>
    WalkingPirate,
    /// <summary>The wall-climbing Space Pirates.</summary>
    WallPirate,
    /// <summary>The Ninja Space Pirates.</summary>
    NinjaPirate,
    /// <summary>Ridley's body.</summary>
    RidleyBody,
    /// <summary>Draygon, its sprite poses and its background-layer poses.</summary>
    Draygon,
    /// <summary>Spore Spawn.</summary>
    SporeSpawn,
    /// <summary>Ceres Station's steam.</summary>
    CeresSteam,
    /// <summary>Maridia's large snail.</summary>
    Oum,
    /// <summary>Crocomire's tongue and claws.</summary>
    CrocomireTongue,
    /// <summary>Crocomire's fighting body.</summary>
    CrocomireBody,
    /// <summary>Bomb Torizo's dormant statue.</summary>
    BombTorizo,
    /// <summary>Golden Torizo's poses.</summary>
    GoldenTorizo,
    /// <summary>Kraid's arm.</summary>
    KraidArm,
    /// <summary>The Torizos' backward jumps.</summary>
    TorizoJumpBack,
    /// <summary>The Torizos' fall facing left.</summary>
    TorizoFallingLeft,
    /// <summary>The Torizos' further combat poses.</summary>
    TorizoCombat,
    /// <summary>Mother Brain's body.</summary>
    MotherBrainBody,
    /// <summary>Phantoon's background-layer poses.</summary>
    Phantoon,
    /// <summary>Crocomire's skeleton.</summary>
    CrocomireSkeleton,
    /// <summary>Kraid's feet.</summary>
    KraidFoot,
}
