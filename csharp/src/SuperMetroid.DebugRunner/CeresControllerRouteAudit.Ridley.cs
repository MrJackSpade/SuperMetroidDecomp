using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Contains the controller policy for Ceres Ridley's reveal, battle, and native escape
/// presentation. Keeping enemy-specific decisions separate prevents the full-route audit
/// from becoming another monolith while still sharing its failure-frame diagnostics.
/// </summary>
internal static partial class CeresControllerRouteAudit
{
    /// <summary>
    /// Waits through Ridley's native reveal and fights using only directional, aim, jump,
    /// and discrete beam inputs. Actor coordinates guide the same decisions visible to a
    /// player, while the room enemy system remains the sole owner of hit counting, damage,
    /// retreat, Baby retrieval, and the Mode-7 getaway.
    /// </summary>
    private static RidleyBattleResult DriveRidleyBattle(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        EarlyControllerRouteAudit.ControllerRouteHost host,
        int maximumFrames)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Ceres Ridley battle began without Samus.");
        RidleyEnemyState ridley = runtime.Enemies.CeresRidley
            ?? throw new InvalidDataException("Ceres room did not load Ridley.");
        RoomEnemySlot ridleySlot = runtime.Enemies.Slots[0];
        if (ridleySlot.EnemyDefinitionPointer != 0xe13f)
        {
            throw new InvalidDataException(
                $"Ceres Ridley slot selected enemy ${ridleySlot.EnemyDefinitionPointer:X4}.");
        }
        int revealFrames = 0;
        bool observedEscapeMusic = false;
        while (ridley.Function != RidleyAiFunction.CeresHovering &&
            revealFrames < 2400)
        {
            host.StepFrame(0);
            observedEscapeMusic |= runtime.Enemies.MusicRequests.Contains(
                new EnemyMusicRequest(Entry: 5, DelayFrames: 8));
            revealFrames++;
        }
        if (ridley.Function != RidleyAiFunction.CeresHovering)
        {
            throw new InvalidDataException(
                $"Ridley reveal stopped at $A6:{(ushort)ridley.Function:X4} after " +
                $"{revealFrames} frames.");
        }
        if (!observedEscapeMusic)
        {
            throw new InvalidDataException(
                "Ridley's final body-fade frame did not queue Ceres battle/escape track five.");
        }

        int fireEdges = 0;
        int aimHoldFrames = 0;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            if (runtime.Enemies.CeresStatus != 0 ||
                ridley.Function == RidleyAiFunction.CeresInactive)
            {
                return new RidleyBattleResult(revealFrames + frame, fireEdges);
            }

            int xDistance = ridleySlot.XPosition - samus.XPosition;
            int yDistance = ridleySlot.YPosition - samus.YPosition;
            bool facesLeft = SamusState.IsFacingLeft(bus, samus.Pose);
            bool facesRidley = xDistance < 0 == facesLeft;
            ushort input = 0;
            if (!facesRidley)
            {
                input = (ushort)(xDistance < 0 ? SnesButton.Left : SnesButton.Right);
                aimHoldFrames = 0;
            }
            else
            {
                int absoluteX = Math.Abs(xDistance);
                if (absoluteX < 52)
                {
                    // Create a firing lane before the body or seven-segment tail can overlap
                    // Samus. A periodic full jump uses ordinary collision to cross the actor
                    // when retreating into the arena wall is no longer possible.
                    input = (ushort)(xDistance < 0 ? SnesButton.Right : SnesButton.Left);
                    if (frame % 96 < 36)
                        input |= (ushort)SnesButton.A;
                    aimHoldFrames = 0;
                }
                else
                {
                    if (yDistance < -18)
                        input |= (ushort)SnesButton.R;
                    else if (yDistance > 18)
                        input |= (ushort)SnesButton.L;
                    if (aimHoldFrames != 0 && frame % 6 == 0)
                    {
                        input |= (ushort)SnesButton.X;
                        fireEdges++;
                    }
                    aimHoldFrames++;
                }
            }

            host.StepFrame(input);
            if (samus.Health == 0)
            {
                WriteFailureFrame(runtime, "Ceres Ridley battle Samus defeated");
                throw new InvalidDataException(
                    $"Samus was defeated after {frame + 1} Ridley battle frames; " +
                    $"hits={ridley.HitCounter}, fire edges={fireEdges}.");
            }
            if (frame % 600 == 599)
            {
                Console.WriteLine(
                    $"  Ceres Ridley f{frame + 1}: Samus=(${samus.XPosition:X4}," +
                    $"${samus.YPosition:X4})/{samus.Health}, Ridley=(${ridleySlot.XPosition:X4}," +
                    $"${ridleySlot.YPosition:X4})/fn=${(ushort)ridley.Function:X4}, " +
                    $"hits={ridley.HitCounter}, fire={fireEdges}.");
            }
        }

        WriteFailureFrame(runtime, "Ceres Ridley battle timeout");
        throw new InvalidDataException(
            $"Ridley did not retreat after {maximumFrames} battle frames; " +
            $"function=$A6:{(ushort)ridley.Function:X4}, hits={ridley.HitCounter}, " +
            $"Samus={samus.Health}, fire edges={fireEdges}.");
    }

    /// <summary>
    /// Advances the cartridge-owned Baby retrieval, rotating getaway, warning transfer,
    /// and self-destruct handoff without pressing a controller shortcut. Completion requires
    /// both status two and completion of Samus's dedicated push handler, so the subsequent
    /// escape policy cannot move during Mode-7 ownership or a partially installed warning.
    /// </summary>
    private static int DriveRidleyEscapePresentation(
        SuperMetroidRuntime runtime,
        EarlyControllerRouteAudit.ControllerRouteHost host,
        int maximumFrames)
    {
        RidleyEnemyState ridley = runtime.Enemies.CeresRidley
            ?? throw new InvalidOperationException("Ridley escape lost its actor state.");
        SamusState samus = runtime.Samus
            ?? throw new InvalidOperationException("Ridley escape lost Samus.");
        bool sawMode7 = ridley.Mode7Active;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            host.StepFrame(0);
            sawMode7 |= ridley.Mode7Active;
            if (runtime.Enemies.CeresStatus == 2 &&
                ridley.Mode7Finished &&
                !ridley.Mode7Active &&
                !samus.CeresRidleyEjection.IsActive)
            {
                if (!sawMode7 || runtime.EscapeTimer.State == EscapeTimerState.Inactive)
                {
                    throw new InvalidDataException(
                        $"Ceres escape handoff omitted Mode 7 or the timer: " +
                        $"mode7={sawMode7}/{ridley.Mode7Finished}, timer={runtime.EscapeTimer.State}.");
                }
                // The wall-collision call restores the normal handler but deliberately
                // leaves `$53/$54` current. Native `$91:A8E4/$A8EC` exits that family only
                // while Jump plus the direction opposite the hurt pose are held. Both table
                // records have required-new word zero, so this is not a synthetic edge or
                // timing shortcut. Its normal arc/gravity returns Samus to the door row.
                ushort ejectionExitInput = samus.Pose == SamusPoseIds.KnockbackLeftPose
                    ? (ushort)(SnesButton.Right | SnesButton.A)
                    : (ushort)(SnesButton.Left | SnesButton.A);
                host.StepFrame(ejectionExitInput);
                if (samus.Pose is SamusPoseIds.KnockbackRightPose or SamusPoseIds.KnockbackLeftPose)
                {
                    throw new InvalidDataException(
                        $"Ceres ejection chord retained type-$0A pose ${samus.Pose:X2}.");
                }
                return frame + 2;
            }
        }

        WriteFailureFrame(runtime, "Ceres Ridley escape presentation timeout");
        throw new InvalidDataException(
            $"Ceres escape presentation did not restore control after {maximumFrames} frames; " +
            $"status={runtime.Enemies.CeresStatus}, function=$A6:{(ushort)ridley.Function:X4}, " +
            $"mode7={ridley.Mode7Active}/{ridley.Mode7Finished}, locked={samus.InputLocked}.");
    }

    private readonly record struct RidleyBattleResult(int Frames, int FireEdges);
}
