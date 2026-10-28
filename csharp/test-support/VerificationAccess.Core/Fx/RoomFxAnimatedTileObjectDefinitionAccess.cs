using SuperMetroid.AssetExtraction.Fx;
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxAnimatedTileLoop"/> members production does not use.</summary>
internal static class RoomFxAnimatedTileObjectDefinitionAccess
{
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
        AnimatedTileObject.WreckedShipTreadmillRightwards or AnimatedTileObject.WreckedShipTreadmillLeftwards
            or AnimatedTileObject.None or AnimatedTileObject.TourianStatuePhantoon or AnimatedTileObject.TourianStatueRidley
            or AnimatedTileObject.TourianStatueKraid or AnimatedTileObject.TourianStatueDraygon or AnimatedTileObject.Empty
            or AnimatedTileObject.VerticalSpikes or AnimatedTileObject.CrateriaLake or AnimatedTileObject.UnusedCrateriaLava
            or AnimatedTileObject.BrinstarPlant or AnimatedTileObject.WreckedShipScreen
            => throw new InvalidDataException($"Room-FX object {animatedObject} has no simple frame loop."),
        _ => throw new InvalidDataException($"Undefined room-FX object {animatedObject}."),
    };

    extension(RoomFxAnimatedTileLoop self)
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
            if (pointer == FxNativeWords.AnimatedTileObjects.Word(self.AnimatedObject))
                value = self.InstructionPointer;
            else if (pointer == unchecked((ushort)(FxNativeWords.AnimatedTileObjects.Word(self.AnimatedObject) + 2)))
                value = checked((ushort)self.FrameByteCount);
            else if (pointer == unchecked((ushort)(FxNativeWords.AnimatedTileObjects.Word(self.AnimatedObject) + 4)))
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

                    value = checked((ushort)frame.Duration);
                    return true;
                }

                value = 0;
                return false;
            }

            return true;
        }
    }
}
