using SuperMetroid.AssetExtraction.Fx;
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to <see cref="RoomFxBlockAnimationLoop"/> members production does not use.</summary>
internal static class RoomFxAnimatedTileObjectDefinitionAccess
{
    /// <summary>The bank-$87 cursor of <paramref name="animatedObject"/>'s first timed frame.</summary>
    internal static ushort FirstCursor(BlockAnimation animatedObject) => animatedObject switch
    {
        BlockAnimation.HorizontalSpikes => 0x817e,
        BlockAnimation.MaridiaSandCeiling => 0x8221,
        BlockAnimation.MaridiaSandFalling => 0x8235,
        BlockAnimation.Lava => 0x8293,
        BlockAnimation.Acid => 0x82b1,
        BlockAnimation.Rain => 0x82cf,
        BlockAnimation.Spores => 0x82ed,
        BlockAnimation.WreckedShipTreadmillRightwards or BlockAnimation.WreckedShipTreadmillLeftwards
            or BlockAnimation.None or BlockAnimation.TourianStatuePhantoon or BlockAnimation.TourianStatueRidley
            or BlockAnimation.TourianStatueKraid or BlockAnimation.TourianStatueDraygon
            or BlockAnimation.VerticalSpikes or BlockAnimation.CrateriaLake
            or BlockAnimation.BrinstarPlant or BlockAnimation.WreckedShipScreen
            => throw new InvalidDataException($"Room-FX object {animatedObject} has no simple frame loop."),
        _ => throw new InvalidDataException($"Undefined room-FX object {animatedObject}."),
    };

    extension(RoomFxBlockAnimationLoop self)
    {
        /// <summary>The first timed instruction in the object's bank-$87 loop.</summary>
        public ushort InstructionPointer => FirstCursor(self.AnimatedObject);
        /// <summary>The address of the terminal <c>goto</c> command after the final frame.</summary>
        public ushort GotoInstructionPointer =>
            unchecked((ushort)(self.InstructionPointer + self.Frames.Count * 4));
        /// <summary>
        /// Reads one immutable mechanics word. Frame source operands are intentionally absent.
        /// </summary>
        public bool TryReadMechanicsWord(ushort pointer, out ushort value)
        {
            if (pointer == FxNativeWords.AnimatedTileObjects.Word(self.AnimatedObject))
                value = self.InstructionPointer;
            else if (pointer == unchecked((ushort)(FxNativeWords.AnimatedTileObjects.Word(self.AnimatedObject) + 2)))
                value = checked((ushort)self.PictureByteCount);
            else if (pointer == unchecked((ushort)(FxNativeWords.AnimatedTileObjects.Word(self.AnimatedObject) + 4)))
                value = AnimatedTileVramDestinations.WordOf(self.AnimatedObject);
            else if (pointer == self.GotoInstructionPointer)
                value = (ushort)AnimatedTileInstruction.Goto;
            else if (pointer == unchecked((ushort)(self.GotoInstructionPointer + 2)))
                value = self.InstructionPointer;
            else
            {
                for (int index = 0; index < self.Frames.Count; index++)
                {
                    if (pointer != self.FrameInstructionPointer(index))
                        continue;

                    value = checked((ushort)self.Frames[index].Duration);
                    return true;
                }

                value = 0;
                return false;
            }

            return true;
        }
    }
}
