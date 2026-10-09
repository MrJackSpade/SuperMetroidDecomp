using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: the head enemy keeps InstList_MotherBrainHead_InitialDummy for life, so its
    // extended spritemap is $A9:A320 and its hitbox Hitbox_MotherBrainBody_0 (-$14,-$15 to
    // +$10,+$17). The visible brain runs a separate list in the draw hook. The port ran the
    // brain list on the head enemy, testing the drawn frame's shape instead: in the 100%
    // movie a charged beam that native lands on the head missed it.
    private static void VerifyMotherBrainHeadHitbox()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(FixtureRoomHeaders.MotherBrain);
        MotherBrainEnemyState state = runtime.Enemies.MotherBrain
            ?? throw new InvalidOperationException("Mother Brain's room has no encounter state.");
        RoomEnemySlot head = state.Head ?? throw new InvalidOperationException("Mother Brain has no head enemy.");
        AssertEqual((ushort)0x9c21, state.BrainInstructionPointer, "the brain list starts at InstList_MotherBrainHead_Initial");

        typeof(RoomEnemySystem).GetMethod("ProcessInstructions", flags)!
            .Invoke(runtime.Enemies, [head, null, null, (ushort)0, (ushort)0, (ushort)0]);
        AssertEqual((ushort)0xa320, head.SpritemapPointer, "the head enemy carries the dummy list's hitbox frame");

        // The movie's frame: second form, head at ($7C,$58) when the beam at ($9E,$81) reaches
        // it. A 16-pixel radius test misses by one pixel (41 > 40); the hitbox overlaps.
        state.Form = 2;
        head.Health = 18000;
        head.XPosition = 0x007c;
        head.YPosition = 0x0058;
        head.InvincibilityTimer = 0;
        runtime.Enemies.PrepareEnemyProcessingList(0x0000, 0x0000);
        var shot = runtime.Projectiles.Slots[0];
        shot.Type = 0x901b; shot.Damage = 900; shot.Direction = 8;
        shot.XPosition = 0x009e; shot.YPosition = 0x0081; shot.XRadius = shot.YRadius = 24;
        shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
        runtime.Enemies.ResolveOrdinaryProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles, runtime.Samus!);
        AssertEqual((ushort)17100, head.Health, "the charged beam lands on the head's native hitbox");
        Console.WriteLine("Mother Brain head hitbox: the head enemy keeps $A320; the brain list runs separately.");
    }
}
