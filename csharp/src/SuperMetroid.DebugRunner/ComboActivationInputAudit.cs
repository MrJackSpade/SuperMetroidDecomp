using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Real held-fire witness for the currently missing special-attack handoff (#416).</summary>
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
            for (int frame = 0; frame < 130; frame++)
                runtime.StepFrame((ushort)SnesButton.X);
            // The dispatch oracle uses this same Charge+one-beam/PB selection and
            // consumes one unit on activation. Room input must actually reach that path.
            if (samus.PowerBombs != 1 || runtime.Projectiles.FlareCounter >= 120)
            {
                failures++;
                Console.WriteLine($"COMBO {beam}: PB={samus.PowerBombs}, charge={runtime.Projectiles.FlareCounter}; expected activated combo with PB=1 and cleared charge.");
            }
        }
        Console.WriteLine($"Combo activation input: 4 families, {failures} missing activations.");
        return failures == 0 ? 0 : 1;
    }
}
