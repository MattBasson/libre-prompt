namespace PromptPortal.Domain.ValueObjects;

public sealed class TagSet : IEquatable<TagSet>
{
    private readonly List<string> _tags;

    public IReadOnlyList<string> Values => _tags.AsReadOnly();

    private TagSet(List<string> tags)
    {
        _tags = tags;
    }

    public static TagSet From(IEnumerable<string> tags)
    {
        var normalized = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        return new TagSet(normalized);
    }

    public static TagSet Empty => new([]);

    public bool Contains(string tag) =>
        _tags.Contains(tag.Trim().ToLowerInvariant());

    public bool Equals(TagSet? other)
    {
        if (other is null) return false;
        return _tags.SequenceEqual(other._tags);
    }

    public override bool Equals(object? obj) => Equals(obj as TagSet);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var tag in _tags)
            hash.Add(tag);
        return hash.ToHashCode();
    }

    public override string ToString() => string.Join(", ", _tags);
}
