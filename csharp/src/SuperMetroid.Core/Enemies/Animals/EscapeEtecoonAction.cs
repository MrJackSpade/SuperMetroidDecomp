namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>What an escape Etecoon is doing (#2022). Its values are arbitrary.</summary>
public enum EscapeEtecoonAction
{
    /// <summary>Walks, turning at walls and keeping to the floor.</summary>
    Walking,

    /// <summary>Stands still until the critters-escaped event, then starts thanking Samus.</summary>
    AwaitingEscape,

    /// <summary>Thanks Samus; its animation moves it.</summary>
    ExpressingGratitude,

    /// <summary>Runs off to the right at three and a half pixels an update, through walls.</summary>
    Escaping,
}
