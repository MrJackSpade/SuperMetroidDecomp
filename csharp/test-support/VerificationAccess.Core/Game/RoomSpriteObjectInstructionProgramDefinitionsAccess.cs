using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="RoomSpriteObjectInstructionProgramDefinitions"/> members production does not use.</summary>
internal static class RoomSpriteObjectInstructionProgramDefinitionsAccess
{
    /// <summary>Finds a sprite-object presentation word from either of its bank-$B4 bytes.</summary>
    extension(RoomSpriteObjectInstructionProgramDefinitions)
    {
        /// <summary>Finds a presentation operand word containing the requested sprite-object byte.</summary>
        internal static bool TryGetPresentationWord(int address, out ushort wordAddress) =>
            RoomSpriteObjectInstructionProgramDefinitions.Layout.TryGetPresentationWord(address, out wordAddress);
    }
}
