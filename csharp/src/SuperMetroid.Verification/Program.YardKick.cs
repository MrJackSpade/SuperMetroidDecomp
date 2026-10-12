using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyYardKickWords(SuperMetroidAddressSpace bus, CartridgeRoomHeader room)
    {
        var assets = LoadFixtureRoomAssets(bus, room);
        int cases = 0;
        foreach (SamusPoseId pose in new[] { SamusPoseId.FacingRightNormalPose, SamusPoseId.FacingLeftNormalPose })
        foreach (ushort whole in new ushort[] { 0, 1, 2, 15, 16, 32 })
        foreach (ushort fraction in new ushort[] { 0, 0x8000, 0xFFFF })
        {
            var enemies = new RoomEnemySystem { TileArtwork = FixtureEnemyTileArtwork() };
            enemies.Load(new RoomEnemyLoadRequest(bus, room.State.EnemyPopulationPointer, room.State.EnemyTilesetPointer, new SnesVram(), new SnesCgram(), new StandaloneRoomEnemyServices(() => 0)));
            var actor = enemies.Slots[0];
            var state = enemies.YardStates[0]!;
            var samus = new SamusState { Pose = pose, Health = 999, MaxHealth = 999,
                Position = FixedPosition<RoomPixels>.FromWhole(actor.Position.Whole) };
            samus.RefreshCollisionRadii(bus);
            enemies.StepFrame(new((ushort)(actor.Position.X.Whole - 128), (ushort)(actor.Position.Y.Whole - 112)),                 false, samus, level: assets.LevelData);
            // Seed the airborne touch-admission case. The real contact dispatcher
            // must install the kick and consume the published distance words.
            state.MovementFunction = YardMovementFunction.Airborne;
            state.Behavior = YardBehavior.NormalCrawling;
            samus.Position = samus.Position.WithWhole(actor.Position.Whole);
            samus.InvincibilityTimer = 0;
            samus.AbsoluteMovedLastFrameXFixed = ((uint)whole << 16) | fraction;
            AssertTrue(enemies.ResolveOrdinarySamusContact(samus, 0, assets.LevelData, actor.NativeIndex),
                "Yard kick enters through native contact dispatcher");
            bool left = pose == SamusPoseId.FacingLeftNormalPose;
            AssertEqual(left ? unchecked((ushort)-whole) : whole, state.AirborneVelocity.X.WholeWord, "kick whole word");
            AssertEqual(left ? unchecked((ushort)-fraction) : fraction, state.AirborneVelocity.X.Fraction, "kick fraction negates independently");
            int index = Math.Min((int)whole, 15);
            (ushort expectedFraction, ushort expectedWhole) = ReadNativeYardKickVerticalSpeed(bus, index);
            AssertEqual(expectedFraction, state.AirborneVelocity.Y.Fraction, "vertical kick table capped independently of horizontal speed");
            AssertEqual(expectedWhole, state.AirborneVelocity.Y.WholeWord, "vertical kick table whole word");
            AssertEqual(YardBehavior.Kicked, state.Behavior, "touch installs kicked behavior");
            cases++;
        }
        Console.WriteLine($"Yard kicks: {cases} contact-dispatch cases preserve signed words, fractions and vertical-table cap.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for the $A3:D517 Yard kick vertical-speed table")]
    private static (ushort Fraction, ushort Whole) ReadNativeYardKickVerticalSpeed(ISnesAddressSpace bus, int index) =>
        (RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xA3D517 + index * 4),
            RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), 0xA3D519 + index * 4));
}
