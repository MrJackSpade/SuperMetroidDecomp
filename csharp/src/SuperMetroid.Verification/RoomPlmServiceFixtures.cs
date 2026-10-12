using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

/// <summary>Samus as a PLM fixture supplies her (#633).</summary>
internal sealed class FixtureRoomSamus(Func<SamusState?> current) : IRoomSamusAccess
{
    public SamusState? Current => current();
}

/// <summary>Tourian statue progress as a PLM fixture supplies it (#633).</summary>
internal sealed class FixtureTourianStatue(Func<bool> finished) : ITourianStatueProgress
{
    public bool IsFinished => finished();
}

/// <summary>Room-shake writes a PLM fixture observes; an uninstalled write fails at the dependency (#633).</summary>
internal sealed class FixtureRoomEarthquake : IRoomEarthquakeWriter
{
    /// <summary>Observes timer writes.</summary>
    public Action<ushort>? WriteTimer { get; init; }

    /// <summary>Observes type writes.</summary>
    public Action<ushort>? WriteType { get; init; }

    public void SetTimer(ushort timer) => (WriteTimer ?? throw Missing("timer"))(timer);

    public void SetType(ushort type) => (WriteType ?? throw Missing("type"))(type);

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

    public void SpawnNoobTubeProjectile(NoobTubeProjectileRequest request) =>
        (NoobTubeProjectile ?? throw Missing("N00b tube projectile"))(request);

    public void SpawnEyeDoorProjectile(EyeDoorProjectileRequest request) =>
        (EyeDoorProjectile ?? throw Missing("eye-door projectile"))(request);

    public void SpawnOldTourianEscapeShaftWallExplosion() =>
        (OldTourianEscapeShaftWallExplosion ?? throw Missing("old Tourian wall explosion"))();

    public void DisableDraygonCannon(ushort variablePointer) =>
        (DraygonCannonDisabled ?? throw Missing("Draygon cannon disable"))(variablePointer);

    private static InvalidOperationException Missing(string what) =>
        new($"The PLM fixture did not install a {what} effect.");
}
