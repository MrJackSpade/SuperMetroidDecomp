using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// Verification access: the cartridge colour a palette-FX colour group starts at (#1464), for checks that compare
/// programs and definitions with their bank-$8D data and for native snapshot import. Where each owner lies in the
/// native image is the import pipeline's <see cref="GameplayColourPositionDefinitions"/>, and a room group's its room-picture ink.
/// </summary>
internal static class PaletteFxColoursPositions
{
    /// <summary>
    /// The cartridge colour each group that recolours no room picture starts at in the native colour image: a title or
    /// cinematic screen's set colour, Samus's suit or beam colour, or an enemy sprite set's colour. Core holds no colour
    /// positions (#1512); the import pre-colours each record as a shade, and native checks place the group here.
    /// </summary>
    private static readonly Dictionary<ColourEffectColours, int> NativeStarts = new()
    {
        [ColourEffectColours.BabyMetroidTubeLight] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 2 * GameplayColourPositionDefinitions.SetColours + 10,
        [ColourEffectColours.TitleScreenDisplays] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 2 * GameplayColourPositionDefinitions.SetColours + 14,
        [ColourEffectColours.CutsceneGunshipEngine] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 5 * GameplayColourPositionDefinitions.SetColours + 15,
        [ColourEffectColours.CeresNavigationLightsBackground] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 6 * GameplayColourPositionDefinitions.SetColours + 13,
        [ColourEffectColours.OldMotherBrainBackgroundLights] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 4,
        [ColourEffectColours.WideZebesExplosionForeground] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 0 * GameplayColourPositionDefinitions.SetColours + 1,
        [ColourEffectColours.WideZebesExplosionBackground] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 1,
        [ColourEffectColours.ZebesExplosionFinale] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 0 * GameplayColourPositionDefinitions.SetColours + 1,
        [ColourEffectColours.SpaceDuringZebesExplosion] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 0 * GameplayColourPositionDefinitions.SetColours + 0,
        [ColourEffectColours.ExplodingZebesLava] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 4 * GameplayColourPositionDefinitions.SetColours + 0,
        [ColourEffectColours.ExplodingZebesCrust] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 4 * GameplayColourPositionDefinitions.SetColours + 1,
        [ColourEffectColours.ExplodingZebesGreyClouds] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 5 * GameplayColourPositionDefinitions.SetColours + 1,
        [ColourEffectColours.GunshipEmergingFromExplosion] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 5 * GameplayColourPositionDefinitions.SetColours + 0,
        [ColourEffectColours.TitleLogo] = GameplayColourPositionDefinitions.SpriteSetsFirst + 2 * GameplayColourPositionDefinitions.SetColours + 1,
        [ColourEffectColours.PlanetZebesText] = GameplayColourPositionDefinitions.SpriteSetsFirst + 0 * GameplayColourPositionDefinitions.SetColours + 1,
        [ColourEffectColours.GunshipGlow] = GameplayColourPositionDefinitions.SpriteSetsFirst + 7 * GameplayColourPositionDefinitions.SetColours + 15,
        [ColourEffectColours.ZoomedOutExplodingZebes] = GameplayColourPositionDefinitions.SpriteSetsFirst + 7 * GameplayColourPositionDefinitions.SetColours + 0,
        [ColourEffectColours.PostCreditsSuperMetroidIcon] = GameplayColourPositionDefinitions.SpriteSetsFirst + 7 * GameplayColourPositionDefinitions.SetColours + 0,
        [ColourEffectColours.NintendoCopyright] = GameplayColourPositionDefinitions.SamusSuitFirst + 9,
        [ColourEffectColours.SamusLoadingSuit] = GameplayColourPositionDefinitions.SamusSuitFirst + 0,
        [ColourEffectColours.HeatSuitGlow] = GameplayColourPositionDefinitions.SamusSuitFirst + 1,
        [ColourEffectColours.CeresNavigationLightsSprite] = GameplayColourPositionDefinitions.SamusBeamsFirst + 13,
        [ColourEffectColours.ZebesPlanetAfterglow] = GameplayColourPositionDefinitions.SamusBeamsFirst + 1,
        [ColourEffectColours.HyperBeam] = GameplayColourPositionDefinitions.SamusBeamsFirst + 1,
        [ColourEffectColours.RidleyStatue] = GameplayColourPositionDefinitions.SpriteSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 0,
        [ColourEffectColours.PhantoonStatue] = GameplayColourPositionDefinitions.SpriteSetsFirst + 2 * GameplayColourPositionDefinitions.SetColours + 0,
        [ColourEffectColours.TorizoBelly] = GameplayColourPositionDefinitions.SpriteSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 9,
        [ColourEffectColours.TourianShutters] = GameplayColourPositionDefinitions.SpriteSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 9,
    };

    /// <summary>Native view of a palette-FX colour group.</summary>
    extension(ColourEffectColours self)
    {
        /// <summary>The cartridge colour the group's first written colour is.</summary>
        internal int CartridgeColour
        {
            // A room's group lies where the import pre-colours the room pictures from: its first ink there (#1512).
            get => NativeStarts.TryGetValue(self, out int start)
                ? start
                : GameplayColourPositionDefinitions.BackgroundSetsFirst + SuperMetroid.AssetExtraction.Fx.PaletteFxRoomShades.FirstInk(self);
        }

        /// <summary>The CGRAM byte index the native SetColorIndex command stores for the group.</summary>
        internal ushort CartridgeByteIndex => (ushort)(self.CartridgeColour * sizeof(ushort));
    }

    /// <summary>The colour groups a palette-FX object's programs name.</summary>
    extension(ColourEffectId self)
    {
        /// <summary>
        /// The one colour group every program the object can reach names, read from its programs' cues through
        /// falls, jumps and counted repeats.
        /// </summary>
        internal ColourEffectColours AnimatedColours
        {
            get
            {
                ColourEffectColours[] named = [.. NamedColours(ColourEffectSelection.Get(self).InitialAnimation).Distinct()];
                return named.Length == 1
                    ? named[0]
                    : throw new InvalidOperationException($"Palette-FX object {self} names {named.Length} colour groups.");
            }
        }
    }

    /// <summary>
    /// The colour group a native snapshot's slot animates: the one among those its object's programs name that starts
    /// at the recorded cartridge colour; none for a free slot, or for a slot whose colour index is still the zero its
    /// spawn wrote because its program has not yet named colours starting anywhere else.
    /// </summary>
    internal static ColourEffectColours? FromCartridge(ColourEffectId? running, int position)
    {
        if (running is not { } paletteFxObject)
            return null;
        ColourEffectColours[] matching = [.. NamedColours(ColourEffectSelection.Get(paletteFxObject).InitialAnimation)
            .Distinct().Where(colours => colours.CartridgeColour == position)];
        return matching.Length switch
        {
            1 => matching[0],
            0 when position == 0 => null,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position,
                $"Palette-FX object {paletteFxObject} names {matching.Length} colour groups starting at this colour."),
        };
    }

    /// <summary>
    /// Every colour group <paramref name="start"/> names; a restart plays on in the same animation, so it names no others.
    /// </summary>
    private static IEnumerable<ColourEffectColours> NamedColours(ColourEffectAnimation start)
    {
        foreach (AnimationPhase<ColourEffectAnimation> phase in ColourEffectAnimations.Of(start).Phases)
        {
            IReadOnlyList<AnimationBeat<int, ColourEffectCue>> beats = ColourEffectAnimations.Steps.Beats(phase.Segment);
            for (int beat = phase.FirstBeat; beat < phase.FirstBeat + phase.BeatCount; beat++)
            {
                switch (beats[beat])
                {
                    case AnimationBeat<int, ColourEffectCue>.Event { Raised: ColourEffectCue.SetColours setColours }:
                        yield return setColours.Target;
                        break;
                    case AnimationBeat<int, ColourEffectCue>.Event or AnimationBeat<int, ColourEffectCue>.Frame:
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined animation beat {beats[beat]}.");
                }
            }
        }
    }
}
