namespace SuperMetroid.Core.Frontend;

/// <summary>Mutually exclusive game-over text elements in native initialization order.</summary>
public enum GameOverTextElement
{
    /// <summary>$81:9206 loads GAME OVER from $81:92DC to BG1 byte $0156.</summary>
    Title,
    /// <summary>$81:920F loads FIND THE METROID LARVA from $81:9304 to BG1 byte $038A.</summary>
    Objective,
    /// <summary>$81:9218 loads TRY AGAIN from $81:9334 to BG1 byte $0414.</summary>
    Prompt,
    /// <summary>$81:9221 loads YES - RETURN TO GAME from $81:934C to BG1 byte $04CE.</summary>
    ReturnToGame,
    /// <summary>$81:922A loads NO - GO TO TITLE from $81:93A0 to BG1 byte $05CE.</summary>
    ReturnToTitle,
}
