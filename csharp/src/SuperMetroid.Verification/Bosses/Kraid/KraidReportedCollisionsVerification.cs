using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>Checks reported Kraid head, nail, lint, and body collision behavior.</summary>
internal static class KraidReportedCollisionsVerification
{
    /// <summary>Runs each reported contact path and collects failures before leaving the verifier.</summary>
    internal static int VerifyKraidReportedCollisions(ISnesAddressSpace bus, SuperMetroidGame game,
        SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState state)
    {
        var failures = new List<Exception>();
        void Check(string name, Action action)
        {
            try { action(); Console.WriteLine(name + ": PASS"); }
            catch (Exception error) { failures.Add(error); Console.WriteLine(name + ": " + error.Message); }
        }
        var samus = runtime.Player.IntroducedSamus;
        samus.SelectedHudItem = HudItem.Beam;
        samus.EquippedBeams = samus.CollectedBeams = SamusBeamFlags.Spazer;
        runtime.SamusPhases.UpdateGameplayBeamTilesAndPalette(samus.EquippedBeams);
        foreach (bool closed in new[] { false, true })
        Check(closed ? "Uncharged Spazer closed-head impact" : "Uncharged Spazer open-head impact", () =>
        {
            runtime.Projectiles.Reset();
            samus.Position = samus.Position.WithWhole(new(120, 491));
            body.Position = body.Position.WithWhole(new(176, 592));
            state.Parts[0].Function = KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = WrappingCountdown.Start(100);
            // The closed case starts the roar; the open case stands just past the roar cry, on the fully open pose.
            state.HeadProgram = closed
                ? AnimationProgramCursor<KraidHeadProgram>.Start(KraidHeadProgram.Roar)
                : new AnimationProgramCursor<KraidHeadProgram>(KraidHeadProgram.Roar, 4);
            runtime.Enemies.KraidBehavior.Head.ExecuteKraidHeadInstruction(body, state);
            bool marked = false;
            bool spawned = false;
            for (int frame = 0; frame < 24; frame++)
            {
                game.Step(frame == 0 ? SnesButton.X : 0);
                foreach (var shot in runtime.Projectiles.Slots.Where(p => p.IsActive))
                {
                    spawned = true;
                    marked |= shot.PackedDirection.HasLowByteLifecycleState;
                    AssertEqual(true, shot.Position.X.Whole < 224, "Spazer must impact before crossing through Kraid's head");
                }
            }
            AssertEqual(true, spawned, "Production firing spawned Spazer");
            AssertEqual(true, marked, "Head collision marks Spazer for removal");
            AssertEqual(false, runtime.Projectiles.Slots.Any(p => p.IsActive), "Impacted Spazer is removed");
            AssertEqual(1000, body.Health, "Uncharged Spazer does not damage Kraid");
        });
        runtime.Projectiles.Reset();
        void ResetSamus(ushort x, ushort y)
        {
            samus.Position = samus.Position.WithWhole(new(x, y));
            samus.Health = 999; samus.EquippedItems = 0;
            samus.KnockbackTimer = WrappingCountdown.Start(0);
            samus.InvincibilityTimer = WrappingCountdown.Start(0);
            samus.HorizontalSpeed.ContactDamageIndex = SamusContactDamageIndex.None;
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWhole(new(0, 0));
            samus.Kinematics.Radius = new(8, 16);
        }
        foreach (int index in new[] { 6, 7 })
        {
            Check("Nail contact slot " + index, () =>
            {
                var nail = runtime.Enemies.SlotTable.Slots[index];
                nail.Position = nail.Position.WithWhole(new(100, 400));
                nail.Properties = nail.Properties.Without(EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted | EnemyProperties.Invisible);
                ResetSamus(100, 400);
                ushort damage = nail.Definition.Damage;
                var interactive = (List<int>)runtime.Enemies.ProcessingWindow.InteractiveEnemyIndexes;
                interactive.Clear(); interactive.Add(nail.EnemyIndex);
                AssertEqual(true, runtime.Enemies.Collision.ResolveOrdinarySamusContact(samus, 0), "Nail callback is dispatched");
                AssertEqual((ushort)(999 - damage), samus.Health, "Nail applies native touch damage");
                AssertEqual(EnemyDefinitionId.Respawn, nail.DefinitionId,
                    "Nail dies into the native respawn placeholder");
            });
        }
        Check("Belly projectile leading edge", () =>
        {
            var lint = runtime.Enemies.SlotTable.Slots[2];
            lint.Position = lint.Position.WithWhole(new(126, 400));
            lint.Properties = lint.Properties.Without(EnemyProperties.IgnoreSamusCollision);
            state.Parts[lint.SlotIndex].Function = KraidAiFunction.LintInactive;
            ResetSamus(100, 400);
            runtime.Enemies.KraidBehavior.Projectiles.RunKraidLintMain(lint, samus);
            AssertEqual((ushort)(999 - lint.Definition.Damage), samus.Health, "Lint leading edge damages Samus");
            AssertEqual((short)-25, samus.Kinematics.ExtraDisplacement.X.WholePixels, "Native complement-based lint push");
            AssertEqual(true, lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "Lint becomes intangible after contact");
        });
        Check("First closed head after rising", () =>
        {
            runtime.Projectiles.Reset();
            body.Position = body.Position.WithWhole(new(176, 457));
            state.Parts[0].Function = KraidAiFunction.RaiseBody;
            state.DisplayedHeadFrame = null;
            runtime.Enemies.KraidBehavior.Rise.RunKraidRiseFunction(body, state, samus);
            AssertEqual((AnimationProgramCursor<KraidHeadProgram>?)KraidHeadAnimations.RoarAfterEntryFrame, state.HeadProgram, "Rise selects native next head cursor");
            var shot = runtime.Projectiles.Slots[0];
            shot.Type = 0x8004; shot.Damage = 40;
            shot.Position = shot.Position.WithWhole(new(200, (ushort)(body.Position.Y.Whole - 106)));
            shot.Radius = new(8, 8);
            runtime.Projectiles.SlotTable.ProjectileCounter = 1;
            runtime.Enemies.ResolveKraidProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles);
            AssertEqual(true, shot.PackedDirection.HasLowByteLifecycleState,
                "Initial closed head blocks shots before the first head animation executes");
        });
        Check("Kraid body contact", () =>
        {
            body.Position = body.Position.WithWhole(new(176, 592));
            state.Parts[0].Function = KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = WrappingCountdown.Start(100);
            ResetSamus(180, 500);
            runtime.Enemies.KraidBehavior.RunKraidBodyMain(body, samus, new Position16<RoomPixels>(0, 384), null, null);
            AssertEqual((ushort)172, samus.Position.X.Whole, "Kraid ejects Samus eight pixels left");
            AssertEqual((ushort)492, samus.Position.Y.Whole, "Kraid ejects Samus eight pixels up");
            AssertEqual((ushort)(999 - body.Definition.Damage), samus.Health, "Kraid body touch damage");
        });
        Check("Kraid body ejection limit", () =>
        {
            // The same contact as above, with the limit below the eight-pixel push: Samus stops at the limit's Y.
            Position16<RoomPixels> smallFormLimit = state.EjectionLimit;
            state.EjectionLimit = new Position16<RoomPixels>(0, 496);
            body.Position = body.Position.WithWhole(new(176, 592));
            state.Parts[0].Function = KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = WrappingCountdown.Start(100);
            ResetSamus(180, 500);
            runtime.Enemies.KraidBehavior.RunKraidBodyMain(body, samus, new Position16<RoomPixels>(0, 384), null, null);
            state.EjectionLimit = smallFormLimit;
            AssertEqual((ushort)172, samus.Position.X.Whole, "Kraid still ejects Samus eight pixels left");
            AssertEqual((ushort)496, samus.Position.Y.Whole, "Kraid's upward push stops at the ejection limit's Y");
        });
        if (failures.Count != 0) throw new AggregateException("Kraid reported collisions", failures);
        return 0;
    }
}
