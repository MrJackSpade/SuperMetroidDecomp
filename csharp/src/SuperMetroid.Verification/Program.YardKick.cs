using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyYardKickWords(SuperMetroidAddressSpace bus, CartridgeRoomHeader room)
    {
        var assets = CartridgeRoomAssets.Load(bus, room);
        int cases = 0;
        foreach (byte pose in new[] { SamusPoseIds.FacingRightNormalPose, SamusPoseIds.FacingLeftNormalPose })
        foreach (ushort whole in new ushort[] { 0, 1, 2, 15, 16, 32 })
        foreach (ushort fraction in new ushort[] { 0, 0x8000, 0xFFFF })
        {
            var enemies = new RoomEnemySystem();
            enemies.Load(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer,
                new SnesVram(), new SnesCgram(), () => 0);
            var actor = enemies.Slots[0];
            var state = enemies.YardStates[0]!;
            var samus = new SamusState { Pose = pose, Health = 999, MaxHealth = 999,
                XPosition = actor.XPosition, YPosition = actor.YPosition };
            samus.RefreshCollisionRadii(bus);
            enemies.StepFrame((ushort)(actor.XPosition - 128), (ushort)(actor.YPosition - 112),
                false, samus, level: assets.LevelData);
            // Seed the airborne touch-admission case. The real contact dispatcher
            // must install the kick and consume the published distance words.
            state.MovementFunction = YardMovementFunction.Airborne;
            state.Behavior = 0;
            samus.XPosition = actor.XPosition;
            samus.YPosition = actor.YPosition;
            samus.InvincibilityTimer = 0;
            samus.AbsoluteMovedLastFrameXFixed = ((uint)whole << 16) | fraction;
            AssertTrue(enemies.ResolveOrdinarySamusContact(samus, 0, assets.LevelData, actor.NativeIndex),
                "Yard kick enters through native contact dispatcher");
            bool left = pose == SamusPoseIds.FacingLeftNormalPose;
            AssertEqual(left ? unchecked((ushort)-whole) : whole, state.AirborneXVelocity, "kick whole word");
            AssertEqual(left ? unchecked((ushort)-fraction) : fraction, state.AirborneXSubvelocity, "kick fraction negates independently");
            int index = Math.Min((int)whole, 15);
            ushort expectedFraction = SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(bus, 0xA3D517 + index * 4);
            ushort expectedWhole = SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(bus, 0xA3D519 + index * 4);
            AssertEqual(expectedFraction, state.AirborneYSubvelocity, "vertical kick table capped independently of horizontal speed");
            AssertEqual(expectedWhole, state.AirborneYVelocity, "vertical kick table whole word");
            AssertEqual((ushort)4, state.Behavior, "touch installs kicked behavior");
            cases++;
        }
        Console.WriteLine($"Yard kicks: {cases} contact-dispatch cases preserve signed words, fractions and vertical-table cap.");
    }
}
