namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The definition data of Mother Brain's hand beam: the arena a fired beam explodes on leaving.</summary>
internal static partial class MotherBrainHandBeamDefinitions
{
    /// <summary>The speed the hand's beam flies at toward Samus: twelve pixels an update.</summary>
    internal static readonly SignedEightEight Speed = SignedEightEight.FromPixels(12);

    /// <summary>The arena's top-left corner: a fired beam left of its X or above its Y has left the arena.</summary>
    internal static readonly Position16<RoomPixels> ArenaTopLeft = new(2, 34);

    /// <summary>
    /// The corner just past the arena's bottom-right: a fired beam at or right of its X, or at or below its Y, has left
    /// the arena.
    /// </summary>
    internal static readonly Position16<RoomPixels> ArenaBottomRight = new(238, 206);
}
