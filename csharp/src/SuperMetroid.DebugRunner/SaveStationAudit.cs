using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Cartridge-room reproduction of the complete save-pod interaction.</summary>
internal static class SaveStationAudit
{
    public static int Run(string romPath, string outputDirectory)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        const ushort crateriaSaveRoom = RoomHeaderPointers.CrateriaSaveStation;
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(
            crateriaSaveRoom,
            cameraX: 0,
            cameraY: 0);

        StationPlmSnapshot station = runtime.Plms.Stations.Single(candidate =>
            candidate.Kind == StationKind.Save);
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "Save-room reproduction omitted level data.");
        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "Save-room reproduction omitted Samus.");
        int blockX = station.BlockIndex % level.WidthInBlocks;
        int blockY = station.BlockIndex / level.WidthInBlocks;
        ushort unsnappedX = unchecked((ushort)(blockX * 16 + 13));
        ushort expectedSnappedX = unchecked((ushort)((unsnappedX + 8) & 0xfff0));
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.AnimationFrame = 0;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = unsnappedX;
        samus.YPosition = unchecked((ushort)(blockY * 16 - samus.Kinematics.YRadius));
        samus.InputLocked = false;
        samus.PrimeGraphics(bus);

        // A direct room audit may stage the actor, but the trigger itself is still the
        // production type-$B downward collision and resident $B76F PLM owner.
        if (!runtime.Plms.TryNotifyStationCollision(
                station.BlockIndex,
                behavior: 0x4d,
                collisionPose: samus.Pose,
                horizontal: false,
                movingPositive: true))
        {
            throw new InvalidDataException("Crateria save trigger did not resolve its resident PLM.");
        }
        runtime.StepFrame(0);
        if (!runtime.MessageBox.IsActive ||
            runtime.MessageBox.MessageId != GameplayMessageIds.SaveConfirmation)
            throw new InvalidDataException("Crateria save PLM did not open message $17.");

        Directory.CreateDirectory(outputDirectory);
        WriteFrame(outputDirectory, "SavePrompt.png", runtime);
        StepMessageTo(runtime, GameplayMessageBoxPhase.AwaitingInput, maximumFrames: 96);
        runtime.StepFrame((ushort)SnesButton.A);
        runtime.StepFrame(0);
        StepMessageTo(runtime, GameplayMessageBoxPhase.Inactive, maximumFrames: 96);

        if (samus.XPosition != expectedSnappedX ||
            !SamusState.IsForwardFacingPose(samus.Pose) || !samus.InputLocked)
        {
            throw new InvalidDataException(
                $"Accepted save did not apply $84:B00E: X=${samus.XPosition:X4}/" +
                $"${expectedSnappedX:X4}, pose=${samus.Pose:X2}, locked={samus.InputLocked}.");
        }
        if (!runtime.Enemies.EnemyProjectiles.Any(projectile =>
                projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity))
        {
            throw new InvalidDataException("Accepted save did not spawn bank-$86 electricity.");
        }

        // Allocation alone is not a visual regression test. The bank-$86 initializer
        // deliberately starts at sentinel spritemap $8000, then its instruction handler
        // selects a real bank-$8D frame on a later main-loop pass. Advance that exact
        // production path and require the isolated projectile OAM to produce colored,
        // opaque pixels through the same OBJ decoder used by the desktop renderer.
        int animationFrames = 0;
        int electricityPixels = 0;
        while (electricityPixels == 0 && animationFrames < 32)
        {
            runtime.StepFrame(0);
            animationFrames++;
            electricityPixels = CountVisibleElectricityPixels(runtime);
        }
        if (electricityPixels == 0)
        {
            throw new InvalidDataException(
                "Save-station electricity existed for 32 frames but never produced " +
                "colored pixels through finalized enemy-projectile OAM.");
        }
        WriteFrame(outputDirectory, "SaveAnimation.png", runtime);

        while (runtime.Plms.Stations.Single(candidate => candidate.Kind == StationKind.Save)
                   .SavePhase != SaveStationPhase.AwaitingCompletionMessageClose &&
               animationFrames < 512)
        {
            runtime.StepFrame(0);
            animationFrames++;
        }
        if (animationFrames == 512 || !runtime.MessageBox.IsActive ||
            runtime.MessageBox.MessageId != GameplayMessageIds.SaveCompleted)
        {
            throw new InvalidDataException(
                $"Save animation did not reach completion message in {animationFrames} frames.");
        }
        StepMessageTo(runtime, GameplayMessageBoxPhase.AwaitingInput, maximumFrames: 96);
        runtime.StepFrame((ushort)SnesButton.A);
        runtime.StepFrame(0);
        StepMessageTo(runtime, GameplayMessageBoxPhase.Inactive, maximumFrames: 96);
        runtime.StepFrame(0);

        StationPlmSnapshot completed = runtime.Plms.Stations.Single(candidate =>
            candidate.Kind == StationKind.Save);
        if (!completed.SaveStationLockedOut || completed.SavePhase != SaveStationPhase.Idle ||
            samus.InputLocked)
        {
            throw new InvalidDataException(
                $"Completed save did not restore control/lock out this room entry: " +
                $"phase={completed.SavePhase}, lockout={completed.SaveStationLockedOut}, " +
                $"inputLocked={samus.InputLocked}.");
        }
        if (runtime.Plms.TryNotifyStationCollision(
                station.BlockIndex,
                0x4d,
                samus.Pose,
                horizontal: false,
                movingPositive: true))
        {
            runtime.StepFrame(0);
        }
        if (runtime.MessageBox.IsActive)
            throw new InvalidDataException("Completed save prompt reopened without room re-entry.");

        Console.WriteLine(
            $"Save-station audit passed in $8F:{crateriaSaveRoom:X4}: " +
            $"Samus X ${unsnappedX:X4}->${expectedSnappedX:X4}, " +
            $"animation {animationFrames} frames, {electricityPixels} visible electricity " +
            "pixels and one-entry lockout verified.");
        return 0;
    }

    /// <summary>
    /// Isolates the save-station electricity from Samus and the room layers, then sends its
    /// cartridge-selected spritemap, VRAM character data, and CGRAM palette through the
    /// production OBJ renderer. A nonzero result proves the effect is actually drawable;
    /// merely observing an allocated projectile slot is insufficient.
    /// </summary>
    private static int CountVisibleElectricityPixels(SuperMetroidRuntime runtime)
    {
        RoomEnemyProjectileSlot[] activeProjectiles = runtime.Enemies.EnemyProjectiles
            .Where(projectile => projectile.IsActive)
            .ToArray();
        if (activeProjectiles.Any(projectile =>
                projectile.Kind != RoomEnemyProjectileKind.SaveStationElectricity))
        {
            throw new InvalidDataException(
                "Save-room visual audit contains an unrelated enemy projectile, so its " +
                "isolated OBJ pixels cannot be attributed to the electricity effect.");
        }
        if (runtime.Enemies.RoomSpriteObjects.Any(sprite => sprite.IsActive))
        {
            throw new InvalidDataException(
                "Save-room visual audit contains an unrelated sprite object, so its " +
                "isolated OBJ pixels cannot be attributed to the electricity effect.");
        }
        if (activeProjectiles.Length == 0 || activeProjectiles.All(projectile =>
                projectile.SpritemapPointer is 0 or 0x8000))
        {
            return 0;
        }

        ScrollBoundaryCamera camera = runtime.Camera ?? throw new InvalidDataException(
            "Save-room visual audit lost its camera state.");
        var projectileOam = new OamBuffer();
        projectileOam.BeginFrame();
        runtime.Enemies.DrawEnemyProjectiles(
            projectileOam,
            camera.XPosition,
            camera.YPosition);
        projectileOam.FinalizeFrame();

        ResolvedObjFrame objects = SnesObjRenderer.RenderResolved(
            projectileOam,
            runtime.Vram,
            runtime.Cgram,
            obsel: 0x03);
        return objects.Pixels.Count(pixel =>
            pixel.A != 0 && (pixel.R != 0 || pixel.G != 0 || pixel.B != 0));
    }

    private static void StepMessageTo(
        SuperMetroidRuntime runtime,
        GameplayMessageBoxPhase phase,
        int maximumFrames)
    {
        for (int frame = 0; frame < maximumFrames && runtime.MessageBox.Phase != phase; frame++)
            runtime.StepFrame(0);
        if (runtime.MessageBox.Phase != phase)
        {
            throw new InvalidOperationException(
                $"Message box stalled in {runtime.MessageBox.Phase}, expected {phase}.");
        }
    }

    private static void WriteFrame(
        string outputDirectory,
        string fileName,
        SuperMetroidRuntime runtime) =>
        PngWriter.WriteRgba(
            Path.Combine(outputDirectory, fileName),
            FrontendFrame.Width,
            FrontendFrame.Height,
            SuperMetroidRuntimeFrameRenderer.Render(runtime));
}
