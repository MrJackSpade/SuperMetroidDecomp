using System.Text;

namespace SuperMetroid.Desktop;

/// <summary>
/// Leaves each existing console destination intact while sharing a single session journal
/// lock across both streams. Whole WriteLine calls cannot interleave with worker output.
/// </summary>
internal sealed class SessionConsoleWriter(DesktopSessionLog session, TextWriter console) : TextWriter
{
    public override Encoding Encoding => console.Encoding;
    public override void Write(char value) => session.Write(console, stackalloc char[] { value });
    public override void Write(string? value) => session.Write(console, value.AsSpan());
    public override void Write(char[] buffer, int index, int count) => session.Write(console, buffer.AsSpan(index, count));
    public override void Write(ReadOnlySpan<char> buffer) => session.Write(console, buffer);
    public override void WriteLine() => session.Write(console, NewLine.AsSpan());
    public override void WriteLine(string? value) => session.Write(console, (value + NewLine).AsSpan());
    public override void WriteLine(ReadOnlySpan<char> buffer) => session.Write(console, (buffer.ToString() + NewLine).AsSpan());
    public override void Flush() => session.Flush(console);
}
