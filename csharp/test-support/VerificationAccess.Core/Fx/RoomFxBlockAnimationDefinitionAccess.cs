using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Enemies.WreckedShip;
namespace SuperMetroid.Core.Fx;

/// <summary>Verification access to the bank-$87 program a <see cref="BlockAnimationLoop"/> replaces.</summary>
internal static class RoomFxBlockAnimationDefinitionAccess
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
        BlockAnimation.WreckedShipTreadmillRightwards => AnimatedTileAnimationScriptPointers.WreckedShipTreadmillRightwardsLoop,
        BlockAnimation.WreckedShipTreadmillLeftwards => AnimatedTileAnimationScriptPointers.WreckedShipTreadmillLeftwardsLoop,
        BlockAnimation.None or BlockAnimation.TourianStatuePhantoon or BlockAnimation.TourianStatueRidley
            or BlockAnimation.TourianStatueKraid or BlockAnimation.TourianStatueDraygon
            or BlockAnimation.VerticalSpikes or BlockAnimation.CrateriaLake
            or BlockAnimation.BrinstarPlant or BlockAnimation.WreckedShipScreen
            => throw new InvalidDataException($"Room-FX object {animatedObject} has no frame loop."),
        _ => throw new InvalidDataException($"Undefined room-FX object {animatedObject}."),
    };

    extension(BlockAnimationLoop self)
    {
        /// <summary>Whether the loop is a Wrecked Ship treadmill's, whose program waits for the area's boss first.</summary>
        internal bool IsTreadmill => self.Start == BlockAnimationStart.OnceAreaBossIsDefeated;
        /// <summary>The first timed instruction in the object's bank-$87 loop.</summary>
        public ushort InstructionPointer => FirstCursor(self.Animation);
        /// <summary>The address of the terminal <c>goto</c> command after the final frame.</summary>
        public ushort GotoInstructionPointer =>
            unchecked((ushort)(self.InstructionPointer + self.Frames.Count * 4));
        /// <summary>
        /// Reads one immutable mechanics word: the header's program, transfer size and destination, a treadmill's boss
        /// wait, each frame's duration and the terminal <c>goto</c>. Frame source operands are intentionally absent.
        /// </summary>
        public bool TryReadMechanicsWord(ushort pointer, out ushort value)
        {
            ushort header = FxNativeWords.AnimatedTileObjects.Word(self.Animation);
            if (pointer == header)
                value = self.IsTreadmill ? self.WaitInstructionPointer : self.InstructionPointer;
            else if (pointer == unchecked((ushort)(header + 2)))
                value = self.IsTreadmill ? (ushort)RoomFxBlockAnimationLoops.TreadmillFrameByteCount : checked((ushort)self.PictureByteCount);
            else if (pointer == unchecked((ushort)(header + 4)))
                value = AnimatedTileVramDestinations.WordOf(self.IsTreadmill ? BlockAnimation.WreckedShipTreadmillRightwards : self.Animation);
            else if (self.IsTreadmill && pointer == self.WaitInstructionPointer)
                value = (ushort)AnimatedTileInstruction.WaitUntilAreaBossIsDead;
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
