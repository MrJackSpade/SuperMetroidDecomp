using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Verification access: the cartridge colour a palette-FX colour group starts at (#1464), for checks that compare
/// programs and definitions with their bank-$8D data and for native snapshot import. Where each owner lies in the
/// native image is the import pipeline's <see cref="GameplayColourPositionDefinitions"/>.
/// </summary>
internal static class PaletteFxColoursPositions
{
    /// <summary>The cartridge colour a placement's first written colour is: where its owner's colour lies in the native image.</summary>
    private static int CartridgeColour(PaletteFxColourPlacement placement) => placement switch
    {
        BackgroundSetPlacement background => GameplayColourPositionDefinitions.BackgroundSetsFirst
            + background.Set * GameplayColourPositionDefinitions.SetColours + background.Colour,
        SpriteSetPlacement sprite => GameplayColourPositionDefinitions.SpriteSetsFirst
            + sprite.Set * GameplayColourPositionDefinitions.SetColours + sprite.Colour,
        SamusSuitPlacement suit => GameplayColourPositionDefinitions.SamusSuitFirst + suit.Colour,
        SamusBeamsPlacement beams => GameplayColourPositionDefinitions.SamusBeamsFirst + beams.Colour,
        _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, "Unknown palette-FX colour placement."),
    };

    /// <summary>Native view of a palette-FX colour group.</summary>
    extension(PaletteFxColours self)
    {
        /// <summary>The cartridge colour the group's first written colour is.</summary>
        internal int CartridgeColour
        {
            get => CartridgeColour(PaletteFxColourPlacementDefinitions.Placements[self]);
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
