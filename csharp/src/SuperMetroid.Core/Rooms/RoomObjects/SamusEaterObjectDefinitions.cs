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
    public static readonly SamusEaterObjectDefinition Floor = new(Ceiling: false, FloorChews,
    [
        new(SamusEaterDraw.FloorChew2, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.FloorChew1, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.FloorChew2, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.FloorChew3, ChewFrameDuration, SamusEaterBite.SoundAndDamage),
        new(SamusEaterDraw.FloorChew2, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.FloorChew1, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.FloorChew2, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.FloorChew3, ChewFrameDuration, SamusEaterBite.Damage),
    ], SamusEaterDraw.FloorChew3, SamusEaterDraw.FloorIdle);

    /// <summary>The Brinstar ceiling plant.</summary>
    public static readonly SamusEaterObjectDefinition Ceiling = new(Ceiling: true, CeilingChews,
    [
        new(SamusEaterDraw.CeilingChew2, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.CeilingChew1, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.CeilingChew2, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.CeilingChew3, ChewFrameDuration, SamusEaterBite.SoundAndDamage),
        new(SamusEaterDraw.CeilingChew2, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.CeilingChew1, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.CeilingChew2, ChewFrameDuration, SamusEaterBite.None),
        new(SamusEaterDraw.CeilingChew3, ChewFrameDuration, SamusEaterBite.Damage),
    ], SamusEaterDraw.CeilingChew3, SamusEaterDraw.CeilingIdle);

    /// <summary>The chew sound.</summary>
    public static readonly SoundEffectId ChewSound = EnemySound.PlantChewing;

    /// <summary>The chew sound's request priority.</summary>
    internal static readonly SoundRequestPriority ChewSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The invincibility bit the hold ORs into Samus's invincibility timer each frame.</summary>
    public const int HeldInvincibilityBit = 1 << 4;

    /// <summary>The immunity the release gives Samus, in frames.</summary>
    public const int ReleaseInvincibilityFrames = 48;

    /// <summary>The periodic damage each chew deals.</summary>
    public const int ChewDamage = 2;

    /// <summary>The plant each supported header places; every other header is not a Samus Eater.</summary>
    public static IReadOnlyDictionary<RoomObjectHeaderId, SamusEaterObjectDefinition> ByHeader { get; } =
        new Dictionary<RoomObjectHeaderId, SamusEaterObjectDefinition>
        {
            [RoomObjectHeaderId.InsideReactionBrinstarFloorPlant] = Floor,
            [RoomObjectHeaderId.InsideReactionBrinstarCeilingPlant] = Ceiling,
        };
}
