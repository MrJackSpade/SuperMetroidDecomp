using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Checks suit effect entry through frontend sound publication and the cartridge APU queue.</summary>
internal static class SuitPickupAudioAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int failures = 0;
        foreach (var kind in new[] { SamusSuitPickupKind.Varia, SamusSuitPickupKind.Gravity })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var game = new SuperMetroidGame(bus);
            typeof(SuperMetroidGame).GetField("runtime", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(game, runtime);
            typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.MainGameplay);
            runtime.SuitPickup.Begin(bus, runtime.Samus!, 0, 0, kind);
            int requests = 0;
            for (int frame = 0; frame < 16; frame++)
            {
                var output = game.Step(0);
                requests += output.AudioCommands.Count(c => c.Kind == CartridgeAudioCommandKind.WritePort && c.Port == 2 && c.Value == 0x56);
                // Echo requests through the real handshake so duplicates would be observable.
                var ports = new byte[4];
                foreach (var command in output.AudioCommands.Where(c => c.Kind == CartridgeAudioCommandKind.WritePort))
                    ports[command.Port] = command.Value;
                game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
            }
            if (requests != 1) failures++;
            Console.WriteLine($"{kind} transformation audio: {requests} library-2/56 writes (expected one).");
        }
        return failures == 0 ? 0 : 1;
    }
}
