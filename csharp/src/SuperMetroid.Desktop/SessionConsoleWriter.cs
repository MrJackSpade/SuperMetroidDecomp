using System.Text;

namespace SuperMetroid.Desktop;

/// <summary>
/// Leaves each existing console destination intact while sharing a single session journal
/// lock across both streams. Whole WriteLine calls cannot interleave with worker output.
/// </summary>
internal sealed class SessionConsoleWriter(DesktopSessionLog session, TextWriter console) : TextWriter
{
    /// <summary>Uses the wrapped console's encoding for inherited <see cref="TextWriter"/> behavior.</summary>
    public override Encoding Encoding => console.Encoding;
    /// <summary>Sends one character through the shared session journal lock.</summary>
    public override void Write(char value) => session.Write(console, stackalloc char[] { value });
    /// <summary>Sends the string, or an empty span for null, through the shared journal lock.</summary>
    public override void Write(string? value) => session.Write(console, value.AsSpan());
    /// <summary>Sends only the requested array slice through the shared journal lock.</summary>
    public override void Write(char[] buffer, int index, int count) => session.Write(console, buffer.AsSpan(index, count));
    /// <summary>Writes a caller-provided character span to both console and journal under one lock.</summary>
    public override void Write(ReadOnlySpan<char> buffer) => session.Write(console, buffer);
    /// <summary>Writes the configured line ending as one journal operation.</summary>
    public override void WriteLine() => session.Write(console, NewLine.AsSpan());
    /// <summary>Keeps the value and its line ending in the same journal operation.</summary>
    public override void WriteLine(string? value) => session.Write(console, (value + NewLine).AsSpan());
    /// <summary>Copies a span into a complete line before sending it under the journal lock.</summary>
    public override void WriteLine(ReadOnlySpan<char> buffer) => session.Write(console, (buffer.ToString() + NewLine).AsSpan());
    /// <summary>Flushes the wrapped console through the session's synchronized boundary.</summary>
    public override void Flush() => session.Flush(console);
}
