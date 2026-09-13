using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Water entry through the actual frontend animation/audio owners.</summary>
internal static class PowerBombLiquidSoundAudit
{
    public static void Run(string rom)
    {
        VerifyPostDrawGuard(rom);
        foreach (bool active in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: true);
            var samus = runtime.Samus!;
            if (active)
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
            var frame = game.Step(0);
            bool requested = samus.LiquidPhysics.SoundRequests.Any(request => request.SoundEffect == LiquidSoundAuditDefinitions.WaterEntry);
            bool played = frame.AudioCommands.Contains(CartridgeAudioCommand.WritePort(2, LiquidSoundAuditDefinitions.WaterEntry.Value));
            Console.WriteLine($"Water entry PB={active}, medium={samus.LiquidPhysics.LiquidMedium}, requested={requested}, played={played}");
            if (samus.LiquidPhysics.LiquidMedium != SamusLiquidMedium.Water || !requested || played == active)
                throw new InvalidDataException("Water entry violated Power Bomb sound suppression.");
        }
    }

    private static void VerifyPostDrawGuard(string rom)
    {
        foreach (bool activeAtRequest in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var samus = runtime.Samus!;
            var explosion = runtime.BombProjectiles.PowerBombExplosion;
            void Activate() { explosion.Arm(); explosion.Spawn(samus.XPosition, samus.YPosition); }
            if (!activeAtRequest) Activate();
            samus.LiquidPhysics.BeginFrameSoundRequests(explosion);
            if (activeAtRequest) Activate(); else explosion.Reset();
            SamusPostDrawAudio.Step(bus, samus, SamusMovementType.SpinJumping, 0);
            var request = samus.LiquidPhysics.SoundRequests.Single();
            if (request.SoundEffect != SoundEffectLibrary1Sounds.StopSpinJump || request.SoundSuppressed != activeAtRequest)
                throw new InvalidDataException("Post-draw spin stop sampled frame-start instead of producer-time suppression.");
            // Publication must not re-read the guard after the request either.
            if (activeAtRequest) explosion.Reset(); else Activate();
            var audio = new CartridgeAudioState();
            new GameplayAudioFramePublication(audio).PublishPrefix(runtime);
            if (audio.HasQueuedSounds == activeAtRequest)
                throw new InvalidDataException("Post-draw spin-stop publication lost captured suppression.");
            Console.WriteLine($"Post-draw spin stop: active at request={activeAtRequest}, admission correct despite earlier/later status changes");
        }
    }
}

internal static class LiquidSoundAuditDefinitions
{
    /// <summary>$90:80E2 queues library-two $0D upon entering water.</summary>
    public static readonly SoundEffectId WaterEntry = new(SoundEffectLibrary.Library2, 0x0d);
}
