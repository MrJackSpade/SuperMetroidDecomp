namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Samus's shot direction, running state, pose Y offset and facing as the charge flare reads them (#633).</summary>
internal readonly record struct ChargeFlarePose
{
    /// <summary>
    /// The flare's placement selection (<see cref="Artwork.ChargeFlarePlacements.Selection"/>): one of the ten aims'
    /// muzzles, 0 through 9, or a turning placement after them.
    /// </summary>
    public required int Direction { get; init; }

    /// <summary>Running.</summary>
    public required bool Running { get; init; }

    /// <summary>
    /// Pose graphics Y offset in pixels, 0 through 255: the pose's offset taken as an unsigned byte, so a negative
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
