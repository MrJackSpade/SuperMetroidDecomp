namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's projectile animations, one per projectile kind.</summary>
public enum CrocomireProjectileSegment
{
    /// <summary>The mouth projectile in flight.</summary>
    Mouth,
    /// <summary>A crumbling bridge fragment.</summary>
    BridgeFragment,
    /// <summary>A piece of the broken spike wall, one still picture as it falls.</summary>
    SpikeWallPiece,
    /// <summary>The mouth projectile's explosion after a Samus shot.</summary>
    Shot,
}
