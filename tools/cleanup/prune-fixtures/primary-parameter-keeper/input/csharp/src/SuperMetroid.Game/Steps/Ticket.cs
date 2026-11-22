namespace SuperMetroid.Game.Steps;

/// <summary>A ticket, whose code's length production reads, and which keeps its code, trimmed, for its own rebuild.</summary>
/// <param name="code">The ticket's code.</param>
internal sealed class Ticket(string code)
{
    /// <summary>How long the ticket's code is.</summary>
    internal int Length { get; } = code.Length;

    /// <summary>The ticket's code, trimmed, which it keeps so it can be made again from what it holds.</summary>
    private string Code { get; } = code.Trim();
}
