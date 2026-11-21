namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Where the bomb jump's hold on Samus's movement stands (<see cref="SamusMovementOwner.BombJump"/>, #2560).</summary>
public enum SamusBombJumpPhase
{
    /// <summary>The bomb jump does not move Samus.</summary>
    None,

    /// <summary>The bomb jump's one-update start, which sets her velocity.</summary>
    Starting,

    /// <summary>The bomb jump moves Samus along its rising arc.</summary>
    Rising,
}
