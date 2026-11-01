using System.Collections.Immutable;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// Names a pre-coloured picture by what it pictures and the values of the appearance inputs it depends on (#1466):
/// the subject's own name, then each input's token and value in the order extraction recorded the inputs. Extraction and
/// the room spell the same name, so the room chooses among a subject's pictures by its inputs alone.
/// </summary>
public static class AppearancePictureNames
{
    /// <summary>The name of the picture of <paramref name="subject"/> while <paramref name="inputs"/> take <paramref name="values"/>.</summary>
    /// <exception cref="ArgumentException">The subject is empty, or there is not one value for each input.</exception>
    public static string Of(string subject, ImmutableArray<AppearanceInput> inputs, ReadOnlySpan<string> values)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        Ensure.LengthEqual(values, inputs.Length);
        // Extraction names a picture for every assignment of every block, cell and written subject: the name is spelled
        // into one buffer and copied once into the string, so naming allocates only the name.
        int length = subject.Length;
        for (int input = 0; input < inputs.Length; input++)
            length += 1 + inputs[input].Token.Length + 1 + values[input].Length;
        Span<char> name = length <= NameBufferChars ? stackalloc char[NameBufferChars] : new char[length];
        subject.CopyTo(name);
        int written = subject.Length;
        for (int input = 0; input < inputs.Length; input++)
        {
            name[written++] = '|';
            inputs[input].Token.CopyTo(name[written..]);
            written += inputs[input].Token.Length;
            name[written++] = '=';
            values[input].CopyTo(name[written..]);
            written += values[input].Length;
        }
        return new string(name[..length]);
    }

    /// <summary>The characters of a name spelled on the stack; longer names are spelled on the heap.</summary>
    private const int NameBufferChars = 256;
}
