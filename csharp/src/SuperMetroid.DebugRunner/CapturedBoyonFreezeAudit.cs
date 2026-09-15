using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Desktop;

/// <summary>Inspects and replays the release debugger state preserved for issue #524.</summary>
internal static class CapturedBoyonFreezeAudit
{
    public static int Run(string romPath, string statePath)
    {
        string directory = Path.Combine(Path.GetTempPath(), "sm-524-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        string copy = Path.Combine(directory, "SuperMetroid-debug-slot-0.smstate");
        File.Copy(statePath, copy);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        try
        {
            var store = new DebuggerSaveStateStore(romPath, bus.Rom, directory);
            DebuggerSaveStateLoadResult loaded = store.Load(0);
            Inspect(loaded);
            store.Save(1, loaded.AddressSpace, loaded.Game, loaded.AudioPlayer);
            SearchCapturedTrajectory(store);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }

        return 0;
    }

    private static void Inspect(DebuggerSaveStateLoadResult loaded)
    {
        var game = loaded.Game;
        var runtime = game.RuntimeForVerification ??
            throw new InvalidDataException("#524 state contains no gameplay runtime.");
        SamusState samus = runtime.Samus ??
            throw new InvalidDataException("#524 state contains no Samus actor.");
        if (game.GameplayActiveRoomPointer != BoyonAuditDefinitions.AlphaPowerBombRoomHeader)
        {
            throw new InvalidDataException(
                $"#524 state is room ${game.GameplayActiveRoomPointer:X4}, not the captured " +
                $"Alpha Power Bomb room ${BoyonAuditDefinitions.AlphaPowerBombRoomHeader:X4}.");
        }
        RoomEnemySlot[] boyons = runtime.Enemies.Slots
            .Where(slot => slot.EnemyDefinitionPointer == BoyonAuditDefinitions.BoyonEnemyDefinition)
            .ToArray();
        if (boyons.Length != 4)
            throw new InvalidDataException($"#524 state contains {boyons.Length} Boyons, expected four.");
        Console.WriteLine(
            $"state={game.GameState} frame={game.FrameNumber} room=${game.GameplayActiveRoomPointer:X4} " +
            $"area/room=${(byte?)game.GameplayActiveAreaIndex:X2}/${game.GameplayActiveRoomIndex:X2} " +
            $"Samus=(${samus.XPosition:X4}.{samus.Kinematics.XSubposition:X4}," +
            $"${samus.YPosition:X4}.{samus.Kinematics.YSubposition:X4}) pose=${samus.Pose:X2} " +
            $"beams=${samus.EquippedBeams:X4} input=${runtime.Controller1.Current:X4}/" +
            $"${runtime.Controller1.Previous:X4} bindings={runtime.ControllerBindings}");
        for (int index = 0; index < boyons.Length; index++)
        {
            RoomEnemySlot boyon = boyons[index];
            Console.WriteLine(
                $"Boyon[{index}] native={boyon.NativeIndex} position=(${boyon.XPosition:X4}." +
                $"{boyon.XSubposition:X4},${boyon.YPosition:X4}.{boyon.YSubposition:X4}) " +
                $"radii={boyon.XRadius}/{boyon.YRadius} frozen={boyon.FrozenTimer} " +
                $"map=${boyon.SpritemapPointer:X4} properties=${boyon.Properties:X4}");
        }
        foreach (SamusProjectileSlot projectile in runtime.Projectiles.Slots.Where(slot => slot.IsActive))
        {
            Console.WriteLine(
                $"Projectile[{projectile.SlotIndex}] type=${projectile.Type:X4} " +
                $"direction=${projectile.Direction:X4} position=(${projectile.XPosition:X4}." +
                $"{projectile.XSubposition:X4},${projectile.YPosition:X4}." +
                $"{projectile.YSubposition:X4}) velocity={projectile.XVelocity}/{projectile.YVelocity} " +
                $"radii={projectile.XRadius}/{projectile.YRadius} list=${projectile.InstructionPointer:X4}.");
        }
    }

    private static void SearchCapturedTrajectory(DebuggerSaveStateStore store)
    {
        ReplayDiagonalShot(store.Load(1), fromRight: false, fireFrame: 14);
        ReplayDiagonalShot(store.Load(1), fromRight: true, fireFrame: 8);
    }

    private static void ReplayDiagonalShot(
        DebuggerSaveStateLoadResult loaded,
        bool fromRight,
        int fireFrame)
    {
        var game = loaded.Game;
        var runtime = game.RuntimeForVerification!;
        SamusState samus = runtime.Samus!;
        if (fromRight)
        {
            // Keep the same captured room/enemy graph, but place the control between its
            // two Boyon pairs so the reported target is the first actor in the shot path.
            samus.XPosition = BoyonAuditDefinitions.RightSideControlSamusX;
            samus.Pose = SamusPoseIds.FacingLeftNormalPose;
            samus.RefreshCollisionRadii(loaded.AddressSpace);
            samus.InitializeAnimation(loaded.AddressSpace);
        }

        RoomEnemySlot front = runtime.Enemies.Slots.Single(slot =>
            slot.NativeIndex == BoyonAuditDefinitions.ForegroundBoyonNativeIndex);
        RoomEnemySlot target = runtime.Enemies.Slots.Single(slot =>
            slot.NativeIndex == BoyonAuditDefinitions.ReportedBoyonNativeIndex);
        if (front.XPosition != BoyonAuditDefinitions.ForegroundBoyonX ||
            target.XPosition != BoyonAuditDefinitions.ReportedBoyonX)
        {
            throw new InvalidDataException(
                $"#524 captured Boyon geometry changed: front=${front.XPosition:X4}, " +
                $"target=${target.XPosition:X4}.");
        }
        ushort frontBefore = front.FrozenTimer;
        ushort targetBefore = target.FrozenTimer;
        SamusProjectileSpawnSnapshot? shot = null;
        SamusProjectileSlot? contact = null;
        int contactFrame = -1;
        for (int frame = 0; frame < 60; frame++)
        {
            SnesButton input = frame == 0 ? SnesButton.None : SnesButton.A | SnesButton.L;
            if (frame == fireFrame) input |= SnesButton.X;
            game.Step((ushort)input);
            shot ??= runtime.Projectiles.LastFiredProjectileSnapshot;
            if (front.FrozenTimer > frontBefore || target.FrozenTimer > targetBefore)
            {
                contact = runtime.Projectiles.Slots.First(slot => slot.IsActive);
                contactFrame = frame;
                break;
            }
        }

        SamusProjectileDirection expectedDirection = fromRight
            ? SamusProjectileDirection.DownLeft
            : SamusProjectileDirection.DownRight;
        if (shot is null || (SamusProjectileDirection)shot.Value.Direction != expectedDirection ||
            contact is null)
        {
            throw new InvalidDataException(
                $"#524 {(fromRight ? "right" : "left")}-side control did not produce its " +
                $"${(ushort)expectedDirection:X2} diagonal Ice contact: shot={shot}, frame={contactFrame}.");
        }

        if (!fromRight && (front.FrozenTimer <= frontBefore || target.FrozenTimer != targetBefore))
        {
            throw new InvalidDataException(
                $"#524 left-side reproduction did not refresh the foreground Boyon alone: " +
                $"front={frontBefore}->{front.FrozenTimer}, target={targetBefore}->{target.FrozenTimer}.");
        }
        if (fromRight && target.FrozenTimer <= targetBefore)
        {
            throw new InvalidDataException(
                $"#524 right-side control did not freeze the reported target: " +
                $"{targetBefore}->{target.FrozenTimer}.");
        }

        Console.WriteLine(
            $"#524 {(fromRight ? "right" : "left")}-side diagonal shot: " +
            $"spawn=(${shot.Value.XPosition:X4},${shot.Value.YPosition:X4}), " +
            $"contact=(${contact.XPosition:X4},${contact.YPosition:X4}) on frame {contactFrame}, " +
            $"front=${front.XPosition:X4} freeze {frontBefore}->{front.FrozenTimer}, " +
            $"target=${target.XPosition:X4} freeze {targetBefore}->{target.FrozenTimer}.");
    }
}
