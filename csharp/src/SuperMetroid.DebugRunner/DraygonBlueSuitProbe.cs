using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

/// <summary>Controller-earned spark interrupted through the retail Draygon eye callback.</summary>
internal static class DraygonBlueSuitProbe
{
    public static int DeathFrame(int mode) => (mode & 1) == 0 ? 175 : 180;

    public static ushort InputAt(int frame, bool left, int mode)
    {
        if (mode >= 8 && frame >= 360)
            return frame == 360 ? (ushort)0x410 : frame < 370 ? (ushort)0x10 : frame == 370 ? (ushort)0x80 : (ushort)0x880;
        if (mode >= 4 && mode < 8 && frame >= 360 && frame < 370)
            return (ushort)(0x8000 | (left ? 0x200 : 0x100));
        if (frame >= 340 && frame < 350) return left ? (ushort)0x200 : (ushort)0x100;
        if (mode % 4 >= 2 && frame >= 141 && frame < 150) return 0;
        if (frame < 150) return TemporaryBlueCancellationInputs.At(frame, left, 0);
        return frame == 150 ? (ushort)0x80 : frame <= 185 ? (ushort)0x880 : (ushort)0;
    }

    public static void KillThroughEye(ISnesAddressSpace bus, SamusState samus)
    {
        // Only the boss/shot boundary is constructed. Samus's entire movement state
        // is the result of the preceding controller sequence and is never reseeded.
        var room = CartridgeRoomHeader.Load(bus, RoomHeaderPointers.Draygon);
        var assets = CartridgeRoomAssets.Load(bus, room);
        var vram = new SnesVram();
        var cgram = new SnesCgram();
        assets.LoadGraphics(vram, cgram);
        var random = new Bank80SystemState(0x4937);
        var enemies = new RoomEnemySystem();
        enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
            vram, cgram, random.NextRandom, random.SetRandomNumber,
            readRandomNumber: () => random.RandomNumber, level: assets.LevelData, samus: samus,
            isAreaBossDefeated: () => false, setAreaBossDefeated: () => { });
        var state = enemies.Draygon ?? throw new InvalidDataException("Missing Draygon.");
        var body = state.Body;
        body.XPosition = body.YPosition = 128;
        body.Health = 1;
        body.InvincibilityTimer = body.FlashTimer = body.AiHandlerBits = 0;
        body.SpritemapPointer = DraygonBlueSuitFixtureData.EyeHitSpritemap;
        body.Properties = 0;
        body.ExtraProperties = 4;
        // Frozen EnemyMain rebuilds the live collision list without advancing AI
        // or touching Samus, matching the list already present at native shot time.
        enemies.StepFrame(0, 0, timeIsFrozen: true);
        var projectiles = new SamusProjectileSystem();
        var shot = projectiles.Slots[0];
        shot.Type = SamusProjectileTypeWord.CreateBeam((ushort)SamusBeamFlags.Plasma, true);
        shot.Damage = 450;
        shot.XPosition = shot.YPosition = 128;
        shot.XRadius = shot.YRadius = 4;
        shot.InstructionPointer = DraygonBlueSuitFixtureData.UnexecutedProjectileProgram;
        shot.InstructionTimer = 100;
        enemies.ResolveOrdinaryProjectileHits(bus, projectiles, new SamusBombProjectileSystem(), samus);
        if (body.Health != 0 || state.Function != DraygonAiFunction.Dying)
            throw new InvalidDataException("Constructed eye hit did not enter the real fatal callback.");
    }

    public static void Verify(SamusState samus, int frame, int mode)
    {
        bool retained = mode % 4 >= 2;
        if (frame == DeathFrame(mode) && (samus.Shinespark.Phase != ShinesparkPhase.Inactive ||
            samus.Pose is not (SamusPoseIds.FacingRightNormalPose or SamusPoseIds.FacingLeftNormalPose) ||
            samus.Kinematics.VerticalSpeedFixed != 0 || samus.HorizontalSpeed.SpeedBoostCounter != 0x400))
            throw new InvalidDataException("Death did not interrupt the real spark while preserving its boost.");
        if (frame is 330 or 359 && (samus.ShinesparkPoseInputLocked ||
            samus.Kinematics.YFixed != 0x01ebffff ||
            samus.HorizontalSpeed.SpeedBoostCounter != (retained ? 0x400 : 0)))
            throw new InvalidDataException("Landing did not restore input and retain/cancel the earned boost correctly.");
        if (frame == 349 && samus.HorizontalSpeed.ContactDamageIndex != (retained ? 1 : 0))
            throw new InvalidDataException("Subsequent walking did not publish native Blue Suit contact damage.");
        if (mode is >= 4 and < 8 && frame == 361 && samus.HorizontalSpeed.SpeedBoostCounter != 1)
            throw new InvalidDataException("Dash did not replace persistent boost with a fresh run counter.");
        if (mode >= 8 && retained && frame == 360 && samus.Shinespark.ShineTimer != 179)
            throw new InvalidDataException("Crouching did not store the acquired Blue Suit.");
        if (mode >= 8 && retained && frame == 399 && samus.Shinespark.Phase != ShinesparkPhase.Vertical)
            throw new InvalidDataException("The acquired Blue Suit did not produce a second controller-launched spark.");
    }
}
