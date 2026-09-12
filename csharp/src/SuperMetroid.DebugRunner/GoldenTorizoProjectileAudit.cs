using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Destructive, encounter-backed interaction probes for Golden Torizo's bank-$86 actors.
/// Keeping these probes separate from the boss choreography audit makes the distinction
/// explicit: the main file proves that authored attacks are selected, while this file
/// consumes fresh copies of those attacks to prove their complete collision lifecycles.
/// </summary>
internal static partial class GoldenTorizoAudit
{
    private const ushort OrdinaryTorizoShotCallback = 0xc97c;
    private const ushort GoldenTorizoLowHealthEggInstruction = 0xd031;

    /// <summary>
    /// Proves every phase in which a Golden Torizo projectile can actually damage Samus.
    /// Three definitions spawn with collision disabled and enable it only after an authored
    /// hatch or terrain impact; accepting their initial allocation would miss the attack.
    /// Independent room loads are intentional because common contact consumes or mutates the
    /// actor exactly as the SNES dispatcher does.
    /// </summary>
    private static void VerifyNaturalProjectileInteractions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        GoldenProjectilePhase[] contactPhases =
        [
            GoldenProjectilePhase.ChozoOrbFloorExplosion,
            GoldenProjectilePhase.SonicBoomFlight,
            GoldenProjectilePhase.HatchedEgg,
            GoldenProjectilePhase.EyeBeamFloorExplosion,
            GoldenProjectilePhase.SuperMissileExplosion,
        ];

        foreach (GoldenProjectilePhase phase in contactPhases)
        {
            (LoadedGoldenTorizo loaded, RoomEnemyProjectileSlot projectile, int frame) =
                AdvanceToNaturalProjectilePhase(bus, room, assets, phase);
            VerifyPhaseDefinition(bus, phase, projectile, frame);
            EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
                bus,
                loaded.Enemies,
                loaded.Samus,
                new SamusBombProjectileSystem(),
                assets.LevelData,
                projectile,
                CameraX,
                CameraY,
                unchecked((byte)frame));
        }

        // The orb and hatched egg are ordinary destructible actors. The caught Super is
        // also beam-blocking while held/thrown and shares its terrain-impact explosion list
        // with its shot response. Execute all three lists through terminal deletion rather
        // than considering the initial collision-dispatch state sufficient evidence.
        VerifyNaturalShotResponse(
            bus,
            room,
            assets,
            GoldenProjectilePhase.ChozoOrbFlight,
            expectedVisibleMaps: 5,
            expectedDropHeader: 0xefbf,
            expectedDropTable: 0xf40a);
        VerifyNaturalShotResponse(
            bus,
            room,
            assets,
            GoldenProjectilePhase.HatchedEgg,
            expectedVisibleMaps: 5);
        VerifyNaturalShotResponse(
            bus,
            room,
            assets,
            GoldenProjectilePhase.HeldSuperMissile,
            expectedVisibleMaps: 6);
    }

    /// <summary>
    /// Validates immutable definition fields plus the precise phase-specific radius change.
    /// The Super Missile explosion is the sole exception to definition radii: opcode
    /// $86:8A24 at list $B2EF reads the literal $10/$10 bytes from the cartridge stream.
    /// </summary>
    private static void VerifyPhaseDefinition(
        SuperMetroidAddressSpace bus,
        GoldenProjectilePhase phase,
        RoomEnemyProjectileSlot projectile,
        int frame)
    {
        RoomEnemyProjectileKind kind = KindForPhase(phase);
        ushort definitionRadii = ReadWord(
            bus,
            0x860000 | unchecked((ushort)((ushort)kind + 6)));
        ushort definitionProperties = ReadWord(
            bus,
            0x860000 | unchecked((ushort)((ushort)kind + 8)));
        byte expectedXRadius = unchecked((byte)definitionRadii);
        byte expectedYRadius = unchecked((byte)(definitionRadii >> 8));
        if (phase == GoldenProjectilePhase.SuperMissileExplosion)
        {
            expectedXRadius = bus.ReadByte(0x86b2f1);
            expectedYRadius = bus.ReadByte(0x86b2f2);
        }

        if (projectile.Kind != kind || !projectile.CanDamageSamus ||
            projectile.Damage != (definitionProperties & 0x0fff) ||
            projectile.XRadius != expectedXRadius || projectile.YRadius != expectedYRadius)
        {
            throw new InvalidDataException(
                $"Natural Golden Torizo phase {phase} at frame {frame} disagrees with its " +
                $"ROM definition/list: kind={projectile.Kind}, damage/radii/collision=" +
                $"{projectile.Damage}/{projectile.XRadius}x{projectile.YRadius}/" +
                $"{projectile.CanDamageSamus}; expected {kind}/" +
                $"{definitionProperties & 0x0fff}/{expectedXRadius}x{expectedYRadius}/true.");
        }
    }

    private static void VerifyNaturalShotResponse(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets,
        GoldenProjectilePhase phase,
        int expectedVisibleMaps,
        ushort? expectedDropHeader = null,
        ushort? expectedDropTable = null)
    {
        (LoadedGoldenTorizo loaded, RoomEnemyProjectileSlot projectile, int producerFrame) =
            AdvanceToNaturalProjectilePhase(bus, room, assets, phase);
        ushort impactX = projectile.XPosition;
        ushort impactY = projectile.YPosition;
        ushort shotResponse = EnemyProjectileAuditAssertions.VerifyNaturalDestructibleSamusShot(
            bus,
            loaded.Enemies,
            new SamusProjectileSystem(),
            new SamusBombProjectileSystem(),
            projectile);

        var responseMaps = new HashSet<ushort>();
        // Native pickup selection needs Samus's inventory. Keep her out of the effect's
        // collision area instead of removing the player context from projectile processing.
        loaded.Samus.XPosition = loaded.Samus.YPosition = 0x1000;
        for (int frame = 0; frame < 256 && projectile.IsActive; frame++)
        {
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: loaded.Samus,
                cameraX: CameraX,
                cameraY: CameraY,
                nmiFrameCounter8: unchecked((byte)(producerFrame + frame + 1)));
            if (projectile.IsActive && projectile.SpritemapPointer is not 0 and not 0x8000)
                responseMaps.Add(projectile.SpritemapPointer);
        }

        TorizoOrbDropRequest? drop = loaded.Enemies.TorizoOrbDropRequests.Count == 1
            ? loaded.Enemies.TorizoOrbDropRequests[0]
            : null;
        bool dropMatches = expectedDropHeader is null
            ? drop is null
            : drop is not null && drop.Value.X == impactX && drop.Value.Y == impactY &&
              drop.Value.EnemyDefinitionPointer == expectedDropHeader &&
              drop.Value.ItemDropChancesPointer == expectedDropTable;
        if (projectile.IsActive || responseMaps.Count != expectedVisibleMaps || !dropMatches)
        {
            throw new InvalidDataException(
                $"Golden Torizo {phase} shot response $86:{shotResponse:X4} ended with " +
                $"live={projectile.IsActive}, maps={responseMaps.Count}/" +
                $"{expectedVisibleMaps}, drop=" +
                (drop is null
                    ? "none."
                    : $"(${drop.Value.X:X4},${drop.Value.Y:X4}) header/table=" +
                      $"${drop.Value.EnemyDefinitionPointer:X4}/" +
                      $"${drop.Value.ItemDropChancesPointer:X4}."));
        }
    }

    /// <summary>
    /// Runs a fresh retail encounter until one physical projectile reaches the requested
    /// phase. Enemy AI still receives Samus for attack selection and aiming; projectile
    /// stepping receives null so the candidate cannot damage her before the focused probe.
    /// </summary>
    private static (LoadedGoldenTorizo Loaded, RoomEnemyProjectileSlot Projectile, int Frame)
        AdvanceToNaturalProjectilePhase(
            SuperMetroidAddressSpace bus,
            CartridgeRoomHeader room,
            CartridgeRoomAssets assets,
            GoldenProjectilePhase phase)
    {
        LoadedGoldenTorizo loaded = Load(
            bus,
            room,
            assets,
            alreadyDefeated: false,
            () => { });
        PrepareConditionalProjectileProducer(bus, loaded, assets.LevelData, phase);

        RoomEnemyProjectileKind requestedKind = KindForPhase(phase);
        var observedSlots = new HashSet<int>();
        for (int frame = 0; frame < 30000; frame++)
        {
            byte nmi = unchecked((byte)frame);
            loaded.Enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                nmiFrameCounter8: nmi);
            ObserveKind(loaded.Enemies, requestedKind, observedSlots);
            RoomEnemyProjectileSlot? candidate = FindPhase(
                loaded.Enemies,
                phase,
                observedSlots);
            if (candidate is not null)
                return (loaded, candidate, frame);

            // Golden Torizo's held Super executes the real $B269/$B272 aim opcode, which
            // requires the active Samus coordinates. The other families receive null only
            // to suppress common contact while their terrain/list transitions continue.
            SamusState? projectileSamus = phase is
                GoldenProjectilePhase.HeldSuperMissile or
                GoldenProjectilePhase.SuperMissileExplosion
                    ? loaded.Samus
                    : null;
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: projectileSamus,
                cameraX: CameraX,
                cameraY: CameraY,
                nmiFrameCounter8: nmi);
            ObserveKind(loaded.Enemies, requestedKind, observedSlots);
            candidate = FindPhase(loaded.Enemies, phase, observedSlots);
            if (candidate is not null)
                return (loaded, candidate, frame);
        }

        throw new InvalidDataException(
            $"Golden Torizo never produced phase {phase} within 30000 retail frames.");
    }

    private static void PrepareConditionalProjectileProducer(
        SuperMetroidAddressSpace bus,
        LoadedGoldenTorizo loaded,
        RoomLevelData level,
        GoldenProjectilePhase phase)
    {
        if (phase is GoldenProjectilePhase.HatchedEgg)
        {
            // $AA:D031 is the real low-health list entry that spawns definition $86:B1C0.
            // Selecting the authored branch avoids fabricating the projectile or its state.
            loaded.Head.CurrentInstruction = GoldenTorizoLowHealthEggInstruction;
            loaded.Head.InstructionTimer = 1;
            return;
        }

        if (phase is GoldenProjectilePhase.HeldSuperMissile or
            GoldenProjectilePhase.SuperMissileExplosion)
        {
            CatchSuperMissileThroughAuthoredHitbox(bus, loaded, level);
        }
    }

    /// <summary>
    /// Supplies the player-owned Super Missile required by Golden Torizo's counterattack.
    /// The shot point comes from a currently displayed extended hitbox using callback
    /// $AA:C97C; no projectile actor, boss list, or capture result is manufactured.
    /// </summary>
    private static void CatchSuperMissileThroughAuthoredHitbox(
        SuperMetroidAddressSpace bus,
        LoadedGoldenTorizo loaded,
        RoomLevelData level)
    {
        if (!AdvanceToShotCallback(
                bus,
                loaded,
                level,
                OrdinaryTorizoShotCallback,
                out ushort shotX,
                out ushort shotY))
        {
            throw new InvalidDataException(
                "Golden Torizo never displayed an authored $AA:C97C catch hitbox.");
        }

        loaded.Head.FlashTimer = 0;
        loaded.State.ShotGuard = 0;
        loaded.State.CapturedProjectileFamily = 0;
        loaded.Head.Parameter2 &= 0xefff;
        loaded.Samus.XPosition = (loaded.Head.Parameter1 & 0x8000) != 0
            ? unchecked((ushort)(loaded.Head.XPosition + 32))
            : unchecked((ushort)(loaded.Head.XPosition - 32));
        var shots = new SamusProjectileSystem();
        var bombs = new SamusBombProjectileSystem();
        ArmProjectile(shots.Slots[0], loaded.Head, type: 0x0200, damage: 300);
        shots.Slots[0].XPosition = shotX;
        shots.Slots[0].YPosition = shotY;
        shots.Slots[0].XRadius = 1;
        shots.Slots[0].YRadius = 1;
        ushort healthBefore = loaded.Head.Health;
        int hits = loaded.Enemies.ResolveOrdinaryProjectileHits(
            bus,
            shots,
            bombs,
            loaded.Samus);
        if (hits != 1 || loaded.Head.Health != healthBefore ||
            loaded.State.CapturedProjectileFamily != 0x0200 ||
            (loaded.Head.Parameter2 & 0x1000) == 0 ||
            (shots.Slots[0].Direction & 0x0010) == 0)
        {
            throw new InvalidDataException(
                $"Golden Torizo authored Super catch mismatch: hits={hits}, " +
                $"health={healthBefore}->{loaded.Head.Health}, family=" +
                $"${loaded.State.CapturedProjectileFamily:X4}, parameter2=" +
                $"${loaded.Head.Parameter2:X4}, direction=${shots.Slots[0].Direction:X4}.");
        }
    }

    private static void ObserveKind(
        RoomEnemySystem enemies,
        RoomEnemyProjectileKind kind,
        HashSet<int> observedSlots)
    {
        foreach (RoomEnemyProjectileSlot projectile in enemies.EnemyProjectiles)
        {
            if (projectile.IsActive && projectile.Kind == kind)
                observedSlots.Add(projectile.SlotIndex);
        }
    }

    private static RoomEnemyProjectileSlot? FindPhase(
        RoomEnemySystem enemies,
        GoldenProjectilePhase phase,
        HashSet<int> observedSlots)
    {
        RoomEnemyProjectileKind kind = KindForPhase(phase);
        return enemies.EnemyProjectiles.FirstOrDefault(projectile =>
            projectile.IsActive && projectile.Kind == kind &&
            observedSlots.Contains(projectile.SlotIndex) &&
            PhasePredicate(phase, projectile));
    }

    private static bool PhasePredicate(
        GoldenProjectilePhase phase,
        RoomEnemyProjectileSlot projectile) => phase switch
    {
        GoldenProjectilePhase.ChozoOrbFlight =>
            !projectile.CanDamageSamus && projectile.BlocksSamusProjectiles,
        GoldenProjectilePhase.ChozoOrbFloorExplosion =>
            projectile.CanDamageSamus && projectile.PersistsOnSamusContact,
        GoldenProjectilePhase.SonicBoomFlight => projectile.CanDamageSamus,
        GoldenProjectilePhase.HatchedEgg =>
            projectile.CanDamageSamus && projectile.BlocksSamusProjectiles &&
            projectile.PersistsOnSamusContact,
        GoldenProjectilePhase.HeldSuperMissile =>
            !projectile.CanDamageSamus && projectile.BlocksSamusProjectiles,
        GoldenProjectilePhase.SuperMissileExplosion =>
            projectile.CanDamageSamus && projectile.PersistsOnSamusContact &&
            projectile.XRadius == 16 && projectile.YRadius == 16,
        GoldenProjectilePhase.EyeBeamFloorExplosion =>
            projectile.CanDamageSamus && projectile.PersistsOnSamusContact,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null),
    };

    private static RoomEnemyProjectileKind KindForPhase(GoldenProjectilePhase phase) =>
        phase switch
        {
            GoldenProjectilePhase.ChozoOrbFlight or
            GoldenProjectilePhase.ChozoOrbFloorExplosion =>
                RoomEnemyProjectileKind.GoldenTorizoChozoOrb,
            GoldenProjectilePhase.SonicBoomFlight =>
                RoomEnemyProjectileKind.GoldenTorizoSonicBoom,
            GoldenProjectilePhase.HatchedEgg =>
                RoomEnemyProjectileKind.GoldenTorizoEgg,
            GoldenProjectilePhase.HeldSuperMissile or
            GoldenProjectilePhase.SuperMissileExplosion =>
                RoomEnemyProjectileKind.GoldenTorizoSuperMissile,
            GoldenProjectilePhase.EyeBeamFloorExplosion =>
                RoomEnemyProjectileKind.GoldenTorizoEyeBeam,
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null),
        };

    private enum GoldenProjectilePhase
    {
        ChozoOrbFlight,
        ChozoOrbFloorExplosion,
        SonicBoomFlight,
        HatchedEgg,
        HeldSuperMissile,
        SuperMissileExplosion,
        EyeBeamFloorExplosion,
    }
}
