using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The shades the import pre-colours Mother Brain's encounter in (#1512): each colour load of the encounter over her
/// body, brain, legs and spawned objects and over her room's pictures, which each owner lays in the order it loads them.
/// Her authored fades show their images' shades (<see cref="Fade"/>).
/// </summary>
public static class MotherBrainShades
{
    /// <summary>Her body, brain and rear legs in <paramref name="band"/>'s colours (#2259).</summary>
    public static PictureShade Health(MotherBrainHealthBand band) => new($"mother-brain-health-{Ensure.NotNull(band).Key}", 0);

    /// <summary>Her body, brain and rear legs at rainbow image <paramref name="image"/> (#2259).</summary>
    public static PictureShade Rainbow(MotherBrainRainbowImage image) => new("mother-brain-rainbow", image.Step);

    /// <summary>Her body, brain and rear legs restored after the rainbow beam.</summary>
    public static PictureShade Restored { get; } = new("mother-brain-restored", 0);

    /// <summary>The colours <paramref name="image"/> of one of her encounter's authored fades shows (#2270).</summary>
    public static PictureShade Fade(MotherBrainFadeImage image) => new(Ensure.NotNull(image.Fade).Key, image.Step);

    /// <summary>Her phase-two attack and rear-leg colours, which her ascent loads.</summary>
    public static PictureShade PhaseTwo { get; } = new("mother-brain-phase-two", 0);

    /// <summary>The glass shards' and tube projectiles' colours her room's entry loads.</summary>
    public static PictureShade RoomEntry { get; } = new("mother-brain-room-entry", 0);


    /// <summary>Her room at image <paramref name="image"/> of the fake death's flash (#2259).</summary>
    public static PictureShade Flash(MotherBrainRoomFlashImage image) => new("mother-brain-room-flash", image.Step);

    /// <summary>Her room's steady grey colours after the flash stops.</summary>
    public static PictureShade FinalRoom { get; } = new("mother-brain-final-room", 0);

    /// <summary>Her room's lights cleared to black as the Baby dies.</summary>
    public static PictureShade LightsOut { get; } = new("mother-brain-lights-out", 0);


    /// <summary>The Baby's colours the cutscene loads as it arrives.</summary>
    public static PictureShade BabyArrives { get; } = new("mother-brain-baby-arrives", 0);

}
