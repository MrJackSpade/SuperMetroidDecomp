using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Drives Bomb Torizo's cartridge-authored pickup and awakening presentation. Combat lives
/// beside this sequence as it is extracted from the legacy route planner incrementally.
/// </summary>
internal static partial class EarlyControllerRouteAudit
{
    private static int DriveBombTorizoPickup(
        SuperMetroidRuntime runtime,
        ControllerRouteHost host,
        int maximumFrames)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Bomb Torizo pickup route began without Samus.");
        TorizoEnemyState torizo = runtime.Enemies.BombTorizo ??
            throw new InvalidOperationException("Bomb Torizo room loaded without its enemy.");
        if (!runtime.Plms.HasActiveHeader(0xd6ea))
            throw new InvalidDataException("Bomb Torizo hand was absent on undefeated entry.");

        bool descendedFromStatue = false;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            CollectiblePlmSnapshot collectible = runtime.Plms.Collectibles.Single();
            // The opening shot can reach the orb while Samus is still crossing the room,
            // but its nine-frame burst does not expose the type-B pickup immediately. A
            // human player naturally turns back after overshooting it. Do the same with
            // ordinary controller input: once the cartridge-owned PLM reports Visible,
            // steer toward its block centre instead of continuing blindly into the wall.
            int collectibleX = collectible.BlockIndex % runtime.LevelData!.WidthInBlocks * 16 + 8;
            int collectibleY = collectible.BlockIndex / runtime.LevelData.WidthInBlocks * 16 + 8;
            bool itemIsVisible = collectible.Phase == CollectiblePhase.Visible;
            // The repeated setup jump can place Samus on the dormant statue, above the
            // pickup block. Walk one native block-radius clear of its left side and wait
            // for gravity to bring the body down to the item's row before approaching it
            // again. This remains a pure input script: no pose, position, collision, or PLM
            // state is written by the audit.
            if (itemIsVisible && !descendedFromStatue &&
                samus.YPosition + samus.Kinematics.YRadius >= collectibleY - 8)
            {
                descendedFromStatue = true;
            }
            int dismountX = collectibleX - 48;
            SnesButton horizontal = itemIsVisible && !descendedFromStatue
                ? SnesButton.Left
                : itemIsVisible && samus.XPosition > collectibleX
                    ? SnesButton.Left
                    : SnesButton.Right;
            if (itemIsVisible && !descendedFromStatue && samus.XPosition <= dismountX)
                horizontal = 0;
            ushort input = (ushort)horizontal;
            // Fire one-frame power-beam edges. Holding X would begin charging after the
            // first shot and hide whether the ordinary projectile producer can reopen.
            if (frame % 18 == 0)
                input |= (ushort)SnesButton.X;
            // A short jump every two seconds lets the controller route cross authored
            // pedestal geometry if the horizontal body becomes stationary below the orb.
            if (!itemIsVisible && frame % 120 is >= 72 and < 96)
                input |= (ushort)SnesButton.A;

            host.StepFrame(input);
            if (frame % 60 == 0)
            {
                RoomCollisionBlock itemBlock = runtime.LevelData.GetCollisionBlockByIndex(
                    collectible.BlockIndex);
                Console.WriteLine(
                    $"  Bomb pickup f{frame + 1}: Samus=(${samus.XPosition:X4}," +
                    $"${samus.YPosition:X4}) pose=${samus.Pose:X2}, " +
                    $"radius=({samus.Kinematics.XRadius},{samus.Kinematics.YRadius}), " +
                    $"item=[{string.Join(' ', runtime.Plms.Collectibles)}], " +
                    $"collision={itemBlock.CollisionType:X1}/{itemBlock.Behavior:X2}" +
                    $"@({collectible.BlockIndex % runtime.LevelData.WidthInBlocks:X2}," +
                    $"{collectible.BlockIndex / runtime.LevelData.WidthInBlocks:X2}), " +
                    $"hand={runtime.Plms.HasActiveHeader(0xd6ea)}, " +
                    $"awake={torizo.AwakeningReleased}.");
            }

            if (samus.CollectedItems.HasAny(SamusEquipmentFlags.Bombs))
            {
                if (!runtime.Plms.HasActiveHeader(0xd6ea))
                {
                    throw new InvalidDataException(
                        "Bomb Torizo hand deleted on the pickup frame instead of running " +
                        "its cartridge-authored post-Bombs sequence.");
                }
                return frame + 1;
            }
            if (torizo.AwakeningReleased)
            {
                throw new InvalidDataException(
                    "Bomb Torizo awakened while the D6EA hand trigger was still required.");
            }
        }

        throw new InvalidDataException(
            $"Bombs were not acquired after {maximumFrames} controller frames; " +
            $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), pose=${samus.Pose:X2}, " +
            $"item=[{string.Join(' ', runtime.Plms.Collectibles)}].");
    }

    /// <summary>
    /// Closes the cartridge item message and waits through PLM $D6EA's complete hand
    /// sequence. This deliberately observes the native boundaries instead of skipping to
    /// the fight: the message box must freeze the PLM, the hand must DMA the cracked statue
    /// graphics and emit all eight $A993 fragments, and only its deletion may release the
    /// enemy's awakening AI.
    /// </summary>
    private static BombTorizoAwakeningResult DriveBombTorizoAwakening(
        SuperMetroidRuntime runtime,
        ControllerRouteHost host,
        int maximumFrames)
    {
        TorizoEnemyState torizo = runtime.Enemies.BombTorizo ??
            throw new InvalidOperationException(
                "Bomb Torizo awakening audit began without its enemy.");
        if (!runtime.MessageBox.IsActive)
        {
            throw new InvalidDataException(
                "Bomb pickup did not open its cartridge-authored item message.");
        }
        if (!runtime.Plms.HasActiveHeader(0xd6ea))
        {
            throw new InvalidDataException(
                "Bomb Torizo hand was absent while the item message was open.");
        }

        int messageFrames = 0;
        while (runtime.MessageBox.IsActive && messageFrames < maximumFrames)
        {
            // The genuine message handler accepts a held face button once its mandatory
            // acknowledgement delay expires. Step the complete runtime so this also proves
            // the surrounding early-return path freezes gameplay rather than merely hiding
            // the box in the frontend.
            host.StepFrame((ushort)SnesButton.X);
            messageFrames++;
            if (!runtime.Plms.HasActiveHeader(0xd6ea))
            {
                throw new InvalidDataException(
                    "Bomb Torizo hand advanced or deleted while the item message paused gameplay.");
            }
            if (torizo.AwakeningReleased)
            {
                throw new InvalidDataException(
                    "Bomb Torizo awakened while the item message paused gameplay.");
            }
        }
        if (runtime.MessageBox.IsActive)
        {
            throw new InvalidDataException(
                $"Bomb item message remained open after {maximumFrames} acknowledgement frames.");
        }

        int maximumLiveFragments = 0;
        int sequenceFrames = 0;
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Bomb Torizo hand sequence began without Samus.");
        while (runtime.Plms.HasActiveHeader(0xd6ea) && sequenceFrames < maximumFrames)
        {
            // The native debris is dangerous. A player is free to retreat while the hand
            // runs, so walk to a safe left-side lane without touching the room's door block.
            // This also proves the PLM sequence does not incorrectly lock ordinary movement.
            ushort retreatInput = samus.XPosition > 32
                ? (ushort)(SnesButton.Left | SnesButton.B |
                    (sequenceFrames < 28 ? SnesButton.A : 0))
                : (ushort)0;
            host.StepFrame(retreatInput);
            sequenceFrames++;
            maximumLiveFragments = Math.Max(
                maximumLiveFragments,
                runtime.Enemies.EnemyProjectiles.Count(projectile =>
                    projectile.IsActive &&
                    projectile.Kind == RoomEnemyProjectileKind.BombTorizoStatueBreaking));
            if (torizo.AwakeningReleased)
            {
                throw new InvalidDataException(
                    "Bomb Torizo awakening escaped the cartridge hand-PLM deletion gate.");
            }
        }
        if (runtime.Plms.HasActiveHeader(0xd6ea))
        {
            throw new InvalidDataException(
                $"Bomb Torizo hand remained active after {maximumFrames} sequence frames.");
        }
        if (maximumLiveFragments != 8)
        {
            throw new InvalidDataException(
                $"Bomb Torizo hand exposed at most {maximumLiveFragments} statue fragments; " +
                "the cartridge sequence requires all eight $A993 projectiles.");
        }

        // Enemy main executes before PLM main in a gameplay frame. Consequently the enemy
        // sees the hand's deletion on the following frame, exactly matching the original
        // scheduler rather than gaining a special cross-system notification here.
        host.StepFrame(0);
        sequenceFrames++;
        if (!torizo.AwakeningReleased)
        {
            throw new InvalidDataException(
                "Bomb Torizo did not awaken on the enemy frame after hand-PLM deletion.");
        }

        Console.WriteLine(
            $"  Bomb Torizo hand: message={messageFrames} frames, " +
            $"sequence={sequenceFrames} frames, fragments={maximumLiveFragments}, " +
            $"Samus-health={samus.Health}, enemy={torizo}.");
        return new BombTorizoAwakeningResult(messageFrames, sequenceFrames);
    }
}
