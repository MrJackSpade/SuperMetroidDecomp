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
        return 0;
    }
}
