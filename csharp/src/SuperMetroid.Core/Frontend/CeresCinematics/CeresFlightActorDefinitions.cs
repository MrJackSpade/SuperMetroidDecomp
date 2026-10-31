using SuperMetroid.Core.Frontend.Intro;
namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>Fixed actor metadata and motion parameters for Samus's flight toward Ceres.</summary>
internal static class CeresFlightActorDefinitions
{
    /// <summary>Number of actors allocated by <c>$8B:BE3B-$8B:BE5C</c>.</summary>
    public const int RearViewActorCount = 5;

    /// <summary><c>$8B:BEC2</c>, signed-8.8 acceleration applied to the opening star field.</summary>
    public const ushort FrontStarAcceleration = 128;

    /// <summary>
    /// <c>$8B:CF0F</c> with initializer parameter zero: opening star field and its
    /// signed-8.8 motion accumulator.
    /// </summary>
    /// <remarks>
    /// Issues #625 and #1014: front stars and RearViewActor(4) alias the
    /// same native $8B:CF0F definition and list $8B:CDA3..CDAA. Its four
    /// words match pinned NTSC J/U v1.0 ROM and bank_8B.asm: duration
    /// $000A, bank-$8C star spritemap $9478, goto $94BC, target $CDA3.
    /// The initializer parameter changes actor motion, not this list.
    /// The exact bounded rule displays the same authored star sheet for
    /// ten handler calls and repeats while its scene owns the actor.
    /// IntroDiscoverySprite.Step never falls into adjacent list $CDAB.
    /// Both aliases select the same installed visual frame.
    /// </remarks>
    public static CeresFlightActorDefinition FrontStars => new(
        ActivePreInstruction: CinematicSpritePreInstruction.CeresStars,
        Program: CinematicSpriteProgram.CeresStars,
        Position: new(112, 87),
        Colouring: ScreenColourGroup.CeresFlightSpaceObjects,
        InitialTimer: 64512,
        HorizontalDelta: 0,
        WrapX: false);

    /// <summary>$8B:CF39, CinematicSpriteObjectDefs_CeresExplosionLargeAsteroids.
    /// BF22 initializes (80,159), palette0800; BF35 adds 1/4 pixel and masks X to nine bits;
    /// CE4B selects the large-asteroid loop. The destruction scene substitutes CE7F/CC3F.</summary>
    internal static CeresFlightActorDefinition LargeAsteroids =>
        new(CinematicSpritePreInstruction.CeresLargeAsteroids, CinematicSpriteProgram.CeresLargeAsteroid,
            new(80, 159), ScreenColourGroup.CeresFlightSpaceObjects, 0, 16384, true);

    /// <summary>$8B:CE85, CinematicSpriteObjectDefinitions_CeresUnderAttack.
    /// BF4C initializes (116,160), palette0C00; BF5F adds 1/16 pixel with nine-bit X wrap;
    /// CC47 selects the station-under-attack loop.</summary>
    internal static CeresFlightActorDefinition StationUnderAttack =>
        new(CinematicSpritePreInstruction.CeresUnderAttack, CinematicSpriteProgram.CeresUnderAttack,
            new(116, 160), ScreenColourGroup.CeresFlightStation, 0, 4096, true);

    /// <summary>$8B:CE8B, CinematicSpriteObjectDefinitions_CeresSmallAsteroids.
    /// BF76 initializes (128,96), palette0800; BF89 adds 1/32 pixel with nine-bit X wrap;
    /// CC4F selects the small-asteroid loop. Both Ceres scenes share this identity.</summary>
    internal static CeresFlightActorDefinition SmallAsteroids =>
        new(CinematicSpritePreInstruction.CeresSmallAsteroid, CinematicSpriteProgram.CeresSmallAsteroid,
            new(128, 96), ScreenColourGroup.CeresFlightSpaceObjects, 0, 2048, true);

    /// <summary>$8B:CE91, CinematicSpriteObjectDefinitions_CeresPurpleSpaceVortex, parameter1.
    /// BFA0 takes the nonzero branch to (224,87), palette0800, preserving BFC6 movement;
    /// CC57 alternates two frames. Parameter0 is the stationary destruction-scene variant.</summary>
    internal static CeresFlightActorDefinition MovingVortex =>
        new(CinematicSpritePreInstruction.CeresPurpleSpaceVortex, CinematicSpriteProgram.CeresVortex,
            new(224, 87), ScreenColourGroup.CeresFlightSpaceObjects, 0, VortexHorizontalDelta, false);

    /// <summary>$8B:CF0F, CinematicSpriteObjectDefinitions_CeresStars, parameter1.
    /// BE7E takes the nonzero branch: X=-32, Y87, palette0800, timer remains cleared;
    /// BE9C replaces BEB5 with BFC6. The definition and CDA3 program alias FrontStars.</summary>
    internal static CeresFlightActorDefinition RearStars => FrontStars with
    {
        ActivePreInstruction = CinematicSpritePreInstruction.CeresPurpleSpaceVortex,
        Position = FrontStars.Position.WithX(unchecked((ushort)-32)),
        InitialTimer = 0,
        HorizontalDelta = VortexHorizontalDelta,
    };

    /// <summary>$8B:BFCB, subtraction immediate2000: signed16.16 movement of -1/8 pixel,
    /// with borrow into the whole X word and no nine-bit mask.</summary>
    private const int VortexHorizontalDelta = -8192;
}
