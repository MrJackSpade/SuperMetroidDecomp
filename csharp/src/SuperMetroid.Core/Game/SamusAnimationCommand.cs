namespace SuperMetroid.Core.Game;

/// <summary>Bank-$91 animation control commands, as interpreted by <c>$90:8324-$84DB</c>.</summary>
/// <remarks>Retail never compiles <c>$F1-$F5</c> or <c>$FA</c>; the interpreter still accepts them.</remarks>
internal enum SamusAnimationCommand : byte
{
    /// <summary><c>$F0</c>: one of six no-op slots. The timer is not reloaded, so the next delay starts a frame later.</summary>
    Hold = 0xF0,

    /// <summary><c>$F6</c>, <c>$90:8346</c>: loop to frame zero unless energy is below 30, then continue into the drained breathing frames.</summary>
    LoopUnlessLowEnergy = 0xF6,

    /// <summary><c>$F7</c>, <c>$90:8360</c>: install the drained falling handler and continue past the command.</summary>
    InstallDrainedFall = 0xF7,

    /// <summary><c>$F8 pp</c>, <c>$90:8370</c>: publish a turn's final pose unless an auto-jump input supersedes it.</summary>
    TurnTransition = 0xF8,

    /// <summary><c>$F9 eeee gg aa GG AA</c>, <c>$90:839A</c>: item- and vertical-motion-selected transitional pose.</summary>
    ItemAirborneTransition = 0xF9,

    /// <summary><c>$FB</c>, <c>$90:841D</c>: continue into the ordinary, Space Jump or Screw Attack wall-jump frames.</summary>
    WallJumpSelect = 0xFB,

    /// <summary><c>$FC eeee gg aa</c>, <c>$90:848B</c>: item-selected transitional pose.</summary>
    ItemTransition = 0xFC,

    /// <summary><c>$FD pp</c>, <c>$90:83A0</c>: publish a transitional pose.</summary>
    Transition = 0xFD,

    /// <summary><c>$FE nn</c>, <c>$90:84C7</c>: move back <c>nn</c> bytes to an earlier frame.</summary>
    RepeatFrom = 0xFE,

    /// <summary><c>$FF</c>, <c>$90:84DB</c>: restart at frame zero.</summary>
    Loop = 0xFF,
}
