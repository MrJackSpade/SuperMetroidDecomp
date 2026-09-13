using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>#422: real item selection with and without the active Power Bomb sound guard.</summary>
internal static class PowerBombSoundSuppressionAudit
{
    public static int Run(string rom)
    {
        PowerBombImpactSoundAudit.Run(rom);
        PowerBombProjectileSoundAudit.Run(rom);
        VerifyBombExplosionPublication(rom);
        VerifySameFrameBombOrdering(rom);
        foreach (bool exploding in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            runtime.Samus!.Missiles = runtime.Samus.MaxMissiles = 5;
            runtime.Samus.SelectedHudItem = 0;
            if (exploding)
            {
                runtime.BombProjectiles.PowerBombExplosion.Arm();
                runtime.BombProjectiles.PowerBombExplosion.Spawn(runtime.Samus.XPosition, runtime.Samus.YPosition);
            }
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
                .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            var result = game.Step((ushort)SnesButton.Select);
            bool played = result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(1, SoundEffectLibrary1Sounds.HudWeaponSelect.Value));
            Console.WriteLine($"PowerBomb={exploding} status={runtime.PowerBombExplosionStatus:X4} selected={runtime.Samus.SelectedHudItem} selectionSound={played}");
            if (runtime.Samus.SelectedHudItem != 1)
                throw new InvalidDataException("Control fixture failed to select missiles.");
            if (played == exploding)
                throw new InvalidDataException("Frontend item selection violated the native active-Power-Bomb sound suppression guard.");
        }
        foreach (bool suppressedAtRequest in new[] { false, true })
        foreach (bool explodingAtPublication in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            runtime.Samus!.MaxMissiles = runtime.Samus.Missiles = 5;
            runtime.Samus.SelectedHudItem = 1;
            runtime.Hud.UpdateGameplayCounters(bus, runtime.Samus,
                soundSuppressed: suppressedAtRequest);
            if (explodingAtPublication)
            {
                runtime.BombProjectiles.PowerBombExplosion.Arm();
                runtime.BombProjectiles.PowerBombExplosion.Spawn(runtime.Samus.XPosition, runtime.Samus.YPosition);
            }
            var audio = new CartridgeAudioState();
            new GameplayAudioFramePublication(audio).PublishPrefix(runtime);
            if (audio.HasQueuedSounds == suppressedAtRequest)
                throw new InvalidDataException("Deferred HUD publication used final explosion status instead of producer-time suppression.");
            Console.WriteLine($"HUD request suppressed={suppressedAtRequest}, publication explosion={explodingAtPublication}: queue admission correct");
        }
        return 0;
    }

    private static void VerifyBombExplosionPublication(string rom)
    {
        foreach ((bool powerBombActive, bool plantPowerBomb) in new[] { (false, false), (true, false), (false, true) })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.PowerBombs = samus.MaxPowerBombs = 5;
            samus.SelectedHudItem = plantPowerBomb ? (ushort)3 : (ushort)0;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            var planted = runtime.BombProjectiles.StepFrame(bus, runtime.LevelData!, samus,
                (ushort)SnesButton.X, (ushort)SnesButton.X);
            if (planted.PlacedSlot is not int index)
                throw new InvalidDataException("Bomb suppression fixture did not plant a bomb.");
            // Shorten only the fuse: placement supplied the real instruction list and slot.
            runtime.BombProjectiles.Slots[index].BombTimer = 1;
            if (powerBombActive)
            {
                runtime.BombProjectiles.PowerBombExplosion.Arm();
                runtime.BombProjectiles.PowerBombExplosion.Spawn(samus.XPosition, samus.YPosition);
            }
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
                .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            var result = game.Step(0);
            var expected = plantPowerBomb ? SoundEffectLibrary1Sounds.PowerBombExplosion : SoundEffectLibrary2Sounds.BombExplosion;
            bool played = result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(plantPowerBomb ? (byte)1 : (byte)2, expected.Value));
            Console.WriteLine($"Bomb fuse expired: planted PB={plantPowerBomb}, PB already active={powerBombActive}, explosion sound={played}");
            if (!runtime.BombProjectiles.LastFrameResult.ExplosionStarted || played == powerBombActive)
                throw new InvalidDataException("Normal bomb explosion violated producer-time Power Bomb sound suppression.");
        }
    }

    private static void VerifySameFrameBombOrdering(string rom)
    {
        foreach (bool powerBombPlacedFirst in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            samus.Pose = SamusPoseIds.MorphBallGroundRightPose;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs);
            samus.PowerBombs = samus.MaxPowerBombs = 5;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            int normalIndex = -1, powerIndex = -1;
            foreach (bool power in new[] { powerBombPlacedFirst, !powerBombPlacedFirst })
            {
                samus.SelectedHudItem = power ? (ushort)3 : (ushort)0;
                runtime.BombProjectiles.SetSharedCooldown(0);
                int index = runtime.BombProjectiles.TryPlaceBomb(bus, samus,
                    (ushort)SnesButton.X, (ushort)SnesButton.X, out _) ??
                    throw new InvalidDataException("Simultaneous bomb fixture failed to allocate both slots.");
                runtime.BombProjectiles.Slots[index].BombTimer = 1;
                if (power) powerIndex = index; else normalIndex = index;
            }
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
                .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            var result = game.Step(0);
            bool normalPlayed = result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(2, SoundEffectLibrary2Sounds.BombExplosion.Value));
            bool powerPlayed = result.AudioCommands.Contains(CartridgeAudioCommand.WritePort(1, SoundEffectLibrary1Sounds.PowerBombExplosion.Value));
            // Cartridge HandleProjectile visits high slots first. Both cases end with
            // an active explosion, but only the earlier normal bomb sound is admitted.
            bool normalPrecedesPower = normalIndex > powerIndex;
            if (!powerPlayed || normalPlayed != normalPrecedesPower)
                throw new InvalidDataException("Same-frame bomb sounds lost native descending-slot suppression order.");
            Console.WriteLine($"Simultaneous bombs: normal slot={normalIndex}, PB slot={powerIndex}, normal sound={normalPlayed}, startup={powerPlayed}");
        }
    }
}
