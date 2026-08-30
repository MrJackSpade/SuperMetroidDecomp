using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Focused lifecycle and interaction coverage for projectile $86:EC48. Each destructive
/// probe begins from a fresh retail Botwoon encounter; the test never manufactures a spit,
/// rewrites its angle, or bypasses the boss's real path/attack decision.
/// </summary>
internal static partial class BotwoonAudit
{
    private const ushort BotwoonSpitPreInstruction = 0xec05;
    private const ushort BotwoonSpitInstructionList = 0xebae;

    private static void VerifySpitLifecycleAndInteractions(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        VerifySpitMovementAnimationAndDisposal(bus, room, assets);
        VerifySpitShotAndContact(bus, room, assets);
    }

    private static void VerifySpitMovementAnimationAndDisposal(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (LoadedBotwoon loaded, RoomEnemyProjectileSlot spit) =
            AdvanceFreshEncounterToSpit(bus, room, assets);
        VerifySpitInitialization(bus, loaded.Head, spit);

        ushort initialX = spit.XPosition;
        ushort initialXSubposition = spit.XSubposition;
        ushort initialY = spit.YPosition;
        ushort initialYSubposition = spit.YSubposition;
        (int xDisplacement, int yDisplacement) = GetSpitDisplacement(spit);
        (ushort expectedX, ushort expectedXSubposition) = AddFixed(
            initialX,
            initialXSubposition,
            xDisplacement);
        (ushort expectedY, ushort expectedYSubposition) = AddFixed(
            initialY,
            initialYSubposition,
            yDisplacement);

        loaded.Enemies.StepEnemyProjectiles(
            assets.LevelData,
            samus: null,
            cameraX: CameraX,
            cameraY: CameraY,
            nmiFrameCounter8: 0);
        if (!spit.IsActive || spit.XPosition != expectedX ||
            spit.XSubposition != expectedXSubposition || spit.YPosition != expectedY ||
            spit.YSubposition != expectedYSubposition)
        {
            throw new InvalidDataException(
                $"Botwoon spit first fixed-point movement failed: active={spit.IsActive}, " +
                $"X=${initialX:X4}:{initialXSubposition:X4}->" +
                $"${spit.XPosition:X4}:{spit.XSubposition:X4} expected " +
                $"${expectedX:X4}:{expectedXSubposition:X4}, Y=" +
                $"${initialY:X4}:{initialYSubposition:X4}->" +
                $"${spit.YPosition:X4}:{spit.YSubposition:X4} expected " +
                $"${expectedY:X4}:{expectedYSubposition:X4}.");
        }

        var maps = new HashSet<ushort>();
        var positions = new HashSet<(ushort X, ushort Y)>
        {
            (spit.XPosition, spit.YPosition),
        };
        if (spit.SpritemapPointer is not 0 and not 0x8000)
            maps.Add(spit.SpritemapPointer);
        for (int frame = 1; frame < 1024 && spit.IsActive; frame++)
        {
            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY,
                nmiFrameCounter8: unchecked((byte)frame));
            if (!spit.IsActive)
                continue;
            positions.Add((spit.XPosition, spit.YPosition));
            if (spit.SpritemapPointer is not 0 and not 0x8000)
                maps.Add(spit.SpritemapPointer);
        }

        // EC0C uses the inclusive 256x256 viewport: equality survives and the first wrapped
        // coordinate strictly outside it deletes. Five maps are the complete EBAE loop.
        if (spit.IsActive || positions.Count < 2 || maps.Count != 5)
        {
            throw new InvalidDataException(
                $"Botwoon spit terminal lifecycle failed: live={spit.IsActive}, " +
                $"positions={positions.Count}, maps={maps.Count}.");
        }
    }

    private static void VerifySpitShotAndContact(
        SuperMetroidAddressSpace bus,
        CartridgeRoomHeader room,
        CartridgeRoomAssets assets)
    {
        (LoadedBotwoon loaded, RoomEnemyProjectileSlot spit) =
            AdvanceFreshEncounterToSpit(bus, room, assets);
        VerifySpitInitialization(bus, loaded.Head, spit);

        // Definition property $8000 is clear. Even in the same coarse 32-pixel collision
        // cell, an ordinary beam must remain live and the spit must not enter its defensive
        // delete list.
        var shots = new SamusProjectileSystem();
        SamusProjectileSlot beam = shots.Slots[0];
        beam.Type = 0x0001;
        beam.Damage = 20;
        beam.Direction = (ushort)SamusProjectileDirection.Right;
        beam.XPosition = spit.XPosition;
        beam.YPosition = spit.YPosition;
        beam.XRadius = 4;
        beam.YRadius = 4;
        beam.InstructionPointer = 0x9000;
        beam.InstructionTimer = 1;
        int hits = loaded.Enemies.ResolveEnemyProjectileSamusProjectileHits(
            bus,
            shots,
            new SamusBombProjectileSystem());
        if (hits != 0 || beam.InstructionPointer != 0x9000 || !spit.IsActive)
        {
            throw new InvalidDataException(
                $"Botwoon spit incorrectly blocked a Samus beam: hits={hits}, " +
                $"beam=${beam.InstructionPointer:X4}, live={spit.IsActive}.");
        }

        // This proves exact 96 damage from properties $1060, common 96-frame invincibility,
        // five-frame knockback, and deletion because property $4000 is clear.
        EnemyProjectileAuditAssertions.VerifyNaturalSamusContact(
            bus,
            loaded.Enemies,
            loaded.Samus,
            new SamusBombProjectileSystem(),
            assets.LevelData,
            spit,
            cameraX: CameraX,
            cameraY: CameraY);
    }

    private static (LoadedBotwoon Loaded, RoomEnemyProjectileSlot Spit)
        AdvanceFreshEncounterToSpit(
            SuperMetroidAddressSpace bus,
            CartridgeRoomHeader room,
            CartridgeRoomAssets assets)
    {
        LoadedBotwoon loaded = Load(bus, room, assets, alreadyDefeated: false);
        for (int frame = 0; frame < 4000; frame++)
        {
            // Stop between the producer and bank-$86 consumer passes. This exposes exactly
            // the post-initializer state—including map sentinel and untouched subpositions—
            // while every earlier body link still received its normal projectile frames.
            loaded.Enemies.StepFrame(
                CameraX,
                CameraY,
                timeIsFrozen: false,
                loaded.Samus,
                level: assets.LevelData,
                nmiFrameCounter8: unchecked((byte)frame));
            RoomEnemyProjectileSlot? spit = loaded.Enemies.EnemyProjectiles.FirstOrDefault(
                projectile => projectile.Kind == RoomEnemyProjectileKind.BotwoonSpit);
            if (spit is not null)
                return (loaded, spit);

            loaded.Enemies.StepEnemyProjectiles(
                assets.LevelData,
                samus: null,
                cameraX: CameraX,
                cameraY: CameraY,
                nmiFrameCounter8: unchecked((byte)frame));
        }

        throw new InvalidDataException("Fresh Botwoon encounter produced no aimed spit in 4000 frames.");
    }

    private static void VerifySpitInitialization(
        ISnesAddressSpace bus,
        RoomEnemySlot head,
        RoomEnemyProjectileSlot spit)
    {
        int definition = 0x860000 | (ushort)RoomEnemyProjectileKind.BotwoonSpit;
        ushort radii = ReadWord(bus, definition + 6);
        ushort properties = ReadWord(bus, definition + 8);
        if (spit.PreInstruction != ReadWord(bus, definition + 2) ||
            spit.PreInstruction != BotwoonSpitPreInstruction ||
            spit.InstructionPointer != ReadWord(bus, definition + 4) ||
            spit.InstructionPointer != BotwoonSpitInstructionList ||
            spit.InstructionTimer != 1 || spit.SpritemapPointer != 0x8000 ||
            spit.XRadius != unchecked((byte)radii) ||
            spit.YRadius != unchecked((byte)(radii >> 8)) ||
            spit.Damage != (properties & 0x0fff) || spit.Damage != 96 ||
            spit.InvincibilityFrames != 96 || !spit.CanDamageSamus ||
            spit.PersistsOnSamusContact || spit.BlocksSamusProjectiles ||
            spit.CollisionOption != 0 || spit.XPosition != head.XPosition ||
            spit.YPosition != head.YPosition || spit.XSubposition != 0 ||
            spit.YSubposition != 0 ||
            spit.GraphicsIndex != unchecked((ushort)(head.VramTilesIndex | head.PaletteIndex)) ||
            (spit.XVelocity == 0 && spit.Variable0 == 0 &&
             spit.YVelocity == 0 && spit.Variable1 == 0))
        {
            throw new InvalidDataException(
                $"Botwoon spit initialization diverged from $86:EC48: position=" +
                $"(${spit.XPosition:X4},${spit.YPosition:X4}), angle=" +
                $"${spit.DirectionParameter:X4}, vector=${spit.XVelocity:X4}:" +
                $"${spit.Variable0:X4}/${spit.YVelocity:X4}:${spit.Variable1:X4}, " +
                $"list/pre/map=${spit.InstructionPointer:X4}/${spit.PreInstruction:X4}/" +
                $"${spit.SpritemapPointer:X4}, radii={spit.XRadius}/{spit.YRadius}, " +
                $"damage={spit.Damage}, properties damage/persist/block=" +
                $"{spit.CanDamageSamus}/{spit.PersistsOnSamusContact}/" +
                $"{spit.BlocksSamusProjectiles}.");
        }
    }

    private static (int X, int Y) GetSpitDisplacement(RoomEnemyProjectileSlot spit)
    {
        int xMagnitude = unchecked((spit.XVelocity << 16) | spit.Variable0);
        int yMagnitude = unchecked((spit.YVelocity << 16) | spit.Variable1);
        return (
            ((spit.DirectionParameter + 64) & 0x80) != 0 ? -xMagnitude : xMagnitude,
            ((spit.DirectionParameter + 128) & 0x80) != 0 ? -yMagnitude : yMagnitude);
    }

    private static (ushort Position, ushort Subposition) AddFixed(
        ushort position,
        ushort subposition,
        int displacement)
    {
        int fixedPosition = unchecked((position << 16) | subposition);
        fixedPosition = unchecked(fixedPosition + displacement);
        return (unchecked((ushort)(fixedPosition >> 16)), unchecked((ushort)fixedPosition));
    }
}
