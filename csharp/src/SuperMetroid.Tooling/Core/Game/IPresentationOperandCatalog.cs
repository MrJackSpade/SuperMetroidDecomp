namespace SuperMetroid.Core.Game;

/// <summary>A catalog whose sprite-pointer presentation operands are an indexed list.</summary>
internal interface IPresentationOperandCatalog
{
    /// <summary>Number of sprite-pointer operands selected by the program.</summary>
    static abstract int PresentationWordCount { get; }

    /// <summary>Gets the native address of one sprite-pointer operand.</summary>
    static abstract ushort PresentationWordAddress(int index);
}
