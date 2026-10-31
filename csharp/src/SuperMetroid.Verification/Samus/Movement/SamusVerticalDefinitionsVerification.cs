using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Fx;
using static SuperMetroid.Verification.Rooms.LevelData.SharedSlopeHeightsVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Samus.Movement;

/// <summary>Checks compiled jump, wall-jump, and dry-air launch words against
/// native vertical physics across equipment and liquid combinations.</summary>
internal static class SamusVerticalDefinitionsVerification
{
    /// <summary>Exercises production launch initialization for every source
    /// speed word, including independent whole and fractional booster additions.</summary>
    internal static void VerifySamusVerticalDefinitions(SuperMetroidAddressSpace rom)
    {
        var guard = NewSlopeHeightNoReadBus();
        var samus = new SamusState(InstalledFixtures.AnimationContinuation) { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)) };
        int cases = 0;
        for (int equipment = 0; equipment < 8; equipment++)
        for (int environment = 0; environment < 4; environment++)
        for (int launch = 0; launch < 3; launch++)
        {
            bool high = (equipment & 1) != 0;
            bool gravity = (equipment & 2) != 0;
            bool boost = (equipment & 4) != 0;
            samus.EquippedItems = ((high ? SamusEquipmentFlags.HiJumpBoots : 0) | (gravity ? SamusEquipmentFlags.GravitySuit : 0) | (boost ? SamusEquipmentFlags.SpeedBooster : 0));
            samus.LiquidPhysics.FxYPosition = environment is 1 or 3 ? (ushort)0 : ushort.MaxValue;
            samus.LiquidPhysics.LavaAcidYPosition = environment == 2 ? (ushort)0 : ushort.MaxValue;
            samus.LiquidPhysics.LiquidOptions = environment == 3 ? RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave : RoomLiquidOptions.None;
            int medium = launch == 2 || gravity || environment == 3 ? 0 : environment;
            int address = launch == 2 ? 0x909eb9 : launch == 1
                ? high ? 0x909edd : 0x909ed1
                : high ? 0x909ec5 : 0x909eb9;
            ushort whole = CartridgeImportSource.Require(rom).ReadCartridgeWord((address + medium * 2));
            ushort fraction = CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6 + medium * 2));
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen((ushort)raw, (ushort)raw);
                samus.Kinematics.YAcceleration = new UnsignedSixteenSixteen(999, 999);
                if (launch == 2) SamusAerialMovement.InitializeDryAirJump(guard, samus);
                else if (launch == 1) SamusAerialMovement.InitializeWallJump(samus);
                else SamusAerialMovement.InitializeJump(samus);
                AssertEqual(unchecked((ushort)(whole + (boost ? raw >> 1 : 0))), samus.Kinematics.YSpeed.Whole, "Native independent whole launch addition");
                AssertEqual(unchecked((ushort)(fraction + (boost ? raw : 0))), samus.Kinematics.YSpeed.Fraction, "Native fractional overflow does not carry");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x909ea7 + medium * 2)), samus.Kinematics.YAcceleration.Whole, "Native gravity whole word");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x909ea1 + medium * 2)), samus.Kinematics.YAcceleration.Fraction, "Native gravity fractional word");
                AssertEqual(SamusVerticalDirection.Up, samus.Kinematics.YDirection, "Launch remains upward");
                cases++;
            }
        }
        Console.WriteLine($"Compiled Samus vertical definitions: {cases} actual jump/wall-jump/dry launches match native words with all ROM access denied.");
    }
}
