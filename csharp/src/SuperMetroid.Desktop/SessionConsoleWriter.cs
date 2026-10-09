using System.Text;

namespace SuperMetroid.Desktop;

/// <summary>
/// Leaves each existing console destination intact while sharing a single session journal
/// lock across both streams. Whole WriteLine calls cannot interleave with worker output.
/// </summary>
/// <param name="session">Session journal that serializes writes and records their output.</param>
/// <param name="console">Existing console writer that receives each serialized write.</param>
internal sealed class SessionConsoleWriter(DesktopSessionLog session, TextWriter console) : TextWriter
{
    /// <summary>Gets the character encoding used by the wrapped console destination.</summary>
    public override Encoding Encoding => console.Encoding;

    /// <summary>Writes one character through the session's serialized console-and-journal path.</summary>
    /// <param name="value">Character to send to both destinations.</param>
    public override void Write(char value) => session.Write(console, stackalloc char[] { value });

    /// <summary>Writes text through the session's serialized console-and-journal path.</summary>
    /// <param name="value">Text to send; null is treated as an empty span.</param>
    public override void Write(string? value) => session.Write(console, value.AsSpan());

    /// <summary>Writes the selected character-array segment through the shared session log.</summary>
    /// <param name="buffer">Array containing the text to write.</param>
    /// <param name="index">Starting offset in <paramref name="buffer"/>.</param>
    /// <param name="count">Number of characters to write.</param>
    public override void Write(char[] buffer, int index, int count) => session.Write(console, buffer.AsSpan(index, count));

    /// <summary>Writes the supplied span through the session's serialized console-and-journal path.</summary>
    /// <param name="buffer">Text to send to both destinations.</param>
    public override void Write(ReadOnlySpan<char> buffer) => session.Write(console, buffer);

    /// <summary>Writes the console's newline sequence through the shared session log.</summary>
    public override void WriteLine() => session.Write(console, NewLine.AsSpan());

    /// <summary>Writes text followed by the console's newline sequence as one serialized operation.</summary>
    /// <param name="value">Text to send before the newline; null contributes no text.</param>
    public override void WriteLine(string? value) => session.Write(console, (value + NewLine).AsSpan());

    /// <summary>Writes a span followed by the console's newline sequence as one serialized operation.</summary>
    /// <param name="buffer">Text to send before the newline.</param>
    public override void WriteLine(ReadOnlySpan<char> buffer) => session.Write(console, (buffer.ToString() + NewLine).AsSpan());

    /// <summary>Flushes the wrapped console destination through the session log.</summary>
    public override void Flush() => session.Flush(console);
}
