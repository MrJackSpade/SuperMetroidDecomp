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
    /// The production Kzan rise from every fraction at whole heights from the room's top row to below the lowest Kzan
    /// placement: each rise moves the top half a pixel up as the original's subtraction with its borrow does. Every Kzan
    /// rises back to its spawn height inside the room (#2438), so the heights checked lie in the room as well.
    /// </summary>
    internal static void VerifyKzanRiseStep()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        place.Bind(EnemyDefinitionId.KzanTop, place.Definition);
        place.SetSpawnParameter1(EnemyDefinitionId.KzanTop, 0);
        place.SetSpawnParameter2(EnemyDefinitionId.KzanTop, 32 * 256);
        // The rising target lies on the room's top edge, so every height checked keeps rising.
        place.Position = place.Position.WithWhole(new(128, 0));
        KzanBehavior.InitializeKzanTop(place);
        KzanEnemyState state = SuperMetroid.Core.Enemies.EnemyPartners.Listed<KzanTopEntity>(place).State;
        var frame = new ClockedEnemyAiFrame(
            RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false },
            new FrameCounters(0));
        foreach (int whole in new[] { 1, 16, 96, 300, 1368, 1500 })
        for (int fraction = 0; fraction < WordRange; fraction++)
        {
            state.Action = KzanAction.Rising;
            place.Position = place.Position.WithY(new FixedCoordinate(whole, fraction));
            enemies.KzanBehavior.RunKzanTopMainAi(place, frame);
            int lowered = fraction - 0x8000;
            int borrow = lowered < 0 ? 1 : 0;
            var expected = new FixedCoordinate(whole - borrow, lowered & ushort.MaxValue);
            if (state.Action != KzanAction.Rising || place.Position.Y != expected)
                throw new InvalidOperationException($"A Kzan rising from {whole}+{fraction:X4} reached {place.Position.Y}, not {expected}.");
        }
        Console.WriteLine("  Kzan rise step: every fraction rises half a pixel with the original's borrow.");
    }
}
