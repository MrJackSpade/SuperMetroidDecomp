namespace SuperMetroid.Core.Game;

/// <summary>The three bank-$86 enemy-projectile definitions spawned by eye-door PLMs.</summary>
public enum EyeDoorProjectileDefinition : ushort
{
    /// <summary><c>$86:B743</c>, native <c>EnemyProjectile_EyeDoorProjectile</c>: the eye-door attack actor, whose initial animation later installs aimed movement and whose shot/impact lists terminate it.</summary>
    Projectile = 0xb743,
    /// <summary><c>$86:B751</c>, native <c>EnemyProjectile_EyeDoorSweat</c>: the falling sweat actor spawned by the door PLM with side-selected launch velocity and a floor-impact animation.</summary>
    Sweat = 0xb751,
    /// <summary><c>$86:E517</c>, native <c>EnemyProjectile_MiscDustPLM</c>: the shared PLM dust/explosion definition used for eye-door smoke, with parameter-selected artwork and RNG-offset placement around the spawning block.</summary>
    Smoke = 0xe517,
}
