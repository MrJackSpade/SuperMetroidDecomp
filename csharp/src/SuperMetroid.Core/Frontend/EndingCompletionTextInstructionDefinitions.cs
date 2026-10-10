using static SuperMetroid.Core.Assets.EndingCompletionTextSpriteDefinitions;

namespace SuperMetroid.Core.Frontend;

/// <summary>Ending typewriter and clear-time programs generated from text pacing and named control operations.</summary>
internal static class EndingCompletionTextInstructionDefinitions
{
    /// <summary>$8B:EB91, THE OPERATION WAS program.</summary>
    internal const ushort Start = 0xeb91;
    /// <summary>$8B:ECD9, exclusive end after the colon loop.</summary>
    internal const ushort End = 0xecd9;
    /// <summary>$8B:EBD7, COMPLETED SUCCESSFULLY program.</summary>
    private const ushort CompletedStart = 0xebd7;
    /// <summary>$8B:EC35, CLEAR TIME program.</summary>
    private const ushort ClearTimeStart = 0xec35;
    /// <summary>$8B:EC81, first of ten digit loops followed by the colon.</summary>
    private const ushort DigitsStart = 0xec81;
    /// <summary>Aligned words only. Native NTSC typewriter frames last8 calls, or15 at
    /// word ends. Each prefix map has a two-byte header and two five-byte sprites per
    /// letter, giving offset2*n+5*n*(n+1) for zero-based prefix n. No stored program/cache.</summary>
    internal static ushort ReadWord(ushort pointer)
    {
        int offset = pointer - Start;
        if ((uint)offset >= End - Start || (offset & 1) != 0)
            throw new InvalidDataException($"Ending completion instruction $8B:{pointer:X4} leaves its compiled lists.");
        if (pointer < CompletedStart)
            return LineWord((pointer - Start) / 2, Start, "THE OPERATION WAS", OperationMap,
                (ushort)EndingSpriteInstruction.SpawnCompletedText, 15);
        if (pointer < ClearTimeStart)
            return LineWord((pointer - CompletedStart) / 2, CompletedStart, "COMPLETED SUCCESSFULLY", CompletedMap,
                (ushort)EndingSpriteInstruction.SpawnClearTime, 8);
        if (pointer < DigitsStart) return ClearTimeWord((pointer - ClearTimeStart) / 2);
        int word = (pointer - DigitsStart) / 2;
        int glyph = word / 4;
        return (word % 4) switch
        {
            0 => 8,
            1 => glyph == 10 ? ColonMap : DigitMap(glyph),
            2 => (ushort)CinematicSpriteInstruction.Goto,
            _ => (ushort)(DigitsStart + glyph * 8),
        };
    }

    private static ushort LetterDuration(string text, int letter)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ') continue;
            if (letter-- == 0) return (ushort)(i + 1 == text.Length || text[i + 1] == ' ' ? 15 : 8);
        }
        throw new InvalidDataException("Typewriter letter leaves its text.");
    }

    private static ushort LineWord(int word, ushort start, string text, ushort firstMap, ushort callback, ushort holdDelay)
    {
        int letters = text.Count(character => character != ' ');
        if (word < letters * 2)
            return (word & 1) == 0 ? LetterDuration(text, word / 2) : PrefixMap(firstMap, word / 2);
        return (word - letters * 2) switch
        {
            0 => callback,
            1 => holdDelay,
            2 => PrefixMap(firstMap, letters - 1),
            3 => (ushort)CinematicSpriteInstruction.Goto,
            _ => (ushort)(start + letters * 4 + 2),
        };
    }

    private static ushort ClearTimeWord(int word)
    {
        const int letters = 9;
        if (word < letters * 2)
            return (word & 1) == 0 ? LetterDuration("CLEAR TIME", word / 2) : PrefixMap(ClearMap, word / 2);
        word -= letters * 2;
        if (word < 15)
        {
            int position = word / 3;
            if (word % 3 == 1) return (ushort)(position == 4 ? 128 : 8);
            if (word % 3 == 2) return PrefixMap(ClearMap, letters - 1);
            return position switch
            {
                0 => (ushort)EndingSpriteInstruction.SpawnHoursTens,
                1 => (ushort)EndingSpriteInstruction.SpawnHoursUnits,
                2 => (ushort)EndingSpriteInstruction.SpawnColon,
                3 => (ushort)EndingSpriteInstruction.SpawnMinutesTens,
                _ => (ushort)EndingSpriteInstruction.SpawnMinutesUnits,
            };
        }
        return (word - 15) switch
        {
            0 => (ushort)EndingSpriteInstruction.TransitionToCredits,
            1 => 15,
            2 => PrefixMap(ClearMap, letters - 1),
            3 => (ushort)CinematicSpriteInstruction.Goto,
            _ => (ushort)(ClearTimeStart + (letters * 2 + 16) * 2),
        };
    }
}