using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Kzan;

/// <summary>
/// Checks the plain half-pixel rise #1620 gave the Kzan against the original's subtraction with its borrow.
/// </summary>
internal static class KzanRiseStepVerification
{
    /// <summary>The values a sixteen-bit word takes.</summary>
    private const int WordRange = 65536;

    /// <summary>
    /// The production Kzan rise from every fraction at whole heights inside the room and at both ends of the coordinate
    /// space: each rise moves the top half a pixel up as the original's subtraction with its borrow does.
    /// </summary>
    internal static void VerifyKzanRiseStep()
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.KzanTop, slot.Definition);
        slot.SetSpawnParameter1(EnemyDefinitionId.KzanTop, 0);
        slot.SetSpawnParameter2(EnemyDefinitionId.KzanTop, 32 * 256);
        // The rising target lies sixteen pixels above the room's bottom edge, so every height checked keeps rising.
        slot.Position = slot.Position.WithWhole(new(128, 65520));
        KzanBehavior.InitializeKzanTop(slot);
        KzanEnemyState state = slot.RequireSpeciesState<KzanEnemyState>();
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        foreach (int whole in new[] { 0, 1, 16, 300, 32000, 65521, 65535 })
        for (int fraction = 0; fraction < WordRange; fraction++)
        {
            state.Action = KzanAction.Rising;
            slot.Position = slot.Position.WithY(new FixedCoordinate(whole, fraction));
            enemies.KzanBehavior.RunKzanTopMainAi(slot, frame);
            int lowered = fraction - 0x8000;
            int borrow = lowered < 0 ? 1 : 0;
            var expected = new FixedCoordinate((whole - borrow) & ushort.MaxValue, lowered & ushort.MaxValue);
            if (state.Action != KzanAction.Rising || slot.Position.Y != expected)
                throw new InvalidOperationException($"A Kzan rising from {whole}+{fraction:X4} reached {slot.Position.Y}, not {expected}.");
        }
        Console.WriteLine("  Kzan rise step: every fraction rises half a pixel with the original's borrow.");
    }
}
