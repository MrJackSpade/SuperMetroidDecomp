namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The kinds of shot the later forms' shot reaction tells apart.</summary>
public enum MotherBrainProjectileType
{
    /// <summary>A beam shot, or a missile's explosion.</summary>
    Beam,
    /// <summary>A missile.</summary>
    Missile,
    /// <summary>A Super Missile.</summary>
    SuperMissile,
    /// <summary>A Power Bomb.</summary>
    PowerBomb,
    /// <summary>A bomb.</summary>
    Bomb,
    /// <summary>A beam's explosion.</summary>
    BeamExplosion,
}
