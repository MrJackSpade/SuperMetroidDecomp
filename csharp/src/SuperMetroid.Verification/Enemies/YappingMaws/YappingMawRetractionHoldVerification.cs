using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>
/// Checks whether a Yapping Maw carries the Samus it holds on the update it finishes retracting (#1620) against the
/// original attack's end, which moves her only for a reach down whose bite's sector, counted clockwise from straight up
/// in two-step entries, is the straight-right entry four or the straight-left entry twelve.
/// </summary>
internal static class YappingMawRetractionHoldVerification
{
    /// <summary>Where the held Samus stands before the retraction's last update, away from any mouth.</summary>
    private static readonly Position16<RoomPixels> EarlierHold = new(1000, 900);

    /// <summary>
    /// For both reaches (the second parameter zero, reaching down, and one, reaching up) and every bite sector, a maw
    /// holding Samus runs its retraction's last update through the production main update: it starts its cooldown, and
    /// Samus stands at its mouth plus her held offset when the original's branches move her, and where she was otherwise.
    /// The rule itself is compared over the same inputs.
    /// </summary>
    internal static void VerifyYappingMawRetractionHold()
    {
        int row = Enumerable.Range(0, EnemyQuadraticSpeedDefinitions.RowCount).First(candidate =>
            EnemyQuadraticSpeeds.Row(checked((ushort)candidate)).Negative.RawValue < 0);
        int retract = EnemyQuadraticSpeeds.Row(checked((ushort)row)).Negative.RawValue;
        foreach (int reachParameter in new[] { 0, 1 })
        for (int sector = 0; sector < 8; sector++)
        {
            YappingMawAim aim = YappingMawAnimationDefinitions.Aims.At(sector);
            bool moved = reachParameter == 0 && (sector * 2 is 4 or 12);
            string context = $"Yapping Maw with second parameter {reachParameter} that bit {aim}";
            VerticalDirection reach = reachParameter == 0 ? VerticalDirection.Down : VerticalDirection.Up;
            AssertEqual(moved, YappingMawRetractionHold.CarriesHeldSamus(reach, aim is YappingMawAim.Right or YappingMawAim.Left),
                $"{context}: the rule");

            (RoomEnemySystem enemies, RoomEnemySlot slot, YappingMawEnemyState state) = NewYappingMaw(reachParameter);
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState());
            samus.Position = samus.Position.WithWhole(new(128 + 300, 128));
            var frame = new ClockedEnemyAiFrame(
                RoomEnemyFrameFixtures.Standalone(RoomEnemyFrameFixtures.OpenRoom(new(64, 64)), 0) with { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = samus },
                new FrameCounters(0));
            enemies.YappingMawBehavior.RunYappingMawMainAi(slot, frame);
            enemies.YappingMawBehavior.RunYappingMawMainAi(slot, frame);
            AssertEqual(YappingMawAiFunction.ExtendingOrRetracting, state.Function, $"{context}: setup begins the extension");

            state.Aim = aim;
            state.Extension = SixteenSixteenWords.ToDisplacement(-retract - 1);
            state.QuadraticSpeedRow = EnemyQuadraticSpeedIndex.FromRows(row);
            state.QuadraticSpeedSign = EnemySpeedSign.Negative;
            state.HasGrabbedSamus = true;
            state.HeldSamusOffset = new Offset16<RoomPixels>(0, -20);
            samus.Position = samus.Position.WithWhole(EarlierHold);
            enemies.YappingMawBehavior.RunYappingMawMainAi(slot, frame);

            AssertEqual(YappingMawAiFunction.RetractedDelay, state.Function, $"{context}: the curl below nothing ends the retraction");
            AssertEqual(moved ? slot.Position.Whole + state.HeldSamusOffset : EarlierHold, samus.Position.Whole,
                $"{context}: where Samus stands after the retraction's last update");
        }
        Console.WriteLine("  Yapping Maw retraction hold: both reaches and all eight bites carry or leave the held Samus as the original's attack end.");
    }

    /// <summary>A Yapping Maw rooted at (128, 128) through the production initializer, reaching as <paramref name="reachParameter"/> says.</summary>
    private static NewInitializedYappingMawResult NewYappingMaw(int reachParameter)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.YappingMaw, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.Position = slot.Position.WithWhole(new(128, 128));
        slot.SetSpawnParameter1(EnemyDefinitionId.YappingMaw, 301);
        slot.SetSpawnParameter2(EnemyDefinitionId.YappingMaw, reachParameter);
        enemies.YappingMawBehavior.InitializeYappingMaw(slot);
        YappingMawEnemyState state = enemies.YappingMawStates[0] ?? throw new InvalidOperationException("Yapping Maw has no state.");
        return new NewInitializedYappingMawResult(enemies, slot, state);
    }
}
