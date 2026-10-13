using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static SuiteTimingVerification;
using static WallJumpExpansionVerification;

internal static class WallJumpDustVerification
{
    internal static void VerifyWallJumpDust()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyWallJumpExpansion), () => VerifyWallJumpExpansion(bus));
        foreach (bool grapple in new[] { false, true })
        foreach (bool facingRight in new[] { false, true })
        for (int medium = 0; medium < 5; medium++)
        {
            var samus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(128, 96)) };
            // Grapple contact faces the wall; its accepted wall jump reverses that pose.
            samus.Pose = grapple
                ? facingRight ? SamusPoseId.GrappleWallContactRightPose : SamusPoseId.GrappleWallContactLeftPose
                : facingRight ? SamusPoseId.SpinJumpRightPose : SamusPoseId.SpinJumpLeftPose;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            var liquid = samus.LiquidPhysics;
            liquid.FxYPosition = medium is 1 or 3 ? (ushort)100 : medium == 4 ? (ushort)114 : ushort.MaxValue;
            liquid.LavaAcidYPosition = medium == 2 ? (ushort)100 : ushort.MaxValue;
            liquid.LiquidOptions = medium == 3 ? (ushort)4 : (ushort)0;
            liquid.AtmosphericEffects.SetSlot(3, 1, 2, 7, new(33, 44));
            liquid.AtmosphericEffects.SetSlot(2, 1, 1, 9, new(55, 66));
            if (grapple) samus.PoseTransitions.ApplyGrappleWallJump(bus);
            else samus.PoseTransitions.ApplyWallJumpTrigger(bus);

            bool suppressed = medium is 1 or 2;
            var dust = liquid.AtmosphericEffects.Slots[3];
            string context = $"wall dust grapple={grapple} right={facingRight} medium={medium}";
            // These exact four native output words are independently checked by the
            // cartridge-byte fixture: GrapplePoseAudit audit.exe ROM wall-jump-dust.
            AssertEqual(suppressed ? 0x0102 : 0x0600, dust.FrameAndType, context + " type/frame");
            AssertEqual(suppressed ? 7 : 3, dust.AnimationTimer.RawValue, context + " timer");
            AssertEqual(suppressed ? 33 : facingRight ? 122 : 134, dust.Position.X, context + " X");
            AssertEqual(suppressed ? 44 : 114, dust.Position.Y, context + " Y");
            AssertEqual(0x0101, liquid.AtmosphericEffects.Slots[2].FrameAndType, context + " preserves other slot");
        }
        Console.WriteLine("Wall-jump dust: both launch paths match native facing, liquid boundary, timer and slot-preservation outputs.");
    }

    [VerificationCommand("--wall-jump-dust")]
    internal static int WallJumpDustCommand()
    {
        Suite(nameof(VerifyWallJumpDust), () => VerifyWallJumpDust());
        return 0;
    }
}
