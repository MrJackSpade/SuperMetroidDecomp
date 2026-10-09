namespace SuperMetroid.Core.Assets;

/// <summary>Compiled visual selectors for the bank-$B4 room sprite-object programs.</summary>
internal static class RoomSpriteObjectVisualDefinitions
{
    /// <summary>Cartridge bank containing the compiled room sprite-object instruction lists and visual operands.</summary>
    internal const byte Bank = 0xb4;

    /// <summary>
    /// Enumerates the distinct visual targets of all 62 compiled bank-$B4 sprite-object
    /// programs. Several object kinds share spritemaps, so the address is the stable
    /// identity for frames that do not already have a more descriptive art name.
    /// Instruction timing and object ownership remain compiled mechanics.
    /// </summary>
    internal static EnemySpritemapDefinition[] AdditionalFrames(
        IEnumerable<EnemySpritemapDefinition> namedFrames)
    {
        var known = new HashSet<ushort>();
        foreach (EnemySpritemapDefinition frame in namedFrames)
        {
            if (frame.Bank == Bank)
                known.Add(frame.Pointer);
        }

        var additional = new List<EnemySpritemapDefinition>();
        for (int index = 0;
             index < Game.RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = Game.RoomSpriteObjectInstructionProgramDefinitions
                .PresentationWordAddress(index);
            ushort pointer = FrameAt(operand);
            if (known.Add(pointer))
                additional.Add(new EnemySpritemapDefinition(Bank, pointer,
                    $"room_sprite_b4_{pointer:x4}"));
        }

        return [.. additional.OrderBy(frame => frame.Pointer)];
    }

    /// <summary>Resolves a compiled bank-$B4 presentation operand to its selected enemy spritemap pointer.</summary>
    /// <param name="operandAddress">Native word address of the sprite-object visual selector.</param>
    /// <returns>The spritemap pointer stored for that compiled operand.</returns>
    /// <exception cref="InvalidDataException">The operand address is not part of the compiled selector catalog.</exception>
    internal static ushort FrameAt(ushort operandAddress)
    {
        if (CompiledEnemyVisualSelectors.TryGet(Bank, operandAddress,
                out ushort frame))
            return frame;
        throw new InvalidDataException(
            $"Room sprite-object visual operand $B4:{operandAddress:X4} is not compiled.");
    }
}
