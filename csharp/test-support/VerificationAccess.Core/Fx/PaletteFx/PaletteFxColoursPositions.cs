using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Verification access: the cartridge colour a palette-FX colour group starts at (#1464), for checks that compare
/// programs and definitions with their bank-$8D data and for native snapshot import. Where each owner lies in the
/// native image is the import pipeline's <see cref="GameplayColourPositionDefinitions"/>, and a room group's its room-picture ink.
/// </summary>
internal static class PaletteFxColoursPositions
{
    /// <summary>The cartridge colour a placement's first written colour is: where its owner's colour lies in the native image.</summary>
    private static int CartridgeColour(PaletteFxColourPlacement placement) => placement switch
    {
        ScreenColoursPlacement screen => ScreenCartridgeColour(ScreenPlacement(screen.Colours)),
        SpriteSetPlacement sprite => GameplayColourPositionDefinitions.SpriteSetsFirst
            + sprite.Store.Set * GameplayColourPositionDefinitions.SetColours + sprite.Colour,
        SamusSuitPlacement suit => GameplayColourPositionDefinitions.SamusSuitFirst + suit.Colour,
        SamusBeamsPlacement beams => GameplayColourPositionDefinitions.SamusBeamsFirst + beams.Colour,
        _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, "Unknown palette-FX colour placement."),
    };

    /// <summary>The cartridge colour a screen background placement's first colour is: its set's native position.</summary>
    private static int ScreenCartridgeColour(ScreenColourPlacement held) =>
        (held is ScreenSpriteSetPlacement ? GameplayColourPositionDefinitions.SpriteSetsFirst : GameplayColourPositionDefinitions.BackgroundSetsFirst)
            + held.Set * GameplayColourPositionDefinitions.SetColours + held.Colour;

    /// <summary>
    /// The screen colours, background or sprite, each title or cinematic effect group writes at the native boundary: its
    /// set, its first written colour and how many follow. Frontend screens no longer hold these colours (#1512); the
    /// import pre-colours each record as a shade, and native checks place the group here.
    /// </summary>
    private static readonly Dictionary<PaletteFxColours, ScreenColourPlacement> ScreenPlacements = new()
    {
        [PaletteFxColours.BabyMetroidTubeLight] = new ScreenBackgroundSetPlacement(2, 10, 4),
        [PaletteFxColours.TitleScreenDisplays] = new ScreenBackgroundSetPlacement(2, 14, 2),
        [PaletteFxColours.CutsceneGunshipEngine] = new ScreenBackgroundSetPlacement(5, 15, 1),
        [PaletteFxColours.CeresNavigationLightsBackground] = new ScreenBackgroundSetPlacement(6, 13, 3),
        [PaletteFxColours.OldMotherBrainBackgroundLights] = new ScreenBackgroundSetPlacement(1, 4, 12),
        [PaletteFxColours.WideZebesExplosionForeground] = new ScreenBackgroundSetPlacement(0, 1, 15),
        [PaletteFxColours.WideZebesExplosionBackground] = new ScreenBackgroundSetPlacement(1, 1, 15),
        [PaletteFxColours.ZebesExplosionFinale] = new ScreenBackgroundSetPlacement(0, 1, 15),
        [PaletteFxColours.SpaceDuringZebesExplosion] = ScreenBackgroundSetPlacement.WholeSet(0),
        [PaletteFxColours.ExplodingZebesLava] = ScreenBackgroundSetPlacement.WholeSet(4),
        [PaletteFxColours.ExplodingZebesCrust] = new ScreenBackgroundSetPlacement(4, 1, 15),
        [PaletteFxColours.ExplodingZebesGreyClouds] = new ScreenBackgroundSetPlacement(5, 1, 15),
        [PaletteFxColours.GunshipEmergingFromExplosion] = ScreenBackgroundSetPlacement.WholeSet(5),
        [PaletteFxColours.UnusedCinematicFade] = ScreenBackgroundSetPlacement.WholeSet(5),
        [PaletteFxColours.TitleLogo] = new ScreenSpriteSetPlacement(2, 1, 15),
        [PaletteFxColours.NintendoBootLogo] = new ScreenSpriteSetPlacement(1, 9, 7),
        [PaletteFxColours.PlanetZebesText] = new ScreenSpriteSetPlacement(0, 1, 15),
        [PaletteFxColours.GunshipGlow] = new ScreenSpriteSetPlacement(7, 15, 1),
        [PaletteFxColours.ZoomedOutExplodingZebes] = ScreenSpriteSetPlacement.WholeSet(7),
        [PaletteFxColours.PostCreditsSuperMetroidIcon] = ScreenSpriteSetPlacement.WholeSet(7),
    };

    /// <summary>The screen colour group, background or sprite, that holds the title or cinematic effect colours <paramref name="colours"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The group is not a title or cinematic effect's.</exception>
    internal static ScreenColourPlacement ScreenPlacement(PaletteFxColours colours) => ScreenPlacements.TryGetValue(colours, out ScreenColourPlacement? placement)
        ? placement
        : throw new ArgumentOutOfRangeException(nameof(colours), colours, "Not a title or cinematic effect's colours.");

    /// <summary>Native view of a palette-FX colour group.</summary>
    extension(PaletteFxColours self)
    {
        /// <summary>The cartridge colour the group's first written colour is.</summary>
        internal int CartridgeColour
        {
            // A room's group lies where the import pre-colours the room pictures from: its first ink there (#1512).
            get => PaletteFxColourPlacementDefinitions.Placements.TryGetValue(self, out PaletteFxColourPlacement? placement)
                ? CartridgeColour(placement)
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
                PaletteFxColours[] named = [.. NamedColours(RoomPaletteFxSelection.Get(self).InitialProgram).Distinct()];
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
        PaletteFxColours[] matching = [.. NamedColours(RoomPaletteFxSelection.Get(paletteFxObject).InitialProgram)
            .Distinct().Where(colours => colours.CartridgeColour == position)];
        return matching.Length switch
        {
            1 => matching[0],
            0 when position == 0 => null,
            _ => throw new ArgumentOutOfRangeException(nameof(position), position,
                $"Palette-FX object {paletteFxObject} names {matching.Length} colour groups starting at this colour."),
        };
    }

    /// <summary>Every colour group the programs reachable from <paramref name="start"/> name.</summary>
    private static IEnumerable<PaletteFxColours> NamedColours(PaletteFxProgram start)
    {
        var seen = new HashSet<PaletteFxProgram>();
        var pending = new Stack<PaletteFxProgram>([start]);
        while (pending.TryPop(out PaletteFxProgram program))
        {
            if (!seen.Add(program))
                continue;
            foreach (AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue> step in PaletteFxAnimations.Programs.Steps(program))
            {
                switch (step)
                {
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep { Cue: PaletteFxCue.SetColours setColours }:
                        yield return setColours.Target;
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.FallIntoStep fallInto:
                        pending.Push(fallInto.Target);
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.GoToStep goTo:
                        pending.Push(goTo.Target);
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.RepeatWhileCountingStep repeat:
                        pending.Push(repeat.Target);
                        break;
                    case AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.CueStep
                        or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.ShowStep
                        or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.WaitStep
                        or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.SetCounterStep
                        or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.HoldStep
                        or AnimationProgramStep<PaletteFxProgram, PaletteFxFrame, PaletteFxCue>.StopStep:
                        break;
                    default:
                        throw new InvalidOperationException($"Undefined animation program step {step}.");
                }
            }
        }
    }
}
