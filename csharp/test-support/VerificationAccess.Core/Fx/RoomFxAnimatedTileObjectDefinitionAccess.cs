using SuperMetroid.AssetExtraction.Fx;
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxAnimatedTileObjectDefinition"/> members production does not use.</summary>
internal static class RoomFxAnimatedTileObjectDefinitionAccess
{
    /// <summary>Exposes <see cref="RoomFxAnimatedTileObjectDefinition"/> state that only verification reads.</summary>
    /// <summary>Frame artwork by bank-$87 cursor.</summary>
    extension(SuperMetroid.Core.Fx.Artwork.RoomFxAnimatedTileArtworkDefinitions)
    {
        /// <summary>The native source identity of the frame whose timed control word sits at bank-$87 <paramref name="instructionPointer"/>.</summary>
        internal static int SourceAddress(RoomFxAnimatedTileObjectDefinition definition, ushort instructionPointer)
        {
            foreach (RoomFxAnimatedTileFrameDefinition frame in definition.Frames)
                if (frame.InstructionPointer == instructionPointer)
                    return SuperMetroid.Core.Fx.Artwork.RoomFxAnimatedTileArtworkDefinitions.SourceAddress(definition, frame);
            throw new InvalidDataException(
                $"Room-FX object {definition.AnimatedObject} has no artwork frame at $87:{instructionPointer:X4}.");
        }
    }

    /// <summary>The bank-$87 cursor of <paramref name="animatedObject"/>'s first timed frame.</summary>
    internal static ushort FirstCursor(AnimatedTileObject animatedObject) => animatedObject switch
    {
        AnimatedTileObject.HorizontalSpikes => 0x817e,
        AnimatedTileObject.MaridiaSandCeiling => 0x8221,
        AnimatedTileObject.MaridiaSandFalling => 0x8235,
        AnimatedTileObject.Lava => 0x8293,
        AnimatedTileObject.Acid => 0x82b1,
        AnimatedTileObject.Rain => 0x82cf,
        AnimatedTileObject.Spores => 0x82ed,
        _ => throw new InvalidDataException($"Room-FX object {animatedObject} has no simple frame loop."),
    };

    extension(RoomFxAnimatedTileObjectDefinition self)
    {
        /// <summary>The first timed instruction in the object's bank-$87 loop.</summary>
        public ushort InstructionPointer => FirstCursor(self.AnimatedObject);
        /// <summary>The address of the terminal <c>goto</c> command after the final frame.</summary>
        public ushort GotoInstructionPointer =>
            unchecked((ushort)(self.InstructionPointer + self.frames.Count * 4));
        /// <summary>
        /// Reads one immutable mechanics word. Frame source operands are intentionally absent.
        /// </summary>
        public bool TryReadMechanicsWord(ushort pointer, out ushort value)
        {
            if (pointer == (ushort)self.AnimatedObject)
                value = self.InstructionPointer;
            else if (pointer == unchecked((ushort)((ushort)self.AnimatedObject + 2)))
                value = self.FrameByteCount;
            else if (pointer == unchecked((ushort)((ushort)self.AnimatedObject + 4)))
                value = AnimatedTileVramDestinations.WordOf(self.AnimatedObject);
            else if (pointer == self.GotoInstructionPointer)
                value = (ushort)AnimatedTileInstruction.Goto;
            else if (pointer == unchecked((ushort)(self.GotoInstructionPointer + 2)))
                value = self.InstructionPointer;
            else
            {
                foreach (RoomFxAnimatedTileFrameDefinition frame in self.frames)
                {
                    if (pointer != frame.InstructionPointer)
                        continue;

                    value = frame.Duration;
                    return true;
                }

                value = 0;
                return false;
            }

            return true;
        }
    }
}
