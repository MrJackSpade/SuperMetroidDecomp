using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx.PaletteFx;

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
    private static readonly Dictionary<PaletteFxColours, int> NativeStarts = new()
    {
        [PaletteFxColours.BabyMetroidTubeLight] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 2 * GameplayColourPositionDefinitions.SetColours + 10,
        [PaletteFxColours.TitleScreenDisplays] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 2 * GameplayColourPositionDefinitions.SetColours + 14,
        [PaletteFxColours.CutsceneGunshipEngine] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 5 * GameplayColourPositionDefinitions.SetColours + 15,
        [PaletteFxColours.CeresNavigationLightsBackground] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 6 * GameplayColourPositionDefinitions.SetColours + 13,
        [PaletteFxColours.OldMotherBrainBackgroundLights] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 4,
        [PaletteFxColours.WideZebesExplosionForeground] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 0 * GameplayColourPositionDefinitions.SetColours + 1,
        [PaletteFxColours.WideZebesExplosionBackground] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 1,
        [PaletteFxColours.ZebesExplosionFinale] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 0 * GameplayColourPositionDefinitions.SetColours + 1,
        [PaletteFxColours.SpaceDuringZebesExplosion] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 0 * GameplayColourPositionDefinitions.SetColours + 0,
        [PaletteFxColours.ExplodingZebesLava] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 4 * GameplayColourPositionDefinitions.SetColours + 0,
        [PaletteFxColours.ExplodingZebesCrust] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 4 * GameplayColourPositionDefinitions.SetColours + 1,
        [PaletteFxColours.ExplodingZebesGreyClouds] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 5 * GameplayColourPositionDefinitions.SetColours + 1,
        [PaletteFxColours.GunshipEmergingFromExplosion] = GameplayColourPositionDefinitions.BackgroundSetsFirst + 5 * GameplayColourPositionDefinitions.SetColours + 0,
        [PaletteFxColours.TitleLogo] = GameplayColourPositionDefinitions.SpriteSetsFirst + 2 * GameplayColourPositionDefinitions.SetColours + 1,
        [PaletteFxColours.NintendoBootLogo] = GameplayColourPositionDefinitions.SpriteSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 9,
        [PaletteFxColours.PlanetZebesText] = GameplayColourPositionDefinitions.SpriteSetsFirst + 0 * GameplayColourPositionDefinitions.SetColours + 1,
        [PaletteFxColours.GunshipGlow] = GameplayColourPositionDefinitions.SpriteSetsFirst + 7 * GameplayColourPositionDefinitions.SetColours + 15,
        [PaletteFxColours.ZoomedOutExplodingZebes] = GameplayColourPositionDefinitions.SpriteSetsFirst + 7 * GameplayColourPositionDefinitions.SetColours + 0,
        [PaletteFxColours.PostCreditsSuperMetroidIcon] = GameplayColourPositionDefinitions.SpriteSetsFirst + 7 * GameplayColourPositionDefinitions.SetColours + 0,
        [PaletteFxColours.NintendoCopyright] = GameplayColourPositionDefinitions.SamusSuitFirst + 9,
        [PaletteFxColours.SamusLoadingSuit] = GameplayColourPositionDefinitions.SamusSuitFirst + 0,
        [PaletteFxColours.HeatSuitGlow] = GameplayColourPositionDefinitions.SamusSuitFirst + 1,
        [PaletteFxColours.CeresNavigationLightsSprite] = GameplayColourPositionDefinitions.SamusBeamsFirst + 13,
        [PaletteFxColours.ZebesPlanetAfterglow] = GameplayColourPositionDefinitions.SamusBeamsFirst + 1,
        [PaletteFxColours.HyperBeam] = GameplayColourPositionDefinitions.SamusBeamsFirst + 1,
        [PaletteFxColours.RidleyStatue] = GameplayColourPositionDefinitions.SpriteSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 0,
        [PaletteFxColours.PhantoonStatue] = GameplayColourPositionDefinitions.SpriteSetsFirst + 2 * GameplayColourPositionDefinitions.SetColours + 0,
        [PaletteFxColours.TorizoBelly] = GameplayColourPositionDefinitions.SpriteSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 9,
        [PaletteFxColours.TourianShutters] = GameplayColourPositionDefinitions.SpriteSetsFirst + 1 * GameplayColourPositionDefinitions.SetColours + 9,
    };

    /// <summary>Native view of a palette-FX colour group.</summary>
    extension(PaletteFxColours self)
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
    extension(PaletteFxObject self)
    {
        /// <summary>
        /// The one colour group every program the object can reach names, read from its programs' cues through
        /// falls, jumps and counted repeats.
        /// </summary>
        internal PaletteFxColours AnimatedColours
        {
            get
            {
                PaletteFxColours[] named = [.. NamedColours(RoomPaletteFxSelection.Get(self).InitialAnimation).Distinct()];
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
    internal static PaletteFxColours? FromCartridge(PaletteFxObject? running, int position)
    {
        if (running is not { } paletteFxObject)
            return null;
        PaletteFxColours[] matching = [.. NamedColours(RoomPaletteFxSelection.Get(paletteFxObject).InitialAnimation)
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
    /// Every colour group the animation starting at <paramref name="start"/> names, and those of the animations its
    /// conditions restart it at.
    /// </summary>
    private static IEnumerable<PaletteFxColours> NamedColours(PaletteFxSegment start)
    {
        var seen = new HashSet<PaletteFxSegment>();
        var pending = new Stack<PaletteFxSegment>([start]);
        while (pending.TryPop(out PaletteFxSegment animation))
        {
            if (!seen.Add(animation))
                continue;
            foreach (AnimationPhase<PaletteFxSegment> phase in PaletteFxAnimations.StartingWith(animation).Phases)
            {
                IReadOnlyList<AnimationBeat<int, PaletteFxCue>> beats = PaletteFxSegments.All.Beats(phase.Segment);
                for (int beat = phase.FirstBeat; beat < phase.FirstBeat + phase.BeatCount; beat++)
                {
                    switch (beats[beat])
                    {
                        case AnimationBeat<int, PaletteFxCue>.Event { Raised: PaletteFxCue.SetColours setColours }:
                            yield return setColours.Target;
                            break;
                        case AnimationBeat<int, PaletteFxCue>.Event { Raised: PaletteFxCue.Watch { Condition: ColourEffectCondition.RestartsWhileSamusIsHigh high } }:
                            pending.Push(high.Restart);
                            break;
                        case AnimationBeat<int, PaletteFxCue>.Event or AnimationBeat<int, PaletteFxCue>.Frame:
                            break;
                        default:
                            throw new InvalidOperationException($"Undefined animation beat {beats[beat]}.");
                    }
                }
            }
        }
    }
}
