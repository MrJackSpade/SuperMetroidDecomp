namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The opening's discovery of the Metroid egg on SR388: where Samus runs, when the egg hatches and how the baby moves.</summary>
internal static class IntroBabyDiscoveryDefinitions
{
    /// <summary>Where Samus stands, facing left, as the scene starts.</summary>
    internal static Position16<RoomPixels> SamusStart => new(376, 147);

    /// <summary>The X Samus runs left past to stop and look around.</summary>
    internal const int DemoRunStopX = 178;

    /// <summary>The X Samus passes, moving left, for the waiting egg to hatch.</summary>
    internal const int EggHatchX = 169;

    /// <summary>How far above Samus the hatched baby rises toward on its first arc, in pixels.</summary>
    internal const int HatchedBabyHeightAboveSamus = 32;

    /// <summary>The updates the baby holds at the top of its first arc before it starts dancing around Samus.</summary>
    internal const int ArcTopHold = 128;

    /// <summary>The updates of its dance the baby counts, crying on the sixty-fourth and the last.</summary>
    internal const int CryingDanceUpdates = 128;

    /// <summary>How far above Samus the dancing baby keeps, in pixels.</summary>
    internal const int DanceHeightAboveSamus = 8;


    /// <summary>The hatched baby's fastest vertical speed either way: two and an eighth pixels an update.</summary>
    internal static readonly SignedEightEight VerticalSpeedLimit = SignedEightEight.FromPixelFraction(17, 8);

    /// <summary>The dancing baby's fastest horizontal speed either way: two and a half pixels an update.</summary>
    internal static readonly SignedEightEight HorizontalSpeedLimit = SignedEightEight.FromPixelFraction(5, 2);

    /// <summary>How much the baby's speed changes toward its target on each update: an eighth of a pixel an update.</summary>
    internal static readonly SignedEightEight BabyAcceleration = SignedEightEight.FromPixelFraction(1, 8);
}
