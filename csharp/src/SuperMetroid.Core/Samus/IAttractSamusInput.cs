namespace SuperMetroid.Core.Samus;

/// <summary>The attract demo's substitution of Samus's input, as the frame handler's alpha sees it.</summary>
internal interface IAttractSamusInput
{
    /// <summary>Whether an attract demo is substituting its recorded input for the player's.</summary>
    bool IsAttractDemo { get; }

    /// <summary>Advances the recorded input and substitutes its held and new button edges for this frame.</summary>
    void BeginAttractSamusInput();
}
