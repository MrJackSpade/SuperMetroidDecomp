namespace SuperMetroid.Core.Enemies;

/// <summary>The explosion a killed enemy leaves, before it becomes a drop.</summary>
public enum EnemyDeathExplosion
{
    /// <summary>The small explosion.</summary>
    Small,

    /// <summary>The burst of an enemy Samus kills by contact.</summary>
    SamusContact,

    /// <summary>The normal explosion.</summary>
    Normal,

    /// <summary>Mini-Kraid's explosion.</summary>
    MiniKraid,

    /// <summary>The big explosion.</summary>
    Big,
}
