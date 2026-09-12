using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusVerticalDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        var guard = new SlopeHeightNoReadBus();
        var samus = new SamusState { XPosition = 128, YPosition = 128 };
        int cases = 0;
        for (int equipment = 0; equipment < 8; equipment++)
        for (int environment = 0; environment < 4; environment++)
        for (int launch = 0; launch < 3; launch++)
        {
            bool high = (equipment & 1) != 0;
            bool gravity = (equipment & 2) != 0;
            bool boost = (equipment & 4) != 0;
            samus.EquippedItems = (ushort)(
                (high ? (ushort)SamusEquipmentFlags.HiJumpBoots : 0) |
                (gravity ? (ushort)SamusEquipmentFlags.GravitySuit : 0) |
                (boost ? (ushort)SamusEquipmentFlags.SpeedBooster : 0));
            samus.LiquidPhysics.FxYPosition = environment is 1 or 3 ? (ushort)0 : ushort.MaxValue;
            samus.LiquidPhysics.LavaAcidYPosition = environment == 2 ? (ushort)0 : ushort.MaxValue;
            samus.LiquidPhysics.LiquidOptions = environment == 3 ? (ushort)4 : (ushort)0;
            int medium = launch == 2 || gravity || environment == 3 ? 0 : environment;
            int address = launch == 2 ? 0x909eb9 : launch == 1
                ? high ? 0x909edd : 0x909ed1
                : high ? 0x909ec5 : 0x909eb9;
            ushort whole = Word(address + medium * 2);
            ushort fraction = Word(address + 6 + medium * 2);
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                samus.HorizontalSpeed.ExtraRunSpeed = (ushort)raw;
                samus.HorizontalSpeed.ExtraRunSubspeed = (ushort)raw;
                samus.Kinematics.YAcceleration = 999;
                samus.Kinematics.YSubacceleration = 999;
                if (launch == 2) SamusAerialMovement.InitializeDryAirJump(guard, samus);
                else if (launch == 1) SamusAerialMovement.InitializeWallJump(guard, samus);
                else SamusAerialMovement.InitializeJump(guard, samus);
                AssertEqual(unchecked((ushort)(whole + (boost ? raw >> 1 : 0))), samus.Kinematics.YSpeed, "Native independent whole launch addition");
                AssertEqual(unchecked((ushort)(fraction + (boost ? raw : 0))), samus.Kinematics.YSubspeed, "Native fractional overflow does not carry");
                AssertEqual(Word(0x909ea7 + medium * 2), samus.Kinematics.YAcceleration, "Native gravity whole word");
                AssertEqual(Word(0x909ea1 + medium * 2), samus.Kinematics.YSubacceleration, "Native gravity fractional word");
                AssertEqual((ushort)1, samus.Kinematics.YDirection, "Launch remains upward");
                cases++;
            }
        }
        Console.WriteLine($"Compiled Samus vertical definitions: {cases} actual jump/wall-jump/dry launches match native words with all ROM access denied.");
    }
}
