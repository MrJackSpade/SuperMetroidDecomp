namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A3 function pointer stored in a Skultera's variable A.</summary>
public enum SkulteraEnemyFunction : ushort
{
    /// <summary>$A3:9132, Function_Skultera_SwimmingLeft: applies signed 16.16 leftward displacement and whole-pixel sine-offset changes; either axis collision starts a right turn.</summary>
    SwimmingLeft = 0x9132,
    /// <summary>$A3:91AB, Function_Skultera_SwimmingRight: mirrors leftward swimming with the rightward speed; unobstructed movement advances the low-byte angle, and either axis collision starts a left turn.</summary>
    SwimmingRight = 0x91ab,
    /// <summary>$A3:9224, Function_Skultera_TurningRight: waits without swimming until animation sets the turn-finished flag, then clears it, negates the angle delta, and installs rightward swimming.</summary>
    TurningRight = 0x9224,
    /// <summary>$A3:9256, Function_Skultera_TurningLeft: waits for the animation's turn-finished flag, then clears it, negates the angle delta, and installs leftward swimming.</summary>
    TurningLeft = 0x9256,
}
