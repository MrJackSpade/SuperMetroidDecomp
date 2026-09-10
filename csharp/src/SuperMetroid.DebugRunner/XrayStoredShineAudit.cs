using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Real Run-input X-Ray activation must cancel the previously stored shine.</summary>
internal static class XrayStoredShineAudit
{
    public static int Run(string rom)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        int failures = 0;
        foreach (bool left in new[] { false, true })
        foreach (bool equipped in new[] { false, true })
        foreach (int remaining in new[] { 2, 170, 180 })
        {
            var runtime = FlatFloorMovementFixture.Create(bus, water: false);
            var samus = runtime.Samus!;
            samus.Pose = left ? SamusPoseIds.FacingLeftNormalPose : SamusPoseIds.FacingRightNormalPose;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            samus.EquippedItems = (ushort)(equipped ? SamusEquipmentFlags.XrayScope : 0);
            samus.SelectedHudItem = SamusXrayRomData.SelectedHudItem;
            if (!samus.Shinespark.TryStoreFromSpeedBooster(
                SamusSpecialSequenceRomData.Shinespark.ActiveSpeedBoostCounter))
                throw new InvalidDataException("Expected stored shine.");
            for (int i = remaining; i < 180; i++)
                samus.Shinespark.UpdatePalette(bus, runtime.Cgram, samus.EquippedItems);
            runtime.StepFrame((ushort)SnesButton.B);
            ushort expectedTimer = equipped ? (ushort)0 : (ushort)(remaining - 1);
            if (samus.Xray.IsActive != equipped || samus.Shinespark.ShineTimer != expectedTimer ||
                (equipped && (samus.Shinespark.Phase != ShinesparkPhase.Inactive ||
                    samus.Shinespark.PaletteType != 0 || samus.Shinespark.PaletteFrameOffset != 0)))
            {
                failures++;
                Console.WriteLine($"X-Ray shine left={left}, equipped={equipped}, remaining={remaining}: active={samus.Xray.IsActive}, timer={samus.Shinespark.ShineTimer}, phase={samus.Shinespark.Phase}.");
            }
        }
        Console.WriteLine($"X-Ray stored shine: 12 cases, {failures} failures.");
        return failures == 0 ? 0 : 1;
    }
}
