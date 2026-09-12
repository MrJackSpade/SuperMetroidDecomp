using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Normal Power Bomb/input characterization for issue #604; no mid-sequence actor writes.</summary>
internal static class NoobTubeControlAudit
{
    public static int Run(string rom, string? nativeTrace = null, int wakeFrame = 124)
    {
        string expectedHash = wakeFrame switch
        {
            -1 => "3484FCD892D15B1101F99BCE96D1944FDD1080E34FDC2C316F38AA77D3DD54FF",
            124 => "8127F04068E7FA5195B74BB7F5B18901A23CED4C5E92243AEC87FD682D7F4003",
            125 => "D6B4781B589E537567CDF18796052294A068B859F7A4BE5A133B1E0477FB0387",
            _ => throw new ArgumentOutOfRangeException(nameof(wakeFrame), "Use the pinned no-input/124/125 controls.")
        };
        if (nativeTrace is not null && Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(nativeTrace))) != expectedHash)
            throw new InvalidDataException("Use the accepted original-CPU trace for this wake input.");
        var native = nativeTrace is null ? null : File.ReadLines(nativeTrace).Skip(1)
            .Select(line => line.Split(',').Select(int.Parse).ToArray()).ToArray();
        if (native is not null && native.Length != 700) throw new InvalidDataException("Expected 700 native tube frames.");
        var runtime = new SuperMetroidRuntime(SuperMetroidAddressSpace.LoadRetailRom(rom));
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(NoobTubeControlDefinitions.RoomHeader);
        var placement = runtime.InitializeDebugGroundedSamus(128, 166, 22);
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs | SamusEquipmentFlags.VariaSuit);
        samus.Health = samus.MaxHealth = 999;
        samus.Missiles = samus.MaxMissiles = samus.SuperMissiles = samus.MaxSuperMissiles = 0;
        samus.PowerBombs = samus.MaxPowerBombs = 10;
        Console.WriteLine($"placement={placement}, samus={samus.XPosition}/{samus.YPosition}");
        int wake = -1;
        Console.WriteLine("frame,input,x,y,pose,animation,timer,locked,explosion,pre,instruction,broken");
        for (int frame = 0; frame < 700; frame++)
        {
            ushort input = frame is 0 or 12 ? (ushort)SnesButton.Down : (ushort)0;
            if (frame == 24) input = runtime.ControllerBindings.ItemSelect;
            if (frame == 26) input = runtime.ControllerBindings.Shoot;
            if (frame == 40) input = (ushort)SnesButton.Up;
            var tube = runtime.Plms.PopulationSlots.FirstOrDefault(p => p.HeaderPointer == RoomPlmHeaders.NoobTube);
            if (wake < 0 && tube.PreInstruction == NoobTubePlmRomData.WakeOnAcceptedInputPreInstruction)
                wake = frame;
            if (wakeFrame >= 0 && frame >= wakeFrame && frame < wakeFrame + 30) input = (ushort)SnesButton.Right;
            runtime.StepFrame(input);
            if (native is not null)
            {
                int[] actual = [frame, input, samus.XPosition, samus.YPosition, samus.Pose, samus.AnimationFrame, samus.AnimationFrameTimer];
                if (!actual.SequenceEqual(native[frame].Take(actual.Length)))
                    throw new InvalidDataException($"Tube control differs at frame {frame}: port={string.Join(',', actual)}, native={string.Join(',', native[frame].Take(actual.Length))}.");
                bool nativeLocked = native[frame][8] == NoobTubeControlDefinitions.StationaryBeta;
                if (samus.InputLocked != nativeLocked || samus.StationaryScriptControlLocked != nativeLocked)
                    throw new InvalidDataException($"Tube handler ownership differs at frame {frame}.");
            }
            tube = runtime.Plms.PopulationSlots.FirstOrDefault(p => p.HeaderPointer == RoomPlmHeaders.NoobTube);
            Console.WriteLine($"{frame},{input},{samus.XPosition},{samus.YPosition},{samus.Pose},{samus.AnimationFrame},{samus.AnimationFrameTimer},{samus.InputLocked},{runtime.BombProjectiles.PowerBombExplosion.Phase},{tube.PreInstruction},{tube.InstructionPointer},{runtime.System.HasEvent(EventNumber.MaridiaNoobTubeBroken)}");
        }
        if (wake < 0) throw new InvalidDataException("Normal Power Bomb did not reach tube input wake.");
        bool expectedBroken = wakeFrame >= 0;
        if (runtime.System.HasEvent(EventNumber.MaridiaNoobTubeBroken) != expectedBroken || samus.InputLocked ||
            samus.StationaryScriptControlLocked || samus.PowerBombs != 9)
            throw new InvalidDataException("Tube final event/control/ammunition handoff differs.");
        return 0;
    }
}

internal static class NoobTubeControlDefinitions
{
    /// <summary>RoomHeader_GlassTunnel at $8F:CEFB, Maridia $04/$01, intact state selected with event $0B clear.</summary>
    public const ushort RoomHeader = 0xcefb;
    /// <summary>$90:E8DC, stationary beta installed by command zero; updates contact/minimap, not animation.</summary>
    public const ushort StationaryBeta = 0xe8dc;
}
