using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Harness;

/// <summary>Enemy-side effects a PLM fixture observes; an uninstalled effect fails at the dependency (#633).</summary>
internal sealed class FixturePlmEnemyEffects : IRoomPlmEnemyEffects
{
    /// <summary>Observes N00b tube projectile spawns.</summary>
    public Action<NoobTubeProjectileRequest>? NoobTubeProjectile { get; init; }

    /// <summary>Observes eye-door projectile spawns.</summary>
    public Action<EyeDoorProjectileRequest>? EyeDoorProjectile { get; init; }

    /// <summary>Observes the old Tourian escape-shaft wall explosion.</summary>
    public Action? OldTourianEscapeShaftWallExplosion { get; init; }

    /// <summary>Observes Draygon cannon disables.</summary>
    public Action<DraygonWallCannon>? DraygonCannonDisabled { get; init; }

    /// <summary>Invokes the installed N00b tube projectile observer or fails at the missing dependency.</summary>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        (NoobTubeProjectile ?? throw Missing("N00b tube projectile"))(request);

    /// <summary>Invokes the installed eye-door projectile observer or fails at the missing dependency.</summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        (EyeDoorProjectile ?? throw Missing("eye-door projectile"))(request);

    /// <summary>Invokes the installed escape-shaft wall explosion observer or fails at the missing dependency.</summary>
    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        (OldTourianEscapeShaftWallExplosion ?? throw Missing("old Tourian wall explosion"))();

    /// <summary>Invokes the installed Draygon cannon observer or fails at the missing dependency.</summary>
    public void DisableDraygonCannon(DraygonWallCannon cannon) =>
        (DraygonCannonDisabled ?? throw Missing("Draygon cannon disable"))(cannon);

    /// <summary>Identifies the enemy-side effect the PLM fixture failed to install.</summary>
    private static InvalidOperationException Missing(string what) =>
        new($"The PLM fixture did not install a {what} effect.");
}
