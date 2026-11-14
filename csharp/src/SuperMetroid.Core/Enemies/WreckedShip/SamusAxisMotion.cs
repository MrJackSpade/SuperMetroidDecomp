namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>
/// How Samus moved along one axis since the Wrecked Ship ghost's last tracking update (#1817): toward smaller
/// coordinates (left or up), not at all, or toward larger ones (right or down). Its values are arbitrary.
/// </summary>
public enum SamusAxisMotion
{
    /// <summary>She moved left, or up.</summary>
    TowardSmaller,
    /// <summary>She held still on the axis.</summary>
    Still,
    /// <summary>She moved right, or down.</summary>
    TowardLarger,
}
