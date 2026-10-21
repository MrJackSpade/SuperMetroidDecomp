using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks the Ceres repeating explosion schedule against native countdown and actor playback.</summary>
internal static class CeresSpawnerScheduleVerification
{
    /// <summary>Compares spawn events and timer reloads across reseeds and departure frames, including inactive boundaries.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres spawner schedule")]
    internal static void VerifyCeresSpawnerSchedule(ISnesAddressSpace rom)
    {
        // C33B loads the repeat countdown seed; C4A8 reloads the period after a spawn.
        AssertEqual((byte)0xa9, rom.ReadCartridgeByte(0x8bc33b), "native countdown seed opcode");
        AssertEqual((byte)0xa9, rom.ReadCartridgeByte(0x8bc4a8), "native countdown reload opcode");
        foreach (int reseedFrame in new[] { 1, 211, 273 })
        foreach (int departure in new[] { 1, 209, 210, 222, 273, int.MaxValue })
        {
            var native = new IntroDiscoverySprite(new(0, 0), 0, CinematicSpriteProgram.CeresExplosionSpawner);
            ushort countdown = 0;
            WrappingCountdown actualCountdown = WrappingCountdown.Start(0);
            bool disabled = false;
            for (int frame = 1; frame <= 300; frame++)
            {
                if (frame == reseedFrame)
                {
                    countdown = CartridgeImportSource.Require(rom).ReadCartridgeWord(0x8bc33c);
                    actualCountdown = WrappingCountdown.Start(CeresExplosionDefinitions.RepeatingCountdownSeed);
                }
                CeresExplosionSpawnEvents expected = default;
                if (native.IsActive && native.ListPreInstruction == CinematicSpritePreInstruction.CeresExplosionSpawnerRepeating && !disabled)
                {
                    if (frame >= departure) disabled = true;
                    else
                    {
                        countdown = WrappingCounter.Decrement(countdown).Value;
                        if ((short)countdown <= 0)
                        {
                            expected = expected with { Repeating = true };
                            countdown = CartridgeImportSource.Require(rom).ReadCartridgeWord(0x8bc4a9);
                        }
                    }
                }
                // The spawner's typed program (#641), which the cinematic sprite oracle matches to $8B:CE35.
                native.Step(cue =>
                {
                    expected = cue switch
                    {
                        CinematicSpriteCueKind.SpawnInitialExplosionWave => expected with { Initial = true },
                        CinematicSpriteCueKind.SpawnFinalExplosionWave => expected with { Final = true },
                        CinematicSpriteCueKind.SetPreInstruction or
                            CinematicSpriteCueKind.EndZebesExplosion or
                            CinematicSpriteCueKind.ExplosionFinale or
                            CinematicSpriteCueKind.FadeExplosionPalette or
                            CinematicSpriteCueKind.FadeInZebesText or
                            CinematicSpriteCueKind.FadeOutZebesText or
                            CinematicSpriteCueKind.GreyOut or
                            CinematicSpriteCueKind.Launch or
                            CinematicSpriteCueKind.LaunchHead or
                            CinematicSpriteCueKind.PrepareHead or
                            CinematicSpriteCueKind.Shoot or
                            CinematicSpriteCueKind.SpawnClearTime or
                            CinematicSpriteCueKind.SpawnColon or
                            CinematicSpriteCueKind.SpawnCompletedText or
                            CinematicSpriteCueKind.SpawnExplosionSilhouette or
                            CinematicSpriteCueKind.SpawnHoursTens or
                            CinematicSpriteCueKind.SpawnHoursUnits or
                            CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded or
                            CinematicSpriteCueKind.SpawnMinutesTens or
                            CinematicSpriteCueKind.SpawnMinutesUnits or
                            CinematicSpriteCueKind.SpawnParticles or
                            CinematicSpriteCueKind.SpawnRinkas0And1 or
                            CinematicSpriteCueKind.SpawnRinkas2And3 or
                            CinematicSpriteCueKind.SpawnSuitedJump or
                            CinematicSpriteCueKind.SpawnSuitlessJump or
                            CinematicSpriteCueKind.StartFlyingToZebes or
                            CinematicSpriteCueKind.StartIntroPage2 or
                            CinematicSpriteCueKind.StartIntroPage3 or
                            CinematicSpriteCueKind.StartIntroPage4 or
                            CinematicSpriteCueKind.StartIntroPage5 or
                            CinematicSpriteCueKind.StartRinkaMoving or
                            CinematicSpriteCueKind.StartZebesExplosion or
                            CinematicSpriteCueKind.TransitionToCredits => throw new InvalidOperationException($"The Ceres explosion spawner program publishes no {cue} cue."),
                        _ => throw new InvalidOperationException($"The Ceres explosion spawner program publishes no {cue} cue."),
                    };
                });
                AssertEqual(expected, CeresExplosionDefinitions.EventsAtFrame(frame, frame < departure, ref actualCountdown),
                    $"native spawner events at frame{frame}, reset{reseedFrame}, departure{departure}");
                AssertEqual(countdown, actualCountdown.RawValue, "native repeat countdown including late fade reset");
            }
            AssertEqual(false, native.IsActive, "native spawner terminates");
        }
        foreach (int outside in new[] { int.MinValue, -1, 0, 301, int.MaxValue })
        {
            WrappingCountdown untouched = WrappingCountdown.Start(123);
            AssertEqual(default(CeresExplosionSpawnEvents), CeresExplosionDefinitions.EventsAtFrame(outside, true, ref untouched), "outside finite spawner lifetime");
            AssertEqual((ushort)123, untouched.RawValue, "inactive countdown is untouched");
        }
    }
}
