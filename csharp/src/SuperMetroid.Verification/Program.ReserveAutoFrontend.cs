using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Audio;
using SuperMetroid.Desktop;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifyReserveAutoFrontend()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(PowerBombRuntimeVerificationDefinitions.AlphaPowerBombRoomHeader);
        var samus = runtime.Samus!;
        samus.PoseId = SamusPoseId.FacingRightNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.Health = 0; samus.MaxHealth = 99;
        samus.ReserveEnergy = 2; samus.MaxReserveEnergy = 100; samus.ReserveTankMode = 1;
        var game = new SuperMetroidGame(bus);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        var recovery = (SamusReserveAutoRecoveryState)typeof(SuperMetroidGame)
            .GetField("reserveRecovery", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
        recovery.Begin(samus);
        runtime.GameplayTimeFrozen = true;
        runtime.BombProjectiles.SetSharedCooldown(10);
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!
            .SetValue(game, SuperMetroidGameState.ReserveTanksAuto);
        game.StepCaptured(0, 1, 1);
        AssertEqual(1, samus.Health, "automatic refill first frame");
        AssertTrue(samus.HealthWarning.IsActive, "native external health check starts warning during frozen refill");
        AssertTrue(runtime.TimeIsFrozen && samus.InputLocked, "nonfinal refill keeps native freeze and input lock");
        AssertTrue(runtime.LastGroundedSamusMovement is null, "nonfinal refill does not execute grounded movement");
        AssertEqual(10, runtime.BombProjectiles.CooldownTimer, "nonfinal refill freezes shared projectile clock");
        game.StepCaptured((ushort)SnesButton.Right, 2, 1);
        AssertEqual(2, samus.Health, "automatic refill completion frame");
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "completion restores outer gameplay state");
        AssertTrue(!runtime.TimeIsFrozen && !samus.InputLocked, "completion clears native freeze and input lock");
        AssertEqual(9, runtime.BombProjectiles.CooldownTimer,
            "completion unfreezes the gameplay projectile pass on the same frame");
        AssertTrue(runtime.LastGroundedSamusMovement is not null,
            "native $82:DC18 unfreezes BEFORE the completion frame gameplay/movement pass");
        var audio = (CartridgeAudioState)typeof(SuperMetroidGame).GetField("audio", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
        samus.HealthWarning.Update(99, audio);
        samus.Health = 0; samus.ReserveEnergy = 33;
        // Seed the display at the pre-transfer health. Each subsequent gameplay
        // pass queues its HUD update for the next accepted NMI, including while
        // automatic recovery freezes movement. Inspect presentation, not just HP.
        runtime.Hud.UpdateGameplayCounters(bus, samus);
        runtime.Hud.QueueUpload(bus, runtime.VramWrites);
        runtime.RunNmi(0, true);
        var digitImages = new Dictionary<int, Rgba32[]>();
        recovery.Begin(samus); runtime.GameplayTimeFrozen = true;
        typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.ReserveTanksAuto);
        var positions = (byte[])typeof(CartridgeAudioState).GetField("_soundWritePositions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
        var queues = (byte[,])typeof(CartridgeAudioState).GetField("_soundQueues", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
        for (int frame = 0; frame < 33; frame++)
        {
            // Drain the unrelated host transport between observations. Production
            // refill and warning producers still own every request being inspected.
            audio.Reset();
            var capture = game.StepCaptured(0, frame + 3, 1);
            AssertTrue(capture.Snapshot is not null, "automatic recovery publishes a gameplay render packet");
            var pixels = SoftwareFrameSnapshotRenderer.Render(capture.Snapshot!);
            foreach ((int index, int digit) in new[] { (70, frame / 10), (71, frame % 10) })
            {
                int address = 0x809dbf + digit * 2;
                ushort expected = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                AssertEqual(expected, runtime.Vram.ReadWord(HudState.VramDestination + index),
                    "automatic refill digit becomes visible on the following accepted NMI");
            }
            var onesPixels = Enumerable.Range(24, 8).SelectMany(y =>
                Enumerable.Range(56, 8).Select(x => pixels[y * 256 + x])).ToArray();
            if (digitImages.TryGetValue(frame % 10, out var prior))
                AssertTrue(prior.AsSpan().SequenceEqual(onesPixels), "repeated health digit has identical visible pixels");
            else
                digitImages.Add(frame % 10, onesPixels);
            AssertEqual(frame + 1, samus.Health, "full automatic refill progression");
            AssertEqual(frame < 30, samus.HealthWarning.IsActive, "warning switches off exactly at health 31");
            bool Has(byte sound) => Enumerable.Range(0, positions[2]).Any(i => queues[2, i] == sound);
            AssertEqual(frame == 0, Has(2), "warning start issued only once through real frontend");
            AssertEqual(frame == 30, Has(1), "warning stop issued on 30-to-31 refill crossing");
        }
        AssertEqual(10, digitImages.Count, "automatic refill visibly traverses all ten health digits");
        foreach (var left in digitImages)
            foreach (var right in digitImages.Where(pair => pair.Key > left.Key))
                AssertTrue(!left.Value.AsSpan().SequenceEqual(right.Value), "different health digits render differently during recovery");
        game.StepCaptured(0, 36, 1);
        AssertEqual(SuperMetroidGameState.MainGameplay, game.GameState, "presentation sequence returns to gameplay");
        foreach ((int index, int offset) in new[] { (8, 0), (9, 2), (40, 4), (41, 6), (72, 8), (73, 10) })
        {
            int address = 0x809997 + offset;
            ushort expected = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
            AssertEqual(expected, runtime.Vram.ReadWord(HudState.VramDestination + index),
                "completed automatic refill publishes the native empty AUTO indicator");
        }
        var fields = typeof(SamusState).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).OrderBy(f => f.MetadataToken).ToArray();
        var legacy = DebuggerStateFieldMigrations.SelectSerializedFields(typeof(SamusState), fields, fields.Length - 1);
        AssertTrue(legacy.SequenceEqual(fields.Where(f => f.Name != "_healthWarning")), "legacy Samus layout omits only warning owner");
        samus.HealthWarning.Update(30, audio);
        using var saved = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(saved, samus); saved.Position = 0;
        AssertTrue(DebuggerObjectGraphSerializer.Deserialize<SamusState>(saved).HealthWarning.IsActive,
            "Samus debugger graph retains warning latch");
        // Continue in actual state eight: ordinary beta must own the same latch,
        // while a generic locked handler must not acquire that responsibility.
        samus.Health = 31; audio.Reset(); game.StepCaptured(0, 40, 1);
        AssertTrue(!samus.HealthWarning.IsActive, "ordinary beta stops warning at healthy threshold");
        samus.InputLocked = true; samus.Health = 30;
        audio.Reset(); game.StepCaptured(0, 41, 1);
        AssertTrue(!samus.HealthWarning.IsActive, "locked handler does not run ordinary health check");
        samus.InputLocked = false;
        audio.Reset(); game.StepCaptured(0, 42, 1);
        AssertTrue(samus.HealthWarning.IsActive, "ordinary gameplay acquires critical warning after unlock");
        AssertTrue(Enumerable.Range(0, positions[2]).Any(i => queues[2, i] == 2), "ordinary gameplay publishes actual warning command");
        runtime.InitializePostCeresZebesRoom();
        AssertTrue(!runtime.Enemies.HasGunshipHealthHandler, "initial post-Ceres descent does not install command 1A");
        var ship = runtime.Enemies.Slots[0];
        ship.VariableF = GunshipCodePointers.WaitForEntranceToOpen;
        ship.VariableA = 100;
        samus.HealthWarning.Update(99, audio);
        samus.Health = 30; samus.InputLocked = true;
        audio.Reset(); game.StepCaptured(0, 43, 1);
        AssertTrue(runtime.Enemies.HasGunshipHealthHandler && samus.HealthWarning.IsActive,
            "gunship entry handler checks low health despite locked input");
        AssertTrue(Enumerable.Range(0, positions[2]).Any(i => queues[2, i] == 2), "gunship handler publishes warning audio");
        Console.WriteLine("Automatic reserve frontend: nonfinal freeze and same-frame completion movement pass.");
    }
}
