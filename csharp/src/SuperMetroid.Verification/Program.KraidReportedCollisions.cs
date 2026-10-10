using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    private static int VerifyKraidReportedCollisions(ISnesAddressSpace bus, SuperMetroidGame game,
        SuperMetroidRuntime runtime, RoomEnemySlot body, KraidEnemyState state)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var failures = new List<Exception>();
        void Check(string name, Action action)
        {
            try { action(); Console.WriteLine(name + ": PASS"); }
            catch (Exception error) { failures.Add(error); Console.WriteLine(name + ": " + error.Message); }
        }
        var samus = runtime.Samus!;
        samus.SelectedHudItem = 0;
        samus.EquippedBeams = samus.CollectedBeams = (ushort)SamusBeamFlags.Spazer;
        runtime.QueueGameplayBeamTilesAndLoadPalette(samus.EquippedBeams);
        foreach (bool closed in new[] { false, true })
        Check(closed ? "Uncharged Spazer closed-head impact" : "Uncharged Spazer open-head impact", () =>
        {
            runtime.Projectiles.Reset();
            samus.XPosition = 120; samus.YPosition = 491;
            body.XPosition = 176; body.YPosition = 592;
            body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = 100;
            body.VariableB = closed ? KraidHeadInstructionDefinitions.RoarInitial : (ushort)0x96ec;
            typeof(RoomEnemySystem).GetMethod("ExecuteKraidHeadInstruction", flags)!.Invoke(runtime.Enemies, new object[] { body, state });
            bool marked = false;
            bool spawned = false;
            for (int frame = 0; frame < 24; frame++)
            {
                game.Step(frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                foreach (var shot in runtime.Projectiles.Slots.Where(p => p.IsActive))
                {
                    spawned = true;
                    marked |= shot.PackedDirection.HasLowByteLifecycleState;
                    AssertEqual(true, shot.XPosition < 224, "Spazer must impact before crossing through Kraid's head");
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
            samus.XPosition = x; samus.YPosition = y;
            samus.Health = 999; samus.EquippedItems = 0;
            samus.InvincibilityTimer = samus.KnockbackTimer = 0;
            samus.HorizontalSpeed.ContactDamageIndex = 0;
            samus.Kinematics.ExtraXDisplacement = samus.Kinematics.ExtraYDisplacement = 0;
            samus.Kinematics.XRadius = 8; samus.Kinematics.YRadius = 16;
        }
        foreach (int index in new[] { 6, 7 })
        {
            Check("Nail contact slot " + index, () =>
            {
                var nail = runtime.Enemies.Slots[index];
                nail.XPosition = 100; nail.YPosition = 400;
                nail.Properties = nail.Properties.Without(EnemyProperties.IgnoreSamusCollision | EnemyProperties.Deleted | EnemyProperties.Invisible);
                ResetSamus(100, 400);
                ushort damage = nail.Definition.Damage;
                var interactive = (List<ushort>)typeof(RoomEnemySystem).GetField("_interactiveEnemyIndexes", flags)!.GetValue(runtime.Enemies)!;
                interactive.Clear(); interactive.Add(nail.NativeIndex);
                AssertEqual(true, runtime.Enemies.ResolveOrdinarySamusContact(samus, 0), "Nail callback is dispatched");
                AssertEqual((ushort)(999 - damage), samus.Health, "Nail applies native touch damage");
                AssertEqual(EnemyDefinitionId.Respawn, nail.EnemyDefinitionPointer,
                    "Nail dies into the native respawn placeholder");
            });
        }
        Check("Belly projectile leading edge", () =>
        {
            var lint = runtime.Enemies.Slots[2];
            lint.XPosition = 126; lint.YPosition = 400;
            lint.Properties = lint.Properties.Without(EnemyProperties.IgnoreSamusCollision);
            lint.VariableA = (ushort)KraidAiFunction.LintInactive;
            ResetSamus(100, 400);
            typeof(RoomEnemySystem).GetMethod("RunKraidLintMain", flags)!.Invoke(runtime.Enemies, new object?[] { lint, samus });
            AssertEqual((ushort)(999 - lint.Definition.Damage), samus.Health, "Lint leading edge damages Samus");
            AssertEqual(unchecked((ushort)-25), samus.Kinematics.ExtraXDisplacement, "Native complement-based lint push");
            AssertEqual(true, lint.Properties.HasAny(EnemyProperties.IgnoreSamusCollision), "Lint becomes intangible after contact");
        });
        Check("First closed head after rising", () =>
        {
            runtime.Projectiles.Reset();
            body.XPosition = 176; body.YPosition = 457;
            body.VariableA = (ushort)KraidAiFunction.RaiseBody;
            state.VulnerableMouthHitbox = state.InvulnerableMouthHitbox = 0;
            typeof(RoomEnemySystem).GetMethod("RunKraidRiseFunction", flags)!.Invoke(runtime.Enemies,
                new object?[] { body, state, samus });
            AssertEqual(KraidHeadInstructionDefinitions.RoarContinuation, body.VariableB, "Rise selects native next head cursor");
            var shot = runtime.Projectiles.Slots[0];
            shot.Type = 0x8004; shot.Damage = 40;
            shot.XPosition = 200; shot.YPosition = (ushort)(body.YPosition - 106);
            shot.XRadius = shot.YRadius = 8;
            typeof(SamusProjectileSystem).GetProperty(nameof(SamusProjectileSystem.ProjectileCounter))!
                .SetValue(runtime.Projectiles, (ushort)1);
            runtime.Enemies.ResolveKraidProjectileHits(bus, runtime.Projectiles, runtime.BombProjectiles);
            AssertEqual(true, shot.PackedDirection.HasLowByteLifecycleState,
                "Initial closed head blocks shots before the first head animation executes");
        });
        Check("Kraid body contact", () =>
        {
            body.XPosition = 176; body.YPosition = 592;
            body.VariableA = (ushort)KraidAiFunction.MainloopThinking;
            state.ThinkingTimer = 100;
            ResetSamus(180, 500);
            typeof(RoomEnemySystem).GetMethod("RunKraidBodyMain", flags)!.Invoke(runtime.Enemies,
                new object?[] { body, samus, (ushort)0, (ushort)384, null, null, null });
            AssertEqual((ushort)172, samus.XPosition, "Kraid ejects Samus eight pixels left");
            AssertEqual((ushort)492, samus.YPosition, "Kraid ejects Samus eight pixels up");
            AssertEqual((ushort)(999 - body.Definition.Damage), samus.Health, "Kraid body touch damage");
        });
        if (failures.Count != 0) throw new AggregateException("Kraid reported collisions", failures);
        return 0;
    }
}
