using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

/// <summary>Enemy-owned hurt cry through the real Climb population and frontend.</summary>
internal static class PowerBombEnemySoundAudit
{
    public static void Run(string rom)
    {
        foreach (bool active in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            runtime.System.SetEvent(EventNumber.ZebesAwake);
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.Climb);
            var target = runtime.Enemies.Slots[0];
            ushort header = target.EnemyDefinitionPointer;
            var cry = SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, target.Definition.HurtSoundEffect);
            var samus = runtime.Samus!;
            samus.Pose = SamusPoseIds.FacingLeftNormalPose;
            samus.XPosition = (ushort)(target.XPosition + 64);
            samus.YPosition = (ushort)(target.YPosition + 8);
            samus.Health = samus.MaxHealth = 399;
            samus.SelectedHudItem = 1;
            samus.AutoCancelHudItemIndex = 0;
            samus.Missiles = samus.MaxMissiles = 5;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.CommitPoseHistory(bus);
            runtime.Camera!.SetPosition(256, 128);
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
            byte[] ports = new byte[4];
            bool requested = false, played = false;
            for (int frame = 0; frame < 32; frame++)
            {
                game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
                var result = game.Step(frame == 2 ? (ushort)SnesButton.X : (ushort)0);
                requested |= runtime.Enemies.SoundRequests.Any(request => request.SoundEffect == cry);
                foreach (var command in result.AudioCommands.Where(command => command.Kind == CartridgeAudioCommandKind.WritePort))
                {
                    ports[command.Port] = command.Value;
                    played |= command.Port == 2 && command.Value == cry.Value;
                }
            }
            Console.WriteLine($"Climb hurt cry: PB={active}, requested={requested}, played={played}, target dead={target.EnemyDefinitionPointer != header}");
            if (!requested || target.EnemyDefinitionPointer == header || played == active)
                throw new InvalidDataException("Enemy hurt cry violated Power Bomb suppression.");
        }
    }
}
