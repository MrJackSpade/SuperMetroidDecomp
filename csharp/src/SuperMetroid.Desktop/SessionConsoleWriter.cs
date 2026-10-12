using System.Text;

namespace SuperMetroid.Desktop;

/// <summary>
/// Leaves each existing console destination intact while sharing a single session journal
/// lock across both streams. Whole WriteLine calls cannot interleave with worker output.
/// </summary>
internal sealed class SessionConsoleWriter(DesktopSessionLog session, TextWriter console) : TextWriter
{
    /// <summary>Gets encoding.</summary>
    public override Encoding Encoding => console.Encoding;
    /// <summary>Writes.</summary>
    public override void Write(char value) => session.Write(console, stackalloc char[] { value });
    /// <summary>Writes.</summary>
    public override void Write(string? value) => session.Write(console, value.AsSpan());
    /// <summary>Writes.</summary>
    public override void Write(char[] buffer, int index, int count) => session.Write(console, buffer.AsSpan(index, count));
    /// <summary>Writes.</summary>
    public override void Write(ReadOnlySpan<char> buffer) => session.Write(console, buffer);
    /// <summary>Writes line.</summary>
    public override void WriteLine() => session.Write(console, NewLine.AsSpan());
    /// <summary>Writes line.</summary>
    public override void WriteLine(string? value) => session.Write(console, (value + NewLine).AsSpan());
    /// <summary>Writes line.</summary>
    public override void WriteLine(ReadOnlySpan<char> buffer) => session.Write(console, (buffer.ToString() + NewLine).AsSpan());
    /// <summary>Flush.</summary>
    public override void Flush() => session.Flush(console);
}
