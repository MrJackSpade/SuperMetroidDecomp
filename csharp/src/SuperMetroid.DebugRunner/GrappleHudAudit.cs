using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>Normal HUD selection must reach the translated grapple actor without a diagnostic flag.</summary>
internal static class GrappleHudAudit
{
    private const ushort GrappleRoom = 0xac2b;
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        foreach (var test in new (byte Pose, bool Admitted)[]
        {
            (SamusPoseIds.FacingRightNormalPose, true),
            (SamusPoseIds.StandingAimUpRightPose, true),
            (SamusPoseIds.CrouchingTransitionAimUpRightPose, true),
            (SamusPoseIds.SpinJumpRightPose, false),
            (SamusPoseIds.MorphBallGroundRightPose, false),
            (SamusPoseIds.ForwardFacingPowerSuitPose, false),
            (SamusPoseIds.ForwardFacingSuitedPose, false),
        })
        {
            var probe = new SamusState { Pose = test.Pose, SelectedHudItem = 4 };
            if (SamusGrappleHudInput.IsSelectedAndAdmitted(bus, probe) != test.Admitted)
                throw new InvalidDataException($"Grapple HUD pose admission differs for {test.Pose:X2}.");
            probe.InputLocked = true;
            if (SamusGrappleHudInput.IsSelectedAndAdmitted(bus, probe))
                throw new InvalidDataException("Locked Samus admitted Grapple HUD input.");
        }
        var sentinel = new SamusState { Pose = SamusPoseIds.MorphBallGroundRightPose };
        SamusGrappleMovement.BeginFiring(bus, sentinel);
        if (sentinel.Grapple.Phase != GrapplePhase.CancelPending)
            throw new InvalidDataException("Native sentinel shot direction did not select cancellation.");
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug); runtime.RunNmi(0, true);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(GrappleRoom, 0, 11);
        var samus = runtime.Samus!;
        samus.XPosition = 39; samus.YPosition = 155;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.InitializeAnimation(bus);
        samus.InputLocked = false;
        samus.EquippedItems = (ushort)SamusEquipmentFlags.GrappleBeam;
        samus.SelectedHudItem = 3;
        runtime.StepFrame((ushort)SnesButton.Select);
        if (samus.SelectedHudItem != 4 || runtime.DebugGrappleItemSelected)
            throw new InvalidDataException("Normal Select did not choose Grapple independently of the diagnostic flag.");
        runtime.StepFrame((ushort)SnesButton.X);
        if (samus.Grapple.Phase != GrapplePhase.Firing || !runtime.LastGrappleDrawingHandlerActive)
            throw new InvalidDataException($"Normal HUD Fire failed to start/draw Grapple: {samus.Grapple.Phase}.");
        for (int tick = 0; tick < 20; tick++) runtime.StepFrame((ushort)SnesButton.X);
        runtime.StepFrame(0);
        for (int tick = 0; tick < 10 && samus.Grapple.Phase != GrapplePhase.Inactive; tick++) runtime.StepFrame(0);
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
            throw new InvalidDataException("Released normal HUD Grapple did not cancel.");
        // Aim beneath the actual room's pair of ceiling grapple blocks, then let
        // native extension and terrain acquisition establish the anchor.
        samus.XPosition = 120; samus.YPosition = 120;
        samus.Pose = SamusPoseIds.StandingAimUpRightPose;
        samus.InitializeAnimation(bus);
        bool connected = false;
        for (int tick = 0; tick < 24; tick++)
        {
            runtime.StepFrame((ushort)(SnesButton.Up | SnesButton.X));
            connected |= runtime.LastGrappleMovement?.Connected == true;
        }
        if (!connected)
            throw new InvalidDataException($"Normal HUD grapple failed to connect to retail ceiling tiles: phase={samus.Grapple.Phase}.");
        Console.WriteLine("Grapple normal HUD: pose admission, Select, Fire, drawing, release and retail ceiling connection work without the debug selector.");
        return 0;
    }
}
