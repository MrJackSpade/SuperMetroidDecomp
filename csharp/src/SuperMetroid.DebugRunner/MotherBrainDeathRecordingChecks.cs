using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

/// <summary>Assertions for the actual zero-HP-to-escape recording, not a no-crash smoke test.</summary>
internal sealed class MotherBrainDeathRecordingChecks
{
    private readonly HashSet<MotherBrainBodyFunction> phases = [];
    private readonly HashSet<bool> flickerStates = [];
    private readonly HashSet<ushort> music = [];
    private int headFallStart = -1, headFallEnd, maximumFragments;
    private int bodyPaletteSteps, corpsePaletteSteps;

    public void Observe(SuperMetroidRuntime runtime, int frame)
    {
        if (runtime.Enemies.MotherBrain is not { LastRainbowBeamStep: { } step } brain) return;
        if (step.PhaseBefore < MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom) return;
        foreach (var request in brain.MusicRequests) music.Add(request.Command.RawValue);
        maximumFragments = Math.Max(maximumFragments, runtime.Enemies.EnemyProjectiles.Count(p =>
            p.Kind == RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment));
        if (brain.Function == MotherBrainBodyFunction.ThirdPhaseDeathFadeOutBody)
        {
            flickerStates.Add(brain.DeathBg2Hidden);
            var packet = GameplayDisplayCapture.TryCaptureFrame(runtime)!;
            var ordinary = (OrdinaryGameplayRenderLayer)packet.Layers[0];
            bool shown = (ordinary.Registers.MainScreenLayers & SnesMainScreenLayers.Bg2) != 0;
            Require(shown != brain.DeathBg2Hidden, "body flicker did not reach shared render registers");
            if (step.PaletteRequested)
            {
                VerifyPalette(MotherBrainDeathRomData.BodyFadeTable, MotherBrainDeathRomData.BodyColors,
                    MotherBrainDeathRomData.BodyColorCount);
                bodyPaletteSteps++;
            }
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround)
        {
            int y = brain.Head!.YPosition;
            if (headFallStart < 0) headFallStart = y;
            Require(y >= headFallEnd, "decapitated head moved upward during gravity fall");
            headFallEnd = y;
            Require(y == brain.RainbowBeamSequence!.BrainYPosition, "neck overwrote the falling head position");
        }
        if (step.PhaseBefore == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey && step.PaletteRequested)
        {
            VerifyPalette(MotherBrainDeathRomData.CorpseFadeTable, MotherBrainDeathRomData.CorpseColors,
                MotherBrainDeathRomData.CorpseColorCount);
            corpsePaletteSteps++;
        }
        if (phases.Add(brain.Function))
        {
            string directory = "csharp/test-temp/mother-brain-recording/death";
            Directory.CreateDirectory(directory);
            PngWriter.WriteRgba(Path.Combine(directory, $"{frame}-{brain.Function}.png"), 256, 224,
                SuperMetroidRuntimeFrameRenderer.Render(runtime));
        }

        void VerifyPalette(int table, int destination, int count)
        {
            int index = brain.RainbowBeamSequence!.GreyTransitionCounter - 1;
            var bus = runtime.AddressSpace;
            int pointer = bus.ReadByte(table + index * 2) | bus.ReadByte(table + index * 2 + 1) << 8;
            int source = MotherBrainDeathRomData.PaletteBank | pointer;
            for (int i = 0; i < count; i++)
                Require(runtime.Cgram.Colors[destination + i] ==
                    (bus.ReadByte(source + i * 2) | bus.ReadByte(source + i * 2 + 1) << 8),
                    $"death palette entry {destination + i} does not match native source");
        }
    }

    public void VerifyReadyToLeave(SuperMetroidRuntime runtime)
    {
        var brain = runtime.Enemies.MotherBrain ?? throw new InvalidDataException("Death replay lost the boss room.");
        Require(brain.Function == MotherBrainBodyFunction.ThirdPhaseDeathKeepEarthquakeGoing, "death did not finish");
        Require(flickerStates.Count == 2 && bodyPaletteSteps >= 15 && corpsePaletteSteps == 8,
            $"incomplete visible fades: flicker={flickerStates.Count}, body={bodyPaletteSteps}, corpse={corpsePaletteSteps}");
        Require(headFallStart >= 0 && headFallStart < headFallEnd && headFallEnd == 196, "head never fell to the native floor");
        Require(brain.DeathBg2Cleared && brain.EscapeTypewriter is { Completed: true, GlyphsWritten: > 20 },
            "body clear or actual escape text incomplete");
        Require(!brain.EnableUnpauseHook, "escape did not remove the body-graphics unpause hook");
        Require(runtime.EscapeTimer.IsActive, "escape timer inactive");
        Require(runtime.System.HasEvent(EventNumber.ZebesTimebombSet) &&
            runtime.System.HasAnyBossBits(runtime.ActiveRoom!.AreaIndex, BossBits.AreaMiniBoss), "native progression flags missing");
        Require(music.Contains(0) && music.Contains(0xff24) && music.Contains(7), "death/escape music commands missing");
        Require(maximumFragments == 8, $"expected eight shared-pool fragments, saw {maximumFragments}");
        var door = runtime.LevelData!.GetCollisionBlock(0, 6);
        Require(door.CollisionTypeValue == 9 && door.Behavior == 1, "escape door has no live door-list collision");
        for (int row = 7; row < 10; row++)
        {
            var extension = runtime.LevelData.GetCollisionBlock(0, row);
            Require(extension.CollisionTypeValue == 13 && extension.Behavior == 255, "escape door extension missing");
        }
        Console.WriteLine($"Death verified: {phases.Count} live phases, body/head fades, BG2 flicker, head fall, text, timer, music, flags, eight fragments and door collision.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException("Mother Brain death verification: " + message);
    }
}
