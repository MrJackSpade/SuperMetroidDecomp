namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Samus's shot direction, running state, pose Y offset and facing as the charge flare reads them (#633).</summary>
internal readonly record struct ChargeFlarePose
{
    /// <summary>Direction.</summary>
    public required byte Direction { get; init; }

    /// <summary>Running.</summary>
    public required bool Running { get; init; }

    /// <summary>Pose y offset.</summary>
    public required byte PoseYOffset { get; init; }

    /// <summary>Facing left.</summary>
    public required bool FacingLeft { get; init; }
}
