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
    /// Pose graphics Y offset in pixels, 0 through 255: the pose's offset taken modulo 256, so a negative
    /// pose offset raises the flare by 256 minus its magnitude, as the game's unsigned subtraction does.
    /// </summary>
    public required int PoseYOffset { get; init; }

    /// <summary>Facing left.</summary>
    public required bool FacingLeft { get; init; }

    /// <summary>The rises a flare's pose offset holds, in pixels: a negative offset continues from the top of them.</summary>
    private const int PoseRises = 256;

    /// <summary>
    /// The rise the flare takes from a pose's signed graphics Y offset, 0 through 255: the offset within the flare's
    /// 256 rises, so a negative offset raises the flare by 256 minus its magnitude.
    /// </summary>
    /// <param name="graphicsYOffset">The pose's signed graphics Y offset.</param>
    public static int RiseOf(int graphicsYOffset) => (graphicsYOffset % PoseRises + PoseRises) % PoseRises;
}
