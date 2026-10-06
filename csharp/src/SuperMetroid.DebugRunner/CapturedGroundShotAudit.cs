using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;

internal static class CapturedGroundShotAudit
{
    internal static int Run(string statePath, string installationRoot, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        File.Copy(statePath, Path.Combine(outputDirectory, "SuperMetroid-debug-slot-0.smstate"), overwrite: true);
        var installation = new GameInstallation(installationRoot);
        var identity = GameContentIdentity.Create(installation.LoadAudio(), installation.LoadMaps(), installation.LoadProjectiles());
        var loaded = DebuggerSaveStateStore.ForInstalledGame(installationRoot, null, identity, outputDirectory).Load(0);
        var game = loaded.Game;
        InstalledInputReplay.Bind(game, installation);
        var runtime = game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        var level = runtime.LevelData!;
        if (runtime.ActiveRoom!.Pointer != 0x9cb3 || samus.XPosition != 1635 || samus.YPosition != 171 || samus.EquippedBeams != 0)
            throw new InvalidDataException("#1226 requires the supplied standing unupgraded-beam state.");
        if (game.ConfiguredOptions.GrantAllEquipment || game.ConfiguredOptions.UnlockTourian || runtime.GrantAllEquipmentEnabled || runtime.UnlockTourianEnabled)
            throw new InvalidDataException("Older state migration must leave new tester settings disabled.");
        using var trace = new StreamWriter(Path.Combine(outputDirectory, "trace.txt"));
        trace.WriteLine($"room={runtime.ActiveRoom!.Pointer:X4} id={game.GameplayActiveAreaIndex}/{game.GameplayActiveRoomIndex:X2} state={game.GameState} pos={samus.XPosition},{samus.YPosition} pose={samus.Pose:X4} beams={samus.EquippedBeams:X4} items={samus.EquippedItems:X4}");
        int bx = samus.XPosition / 16, by = samus.YPosition / 16;
        for (int y = Math.Max(0, by - 5); y < Math.Min(level.HeightInBlocks, by + 6); y++)
            trace.WriteLine($"ROW {y}: " + string.Join(" ", Enumerable.Range(Math.Max(0,bx-10), Math.Min(level.WidthInBlocks,bx+11)-Math.Max(0,bx-10)).Select(x => { var b=level.GetCollisionBlock(x,y); return $"{x}:{b.LevelWord:X4}/{b.Behavior:X2}"; })));
        var audio = new CartridgeAudioRenderer(installation.LoadAudio(), loaded.AudioPlayer);
        for (int frame = 0; frame < 40; frame++)
        {
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            var result = game.Step(frame < 2 ? (ushort)0 : runtime.ControllerBindings.Shoot);
            audio.RenderFrame(result.AudioCommands);
            if (frame == 4)
            {
                var shot = runtime.Projectiles.Slots[0];
                if (shot.Type != 0x8700 || shot.XPosition != 1666 || shot.XSubposition != 24576 || shot.YPosition != 166)
                    throw new InvalidDataException($"#1226: cartridge stops at frame 4, X=1666:24576 Y=166 type=8700; actual X={shot.XPosition}:{shot.XSubposition} Y={shot.YPosition} type={shot.Type:X4}.");
            }
            trace.WriteLine($"{frame}: samus={samus.XPosition},{samus.YPosition} pose={samus.Pose:X4}; " + string.Join(" | ",runtime.Projectiles.Slots.Where(p=>p.IsActive).Select(p=>$"{p.SlotIndex} type={p.Type:X4} pos={p.XPosition}:{p.XSubposition}/{p.YPosition}:{p.YSubposition} radius={p.XRadius}/{p.YRadius} vel={p.XVelocity}/{p.YVelocity}")));
            if (frame is 0 or 2 or 5 or 10 or 15 or 20 or 30)
                PngWriter.WriteRgba(Path.Combine(outputDirectory,$"frame-{frame:D2}.png"),256,224,SuperMetroidRuntimeFrameRenderer.Render(runtime));
        }
        Console.WriteLine($"Captured ground-shot trace: {outputDirectory}");
        return 0;
    }
}
