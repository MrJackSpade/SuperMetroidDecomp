namespace SuperMetroid.Core.Game;

/// <summary>A one-frame catalog whose program has a single presentation operand.</summary>
internal interface ISinglePresentationOperand
{
    /// <summary>Native address of the lone sprite-pointer operand.</summary>
    static abstract ushort PresentationWord { get; }
}
