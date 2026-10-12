using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>Samus as a PLM fixture supplies her (#633).</summary>
internal sealed class FixtureRoomSamus(Func<SamusState?> current) : IRoomSamusAccess
{
    /// <summary>Reads the fixture's current Samus owner on demand.</summary>
    public SamusState? Current => current();
}

/// <summary>Tourian statue progress as a PLM fixture supplies it (#633).</summary>
internal sealed class FixtureTourianStatue(Func<bool> finished) : ITourianStatueProgress
{
    /// <summary>Reads fixture-owned Tourian statue completion on demand.</summary>
    public bool IsFinished => finished();
}

/// <summary>Room-shake writes a PLM fixture observes; an uninstalled write fails at the dependency (#633).</summary>
internal sealed class FixtureRoomEarthquake : IRoomEarthquakeWriter
{
    /// <summary>Observes timer writes.</summary>
    public Action<ushort>? WriteTimer { get; init; }

    /// <summary>Observes type writes.</summary>
    public Action<ushort>? WriteType { get; init; }

    /// <summary>Invokes the installed timer observer or fails at the missing fixture dependency.</summary>
    public void SetTimer(ushort timer) => (WriteTimer ?? throw Missing("timer"))(timer);

    /// <summary>Invokes the installed quake-type observer or fails at the missing fixture dependency.</summary>
    public void SetType(ushort type) => (WriteType ?? throw Missing("type"))(type);

    /// <summary>Identifies the earthquake writer the PLM fixture failed to install.</summary>
    private static InvalidOperationException Missing(string what) =>
        new($"The PLM fixture did not install an earthquake {what} writer.");
}

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
    public Action<ushort>? DraygonCannonDisabled { get; init; }

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
    public void DisableDraygonCannon(ushort variablePointer) =>
        (DraygonCannonDisabled ?? throw Missing("Draygon cannon disable"))(variablePointer);

    /// <summary>Identifies the enemy-side effect the PLM fixture failed to install.</summary>
    private static InvalidOperationException Missing(string what) =>
        new($"The PLM fixture did not install a {what} effect.");
}
