using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rendering;

internal static class RescuedAnimalsShipAudit
{
    internal static int Run(string installationRoot)
    {
        foreach (bool rescued in new[] { true, false })
        {
            var installation = new GameInstallation(installationRoot);
            var bus = installation.OpenRuntimeAddressSpace();
            var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
            InstalledInputReplay.Bind(game, installation);
            typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(game, [false]);
            var runtime = game.RuntimeForVerification!;
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            if (rescued) runtime.System.SetEvent(EventNumber.CrittersEscaped);
            typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game, SuperMetroidGameState.SamusEscapesFromZebes);
            typeof(SuperMetroidGame).GetField("endingFadeBrightness", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, (byte)0);
            typeof(SuperMetroidGame).GetField("endingFadeCounter", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, 0);
            var audio = new SuperMetroid.Core.Audio.CartridgeAudioRenderer(installation.LoadAudio());
            audio.RenderFrame(game.Step(0).AudioCommands);
            var ending = (EndingCreditsState)typeof(SuperMetroidGame).GetField("endingCredits", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(game)!;
            if (ending is null) throw new InvalidDataException("Ending handoff did not run.");
            typeof(EndingCreditsState).GetMethod("SetupZebesExplosion", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(ending, null);
            var sprites = (List<EndingSprite>)typeof(EndingCreditsState).GetField("sprites", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(ending)!;
            for (int frame = 0; ending.Phase != EndingCreditsPhase.PlanetEscapeAccelerating; frame++)
            {
                if (frame > 2200) throw new InvalidDataException("Flyaway never reached the pod spawn boundary.");
                if (sprites.Any(s => s.NativeSlot == 2)) throw new InvalidDataException("Pod appeared before the native spawn boundary.");
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
                var frameResult = game.Step(0);
                audio.RenderFrame(frameResult.AudioCommands);
            }
            var pod = sprites.SingleOrDefault(s => s.NativeSlot == 2);
            if ((pod is not null) != rescued) throw new InvalidDataException($"Rescued={rescued}: expected escape pod={rescued}, actual={pod is not null}.");
            if (!rescued) continue;
            int firstVisibleAge = 0;
            for (int age = 1; age <= 143; age++)
            {
                var actor = pod!.Sprite;
                if (!actor.IsActive || actor.XPosition != 128 + age || actor.YPosition != 128 || actor.YSubPosition != age * 128 ||
                    actor.SpriteMapPointer != 0xbc41 + ((age - 1) % 4) * 7 || actor.PaletteBits != 0x0e00)
                    throw new InvalidDataException($"Escape pod position/animation differs at age {age}.");
                if (firstVisibleAge == 0)
                {
                    var withPod = ending.Render().ToArray();
                    sprites.Remove(pod);
                    var withoutPod = ending.Render();
                    sprites.Add(pod);
                    for (int dy = 0; dy < 8; dy++)
                    for (int dx = 0; dx < 8; dx++)
                    {
                        int x = actor.XPosition - 4 + dx;
                        int y = actor.YPosition - 4 + dy;
                        if (x < 256 && withPod[y * 256 + x] != withoutPod[y * 256 + x]) firstVisibleAge = age;
                    }
                }
                ending.Step();
            }
            if (firstVisibleAge == 0) throw new InvalidDataException("Escape pod OAM produced no visible pixels during its flight.");
            Console.WriteLine($"Pod first visible at actor age {firstVisibleAge}.");
            if (pod!.Sprite.IsActive || sprites.Any(s => s.NativeSlot == 2))
                throw new InvalidDataException("Escape pod did not delete at X=$110 on age 144.");
        }
        Console.WriteLine("PASS: frontend rescue event gates the pod at the native boundary; flight produces visible pixels; all 143 active positions and four-frame animation match; deletion at age 144; absent when not rescued.");
        return 0;
    }
}
