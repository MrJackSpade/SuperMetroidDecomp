using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Enemies.Shutters;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Records shutter activation, rider carry, and support gaps for the reported Brinstar overlap scenario.</summary>
internal static class ShutterEmbeddingVerification
{
    /// <summary>Runs the existing approach-sequence diagnostic for both shutters; negative gaps require native rounding comparison before interpretation.</summary>
    internal static void VerifyShutterEmbedding()
    {
        int activatedCases = 0;
        int carriedCases = 0;
        int minimumGap = 0;
        foreach (int slot in new[] { 0, 1 })
        foreach (int bombAt in new[] { 0, 4, 8, 12, 16, 20, 30, 40, 60 })
        foreach (int stopAt in new[] { 8, 12, 16, 20, 40, 60, 90 })
        {
            int offset = slot == 0 ? 32 : -32;
            SnesButton direction = slot == 0 ? SnesButton.Left : SnesButton.Right;
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.XrayScope, new Position16<RoomPixels>(0, 0));
            var samus = runtime.Player.IntroducedSamus;
            var platform = runtime.Enemies.SlotTable.Slots[slot];
            samus.Pose = SamusPoseId.MorphBallGroundRightPose;
            samus.Control.Unlock();
            samus.EquippedItems |= SamusEquipmentFlags.Bombs;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.Position = samus.Position.WithWhole(new((ushort)(platform.Position.X.Whole + offset), (ushort)(platform.Position.Y.Whole - platform.Radius.Y - samus.Kinematics.Radius.Y)));
            runtime.StepFrame(0);
            bool activated = false;
            bool carried = false;
            for (int frame = 0; frame < 220; frame++)
            {
                SnesButton input = frame < stopAt ? direction : SnesButton.None;
                if (frame == bombAt || frame == bombAt + 20) input |= runtime.Options.ControllerBindings.Shoot;
                runtime.StepFrame(input);
                activated |= (runtime.Enemies.VerticalShutterStates[slot] ?? throw new InvalidOperationException("Verification requires runtime.Enemies.VerticalShutterStates[slot].")).Function == VerticalShutterFunction.MovingUp;
                carried |= samus.Kinematics.ExtraDisplacement.Y.RawValue < 0;
                int gap = platform.Position.Y.Whole - platform.Radius.Y - samus.Position.Y.Whole - samus.Kinematics.Radius.Y;
                if (Math.Abs(samus.Position.X.Whole-platform.Position.X.Whole) < platform.Radius.X+samus.Kinematics.Radius.X &&
                    samus.Position.Y.Whole < platform.Position.Y.Whole && gap < minimumGap)
                {
                    minimumGap = gap;
                    Console.WriteLine($"Overlap candidate: slot={slot} offset={offset} bomb={bombAt} stop={stopAt}/{direction} frame={frame} gap={gap} Samus={samus.Position.X.Whole},{samus.Position.Y.Whole}/{(int)samus.Pose:X2} platformY={platform.Position.Y.Whole}");
                }
            }
            if (activated) activatedCases++;
            if (carried) carriedCases++;
        }
        AssertTrue(activatedCases > 0 && carriedCases > 0, "approach sweep reaches bomb activation and upward rider carry");
    // This diagnostic does not itself establish a regression result for #347:
    // a small negative gap alone does not prove the reported trapping.
        Console.WriteLine($"126 shutter approach sequences: {activatedCases} activate the shutter, {carriedCases} reach upward carry; minimum support gap={minimumGap}.");
    }

    /// <summary>Runs the shutter support-gap diagnostic from the verification command host.</summary>
    [VerificationCommand("--shutter-embedding")]
    internal static int ShutterEmbeddingCommand()
    {
        Suite(nameof(VerifyShutterEmbedding), () => VerifyShutterEmbedding());
        return 0;
    }
}
