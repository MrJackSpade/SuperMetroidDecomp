using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

internal static partial class GoldenTorizoAudit
{
    public static int TraceEncounter(string rom, string? nativeTrace = null, int frameCount = 3000, bool detailed = false)
    {
        if (frameCount is < 1 or > 3000)
            throw new ArgumentOutOfRangeException(nameof(frameCount));
        string[][]? native = nativeTrace is null ? null : File.ReadAllLines(nativeTrace)
            .Skip(1).Select(line => line.Split(',')).ToArray();
        if (native is not null && native.Length != 3000)
            throw new InvalidDataException("Expected 3000 original-CPU encounter frames.");
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomPointer);
        runtime.InitializeDebugGroundedSamus(384, 166, 24);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.Health = samus.MaxHealth = 9999;
        samus.Missiles = samus.MaxMissiles = 100;
        samus.SuperMissiles = samus.MaxSuperMissiles = 99;
        samus.EquippedItems = 1;
        samus.EquippedBeams = 0;
        samus.SelectedHudItem = 2;
        runtime.System.SetRandomNumber(0x1234);
        Console.Error.WriteLine($"Initial Samus={samus.XPosition},{samus.YPosition}, camera={runtime.Camera!.XPosition},{runtime.Camera.YPosition}, NMI={runtime.NmiFrameCounter}.");
        string header = "frame,input,x,y,subX,subY,health,flash,list,timer,function,pre,vx,vy,gravity,turn,flags,random,samusX,samusY,samusHealth,pose";
        if (detailed)
            header += ",guard,invulnerability,map" + string.Concat(Enumerable.Range(0, 5)
                .Select(p => $",s{p}_type,s{p}_x,s{p}_y,s{p}_subX,s{p}_subY,s{p}_direction,s{p}_damage"));
        Console.WriteLine(header);
        for (int frame = 0; frame < frameCount; frame++)
        {
            // A bounded, single-room input sequence: turn left once, then fire
            // regularly without walking through a door or repositioning the boss.
            ushort input = frame == 60 ? (ushort)0x200 :
                frame >= 500 && frame % 20 == 0 ? runtime.ControllerBindings.Shoot : (ushort)0;
            runtime.StepFrame(input);
            var state = runtime.Enemies.GoldenTorizo!;
            var head = state.Slot;
            int[] actual = [frame,input,head.XPosition,head.YPosition,
                head.XSubposition,head.YSubposition,head.Health,head.FlashTimer,head.CurrentInstruction,
                head.InstructionTimer,state.Function,state.PreInstruction,state.HorizontalVelocity,
                state.VerticalVelocity,state.VerticalAcceleration,state.AirTransitionTimer,head.Parameter2,
                runtime.System.RandomNumber,samus.XPosition,samus.YPosition,samus.Health,samus.Pose];
            if (detailed)
                actual = actual.Concat(new int[] { state.ShotGuard, head.InvincibilityTimer, head.SpritemapPointer })
                    .Concat(runtime.Projectiles.Slots.Take(5).SelectMany(p => p.Type == 0 ? new int[7] :
                        new int[] { p.Type,p.XPosition,p.YPosition,p.XSubposition,p.YSubposition,p.Direction,p.Damage })).ToArray();
            Console.WriteLine(string.Join(',', actual));
            if (native is not null)
            {
                int[] expected = native[frame].Select(value => int.Parse(value,
                    System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                if (!actual.SequenceEqual(expected))
                {
                    int column = Enumerable.Range(0, Math.Min(actual.Length, expected.Length))
                        .FirstOrDefault(i => actual[i] != expected[i], -1);
                    throw new InvalidDataException($"Golden encounter mismatch: frame={frame}, column={column}, port={(column < 0 ? actual.Length : actual[column])}, native={(column < 0 ? expected.Length : expected[column])}.");
                }
            }
        }
        Console.Error.WriteLine($"Golden encounter: completed {frameCount} frames; nativeCompared={native is not null}.");
        return 0;
    }
}
