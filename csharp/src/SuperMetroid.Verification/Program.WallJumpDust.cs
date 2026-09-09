using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyWallJumpDust()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        VerifyWallJumpExpansion(bus);
        foreach (bool grapple in new[] { false, true })
        foreach (bool facingRight in new[] { false, true })
        for (int medium = 0; medium < 5; medium++)
        {
            var samus = new SamusState { XPosition = 128, YPosition = 96 };
            // Grapple contact faces the wall; its accepted wall jump reverses that pose.
            samus.Pose = grapple
                ? facingRight ? SamusPoseIds.GrappleWallContactRightPose : SamusPoseIds.GrappleWallContactLeftPose
                : facingRight ? (byte)0x19 : (byte)0x1a;
            samus.RefreshCollisionRadii(bus);
            samus.InitializeAnimation(bus);
            var liquid = samus.LiquidPhysics;
            liquid.FxYPosition = medium is 1 or 3 ? (ushort)100 : medium == 4 ? (ushort)114 : ushort.MaxValue;
            liquid.LavaAcidYPosition = medium == 2 ? (ushort)100 : ushort.MaxValue;
            liquid.LiquidOptions = medium == 3 ? (ushort)4 : (ushort)0;
            liquid.AtmosphericEffects.SetSlot(3, 1, 2, 7, 33, 44);
            liquid.AtmosphericEffects.SetSlot(2, 1, 1, 9, 55, 66);
            if (grapple) samus.ApplyGrappleWallJump(bus);
            else samus.ApplyWallJumpTrigger(bus);

            bool suppressed = medium is 1 or 2;
            var dust = liquid.AtmosphericEffects.Slots[3];
            string context = $"wall dust grapple={grapple} right={facingRight} medium={medium}";
            // These exact four native output words are independently checked by the
            // cartridge-byte fixture: GrapplePoseAudit audit.exe ROM wall-jump-dust.
            AssertEqual(suppressed ? 0x0102 : 0x0600, dust.FrameAndType, context + " type/frame");
            AssertEqual(suppressed ? 7 : 3, dust.AnimationTimer, context + " timer");
            AssertEqual(suppressed ? 33 : facingRight ? 122 : 134, dust.XPosition, context + " X");
            AssertEqual(suppressed ? 44 : 114, dust.YPosition, context + " Y");
            AssertEqual(0x0101, liquid.AtmosphericEffects.Slots[2].FrameAndType, context + " preserves other slot");
        }
        Console.WriteLine("Wall-jump dust: both launch paths match native facing, liquid boundary, timer and slot-preservation outputs.");
    }
}
