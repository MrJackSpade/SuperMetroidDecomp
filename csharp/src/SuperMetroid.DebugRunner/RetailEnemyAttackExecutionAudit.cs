using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Executes enemy-produced bank-$86 projectiles in every named retail room state, then
/// drives every naturally observed damaging definition through the shared Samus-contact
/// dispatcher. This complements the ordinary touch and Samus-weapon audits: an enemy is
/// not attack-complete merely because its body can move and receive a shot.
/// </summary>
internal static partial class RetailEnemyExecutionAudit
{
    private const int EnemyAttackFramesPerPlacement = 512;

    /// <summary>
    /// Discovers attacks from cartridge-authored enemy AI rather than manufacturing a list
    /// of projectile definitions. Five screen-relative Samus positions expose the ordinary
    /// left/right/above/below selectors, and every placement starts with a fresh population
    /// so an earlier probe cannot consume a boss phase or permanently occupy the shared pool.
    /// </summary>
    public static int RunEnemyAttackCombat(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        RetailRoomState[] states = LoadNamedRetailStates();

        var observations = new Dictionary<RoomEnemyProjectileKind, ProjectileObservation>();
        var contactTestedKinds = new HashSet<RoomEnemyProjectileKind>();
        var failures = new List<EnemyAttackFailure>();
        long executedFrames = 0;
        int executedScenarios = 0;

        foreach (RetailRoomState state in states)
        {
            try
            {
                CartridgeRoomHeader defaultRoom = CartridgeRoomHeader.Load(bus, state.RoomPointer);
                CartridgeRoomState exactState = CartridgeRoomState.Load(bus, state.StatePointer);
                CartridgeRoomHeader room = defaultRoom with { State = exactState };
                if (ReadPopulationDefinitions(bus, exactState.EnemyPopulationPointer).Length == 0)
                    continue;

                CartridgeRoomAssets assets = CartridgeRoomAssets.Load(bus, room);
                LoadedRetailState viewSource = LoadState(bus, room, assets);
                (ushort X, ushort Y)[] views = viewSource.Enemies.Slots
                    .Where(slot => slot.EnemyDefinitionPointer is not 0 and not 0xffff)
                    .Select(slot => CameraFor(room, slot))
                    .Distinct()
                    .ToArray();

                foreach ((ushort cameraX, ushort cameraY) in views)
                {
                    foreach (SamusPlacement placement in DirectionalSamusPlacements)
                    {
                        LoadedRetailState loaded = LoadState(bus, room, assets);
                        loaded.Samus.XPosition = unchecked((ushort)(cameraX + placement.X));
                        loaded.Samus.YPosition = unchecked((ushort)(cameraY + placement.Y));

                        for (int frame = 0; frame < EnemyAttackFramesPerPlacement; frame++)
                        {
                            try
                            {
                                bool stopScenarioAfterContactProbe = false;
                                // Discovery must not accidentally remove Samus or steer an enemy
                                // through host-only death behavior. A nonzero invincibility timer
                                // disables only the later common projectile-contact pass; producer
                                // AI, projectile pre-instructions, instruction lists, animation,
                                // room collision, and pool allocation still execute normally.
                                loaded.Samus.InvincibilityTimer = ushort.MaxValue;
                                loaded.Enemies.StepFrame(
                                    cameraX,
                                    cameraY,
                                    timeIsFrozen: false,
                                    loaded.Samus,
                                    level: assets.LevelData,
                                    samusProjectiles: loaded.SamusProjectiles,
                                    nmiFrameCounter8: unchecked((byte)frame),
                                    mode7Transform: loaded.Mode7Transform,
                                    sharedProjectiles: loaded.SharedProjectiles);

                                ObserveLiveProjectiles(
                                    loaded.Enemies,
                                    observations,
                                    state,
                                    cameraX,
                                    cameraY,
                                    placement.Name,
                                    frame);

                                // Probe at most one previously unverified kind in this frame. The
                                // actor itself is real and fully initialized by its producer. We
                                // freeze only its next pre-instruction and instruction-list tick,
                                // put Samus on its authored position, and temporarily mask sibling
                                // projectile collision so the common dispatcher's result is exact.
                                RoomEnemyProjectileSlot? contactTarget = loaded.Enemies.EnemyProjectiles
                                    .Where(projectile => projectile.IsActive && projectile.CanDamageSamus)
                                    .FirstOrDefault(projectile => !contactTestedKinds.Contains(projectile.Kind));
                                if (contactTarget is not null)
                                {
                                    RoomEnemyProjectileKind contactKind = contactTarget.Kind;
                                    VerifyNaturalProjectileContact(
                                        bus,
                                        loaded,
                                        assets.LevelData,
                                        contactTarget,
                                        cameraX,
                                        cameraY,
                                        unchecked((byte)frame));
                                    // Nonpersistent contact correctly clears Kind to None, so
                                    // retain the naturally produced definition from before dispatch.
                                    contactTestedKinds.Add(contactKind);
                                    // SamusKnockbackMovement intentionally publishes a pending
                                    // bank-$90 request. This isolated producer scenario has now
                                    // proved its contact lifecycle; discard it instead of asking a
                                    // later synthetic overlap to stack another request onto the
                                    // same host Samus instance.
                                    stopScenarioAfterContactProbe = true;
                                }
                                else
                                {
                                    loaded.Enemies.StepEnemyProjectiles(
                                        assets.LevelData,
                                        loaded.Samus,
                                        cameraX: cameraX,
                                        cameraY: cameraY,
                                        nmiFrameCounter8: unchecked((byte)frame),
                                        samusBombs: loaded.SharedProjectiles);
                                }

                                ObserveLiveProjectiles(
                                    loaded.Enemies,
                                    observations,
                                    state,
                                    cameraX,
                                    cameraY,
                                    placement.Name,
                                    frame);
                                executedFrames++;
                                if (stopScenarioAfterContactProbe)
                                    break;
                            }
                            catch (Exception exception)
                            {
                                failures.Add(new EnemyAttackFailure(
                                    state,
                                    cameraX,
                                    cameraY,
                                    placement.Name,
                                    frame,
                                    exception));
                                break;
                            }
                        }

                        executedScenarios++;
                    }
                }
            }
            catch (Exception exception)
            {
                failures.Add(new EnemyAttackFailure(
                    state,
                    CameraX: null,
                    CameraY: null,
                    Placement: null,
                    Frame: -1,
                    exception));
            }
        }

        if (failures.Count != 0)
        {
            Console.Error.WriteLine(
                $"Retail enemy attack execution found {failures.Count} failing scenarios:");
            foreach (IGrouping<string, EnemyAttackFailure> group in failures
                .GroupBy(failure => $"{failure.Exception.GetType().Name}: {failure.Exception.Message}")
                .OrderBy(group => group.Key, StringComparer.Ordinal))
            {
                Console.Error.WriteLine($"  {group.Key}");
                foreach (EnemyAttackFailure failure in group.Take(12))
                {
                    string location = failure.CameraX.HasValue
                        ? $" view=({failure.CameraX:X4},{failure.CameraY:X4})/" +
                          $"{failure.Placement} frame={failure.Frame}"
                        : string.Empty;
                    Console.Error.WriteLine(
                        $"    room/state $8F:{failure.State.RoomPointer:X4}/" +
                        $"${failure.State.StatePointer:X4} {failure.State.Symbol}{location}");
                }
                if (group.Count() > 12)
                    Console.Error.WriteLine($"    ... and {group.Count() - 12} more scenarios");
            }
            return 1;
        }

        RoomEnemyProjectileKind[] damagingKinds = observations
            .Where(pair => pair.Value.SawDamageEnabled)
            .Select(pair => pair.Key)
            .OrderBy(kind => (ushort)kind)
            .ToArray();
        RoomEnemyProjectileKind[] untestedDamagingKinds = damagingKinds
            .Where(kind => !contactTestedKinds.Contains(kind))
            .ToArray();
        if (untestedDamagingKinds.Length != 0)
        {
            throw new InvalidDataException(
                "Naturally observed damaging enemy projectiles escaped contact coverage: " +
                string.Join(", ", untestedDamagingKinds.Select(kind =>
                    $"{kind} ($86:{(ushort)kind:X4})")));
        }

        int movedKinds = observations.Count(pair => pair.Value.Positions.Count > 1);
        int animatedKinds = observations.Count(pair => pair.Value.Spritemaps.Count > 1);
        RoomEnemyProjectileKind[] phaseOrEventKinds = Enum
            .GetValues<RoomEnemyProjectileKind>()
            .Where(kind => kind != RoomEnemyProjectileKind.None &&
                !observations.ContainsKey(kind))
            .OrderBy(kind => (ushort)kind)
            .ToArray();
        Console.WriteLine(
            $"Retail enemy attack execution audit passed: {executedScenarios} fresh retail " +
            $"view/placement scenarios completed {executedFrames} enemy and projectile frames; " +
            $"naturally spawned {observations.Count} bank-$86 definitions, observed movement " +
            $"in {movedKinds}, animation in {animatedKinds}, and verified exact common Samus " +
            $"damage/knockback/disposal for all {contactTestedKinds.Count} damage-enabled kinds.");
        Console.WriteLine(
            $"Phase/event-triggered residual inventory ({phaseOrEventKinds.Length}): " +
            string.Join(", ", phaseOrEventKinds.Select(kind =>
                $"{kind}=$86:{(ushort)kind:X4}")));
        return 0;
    }

    private static void ObserveLiveProjectiles(
        RoomEnemySystem enemies,
        Dictionary<RoomEnemyProjectileKind, ProjectileObservation> observations,
        RetailRoomState state,
        ushort cameraX,
        ushort cameraY,
        string placement,
        int frame)
    {
        foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles
            .Where(projectile => projectile.IsActive))
        {
            if (!observations.TryGetValue(projectile.Kind, out ProjectileObservation? observation))
            {
                observation = new ProjectileObservation(
                    state,
                    cameraX,
                    cameraY,
                    placement,
                    frame);
                observations.Add(projectile.Kind, observation);
            }

            observation.Positions.Add((projectile.XPosition, projectile.YPosition));
            if (projectile.SpritemapPointer != 0)
                observation.Spritemaps.Add(projectile.SpritemapPointer);
            observation.SawDamageEnabled |= projectile.CanDamageSamus;
        }
    }

    private static void VerifyNaturalProjectileContact(
        ISnesAddressSpace bus,
        LoadedRetailState loaded,
        RoomLevelData level,
        RoomEnemyProjectileSlot target,
        ushort cameraX,
        ushort cameraY,
        byte frame)
    {
        EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
            bus,
            loaded.Enemies,
            loaded.Samus,
            loaded.SharedProjectiles,
            level,
            target,
            cameraX,
            cameraY,
            frame);
    }

    private sealed class ProjectileObservation
    {
        public ProjectileObservation(
            RetailRoomState firstState,
            ushort firstCameraX,
            ushort firstCameraY,
            string firstPlacement,
            int firstFrame)
        {
            FirstState = firstState;
            FirstCameraX = firstCameraX;
            FirstCameraY = firstCameraY;
            FirstPlacement = firstPlacement;
            FirstFrame = firstFrame;
        }

        public RetailRoomState FirstState { get; }
        public ushort FirstCameraX { get; }
        public ushort FirstCameraY { get; }
        public string FirstPlacement { get; }
        public int FirstFrame { get; }
        public HashSet<(ushort X, ushort Y)> Positions { get; } = [];
        public HashSet<ushort> Spritemaps { get; } = [];
        public bool SawDamageEnabled { get; set; }
    }

    private sealed record EnemyAttackFailure(
        RetailRoomState State,
        ushort? CameraX,
        ushort? CameraY,
        string? Placement,
        int Frame,
        Exception Exception);
}
