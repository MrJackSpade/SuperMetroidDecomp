using System.Collections.Immutable;
using System.Text;

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
        return subject + Spelling(inputs, values);
    }

    /// <summary>
    /// How a picture's name spells <paramref name="inputs"/> taking <paramref name="values"/>: each input's token and
    /// value, in order; empty for a subject that depends on no input.
    /// </summary>
    /// <exception cref="ArgumentException">There is not one value for each input.</exception>
    public static string Spelling(ImmutableArray<AppearanceInput> inputs, ReadOnlySpan<string> values)
    {
        Ensure.LengthEqual(values, inputs.Length);
        var name = new StringBuilder();
        for (int input = 0; input < inputs.Length; input++)
            name.Append('|').Append(inputs[input].Token).Append('=').Append(values[input]);
        return name.ToString();
    }
}
