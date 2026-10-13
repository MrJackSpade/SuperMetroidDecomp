using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static class KraidReportedCollisionsVerification
{
    internal static int VerifyKraidReportedCollisions(ISnesAddressSpace bus, SuperMetroidGame game,
        SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState state)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var failures = new List<Exception>();
        void Check(string name, Action action)
        {
            try { action(); Console.WriteLine(name + ": PASS"); }
            catch (Exception error) { failures.Add(error); Console.WriteLine(name + ": " + error.Message); }
        }
        var samus = runtime.Player.Samus!;
        samus.SelectedHudItem = 0;
        samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Spazer;
        runtime.SamusPhases.QueueGameplayBeamTilesAndLoadPalette(samus.EquippedBeams);
        foreach (bool closed in new[] { false, true })
        Check(closed ? "Uncharged Spazer closed-head impact" : "Uncharged Spazer open-head impact", () =>
        {
            runtime.Projectiles.Reset();
            samus.Position = samus.Position.WithWhole(new(120, 491));
            body.Position = body.Position.WithWhole(new(176, 592));
            body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = NativeCountdown.Start(100);
            body.VariableB = closed ? KraidHeadInstructionDefinitions.RoarInitial : (ushort)0x96ec;
            runtime.Enemies.KraidBehavior.ExecuteKraidHeadInstruction(body, state);
            bool marked = false;
            bool spawned = false;
            for (int frame = 0; frame < 24; frame++)
            {
                game.Step(frame == 0 ? (ushort)SnesButton.X : (ushort)0);
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
            AssertEqual((ushort)1000, body.Health, "Uncharged Spazer does not damage Kraid");
        });
        runtime.Projectiles.Reset();
        void ResetSamus(ushort x, ushort y)
        {
            samus.Position = samus.Position.WithWhole(new(x, y));
            samus.Health = 999; samus.EquippedItems = 0;
            samus.KnockbackTimer = NativeCountdown.Start(0);
            samus.InvincibilityTimer = NativeCountdown.Start(0);
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
                var interactive = (List<ushort>)runtime.Enemies.ProcessingWindow._interactiveEnemyIndexes;
                interactive.Clear(); interactive.Add(nail.NativeIndex);
                AssertEqual(true, runtime.Enemies.Collision.ResolveOrdinarySamusContact(samus, 0), "Nail callback is dispatched");
                AssertEqual((ushort)(999 - damage), samus.Health, "Nail applies native touch damage");
                AssertEqual(EnemyDefinitionId.Respawn, nail.EnemyDefinitionPointer,
                    "Nail dies into the native respawn placeholder");
            });
        }
        Check("Belly projectile leading edge", () =>
        {
            var lint = runtime.Enemies.SlotTable.Slots[2];
            lint.Position = lint.Position.WithWhole(new(126, 400));
            lint.Properties = lint.Properties.Without(EnemyProperties.IgnoreSamusCollision);
            lint.VariableA = (ushort)KraidAiFunction.LintInactive;
            ResetSamus(100, 400);
            runtime.Enemies.KraidBehavior.RunKraidLintMain(lint, samus);
            AssertEqual((ushort)(999 - lint.Definition.Damage), samus.Health, "Lint leading edge damages Samus");
            AssertEqual((short)-25, samus.Kinematics.ExtraDisplacement.X.WholePixels, "Native complement-based lint push");
            AssertEqual(true, lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "Lint becomes intangible after contact");
        });
        Check("First closed head after rising", () =>
        {
            runtime.Projectiles.Reset();
            body.Position = body.Position.WithWhole(new(176, 457));
            body.VariableA = (ushort)KraidAiFunction.RaiseBody;
            state.VulnerableMouthHitbox = state.InvulnerableMouthHitbox = 0;
            runtime.Enemies.KraidBehavior.RunKraidRiseFunction(body, state, samus);
            AssertEqual(KraidHeadInstructionDefinitions.RoarContinuation, body.VariableB, "Rise selects native next head cursor");
            var shot = runtime.Projectiles.Slots[0];
            shot.Type = 0x8004; shot.Damage = 40;
            shot.Position = shot.Position.WithWhole(new(200, (ushort)(body.Position.Y.Whole - 106)));
            shot.Radius = new(8, 8);
            runtime.Projectiles.SlotTable.ProjectileCounter = (ushort)1;
            runtime.Enemies.ResolveKraidProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles);
            AssertEqual(true, shot.PackedDirection.HasLowByteLifecycleState,
                "Initial closed head blocks shots before the first head animation executes");
        });
        Check("Kraid body contact", () =>
        {
            body.Position = body.Position.WithWhole(new(176, 592));
            body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = NativeCountdown.Start(100);
            ResetSamus(180, 500);
            runtime.Enemies.KraidBehavior.RunKraidBodyMain(body, samus, new Position16<RoomPixels>((ushort)0, (ushort)384), null, null, null);
            AssertEqual((ushort)172, samus.Position.X.Whole, "Kraid ejects Samus eight pixels left");
            AssertEqual((ushort)492, samus.Position.Y.Whole, "Kraid ejects Samus eight pixels up");
            AssertEqual((ushort)(999 - body.Definition.Damage), samus.Health, "Kraid body touch damage");
        });
        if (failures.Count != 0) throw new AggregateException("Kraid reported collisions", failures);
        return 0;
    }
}
