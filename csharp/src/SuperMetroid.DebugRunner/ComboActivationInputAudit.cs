using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Real held-fire witness for the special-attack handoff and live trail rendering (#416).</summary>
internal static class ComboActivationInputAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int failures = 0;
        foreach (SamusBeamFlags beam in new[] { SamusBeamFlags.Wave, SamusBeamFlags.Ice,
            SamusBeamFlags.Spazer, SamusBeamFlags.Plasma })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            foreach (var actor in runtime.Enemies.EnemyProjectiles) actor.Clear();
            runtime.Plms.Reset();
            var samus = runtime.Samus!;
            samus.EquippedBeams = (ushort)(SamusBeamFlags.Charge | beam);
            samus.PowerBombs = samus.MaxPowerBombs = 2;
            samus.SelectedHudItem = 3;
            bool witnessedActivation = false;
            for (int frame = 0; frame < 130; frame++)
            {
                ushort previousAmmo = samus.PowerBombs;
                runtime.StepFrame((ushort)SnesButton.X);
                if (samus.PowerBombs < previousAmmo)
                {
                    ushort expectedSound = beam switch
                    {
                        SamusBeamFlags.Wave => 0x28,
                        SamusBeamFlags.Ice => 0x23,
                        SamusBeamFlags.Spazer => 0x25,
                        SamusBeamFlags.Plasma => 0x27,
                        _ => throw new InvalidOperationException()
                    };
                    witnessedActivation = runtime.Projectiles.ProjectileCounter == 4 &&
                        runtime.Projectiles.FlareCounter == 0 &&
                        runtime.Projectiles.LastFrameResult.QueuedSoundEffect ==
                            SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, expectedSound);
                    Console.WriteLine($"COMBO {beam}: activation at input frame {frame}, particles={runtime.Projectiles.ProjectileCounter}, sound={runtime.Projectiles.LastFrameResult.QueuedSoundEffect:X2}.");
                }
            }
            // The dispatch oracle uses this same Charge+one-beam/PB selection and
            // consumes one unit on activation. Room input must actually reach that path.
            if (!witnessedActivation || samus.PowerBombs != 1 || runtime.Projectiles.FlareCounter >= 120)
            {
                failures++;
                Console.WriteLine($"COMBO {beam}: PB={samus.PowerBombs}, charge={runtime.Projectiles.FlareCounter}; expected activated combo with PB=1 and cleared charge.");
            }
        }
        Console.WriteLine($"Combo activation input: 4 families, {failures} missing activations.");
        return failures == 0 ? 0 : 1;
    }
}
