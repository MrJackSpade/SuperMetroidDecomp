using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Bosses.Botwoon;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Botwoon's spit cooldown is a countdown that expires by wrapping below zero (DEC then BMI): the expiring step pins
/// it back to zero and returns the head to following its movement, and the hidden-spit state waits while it is nonzero.
/// </summary>
internal static class BotwoonAttackCooldownVerification
{
    /// <summary>
    /// Drives the production main AI (<c>RunBotwoonMainAi</c>) on a constructed head with its thirteen real body
    /// segments. In the moving frame the cooldown at one steps to zero and keeps cooling down, then at zero wraps to
    /// the negative branch, is pinned to zero and hands the head back to movement. In the hidden-spit state the gate
    /// keeps running the cooldown while the timer is nonzero and leaves the state once it reads zero.
    /// </summary>
    internal static void VerifyBotwoonAttackCooldown()
    {
        // Moving frame: the only path on which the cooldown starts at zero and wraps.
        (RoomEnemySystem moving, RoomEnemySlot movingHead, BotwoonEnemyState movingState) = NewBotwoon();
        movingState.Function = BotwoonEnemyFunction.ChooseNextAction;
        movingState.MovementFunction = BotwoonMovementFunction.MoveTowardHole;
        movingState.HeadFunction = BotwoonHeadFunction.AttackCooldown;
        movingState.AttackTimer = NativeCountdown.Start(1);

        moving.BotwoonBehavior.RunBotwoonMainAi(movingHead, Frame());
        AssertEqual(NativeCountdown.Start(0), movingState.AttackTimer, "a cooldown at one steps to zero");
        AssertEqual(BotwoonHeadFunction.AttackCooldown, movingState.HeadFunction, "reaching zero does not end the cooldown");

        moving.BotwoonBehavior.RunBotwoonMainAi(movingHead, Frame());
        AssertEqual(NativeCountdown.Start(0), movingState.AttackTimer, "the cooldown that wraps below zero is pinned back to zero");
        AssertEqual(BotwoonHeadFunction.AnimateFromMovement, movingState.HeadFunction, "the wrapped cooldown returns the head to following its movement");

        // Hidden spit: the state's gate runs the cooldown while it is nonzero and leaves once it reads zero.
        (RoomEnemySystem hidden, RoomEnemySlot hiddenHead, BotwoonEnemyState hiddenState) = NewBotwoon();
        hiddenState.Function = BotwoonEnemyFunction.SpitWhileHidden;
        hiddenState.HeadFunction = BotwoonHeadFunction.AttackCooldown;
        hiddenState.AttackTimer = NativeCountdown.Start(1);

        hidden.BotwoonBehavior.RunBotwoonMainAi(hiddenHead, Frame());
        AssertEqual(NativeCountdown.Start(0), hiddenState.AttackTimer, "the hidden-spit gate runs a nonzero cooldown");
        AssertEqual(BotwoonEnemyFunction.SpitWhileHidden, hiddenState.Function, "a running cooldown keeps Botwoon spitting from the hole");

        hidden.BotwoonBehavior.RunBotwoonMainAi(hiddenHead, Frame());
        AssertEqual(NativeCountdown.Start(0), hiddenState.AttackTimer, "the gate does not decrement a zero cooldown");
        AssertEqual(BotwoonEnemyFunction.FollowAuthoredPath, hiddenState.Function, "a zero cooldown ends the hidden spit");
        AssertEqual(BotwoonHeadFunction.AnimateFromMovement, hiddenState.HeadFunction, "leaving the hidden spit returns the head to movement");

        Console.WriteLine("  Botwoon attack cooldown: steps to zero, wraps and pins to zero, and gates the hidden spit on zero.");

        static ClockedEnemyAiFrame Frame() => new(
            new RoomEnemyFrame { Camera = Position16<RoomPixels>.Zero, TimeIsFrozen = false, Samus = new SamusState() },
            new FrameCounters(0));
    }

    /// <summary>
    /// A head in slot zero bound as Botwoon, away from every hole, with its thirteen body segments spawned by the
    /// production spawner and its health palette complete so the update reads no artwork.
    /// </summary>
    private static (RoomEnemySystem Enemies, RoomEnemySlot Head, BotwoonEnemyState State) NewBotwoon()
    {
        var enemies = new RoomEnemySystem();
        enemies.VerificationServices().NextRandom = (Func<ushort>)(() => 0);
        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        head.Bind(EnemyDefinitionId.Botwoon, head.Definition);
        head.Position = head.Position.WithWhole(new(1000, 1000));
        var state = new BotwoonEnemyState(head)
        {
            PalettePhase = BotwoonHealthPaletteDefinitions.CompletePhase,
            SegmentSpacingEntries = 3,
            Speed = 0,
        };
        enemies.BotwoonBehavior._botwoonState = state;
        for (int argument = 24; argument >= 0; argument -= 2)
            enemies.BotwoonBehavior.SpawnBotwoonBodySegment(head, state, (ushort)argument);
        return (enemies, head, state);
    }

    /// <summary>Runs the Botwoon attack-cooldown check.</summary>
    [VerificationCommand("--botwoon-attack-cooldown")]
    internal static int BotwoonAttackCooldownCommand()
    {
        Suite(nameof(VerifyBotwoonAttackCooldown), () => VerifyBotwoonAttackCooldown());
        return 0;
    }
}
