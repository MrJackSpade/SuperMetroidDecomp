using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Verification access: the cartridge colour a palette-FX colour group starts at (#1464), for checks that compare
/// programs and definitions with their bank-$8D data and for native snapshot import. The background sets are the
/// colour memory's first half and the sprite sets its second, except where Samus's suit and beams are.
/// </summary>
internal static class PaletteFxColoursPositions
{
    /// <summary>The cartridge colour each owner's first colour is.</summary>
    private static int FirstCartridgeColour(PaletteFxColourOwner owner) => owner switch
    {
        PaletteFxColourOwner.Background => 0,
        PaletteFxColourOwner.Sprite => 128,
        PaletteFxColourOwner.SamusSuit => 192,
        PaletteFxColourOwner.SamusBeams => 224,
        _ => throw new ArgumentOutOfRangeException(nameof(owner), owner, "Unknown palette-FX colour owner."),
    };

    /// <summary>Native view of a palette-FX colour group.</summary>
    extension(PaletteFxColours self)
    {
        /// <summary>The cartridge colour the group's first written colour is.</summary>
        internal int CartridgeColour
        {
            get
            {
                PaletteFxColourPlacement placement = PaletteFxColourPlacementDefinitions.Placements[self];
                return FirstCartridgeColour(placement.Owner) + placement.Colour;
            }
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
