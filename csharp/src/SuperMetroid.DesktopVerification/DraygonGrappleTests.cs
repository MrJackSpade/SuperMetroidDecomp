using SuperMetroid.Core.Game;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    private static void VerifyDraygonGrapple()
    {
        var loaded = DebuggerFixtureLoader.Load("draygon-body-position", 0);
        var runtime = loaded.Game.RuntimeForVerification!;
        var samus = runtime.Samus!;
        for (int frame = 0; !samus.DraygonGrabbed.IsActive && frame < 600; frame++)
            loaded.Game.Step(0);
        if (!samus.DraygonGrabbed.IsActive) throw new InvalidOperationException("Fixture did not reach Draygon's grab.");
        samus.SelectedHudItem = 4;
        loaded.Game.Step(0);
        loaded.Game.Step((ushort)SnesButton.X);
        if (samus.Grapple.Phase != GrapplePhase.Firing)
            throw new InvalidOperationException($"Held Samus cannot fire: pose {samus.Pose:X2}, grapple {samus.Grapple.Phase}.");
        loaded.Game.Step((ushort)SnesButton.X);
        if (samus.Grapple.RopeLength == 0)
            throw new InvalidOperationException("Held grapple did not extend on the next frame.");
        byte heldPose = samus.Pose;
        ushort heldX = samus.XPosition, heldY = samus.YPosition;
        // Exercise the real connection consumer with a controlled attachable endpoint.
        // Acquisition semantics are shared with the room-block path; no pose is fabricated.
        var connected = SamusGrappleMovement.StepFiring(loaded.AddressSpace, runtime.LevelData!, samus,
            (ushort)SnesButton.X, runtime.Plms,
            (x, y) => new GrappleEnemyCollision(true, GrappleEnemyReaction.Attach, 0, x, y, 0));
        if (!connected.Connected || samus.Grapple.Phase != GrapplePhase.ConnectedLocked)
            throw new InvalidOperationException("Held grapple did not select the native locked connection.");
        CheckHeldBody();
        SamusGrappleMovement.Step(loaded.AddressSpace, runtime.LevelData!, samus, 0, 0);
        SamusGrappleMovement.CompleteFiringCancellation(loaded.AddressSpace, runtime.LevelData!, samus);
        if (samus.Grapple.Phase != GrapplePhase.Inactive)
            throw new InvalidOperationException("Held grapple did not cancel on release.");
        CheckHeldBody();
        foreach (byte pose in new[] { SamusPoseIds.DraygonGrabbedMovingLeftPose, SamusPoseIds.DraygonGrabbedMovingRightPose })
        {
            for (int dpad = 0; dpad < 16; dpad++)
            {
                samus.Pose = pose;
                ushort input = (ushort)(dpad << 8);
                SamusGrappleMovement.BeginFiring(loaded.AddressSpace, samus, input);
                if (samus.Grapple.Phase != GrapplePhase.Firing)
                    throw new InvalidOperationException($"Moving held pose {pose:X2} rejects grapple instead of using $9B:C6B2.");
                bool left = pose == SamusPoseIds.DraygonGrabbedMovingLeftPose;
                int expected = left ? 7 : 2;
                if ((input & (left ? 0x200 : 0x100)) != 0)
                    expected += (input & 0x400) != 0 ? (left ? -1 : 1) : (input & 0x800) != 0 ? (left ? 1 : -1) : 0;
                if (samus.Grapple.FireDirection != expected)
                    throw new InvalidOperationException("Held grapple direction differs from native D-pad restriction.");
                samus.Grapple.Phase = GrapplePhase.Inactive;
            }
        }
        Console.WriteLine("Draygon held grapple: runtime input fires/extends; connection locks and release cancels without changing held pose, position or owner.");

        void CheckHeldBody()
        {
            if (!samus.DraygonGrabbed.IsActive || samus.Pose != heldPose || samus.XPosition != heldX || samus.YPosition != heldY)
                throw new InvalidOperationException("Grapple connection/release displaced Draygon's held body.");
        }
    }
}
