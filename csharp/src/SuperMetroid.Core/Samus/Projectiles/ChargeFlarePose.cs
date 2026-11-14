using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Samus's flare aim, running state, pose Y offset and facing as the charge flare reads them (#633).</summary>
internal readonly record struct ChargeFlarePose
{
    /// <summary>Where the pose places the flare: at the muzzle of one of the ten aims, or at a turning placement.</summary>
    public required SamusFlareAim Aim { get; init; }

    /// <summary>Running.</summary>
    public required bool Running { get; init; }

    /// <summary>
    /// The pose's signed graphics Y offset in pixels, by which the flare is raised as Samus's body is: a negative offset
    /// lowers it. The game read the offset as a byte, raising the flare of a pose with a negative offset by 256 less its
    /// magnitude; that presentation quirk is deleted (#1795).
    /// </summary>
    public required int PoseYOffset { get; init; }

    /// <summary>Facing left.</summary>
    public required bool FacingLeft { get; init; }
}
