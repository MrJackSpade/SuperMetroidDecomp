using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>Host-only invincibility contract, separate from cartridge parity assertions.</summary>
internal static class SparkInvincibilityAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int cases = 0;
        foreach (bool left in new[] { false, true })
        foreach (int direction in new[] { 0, 1, 2 })
        {
            var reference = Create(false, 999, left);
            var variants = new ushort[] { 1, 2, 29, 30 }.Select(energy => Create(true, energy, left)).ToArray();
            bool crashed = false;
            for (int frame = 0; frame < 96; frame++)
            {
                ushort input = frame >= 24 ? (ushort)SnesButton.A : (ushort)0;
                if (frame >= 28)
                    input |= direction == 0 ? (ushort)(left ? SnesButton.Left : SnesButton.Right) :
                        direction == 2 ? (ushort)SnesButton.R : (ushort)0;
                if (frame == 20)
                    foreach (var runtime in variants.Prepend(reference))
                    {
                        runtime.Samus!.HorizontalSpeed.SpeedBoostCounter = SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter;
                        runtime.Samus.Shinespark.TryStoreFromSpeedBooster(runtime.Samus.HorizontalSpeed.SpeedBoostCounter);
                    }
                reference.StepFrame(input);
                foreach (var runtime in variants)
                {
                    var samus = runtime.Samus!;
                    ushort before = samus.Health;
                    bool moving = samus.Shinespark.Phase is ShinesparkPhase.Horizontal or ShinesparkPhase.Vertical or ShinesparkPhase.Diagonal;
                    runtime.StepFrame(input);
                    int expectedHealth = moving ? Math.Max(1, before - 1) : before;
                    var control = reference.Samus!;
                    if (samus.Health != expectedHealth || samus.Pose != control.Pose ||
                        samus.Shinespark.Phase != control.Shinespark.Phase ||
                        samus.XPosition != control.XPosition || samus.YPosition != control.YPosition ||
                        samus.Kinematics.XSubposition != control.Kinematics.XSubposition ||
                        samus.Kinematics.YSubposition != control.Kinematics.YSubposition)
                        throw new InvalidDataException($"Invincible spark differs: left={left}, direction={direction}, frame={frame}, health={samus.Health}/{expectedHealth}.");
                }
                if (reference.Samus!.Shinespark.Phase == ShinesparkPhase.Crash)
                {
                    crashed = true;
                    cases += variants.Length;
                    break;
                }
            }
            if (!crashed) throw new InvalidDataException("Fixture failed to reach terrain collision.");
        }
        Console.WriteLine($"Invincible spark: {cases} full-runtime cases pass through terrain collision.");
        return 0;

        SuperMetroidRuntime Create(bool invincible, ushort health, bool left)
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false, playerInvincibilityEnabled: invincible);
            var level = runtime.LevelData!;
            for (int x = 0; x < 16; x++) level.SetForegroundEntry(x, 0x8000);
            for (int y = 0; y < 16; y++)
            {
                level.SetForegroundEntry(y * level.WidthInBlocks, 0x8000);
                level.SetForegroundEntry(y * level.WidthInBlocks + 15, 0x8000);
            }
            var samus = runtime.Samus!;
            samus.Health = samus.MaxHealth = health;
            samus.XPosition = 128;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.EquippedItems = (ushort)SamusEquipmentFlags.SpeedBooster;
            return runtime;
        }
    }
}
