using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using static VerificationHelpersVerification;

/// <summary>
/// #1296: the save station electricity's typed program against the retail cartridge. It must match its bank-$86 list
/// update by update, and the real producer must start it above the station.
/// </summary>
internal static class SaveStationElectricityAnimationsVerification
{
    /// <summary>The electricity matches its native list through its last cycle and deletion.</summary>
    internal static void VerifySaveStationElectricityAnimations(SuperMetroidAddressSpace rom)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(new TestAddressSpace());
        enemies.Projectiles.SpawnSaveStationElectricity(plmBlockIndex: 42, roomWidthInBlocks: 16);
        RoomEnemyProjectileSlot electricity = enemies.Projectiles.EnemyProjectiles.Single(
            projectile => projectile.Kind == RoomEnemyProjectileKind.SaveStationElectricity);
        AssertTrue(electricity.Animation is SaveStationElectricityAnimation, "the producer starts the typed electricity");
        AssertEqual((ushort)176, electricity.Position.X.Whole, "the electricity starts one block right of the station");
        AssertEqual((ushort)0, electricity.Position.Y.Whole, "the electricity starts two blocks above the station");

        var native = new NativeEnemyProjectileListDefinitions.Reader(RoomEffectNativeListAddresses.SaveStationElectricity, electricity.InstructionTimer.RawValue);
        var shown = new HashSet<ushort>();
        int update = 0;
        for (; update < 400 && !native.Deleted; update++)
        {
            enemies.Projectiles.ProcessEnemyProjectileInstructions(electricity, null, Position16<RoomPixels>.Zero);
            native.Step(cartridge);
            string at = $"save station electricity update {update}";
            AssertEqual(!native.Deleted, electricity.IsActive, $"{at}: deletion");
            if (native.Deleted)
                break;
            AssertEqual(native.Timer, electricity.InstructionTimer.RawValue, $"{at}: timer");
            AssertEqual(native.Operand, electricity.PresentationOperandAddress, $"{at}: frame");
            shown.Add(electricity.PresentationOperandAddress);
        }
        AssertTrue(native.Deleted, "the native electricity ends within the compared updates");
        AssertTrue(shown.SetEquals(SaveStationElectricityVisualDefinitions.ProjectileFrames.Select(SaveStationElectricityVisualDefinitions.PresentationKey)),
            "the electricity shows every installed frame");
        Console.WriteLine($"  Save station electricity: {update} updates of its twenty cycles and deletion match bank $86.");
    }

    /// <summary>Runs the save station electricity program parity check against the repository cartridge.</summary>
    [VerificationCommand("--save-station-electricity-animations")]
    internal static int SaveStationElectricityAnimationsCommand()
    {
        Suite(nameof(VerifySaveStationElectricityAnimations), () => VerifySaveStationElectricityAnimations(LoadRepositoryRom()));
        return 0;
    }
}
