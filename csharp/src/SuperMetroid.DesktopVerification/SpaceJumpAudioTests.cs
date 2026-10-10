using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static void VerifySpaceJumpAudio()
    {
        var loaded = DebuggerFixtureLoader.Load("space-jump-audio-loop", 0);
        var player = loaded.AudioPlayer!;
        var audio = new CartridgeAudioRenderer(ExtractedAudioAssetCatalog.Load(Path.GetFullPath("standalone-assets/audio")), player);
        var libraries = (Array)typeof(ManagedSpcPlayer).GetField("soundLibraries", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(player)!;
        for (int frame = 0; frame < 300; frame++)
        {
            var next = loaded.Game.Step(0);
            audio.RenderFrame(next.AudioCommands);
            loaded.Game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            if (frame % 60 == 0 || next.AudioCommands.Count != 0)
            {
                var samus = loaded.Game.RuntimeForVerification!.Samus!;
                Console.WriteLine($"frame {frame}: pose {(int)samus.Pose:X2}, sounds " + string.Join(",", libraries.Cast<object>().Select(lib =>
                    lib.GetType().GetField("CurrentSound", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(lib))) +
                    " commands " + string.Join(",", next.AudioCommands));
            }
        }
        bool spun = false, stopped = false;
        for (int frame = 0; frame < 100; frame++)
        {
            ushort input = frame < 3 ? (ushort)SnesButton.Left : frame < 20 ? (ushort)(loaded.Game.RuntimeForVerification!.ControllerBindings.Jump | (ushort)SnesButton.Left) : (ushort)SnesButton.Up;
            var next = loaded.Game.Step(input);
            var samus = loaded.Game.RuntimeForVerification!.Samus!;
            if (frame is < 4 or 20) Console.WriteLine($"jump test {frame}: pose={(int)samus.Pose:X2}, locked={samus.InputLocked}, phase={next.Phase}, input={input:X4}");
            spun |= SamusState.IsSpinJumpPose(samus.Pose);
            stopped |= next.AudioCommands.Any(command => command.Kind == CartridgeAudioCommandKind.WritePort && command.Port == 1 && command.Value == 0x32);
            audio.RenderFrame(next.AudioCommands);
            loaded.Game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
        }
        if (!spun || !stopped) throw new InvalidOperationException($"Spin cancellation failed to stop audio: entered spin={spun}, stop command={stopped}.");
    }
}
