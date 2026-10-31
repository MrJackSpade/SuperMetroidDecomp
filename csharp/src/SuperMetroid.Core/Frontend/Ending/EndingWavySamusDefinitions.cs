namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The post-credits wave that ripples Samus sideways while the backdrop waits, and how it moves.</summary>
internal static class EndingWavySamusDefinitions
{
    /// <summary>The wavy Samus effect: the wave's peak displacement, in 1/256ths of a pixel (64 pixels).</summary>
    internal const int Amplitude = 16384;
    /// <summary>
    /// The wavy Samus effect: the sine steps the wave's angle advances each frame, so the ripple travels up
    /// the screen.
    /// </summary>
    internal const int AngleStepPerFrame = 8;
    /// <summary>$88:ECA2 / Instruction_Setup_WavySamus: the wave's angle at screen line zero before its first advance, one step before the cycle's start.</summary>
    internal const int InitialAngle = -1;
    /// <summary>
    /// $88:ECD1-ECD6 / PreInstruction_WavySamus: the screen lines of one whole cycle of the wave, a half cycle of 64 lines
    /// and its sign-inverted mirror.
    /// </summary>
    internal const int Period = 128;
    /// <summary>
    /// The frame the wave first shows: the frame it starts on only sets it up (Instruction_Setup_WavySamus), and the
    /// following frame is its first advance.
    /// </summary>
    internal const int FirstWaveAge = 2;
    /// <summary>$8B:E193 / CinematicFunction_PostCredits_WavySamus advances BG3Y by two per call.</summary>
    internal const int VerticalStep = 2;
}
