using SuperMetroid.Core.Game;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>Drives the runtime's synchronous ship prompt and both return paths.</summary>
internal static class GunshipSaveAudit
{
    public static int Run(string romPath)
    {
        foreach (bool save in new[] { false, true })
        {
            var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
            var runtime = new SuperMetroidRuntime(bus);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RunNmi(0, true);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, 1024, 1024);
            var samus = runtime.Samus!;
            var ship = runtime.Enemies.Slots[0];
            samus.XPosition = ship.XPosition;
            samus.YPosition = (ushort)(ship.VariableE + 18);
            samus.InputLocked = true;
            samus.ApplyForwardFacingPoseSetup(bus);
            samus.Health = samus.MaxHealth;
            ship.VariableF = GunshipCodePointers.BeginLiftoffOrRestoreSamus;
            for (int tick = 0; tick < 30 && !runtime.MessageBox.IsActive; tick++) runtime.StepFrame(0);
            if (!runtime.MessageBox.IsActive)
                throw new InvalidDataException("Gunship requested save but normal runtime never displayed its confirmation.");
            for (int tick = 0; tick < 30 && runtime.MessageBox.Phase != GameplayMessageBoxPhase.AwaitingInput; tick++) runtime.StepFrame(0);
            if (runtime.MessageBox.Phase != GameplayMessageBoxPhase.AwaitingInput)
                throw new InvalidDataException("Gunship confirmation never reached its interactive selector.");
            runtime.StepFrame((ushort)(save ? SnesButton.A : SnesButton.B));
            bool sawCompletion = false;
            SaveStationPersistenceRequest? persistence = null;
            for (int tick = 0; tick < 800 && samus.InputLocked; tick++)
            {
                sawCompletion |= runtime.MessageBox.MessageId == GameplayMessageIds.SaveCompleted;
                runtime.StepFrame(runtime.MessageBox.Phase == GameplayMessageBoxPhase.AwaitingInput ? (ushort)SnesButton.A : (ushort)0);
                var request = runtime.ConsumeSaveStationPersistenceRequest();
                if (request is not null)
                {
                    if (persistence is not null) throw new InvalidDataException("Gunship saved more than once.");
                    persistence = request;
                }
            }
            if (samus.InputLocked || ship.VariableF != GunshipCodePointers.Idle || runtime.MessageBox.IsActive || runtime.Enemies.GunshipSavePromptPending)
                throw new InvalidDataException($"Gunship did not release Samus after answer {save}.");
            if (sawCompletion != save || (persistence is not null) != save ||
                (persistence is { } p && (p.AreaIndex != AreaId.Crateria || p.StationIndex != 0)))
                throw new InvalidDataException($"Gunship answer {save} produced incorrect completion/persistence.");
        }
        RunFrontend(romPath, false);
        RunFrontend(romPath, true);
        Console.WriteLine("Gunship runtime: YES/NO, completion notice, exactly one Crateria station-zero save request on YES, and release pass.");
        return 0;
    }

    private static void RunFrontend(string romPath, bool save)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var saves = new SuperMetroidSaveRam(bus);
        saves.SaveSlot(2, new SuperMetroidSaveSnapshot { Area = 0, SaveStation = 1, Health = 75, MaxHealth = 99 });
        saves.SelectSlot(2);
        var game = new SuperMetroidGame(bus, gameOptions: null, renderGameplayFrames: false);
        var ports = new byte[4];
        int savingSounds = 0, padSounds = 0, closingSounds = 0, persistenceEvents = 0;
        game.SaveRamChanged += () => persistenceEvents++;
        FrontendFrame frame = default;
        void Step(ushort input = 0)
        {
            game.SetAudioAcknowledgements(new(ports[0], ports[1], ports[2], ports[3]));
            frame = game.Step(input);
            foreach (var command in frame.AudioCommands)
            {
                if (command.Kind != CartridgeAudioCommandKind.WritePort) continue;
                ports[command.Port] = command.Value;
                if (command.Port == 1 && command.Value == SoundEffectLibrary1Sounds.Saving.Value) savingSounds++;
                if (command.Port == 3 && command.Value == SoundEffectLibrary3Sounds.GunshipEntrancePad.Value) padSounds++;
                if (command.Port == 3 && command.Value == SoundEffectLibrary3Sounds.GunshipEntrancePadClosing.Value) closingSounds++;
            }
        }
        void Until(Func<bool> condition, int limit)
        {
            for (int tick = 0; tick < limit && !condition(); tick++) Step();
            if (!condition()) throw new InvalidDataException($"Ship frontend fixture stalled at {frame.GameState}/{frame.Phase}.");
        }
        Step(); Step((ushort)SnesButton.Start);
        Until(() => frame.Phase == nameof(TitleSequencePhase.TitleScreen), 150);
        Step((ushort)SnesButton.Start);
        Until(() => frame.GameState == SuperMetroidGameState.FileSelectMenus, 150);
        for (int tick = 0; tick < 16; tick++) Step();
        Step((ushort)SnesButton.A);
        Until(() => frame.GameState == SuperMetroidGameState.GameOptionsMenu, 200);
        for (int tick = 0; tick < 16; tick++) Step();
        Step((ushort)SnesButton.A);
        Until(() => frame.GameState == SuperMetroidGameState.FileSelectMap && frame.Phase == "Area", 200);
        Step((ushort)SnesButton.Start);
        Until(() => frame.Phase == "Room", 100);
        Step((ushort)SnesButton.Start);
        Until(() => game.RuntimeForVerification is not null && game.GameState == SuperMetroidGameState.MainGameplay, 150);
        var runtime = game.RuntimeForVerification!;
        Until(() => !runtime.SamusLoadAppearanceActive, 500);
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite, 1024, 1024);
        var ship = runtime.Enemies.Slots[0];
        var samus = runtime.Samus!;
        samus.XPosition = ship.XPosition;
        samus.YPosition = (ushort)(ship.VariableE - 30);
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InitializeAnimation(bus);
        samus.InputLocked = false;
        samus.Health = 98;
        samus.MaxReserveEnergy = 5;
        samus.ReserveEnergy = 0;
        ship.VariableF = GunshipCodePointers.Idle;
        persistenceEvents = savingSounds = padSounds = closingSounds = 0;
        byte[] before = bus.SaveRam.ToArray();
        Step((ushort)SnesButton.Down);
        if (!samus.InputLocked || ship.VariableF != GunshipCodePointers.WaitForEntranceToOpen)
            throw new InvalidDataException("Standing Down input failed to enter the gunship.");
        Until(() => runtime.MessageBox.Phase == GameplayMessageBoxPhase.AwaitingInput, 500);
        // Exercise the visible NO cursor rather than only the B cancellation shortcut.
        if (!save)
        {
            ushort[] yesTiles = runtime.MessageBox.Tilemap.ToArray();
            Step((ushort)SnesButton.Right);
            if (runtime.MessageBox.ConfirmationSelectionYes || yesTiles.AsSpan().SequenceEqual(runtime.MessageBox.Tilemap))
                throw new InvalidDataException("Gunship NO cursor did not change its actual tilemap.");
            Step();
        }
        Step((ushort)SnesButton.A);
        int soundWaitFrames = 0;
        bool sawCompleted = false;
        for (int tick = 0; tick < 900 && samus.InputLocked; tick++)
        {
            if (runtime.MessageBox.Phase == GameplayMessageBoxPhase.GunshipSavingSound) soundWaitFrames++;
            sawCompleted |= runtime.MessageBox.MessageId == GameplayMessageIds.SaveCompleted;
            if (runtime.MessageBox.IsActive && persistenceEvents != 0)
                throw new InvalidDataException("Gunship persisted before its completion coroutine returned.");
            Step(runtime.MessageBox.Phase == GameplayMessageBoxPhase.AwaitingInput ? (ushort)SnesButton.A : (ushort)0);
        }
        if (samus.InputLocked || runtime.MessageBox.IsActive || ship.VariableF != GunshipCodePointers.Idle)
            throw new InvalidDataException("Gunship frontend failed to return player control.");
        // Verify the pad/actor relocation, not just the input-lock endpoint.
        // $A2:AB83 stops only after crossing fixedY-30, not on equality. Starting
        // at fixedY+18 and moving two each call therefore finishes at fixedY-32.
        if (samus.XPosition != ship.XPosition || samus.YPosition != ship.VariableE - 32)
            throw new InvalidDataException($"Gunship release position {samus.XPosition}/{samus.YPosition} disagrees with ship {ship.XPosition}/{ship.VariableE - 32}.");
        if (persistenceEvents != (save ? 1 : 0) || savingSounds != (save ? 1 : 0) || padSounds != 1 || closingSounds != 1 ||
            soundWaitFrames != (save ? GameplayMessageRomData.Timing.GunshipSavingSoundFrames : 0) || sawCompleted != save)
            throw new InvalidDataException($"Ship frontend answer {save}: saves={persistenceEvents}, save SFX={savingSounds}, pad SFX={padSounds}/{closingSounds}, sound wait={soundWaitFrames}, completion={sawCompleted}.");
        var result = saves.ReadSlot(2)!;
        if (save ? result.SaveStation != 0 || result.Area != 0 || result.Health != 99 || result.ReserveEnergy != 5 || (result.UsedSaveStationBytes[0] & 1) == 0 : !before.AsSpan().SequenceEqual(bus.SaveRam))
            throw new InvalidDataException("Gunship did not preserve NO SRAM or encode YES into the selected slot with station-zero marker.");
        if (saves.ReadSlot(0) is not null || saves.ReadSlot(1) is not null)
            throw new InvalidDataException("Gunship modified an unselected SRAM slot.");
        for (int tick = 0; tick < 30; tick++) Step();
        if (runtime.MessageBox.IsActive || samus.InputLocked || persistenceEvents != (save ? 1 : 0))
            throw new InvalidDataException("Gunship reentered its save loop without another Down edge.");
        Console.WriteLine($"Gunship frontend save={save}: cursor, {soundWaitFrames}-frame sound wait, sound ports, completion, selected-slot SRAM and exact release position pass.");
    }
}
