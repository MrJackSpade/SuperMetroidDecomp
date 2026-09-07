using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyChozoStatueState()
    {
        VerifyChozoHandRejectsStandingAndLivingBoss();
        var loaded = DebuggerFixtureLoader.Load("issue-353-gravity-chozo-hands", 9);
        var runtime = loaded.Game.RuntimeForVerification!;
        // Old states may contain a request published by the enemy initializer before
        // runtime integration existed. A normal frame must consume that pending work;
        // inspecting the saved terrain alone would only prove the historical defect.
        loaded.Game.Step(0);
        var level = runtime.LevelData!;
        var samus = runtime.Samus!;
        var hand = level.GetCollisionBlock(0x4a, 0x17);
        Console.WriteLine($"Saved Chozo setup: room={runtime.ActiveRoom!.Pointer:X4} " +
            $"Samus={samus.XPosition:X4},{samus.YPosition:X4} pose={samus.Pose:X2}; " +
            $"hand={hand.LevelWord:X4}/BTS={hand.Behavior:X2}; " +
            $"pending=[{string.Join(",", runtime.Enemies.ChozoStatuePlmRequests)}]");
        // $AA:E725 spawns $D6EE at this exact room block. Its synchronous $84:D616
        // setup writes B080 (solid special-air hand trigger), not a sprite hitbox.
        if (hand.CollisionType != RoomCollisionType.SpecialBlock || hand.Behavior != 0x80)
            throw new InvalidDataException("Saved Chozo hand lacks the cartridge-authored special collision trigger.");
        // Construct only the approach to the hand; all room data, boss flags, enemy
        // state, collision dispatch and subsequent frame owners remain the player's.
        samus.PoseId = SamusPoseId.MorphBallGroundRightPose;
        samus.InitializeAnimation(loaded.AddressSpace);
        samus.XPosition = 0x4a * 16 + 8;
        samus.YPosition = (ushort)(0x17 * 16 - samus.Kinematics.YRadius - 1);
        var collision = SamusBlockCollision.MoveVertical(loaded.AddressSpace, level,
            samus.Kinematics, 2 << 16, scanLeftToRight: true, canBreakBombBlocks: false,
            includeSolidEnemies: false, plms: runtime.Plms);
        if (!collision.Collided || collision.CollisionBlock?.Index != hand.Index ||
            runtime.Enemies.Slots[0].Parameter1 != 1 || runtime.GroundedSamusMovementEnabled)
            throw new InvalidDataException("Physical morph contact did not activate the Chozo and disable controls.");
        int soundFrames = 0;
        bool sawSlopes = false;
        int frame = 0;
        for (; frame < 3000 && !runtime.GroundedSamusMovementEnabled; frame++)
        {
            loaded.Game.Step(0);
            var statue = runtime.Enemies.Slots[0];
            int offset = runtime.Enemies.ChozoStatueStates[0]!.MovementTableOffset;
            // Compare the physical position on every carried frame with the native
            // hand-offset tables, not merely the final control-release flag.
            ushort expectedX = unchecked((ushort)(statue.XPosition +
                (short)ReadChozoWord(loaded.AddressSpace, 0xaae670 + offset)));
            ushort expectedY = unchecked((ushort)(statue.YPosition +
                (short)ReadChozoWord(loaded.AddressSpace, 0xaae6b0 + offset)));
            if (!runtime.GroundedSamusMovementEnabled &&
                (samus.XPosition != expectedX || samus.YPosition != expectedY))
                throw new InvalidDataException($"Chozo hand alignment failed on frame {frame}.");
            if (runtime.Enemies.SoundRequests.Count != 0) soundFrames++;
            sawSlopes |= level.GetCollisionBlockByIndex(0x1608 / 2).CollisionType == RoomCollisionType.Slope &&
                level.GetCollisionBlockByIndex(0x160a / 2).CollisionType == RoomCollisionType.Slope;
        }
        Console.WriteLine($"Chozo release f{frame}: Samus={samus.XPosition:X4},{samus.YPosition:X4} controls={runtime.GroundedSamusMovementEnabled}");
        if (!runtime.GroundedSamusMovementEnabled || samus.XPosition >= 0x200)
            throw new InvalidDataException("Chozo did not carry Samus across the room and release controls.");
        loaded.Game.Step(0);
        loaded.Game.Step(0);
        if (!sawSlopes || level.GetCollisionBlockByIndex(0x1608 / 2).CollisionType != RoomCollisionType.SpikeBlock ||
            level.GetCollisionBlockByIndex(0x160a / 2).CollisionType != RoomCollisionType.SpikeBlock)
            throw new InvalidDataException("Chozo access slopes were not opened and restored by their PLMs.");
        if (soundFrames is < 2 or > 100)
            throw new InvalidDataException($"Chozo sound requests were missing or repeated every frame: {soundFrames}.");
        Console.WriteLine($"Verified hand collision, per-frame carry alignment, slope restoration and {soundFrames} sound frames.");
    }

    private static ushort ReadChozoWord(SuperMetroid.Core.Hardware.SuperMetroidAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);

    private static void VerifyChozoHandRejectsStandingAndLivingBoss()
    {
        foreach (bool bossAlive in new[] { false, true })
        {
            var loaded = DebuggerFixtureLoader.Load("issue-353-gravity-chozo-hands", 9);
            var runtime = loaded.Game.RuntimeForVerification!;
            loaded.Game.Step(0);
            if (bossAlive) runtime.System.ClearBossBits(AreaId.WreckedShip, BossBits.AreaBoss);
            var samus = runtime.Samus!;
            samus.PoseId = bossAlive ? SamusPoseId.MorphBallGroundRightPose : SamusPoseId.FacingRightNormalPose;
            samus.InitializeAnimation(loaded.AddressSpace);
            samus.XPosition = 0x4a * 16 + 8;
            samus.YPosition = (ushort)(0x17 * 16 - samus.Kinematics.YRadius - 1);
            var result = SamusBlockCollision.MoveVertical(loaded.AddressSpace, runtime.LevelData!,
                samus.Kinematics, 2 << 16, scanLeftToRight: true, canBreakBombBlocks: false,
                includeSolidEnemies: false, plms: runtime.Plms);
            if (!result.Collided || runtime.Enemies.Slots[0].Parameter1 != 0 ||
                !runtime.GroundedSamusMovementEnabled)
                throw new InvalidDataException($"Chozo hand admitted invalid contact: bossAlive={bossAlive}.");
        }
    }
}
