namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A frame Samus's animation starts on or a cue moves it to, with the delay the animation reads there (#2553): the game
/// starts the frame timer from the frame's own byte, whatever the frame holds. On one of the animation's own delays it is
/// that delay; on any other frame a pose start or a cue can reach, it is the byte the import decoded there
/// (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}.DelayRead"/>).
/// </summary>
/// <param name="Frame">The frame the animation stands on.</param>
/// <param name="Delay">The delay its timer starts from, before Samus's extra animation delay is added.</param>
internal readonly record struct SamusAnimationDelayFrame(SamusAnimationFrame Frame, SamusAnimationDelay Delay);
