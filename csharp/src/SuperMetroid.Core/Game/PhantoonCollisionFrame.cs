namespace SuperMetroid.Core.Game;

/// <summary>The Phantoon BG2 frames whose collision differs from the single inert point.</summary>
internal enum PhantoonCollisionFrame : ushort
{
    /// <summary>Body with its complete five-rectangle hitbox, $A7:DEE7.</summary>
    BodyFullHitbox = 0xdee7,
    /// <summary>Body with only the vulnerable eye hitbox, $A7:DEF1.</summary>
    BodyEyeHitboxOnly = 0xdef1,
    /// <summary>First two-component tentacle frame, $A7:DFB3.</summary>
    Tentacles0 = 0xdfb3,
    /// <summary>Second two-component tentacle frame, $A7:DFC5.</summary>
    Tentacles1 = 0xdfc5,
    /// <summary>Third two-component tentacle frame, $A7:DFD7.</summary>
    Tentacles2 = 0xdfd7,
}
