using Microsoft.CodeAnalysis;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>What every sheet holds, as the walks' spawns are routed to them.</summary>
internal sealed class SheetCollections
{
    private readonly Dictionary<Destination, Collected> _byDestination = [];

    /// <summary>What every set's shared sheet holds.</summary>
    public Collected Anywhere => Of(new Destination(DestinationKind.Anywhere));

    /// <summary>What each sheet-specific owner's sheet holds, by the owner property naming it.</summary>
    public IReadOnlyDictionary<string, Collected> Sheets =>
        _byDestination.Where(pair => pair.Key.Kind == DestinationKind.Sheet)
            .ToDictionary(pair => pair.Key.Sheet ?? throw new InvalidOperationException("A sheet destination names no sheet."), pair => pair.Value, StringComparer.Ordinal);

    /// <summary>The collection of <paramref name="destination"/>, created empty when it has none.</summary>
    public Collected Of(Destination destination)
    {
        if (!_byDestination.TryGetValue(destination, out Collected? collected))
            _byDestination.Add(destination, collected = new Collected());
        return collected;
    }
}

/// <summary>
/// Resolves graphics other code decides. Graphics handed to a method draw wherever the graphics its callers hand it do;
/// graphics copied from another object draw wherever the objects the copying code's type spawns can (the sheets its spawn
/// calls name, and the sheets its handlers switch their graphics to).
/// </summary>
internal sealed class PendingResolver
{
    private readonly Dictionary<IMethodSymbol, List<CallRecord>> _callsTo = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, List<CallRecord>> _callsBy = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, HashSet<Destination>> _reassigned = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<INamedTypeSymbol, HashSet<PendingKey>> _reassignedPending = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<PendingKey, IReadOnlySet<Destination>> _resolved = [];

    /// <summary>Indexes the calls and reassignments of <paramref name="walks"/>.</summary>
    public PendingResolver(IEnumerable<Walk> walks)
    {
        foreach (Walk walk in walks)
        {
            foreach (CallRecord call in walk.Calls)
            {
                Of(_callsTo, call.Callee).Add(call);
                Of(_callsBy, call.SiteType).Add(call);
            }
            foreach ((INamedTypeSymbol type, HashSet<Destination> destinations) in walk.Reassigned)
                Of(_reassigned, type).UnionWith(destinations);
            foreach ((INamedTypeSymbol type, HashSet<PendingKey> keys) in walk.ReassignedPending)
                Of(_reassignedPending, type).UnionWith(keys);
        }
    }

    /// <summary>The sheets graphics <paramref name="key"/> stands for can name.</summary>
    public IReadOnlySet<Destination> Resolve(PendingKey key) => Resolve(key, []);

    /// <summary>The sheets each type's handlers switch the graphics of the objects its code spawns to.</summary>
    public IEnumerable<(INamedTypeSymbol Type, IReadOnlySet<Destination> Destinations)> Reassignments()
    {
        foreach (INamedTypeSymbol type in _reassigned.Keys.Concat(_reassignedPending.Keys).Distinct(SymbolEqualityComparer.Default).OfType<INamedTypeSymbol>())
            yield return (type, Reassigned(type, []));
    }

    /// <summary>The sheets <paramref name="key"/> can name, not following keys already being resolved.</summary>
    private IReadOnlySet<Destination> Resolve(PendingKey key, HashSet<PendingKey> visiting)
    {
        if (_resolved.TryGetValue(key, out IReadOnlySet<Destination>? done))
            return done;
        var destinations = new HashSet<Destination>();
        if (!visiting.Add(key))
            return destinations;
        if (key.Source == GraphicsSource.Parameter)
        {
            foreach (CallRecord call in _callsTo.GetValueOrDefault((IMethodSymbol)key.Key) ?? [])
                Add(destinations, call, visiting);
        }
        else
        {
            var type = (INamedTypeSymbol)key.Key;
            foreach (CallRecord call in _callsBy.GetValueOrDefault(type) ?? [])
                Add(destinations, call, visiting);
            destinations.UnionWith(Reassigned(type, visiting));
        }
        visiting.Remove(key);
        if (visiting.Count == 0)
            _resolved.Add(key, destinations);
        return destinations;
    }

    /// <summary>The sheets <paramref name="type"/>'s handlers switch graphics to.</summary>
    private HashSet<Destination> Reassigned(INamedTypeSymbol type, HashSet<PendingKey> visiting)
    {
        var destinations = new HashSet<Destination>(_reassigned.GetValueOrDefault(type) ?? []);
        foreach (PendingKey key in _reassignedPending.GetValueOrDefault(type) ?? [])
            destinations.UnionWith(Resolve(key, visiting));
        return destinations;
    }

    /// <summary>Adds the sheets <paramref name="call"/>'s graphics can name.</summary>
    private void Add(HashSet<Destination> destinations, CallRecord call, HashSet<PendingKey> visiting)
    {
        if (call.Destination is { } destination)
            destinations.Add(destination);
        // Copied graphics within the same type name nothing new; other pending graphics resolve in turn.
        if (call.Pending is { } pending)
            destinations.UnionWith(Resolve(pending, visiting));
    }

    /// <summary>The list for <paramref name="key"/>, created when absent.</summary>
    private static TValue Of<TKey, TValue>(Dictionary<TKey, TValue> values, TKey key) where TKey : notnull where TValue : new()
    {
        if (!values.TryGetValue(key, out TValue? value))
            values.Add(key, value = new TValue());
        return value;
    }
}
