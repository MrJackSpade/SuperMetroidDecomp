namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Samus's shot direction, running state, pose Y offset and facing as the charge flare reads them (#633).</summary>
internal readonly record struct ChargeFlarePose
{
    /// <summary>
    /// Shot-direction table index, 0 through 15. The draw path already rejects larger values, and the flare
    /// placement table is indexed by it directly, so indices beyond the ten projectile directions select the
    /// table's adjacent entries exactly as the cartridge does.
    /// </summary>
    public required int Direction { get; init; }

    /// <summary>Running.</summary>
    public required bool Running { get; init; }

    /// <summary>
    /// Pose graphics Y offset in pixels, 0 through 255: the pose's offset taken as an unsigned byte, so a negative
    /// pose offset raises the flare by 256 minus its magnitude, as the cartridge's unsigned subtraction does.
    /// </summary>
    public required int PoseYOffset { get; init; }

    /// <summary>Facing left.</summary>
    public required bool FacingLeft { get; init; }
}
