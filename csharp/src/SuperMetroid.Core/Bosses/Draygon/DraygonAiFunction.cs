namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's body function, held in its <c>var_A</c>. Each member names the bank-$A5 routine it ports; the
/// slot stores the member, not the routine's address. Members start at one so a cleared slot is never
/// mistaken for a function.
/// </summary>
public enum DraygonAiFunction : ushort
{
    /// <summary><c>$A5:871B Function_DraygonBody_FightIntro_InitialDelay</c>: loads four Evir sprites, then waits 256 gameplay frames.</summary>
    IntroInitialDelay = 1,
    /// <summary><c>$A5:878B Function_DraygonBody_FightIntro_Dance</c>: advances the opening Evir movement stream for 1,232 frames before the first swoop.</summary>
    IntroDance,
    /// <summary><c>$A5:87F4 Function_DraygonBody_SwoopRight_Setup</c>: constructs the swoop-height table, aims horizontal velocity at Samus, and installs right-facing graphics.</summary>
    SwoopRightSetup,
    /// <summary><c>$A5:88B1 Function_DraygonBody_SwoopRight_Descending</c>: traverses height samples backward while moving right toward the bottom of the swoop.</summary>
    SwoopRightDescending,
    /// <summary><c>$A5:8922 Function_DraygonBody_SwoopRight_Apex</c>: recalculates horizontal velocity toward the right exit for the ascending half.</summary>
    SwoopRightApex,
    /// <summary><c>$A5:8951 Function_DraygonBody_SwoopRight_Ascending</c>: traverses height samples forward and selects a leftward swoop or goop pass from the current RNG word.</summary>
    SwoopRightAscending,
    /// <summary><c>$A5:89B3 Function_DraygonBody_SwoopLeft_Setup</c>: reuses the generated height table, aims leftward velocity at Samus, and installs left-facing graphics.</summary>
    SwoopLeftSetup,
    /// <summary><c>$A5:8A00 Function_DraygonBody_SwoopLeft_Descending</c>: traverses height samples backward while moving left toward the bottom of the swoop.</summary>
    SwoopLeftDescending,
    /// <summary><c>$A5:8A50 Function_DraygonBody_SwoopLeft_Apex</c>: recalculates horizontal velocity toward the left reset position for the ascending half.</summary>
    SwoopLeftApex,
    /// <summary><c>$A5:8A90 Function_DraygonBody_SwoopLeft_Ascending</c>: completes the leftward ascent, resets X, and selects a rightward swoop or goop pass from the current RNG word.</summary>
    SwoopLeftAscending,
    /// <summary><c>$A5:8B0A Function_DraygonBody_GoopRight_Setup</c>: starts the one-pixel-per-frame rightward goop path at X=$FFB0, Y=$0180.</summary>
    GoopRightSetup,
    /// <summary><c>$A5:8B52 Function_DraygonBody_GoopRight_MoveUntilSamusInRange</c>: follows the oscillating path until horizontal separation from Samus is below $D0 pixels.</summary>
    GoopRight,
    /// <summary><c>$A5:8BAE Function_DraygonBody_GoopRight_FiringGoops</c>: requests right-facing goop lists from RNG samples and begins chasing when goop slows Samus.</summary>
    GoopRightTail,
    /// <summary><c>$A5:8C33 Function_DraygonBody_GoopRight_MoveUntilOffScreen</c>: finishes the rightward oscillating exit without firing goop.</summary>
    GoopRightRecovery,
    /// <summary><c>$A5:8C8E Function_DraygonBody_GoopLeft_Setup</c>: starts the one-pixel-per-frame leftward goop path at the right reset X and Y=$0180.</summary>
    GoopLeftSetup,
    /// <summary><c>$A5:8CD4 Function_DraygonBody_GoopLeft_MoveUntilSamusInRange</c>: follows the oscillating path until horizontal separation from Samus is below $D0 pixels.</summary>
    GoopLeft,
    /// <summary><c>$A5:8D30 Function_DraygonBody_GoopLeft_FiringGoops</c>: requests left-facing goop lists from RNG samples and begins chasing when goop slows Samus.</summary>
    GoopLeftTail,
    /// <summary><c>$A5:8DB2 Function_DraygonBody_GoopLeft_MoveUntilOffScreen</c>: completes the leftward exit, still allowing attached goop to trigger a chase.</summary>
    GoopLeftRecovery,
    /// <summary><c>$A5:8E19 Function_DraygonBody_ChaseSamus</c>: moves toward goop-slowed Samus at two pixels per frame and checks the eight-pixel claw window.</summary>
    TryGrabSamus,
    /// <summary><c>$A5:8F10 Function_DraygonBody_RepelledByGrapple</c>: despite the managed name, drops an active grapple and enters release without grabbing Samus.</summary>
    GrabbedSamus,
    /// <summary><c>$A5:8F1E Function_DraygonBody_GrabbedSamus_MovingToTargetPosition</c>: carries Samus toward room pixel ($100,$180), then starts the rising spiral.</summary>
    CarrySamus,
    /// <summary><c>$A5:8FD6 Function_DraygonBody_GrabbedSamus_RisingSpiralMovement</c>: expands and raises the carry orbit, with RNG-selected incidental tail whips.</summary>
    FlailWithSamus,
    /// <summary><c>$A5:90D4 Function_DraygonBody_GrabbedSamus_TailWhip</c>: holds the grabbed pair together during a 64-frame whip, then resumes the spiral.</summary>
    TailWhipWithSamus,
    /// <summary><c>$A5:9105 Function_DraygonBody_GrabbedSamus_FinalSpanking_Start</c>: installs the facing-specific four-repeat finishing tail-whip list.</summary>
    FinalTailWhips,
    /// <summary><c>$A5:9124 Function_DraygonBody_GrabbedSamus_FinalSpanking_Ongoing</c>: keeps Samus attached until the tail list changes the body function to release.</summary>
    FinalTailWhipsWait,
    /// <summary><c>$A5:9128 Function_DraygonBody_GrabbedSamus_FlailTail_FlyStraightUp</c>: releases Samus, restores ordinary collision, and starts a tail flail before retreat.</summary>
    ReleaseSamus,
    /// <summary><c>$A5:9154 Function_DraygonBody_GrabbedSamus_FlyStraightUp</c>: retreats upward four pixels per frame, then resets for a rightward swoop.</summary>
    FlyStraightUp,
    /// <summary><c>$A5:9185 Function_DraygonBody_DeathSequence_DriftToDeathSpot</c>: drifts toward room pixel ($100,$1E0), then spawns burial Evirs and retires the arms and tail.</summary>
    Dying,
    /// <summary><c>$A5:9294 Function_DraygonBody_DeathSequence_WaitForEvirs</c>: despite the managed name, waits $1A0 frames while Evirs approach before the body sinks.</summary>
    DyingSink,
    /// <summary><c>$A5:92AB Function_DraygonBody_DeathSequence_BuriedByEvirs</c>: sinks one pixel per frame to Y=$240, scatters drops, deletes the remaining parts, and persists defeat.</summary>
    DyingFinish,
}
