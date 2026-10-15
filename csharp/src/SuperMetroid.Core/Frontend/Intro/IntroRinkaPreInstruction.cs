namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Pre-instructions an intro flashback Rinka actor can select.</summary>
internal enum IntroRinkaPreInstruction : ushort
{
    /// <summary>No pre-instruction has been assigned yet.</summary>
    None = 0,

    /// <summary><c>RTS_8B93D9</c>, the shared cinematic-sprite no-op callback.</summary>
    NoOp = 0x93d9,

    /// <summary><c>PreInstruction_IntroRinka_Moving_HitsSamus</c> at $8B:B8D8.</summary>
    MovingHitsSamus = 0xb8d8,

    /// <summary><c>PreInstruction_IntroRinka_Moving_MissesSamus</c> at $8B:B93B.</summary>
    MovingMissesSamus = 0xb93b,
}
