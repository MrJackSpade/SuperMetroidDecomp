namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The two Samus Eater block actors, the Brinstar floor and ceiling plants (block actors, not Yapping Maws): the floor
/// plant takes the floor setup and bite, the ceiling plant the ceiling setup and bite. Mounting direction follows the
/// setup, including opposite alignment edges and held-Y adjustments. Both run the same chew with their own artwork; the
/// ceiling plant chews twice as many times.
/// </summary>
internal static class SamusEaterObjectDefinitions
{
    /// <summary>How many chews the floor plant makes.</summary>
    internal const int FloorChews = 4;

    /// <summary>How many chews the ceiling plant makes.</summary>
    internal const int CeilingChews = 8;

    /// <summary>How long each chew image shows.</summary>
    internal const int ChewFrameDuration = 5;

    /// <summary>How long the closed mouth shows after the release.</summary>
    internal const int ReleaseDuration = 96;

    /// <summary>The Brinstar floor plant.</summary>
    public static readonly SamusEaterObjectDefinition Floor = new(Side: VerticalDirection.Down, FloorChews, Chew(SamusEaterDraw.FloorTeethBared, SamusEaterDraw.FloorTeethPeeking, SamusEaterDraw.FloorFangsOut), SamusEaterDraw.FloorFangsOut, SamusEaterDraw.FloorIdle);

    /// <summary>The Brinstar ceiling plant.</summary>
    public static readonly SamusEaterObjectDefinition Ceiling = new(Side: VerticalDirection.Up, CeilingChews, Chew(SamusEaterDraw.CeilingTeethBared, SamusEaterDraw.CeilingTeethPeeking, SamusEaterDraw.CeilingFangsOut), SamusEaterDraw.CeilingFangsOut, SamusEaterDraw.CeilingIdle);

    /// <summary>
    /// One chew over a plant's looks, each for <see cref="ChewFrameDuration"/>: its teeth bared, peeking and bared again, its
    /// fangs out with the bite's sound and damage, then again with the damage alone.
    /// </summary>
    private static SamusEaterChewFrame[] Chew(SamusEaterDraw teethBared, SamusEaterDraw teethPeeking, SamusEaterDraw fangsOut) =>
    [
        new(teethBared, ChewFrameDuration, SamusEaterBite.None),
        new(teethPeeking, ChewFrameDuration, SamusEaterBite.None),
        new(teethBared, ChewFrameDuration, SamusEaterBite.None),
        new(fangsOut, ChewFrameDuration, SamusEaterBite.SoundAndDamage),
        new(teethBared, ChewFrameDuration, SamusEaterBite.None),
        new(teethPeeking, ChewFrameDuration, SamusEaterBite.None),
        new(teethBared, ChewFrameDuration, SamusEaterBite.None),
        new(fangsOut, ChewFrameDuration, SamusEaterBite.Damage),
    ];

    /// <summary>The chew sound.</summary>
    public static readonly SoundEffectId ChewSound = EnemySound.PlantChewing;

    /// <summary>The chew sound's request priority.</summary>
    internal static readonly SoundRequestPriority ChewSoundPriority = SoundRequestPriority.Normal;

    /// <summary>
    /// The frames of invincibility the hold sustains on Samus every frame (<see cref="WrappingCountdown.Sustained"/>), so she
    /// stays invincible while held.
    /// </summary>
    public const int HeldInvincibilityFrames = 16;

    /// <summary>The immunity the release gives Samus, in frames.</summary>
    public const int ReleaseInvincibilityFrames = 48;

    /// <summary>The periodic damage each chew deals: two whole energy.</summary>
    public static readonly EnergyAmount ChewDamage = EnergyAmount.FromEnergy(2);
}
