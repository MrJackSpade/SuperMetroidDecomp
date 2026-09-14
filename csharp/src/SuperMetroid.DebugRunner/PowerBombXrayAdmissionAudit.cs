using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>#422: distinguish X-ray's admission gate from missing deferred sound suppression.</summary>
internal static class PowerBombXrayAdmissionAudit
{
    public static void Run(string rom)
    {
        foreach (bool left in new[] { false, true })
        foreach (var phase in new[] { "inactive", "armed", "exploding" })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            var samus = runtime.Samus!;
            samus.EquippedItems |= (ushort)SamusEquipmentFlags.XrayScope;
            samus.SelectedHudItem = 5;
            samus.PoseId = left ? SamusPoseId.FacingLeftNormalPose : SamusPoseId.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            var bomb = runtime.BombProjectiles.PowerBombExplosion;
            if (phase != "inactive") bomb.Arm();
            if (phase == "exploding") bomb.Spawn(samus.XPosition, samus.YPosition);

            var game = new SuperMetroidGame(bus, null, renderGameplayFrames: false);
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", fields)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", fields)!
                .SetValue(game, (ushort?)runtime.ActiveRoom!.State.Pointer);
            typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!
                .SetValue(game, SuperMetroidGameState.MainGameplay);
            var frame = game.Step((ushort)SnesButton.B);
            // Arm owns power_bomb_flag, not power_bomb_explosion_status. Native
            // $91:E16D checks only the latter: an unexpired fuse must still allow X-ray.
            bool expected = phase != "exploding";
            bool sound = frame.AudioCommands.Contains(CartridgeAudioCommand.WritePort(1, 9));
            if (samus.Xray.IsActive != expected || sound != expected)
                throw new InvalidDataException($"X-ray admission/audio mismatch: left={left}, bomb={phase}, active={samus.Xray.IsActive}, sound={sound}");
            Console.WriteLine($"X-ray left={left}, bomb={phase}: activation and sound admitted={expected}");
        }
    }
}
