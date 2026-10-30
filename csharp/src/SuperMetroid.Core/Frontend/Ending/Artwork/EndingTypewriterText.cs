using System.Collections.Immutable;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// The characters the ending's typewriter types (#1467): the item-percentage labels and the final message, each character
/// with the cell it is typed at and the style it shows in. The import compiles them from the installed ending text; the
/// game types them and places each glyph's installed picture, reading no tile of the text.
/// </summary>
public sealed class EndingTypewriterText
{
    /// <summary>The item-percentage labels' characters, in typing order.</summary>
    private readonly ImmutableArray<EndingTextCharacter> itemPercentage;
    /// <summary>The final message's characters, in typing order.</summary>
    private readonly ImmutableArray<EndingTextCharacter> finalMessage;

    /// <summary>Keeps each sequence's characters.</summary>
    /// <param name="itemPercentage">The item-percentage labels' characters, in typing order.</param>
    /// <param name="finalMessage">The final message's characters, in typing order.</param>
    /// <exception cref="ArgumentException">A sequence is default.</exception>
    public EndingTypewriterText(ImmutableArray<EndingTextCharacter> itemPercentage, ImmutableArray<EndingTextCharacter> finalMessage)
    {
        if (itemPercentage.IsDefault || finalMessage.IsDefault)
            throw new ArgumentException("The ending's typewriter needs both of its sequences.");
        this.itemPercentage = itemPercentage;
        this.finalMessage = finalMessage;
    }

    /// <summary>The characters <paramref name="sequence"/> types, in typing order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The selector is not a supported ending text sequence.</exception>
    public ReadOnlySpan<EndingTextCharacter> Compile(EndingTextSequence sequence) => sequence switch
    {
        EndingTextSequence.ItemPercentage => itemPercentage.AsSpan(),
        EndingTextSequence.FinalMessage => finalMessage.AsSpan(),
        _ => throw new ArgumentOutOfRangeException(nameof(sequence), sequence, null),
    };
}
