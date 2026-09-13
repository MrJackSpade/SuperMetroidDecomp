using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Compares the actual wait-to-fade coroutine boundary, not just ring emptiness.</summary>
internal static class DoorWaitNativeComparison
{
    public static int Run(string rom, string csv)
    {
        var lines = File.ReadAllLines(csv);
        if (lines.Length != 226 || lines[0] != "library,count,lag,frame,phase,read,write,state,port,x,y")
            throw new InvalidDataException("Incomplete native door-wait trace.");
        int row = 1;
        for (int library = 0; library < 3; library++)
        for (int count = 0; count <= 3; count++)
        for (int lag = 0; lag <= 2; lag++)
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
            var runtime = FlatFloorMovementFixture.Create(bus, false);
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var projectile in runtime.Enemies.EnemyProjectiles) projectile.Clear();
            runtime.Plms.Reset();
            runtime.Samus!.XPosition = runtime.Samus.YPosition = 128;
            runtime.Samus.Pose = SamusPoseIds.FacingRightNormalPose;
            runtime.Samus.InputLocked = true;
            var audio = new CartridgeAudioState();
            audio.AdvanceFrame(bus, default);
            for (int i = 0; i < count; i++)
                audio.QueueSound(SoundEffectId.FromCartridge(SoundEffectLibraries.FromCartridge((byte)(library + 1), "door wait comparison"), (byte)(i + 1)), 3);
            byte[] Field(string name) => (byte[])typeof(CartridgeAudioState).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(audio)!;
            var read = Field("_soundReadPositions"); var write = Field("_soundWritePositions"); var state = Field("_soundStates");
            var transition = new DoorTransitionState();
            typeof(DoorTransitionState).GetProperty(nameof(transition.Phase))!.SetValue(transition, DoorTransitionPhase.WaitForSoundQueues);
            byte[] history = new byte[32];
            bool completed = false;
            for (int frame = 0; frame < history.Length; frame++)
            {
                // Native dispatches the wait/draw owner before that frame's APU rings.
                // Do not move this test after AdvanceFrame: the one-frame boundary is
                // precisely the property that an isolated queue comparison cannot prove.
                transition.Step(runtime, audio, 0);
                byte ack = frame > lag ? history[frame - lag - 1] : (byte)0;
                var commands = audio.AdvanceFrame(bus, new(0, library == 0 ? ack : (byte)0,
                    library == 1 ? ack : (byte)0, library == 2 ? ack : (byte)0));
                var writes = commands.Where(c => c.Kind == CartridgeAudioCommandKind.WritePort && c.Port == library + 1).ToArray();
                if (writes.Length > 1) throw new InvalidDataException("Multiple sound writes in one wait frame.");
                byte port = writes.Length == 0 ? byte.MaxValue : writes[0].Value;
                history[frame] = port != byte.MaxValue ? port : frame > 0 ? history[frame - 1] : (byte)0;
                string phase = transition.Phase switch
                {
                    DoorTransitionPhase.WaitForSoundQueues => "E29E",
                    DoorTransitionPhase.FadeOutSourcePalette => "E2DB",
                    _ => throw new InvalidDataException("Unexpected door coroutine phase."),
                };
                string actual = $"{library},{count},{lag},{frame},{phase},{read[library]},{write[library]},{state[library]},{port},{runtime.Samus.XPosition},{runtime.Samus.YPosition}";
                if (row >= lines.Length || actual != lines[row])
                    throw new InvalidDataException($"Door handoff mismatch row {row}: {actual}; native={lines.ElementAtOrDefault(row)}");
                row++;
                if (transition.Phase == DoorTransitionPhase.FadeOutSourcePalette) { completed = true; break; }
            }
            if (!completed) throw new InvalidDataException("Door wait failed to complete within the captured bound.");
        }
        if (row != lines.Length) throw new InvalidDataException("Unconsumed native door-wait frames.");
        Console.WriteLine("PASS original CPU door-wait handoff: all 225 frames / 36 cases match phase, queue, writes and Samus position.");
        return 0;
    }
}
