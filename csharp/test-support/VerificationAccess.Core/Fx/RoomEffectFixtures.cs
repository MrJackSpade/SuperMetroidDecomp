using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Room effects loaded through the room's own load of its effect, for checks that put Samus in
/// a room's liquid or weather as a room's load does: still liquid whose surface starts at a given line and never moves,
/// with no colour effects, block animations or tint.
/// </summary>
internal static class RoomEffectFixtures
{
    /// <summary>A room of still water whose surface stands at <paramref name="surface"/>.</summary>
    /// <param name="surface">The water surface's room line.</param>
    /// <param name="ignoresSurfaceUntilEnabled">Whether the water ignores its surface until the room enables it.</param>
    internal static RoomFxOverlayState Water(int surface, bool ignoresSurfaceUntilEnabled = false) =>
        Load(RoomFxType.Water, surface, new RoomWater(LiquidTide.None, SurfaceDrift: false, BackgroundWave: false, ignoresSurfaceUntilEnabled));

    /// <summary>A room of still lava whose surface stands at <paramref name="surface"/>.</summary>
    internal static RoomFxOverlayState Lava(int surface) => Load(RoomFxType.Lava, surface, new RoomLavaOrAcid(LiquidTide.None, HeatHazeDirection.None));

    /// <summary>A room of still acid whose surface stands at <paramref name="surface"/>.</summary>
    internal static RoomFxOverlayState Acid(int surface) => Load(RoomFxType.Acid, surface, new RoomLavaOrAcid(LiquidTide.None, HeatHazeDirection.None));

    /// <summary>A room of rain, which holds no liquid.</summary>
    internal static RoomFxOverlayState Rain() => Load(RoomFxType.Rain, surface: null, liquid: null);

    /// <summary>A room of rain, which holds no liquid, its sounds joining <paramref name="sounds"/>.</summary>
    /// <param name="sounds">The requests the effect's sounds join, which the check reads.</param>
    internal static RoomFxOverlayState Rain(SuperMetroid.Core.Audio.GameplaySoundRequests sounds) =>
        Load(RoomFxType.Rain, surface: null, liquid: null, sounds);

    /// <summary>
    /// Loads a room effect of <paramref name="type"/> whose liquid, if any, starts at <paramref name="surface"/> and never
    /// moves, blending its overlay by brightening the room where the effect shows on the overlay.
    /// </summary>
    private static RoomFxOverlayState Load(RoomFxType type, int? surface, RoomLiquid? liquid,
        SuperMetroid.Core.Audio.GameplaySoundRequests? sounds = null) =>
        RoomFxOverlayState.ForDefinition(new RoomEffectDefinition(LiquidStartLevel: surface, LiquidTargetLevel: null,
            LiquidLevelSpeed: default, LiquidMoveDelay: 0, Type: type, VisorAnimation: RoomVisorAnimation.Steady,
            EffectBlend: RoomFxOverlayState.ShowsOnOverlay(type) ? RoomEffectBlendDefinitions.OverlayBrightens : null,
            Liquid: liquid, ColourEffects: [], BlockAnimations: [], Tint: RoomEffectTint.None), 0,
            sounds ?? SuperMetroid.Core.Audio.GameplayAudioRequests.Standalone());
}
