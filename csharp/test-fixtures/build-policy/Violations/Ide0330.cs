namespace BuildPolicy;

internal sealed class Ide0330
{
    private readonly object _gate = new();
    private int _count;

    internal int Increment()
    {
        lock (_gate)
            return ++_count;
    }
}
