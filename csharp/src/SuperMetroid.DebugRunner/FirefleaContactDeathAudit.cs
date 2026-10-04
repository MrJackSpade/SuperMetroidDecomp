using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;
internal static class FirefleaContactDeathAudit
{
    internal static int Run(string installationRoot)
    {
        Directory.CreateDirectory("out/workbook-investigation");
        var installation=new GameInstallation(installationRoot);
        var bus=installation.OpenRuntimeAddressSpace();
        var game=new SuperMetroidGame(bus);
        InstalledInputReplay.Bind(game, installation);
        typeof(SuperMetroidGame).GetMethod("CreateGameplayRuntime",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(game,[false]);
        var runtime=game.RuntimeForVerification!;
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();runtime.InitializeCeresStartSamus();
        runtime.System.SetEvent(EventNumber.ZebesAwake);
        runtime.LoadCartridgeRoomForDebug(0x9c5e);

        var samus=runtime.Samus!;
        samus.Pose=9;samus.Health=samus.MaxHealth=999;
        samus.EquippedItems=samus.CollectedItems=(ushort)(SamusEquipmentFlags.GravitySuit);
        samus.RefreshCollisionRadii(bus);samus.InitializeAnimation(bus);samus.CommitPoseHistory(bus);
        samus.InputLocked=false;
        var enemy=runtime.Enemies.Slots.First(e=>e.EnemyDefinitionPointer==0xd6bf);
        foreach (var other in runtime.Enemies.Slots.Where(e => e != enemy))
            other.Properties = other.Properties.With(EnemyProperties.IgnoreSamusCollision);
        samus.XPosition=unchecked((ushort)(enemy.XPosition-8));samus.YPosition=enemy.YPosition;
        runtime.Camera!.SetPosition(256,0);
        typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game,SuperMetroidGameState.MainGameplay);
        var renderer=new CartridgeAudioRenderer(installation.LoadAudio());
        ushort deathX = enemy.XPosition, deathY = enemy.YPosition;
        var framesSeen = new HashSet<ushort>();
        int killed=-1;bool sound=false;bool visible=false;
        for(int frame=0;frame<48;frame++)
        {
            var result=game.Step((ushort)(SnesButton.Right));
            renderer.RenderFrame(result.AudioCommands);game.SetAudioAcknowledgements(renderer.ReadAcknowledgements());
            if(enemy.EnemyDefinitionPointer==0 && killed<0)killed=frame;
            var explosion=runtime.Enemies.EnemyProjectiles.FirstOrDefault(p=>p.Kind==RoomEnemyProjectileKind.EnemyDeathExplosion && p.EnemyHeaderPointer==0xd6bf);
            if (samus.HorizontalSpeed.ContactDamageIndex != 0)
                throw new InvalidDataException("Fixture must use ordinary contact throughout.");
            if (explosion is not null && explosion.PresentationOperandAddress is >= 0xed6b and <= 0xed81)
            {
                if (explosion.XPosition != deathX || explosion.YPosition != deathY)
                    throw new InvalidDataException("Fireflea explosion moved away from its contact position.");
                var effectOam = new OamBuffer();
                effectOam.BeginFrame();
                runtime.Enemies.DrawHighPriorityEnemyProjectiles(effectOam, runtime.Camera!.XPosition, runtime.Camera.YPosition);
                runtime.Enemies.DrawLowPriorityEnemyProjectiles(effectOam, runtime.Camera.XPosition, runtime.Camera.YPosition);
                effectOam.FinalizeFrame();
                if (effectOam.LastFinalizedSpriteCount == 0)
                    throw new InvalidDataException("Fireflea death frame emitted no visible sprites.");
                framesSeen.Add(explosion.PresentationOperandAddress);
                visible = true;
            }
            foreach(var command in result.AudioCommands.Where(c=>c.Kind==CartridgeAudioCommandKind.WritePort && c.Port==2 && c.Value!=0))
            {Console.WriteLine($"frame={frame} lib2={command.Value:X2}");if(command.Value==9)sound=true;}
            if(killed>=0 && frame==killed+4)PngWriter.WriteRgba("out/workbook-investigation/fireflea-contact.png",256,224,result.Pixels);
            if(frame%4==0 || frame==killed) Console.WriteLine($"frame={frame} samus={samus.XPosition},{samus.YPosition} contact={samus.HorizontalSpeed.ContactDamageIndex} health={samus.Health} enemyXY={enemy.XPosition},{enemy.YPosition} enemyHP={enemy.Health} props={enemy.Properties:X4} enemy={enemy.EnemyDefinitionPointer:X4} explosion={explosion?.PresentationOperandAddress:X4} occupied={runtime.Enemies.EnemyProjectiles.Count(p=>p.IsActive)}");
        }
        if (samus.Health != 998 || runtime.Enemies.EnemiesKilled != 1 || runtime.Enemies.FirefleaDarknessLevel != 2)
            throw new InvalidDataException("Ordinary contact must retain Samus damage, one kill and one darkness increment.");
        if (!(killed>=0 && visible && sound && framesSeen.Count == 6)) throw new InvalidDataException("Ordinary Fireflea contact must produce death animation and sound.");
        Console.WriteLine($"PASS retail-room seeded contact: killed={killed} animation={visible} sound={sound} distinctFrames={framesSeen.Count}");
        return 0;
    }
}

