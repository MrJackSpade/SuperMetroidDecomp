using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class PowerBombSuitSoundAudit
{
    public static void Run(string rom)
    {
        foreach (var kind in new[] { SamusSuitPickupKind.Varia, SamusSuitPickupKind.Gravity })
        foreach (bool active in new[] { false, true })
        foreach (bool reverseBeforePublication in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            var bomb = runtime.BombProjectiles.PowerBombExplosion;
            if (active) { bomb.Arm(); bomb.Spawn(samus.XPosition, samus.YPosition); }
            runtime.SuitPickup.Begin(bus, samus, 0, 0, kind, soundSuppressed: bomb.IsActive);
            if (reverseBeforePublication)
            {
                if (active) bomb.Reset(); else { bomb.Arm(); bomb.Spawn(samus.XPosition, samus.YPosition); }
            }
            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!.SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            var frame = game.Step(0);
            bool played = frame.AudioCommands.Contains(CartridgeAudioCommand.WritePort(2, SoundEffectLibrary2Sounds.SuitTransformation.Value));
            Console.WriteLine($"Suit {kind}: PB={active}, played={played}, active={runtime.SuitPickup.IsActive}");
            if (!runtime.SuitPickup.IsActive || played == active)
                throw new InvalidDataException("Suit setup violated native Power Bomb sound guard.");
        }
    }
}
