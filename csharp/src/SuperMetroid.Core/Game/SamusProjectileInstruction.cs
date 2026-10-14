namespace SuperMetroid.Core.Game;

/// <summary>The two bank-$93 projectile instruction routines referenced by retail lists.</summary>
public enum SamusProjectileInstruction : ushort
{
    /// <summary><c>$93:822F</c>, delete the current projectile slot.</summary>
    Delete = 0x822f,
    /// <summary><c>$93:8239</c>, replace the instruction-list cursor.</summary>
    GoTo = 0x8239,
}
