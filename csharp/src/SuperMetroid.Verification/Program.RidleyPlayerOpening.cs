using System.Reflection;
using System.Globalization;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifyRidleyPlayerOpening()
    {
        // The player's first movie, from the completed entry into Ridley's room.
        // Feed the recorded Samus state and RNG to isolate the reported boss trajectory.
        int[] addresses = [0xfa8,0xf7a,0xf7c,0xf7e,0xf80,0xfaa,0xfac,0xaf6,0xafa,0xa1c,0x5e5,
            0x911,0x915,0x8b,0xf92,0xf94,0xf90,0xfa4,0xf8e,0xfb2];
        var rows = File.ReadAllLines("csharp/test-fixtures/issue-1266-ridley/opening-native.csv").Skip(1)
            .Select(line => line.Split(',')).ToDictionary(parts => int.Parse(parts[0], CultureInfo.InvariantCulture),
                parts => parts.Skip(1).Select(value => ushort.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToArray());
        ushort[] Read(int frame) => rows[frame];
        ushort W(ushort[] words, int address) => words[Array.IndexOf(addresses, address)];
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(0xb32e, cameraY: 0);
        var enemies = runtime.Enemies;
        var body = enemies.Slots[0]; var state = enemies.Ridley!; var samus = runtime.Samus!;
        var seed = Read(375);
        body.CurrentInstruction = W(seed, 0xf92); body.InstructionTimer = W(seed, 0xf94);
        body.Timer = W(seed, 0xf90); body.FrameCounter = W(seed, 0xfa4);
        body.SpritemapPointer = W(seed, 0xf8e);
        body.XSubposition = W(seed, 0xf7c); body.YSubposition = W(seed, 0xf80);
        ushort random = 0;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(SuperMetroid.Core.Game.RoomEnemySystem).GetField("_readRandomNumber", flags)!.SetValue(enemies, (Func<ushort>)(() => random));
        typeof(SuperMetroid.Core.Game.RoomEnemySystem).GetField("_nextRandom", flags)!.SetValue(enemies, (Func<ushort>)(() => random));
        for (int frame = 376; frame <= 733; frame++)
        {
            var expected = Read(frame);
            samus.XPosition = W(expected, 0xaf6); samus.YPosition = W(expected, 0xafa);
            samus.Pose = (byte)W(expected, 0xa1c); samus.RefreshCollisionRadii(bus);
            random = W(expected, 0x5e5);
            enemies.StepFrame(W(expected, 0x911), W(expected, 0x915), false, samus,
                level: runtime.LevelData, controllerInput: W(expected, 0x8b));
            string actual = $"{(ushort)state.Function:X4},{body.XPosition:X4},{body.XSubposition:X4},{body.YPosition:X4},{body.YSubposition:X4},{state.HorizontalVelocity:X4},{state.VerticalVelocity:X4}";
            string native = string.Join(",", new[] {0xfa8,0xf7a,0xf7c,0xf7e,0xf80,0xfaa,0xfac}.Select(a => W(expected,a).ToString("X4")));
            if (actual != native)
                throw new InvalidDataException($"Ridley movie first divergence at frame {frame}: native {native}; port {actual}; instructions native={W(expected,0xf92):X4}/{W(expected,0xf94)} port={body.CurrentInstruction:X4}/{body.InstructionTimer}; timer={state.FunctionTimer} native={W(expected,0xfb2)}");
        }
        Console.WriteLine("Ridley player opening trajectory matches native movie frames 375..733.");
    }
}
