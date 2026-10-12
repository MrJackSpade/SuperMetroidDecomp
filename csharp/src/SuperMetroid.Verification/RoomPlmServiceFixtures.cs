using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>Samus as a PLM fixture supplies her (#633).</summary>
internal sealed class FixtureRoomSamus(Func<SamusState?> current) : IRoomSamusAccess
{
    /// <summary>Gets current.</summary>
    public SamusState? Current => current();
}

/// <summary>Tourian statue progress as a PLM fixture supplies it (#633).</summary>
internal sealed class FixtureTourianStatue(Func<bool> finished) : ITourianStatueProgress
{
    /// <summary>Gets is finished.</summary>
    public bool IsFinished => finished();
}

/// <summary>Room-shake writes a PLM fixture observes; an uninstalled write fails at the dependency (#633).</summary>
internal sealed class FixtureRoomEarthquake : IRoomEarthquakeWriter
{
    /// <summary>Observes timer writes.</summary>
    public Action<ushort>? WriteTimer { get; init; }

    /// <summary>Observes type writes.</summary>
    public Action<ushort>? WriteType { get; init; }

    /// <summary>set timer.</summary>
    public void SetTimer(ushort timer) => (WriteTimer ?? throw Missing("timer"))(timer);

    /// <summary>set type.</summary>
    public void SetType(ushort type) => (WriteType ?? throw Missing("type"))(type);

    /// <summary>missing.</summary>
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

    /// <summary>spawn noob tube projectile.</summary>
    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        (NoobTubeProjectile ?? throw Missing("N00b tube projectile"))(request);

    /// <summary>spawn eye door projectile.</summary>
    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        (EyeDoorProjectile ?? throw Missing("eye-door projectile"))(request);

    /// <summary>spawn old tourian escape shaft wall explosion.</summary>
    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        (OldTourianEscapeShaftWallExplosion ?? throw Missing("old Tourian wall explosion"))();

    /// <summary>disable draygon cannon.</summary>
    public void DisableDraygonCannon(ushort variablePointer) =>
        (DraygonCannonDisabled ?? throw Missing("Draygon cannon disable"))(variablePointer);

    /// <summary>missing.</summary>
    private static InvalidOperationException Missing(string what) =>
        new($"The PLM fixture did not install a {what} effect.");
}
