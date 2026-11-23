using System.Text;
using ZeroCool.Core.Storage;

namespace ZeroCool.Data;

/// <summary>File store rooted in a folder. Writes are atomic (temp file then replace) and paths cannot escape the root.</summary>
public sealed class LocalFileStore : IFileStore
{
    private readonly string _root;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public LocalFileStore(string rootFolder)
    {
        _root = Path.GetFullPath(rootFolder);
        Directory.CreateDirectory(_root);
    }

    private string Resolve(string relative)
    {
        if (string.IsNullOrWhiteSpace(relative)) throw new ArgumentException("Path is required.", nameof(relative));
        var full = Path.GetFullPath(Path.Combine(_root, relative));
        var rootWithSep = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;
        if (!full.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("Path escapes the storage root.");
        return full;
    }

    public async Task<string?> ReadAsync(string path, CancellationToken ct = default)
    {
        var full = Resolve(path);
        if (!File.Exists(full)) return null;
        return await File.ReadAllTextAsync(full, Encoding.UTF8, ct);
    }

    public async Task WriteAsync(string path, string content, CancellationToken ct = default)
    {
        var full = Resolve(path);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await _gate.WaitAsync(ct);
        try
        {
            var tmp = full + ".tmp";
            await File.WriteAllTextAsync(tmp, content, new UTF8Encoding(false), ct);
            File.Move(tmp, full, overwrite: true);
        }
        finally { _gate.Release(); }
    }

    public Task<bool> DeleteAsync(string path, CancellationToken ct = default)
    {
        var full = Resolve(path);
        if (!File.Exists(full)) return Task.FromResult(false);
        File.Delete(full);
        return Task.FromResult(true);
    }

    public Task<IReadOnlyList<string>> ListAsync(string folder, CancellationToken ct = default)
    {
        var full = Resolve(folder);
        if (!Directory.Exists(full)) return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
        IReadOnlyList<string> files = Directory.GetFiles(full, "*.json")
            .Select(f => folder.TrimEnd('/', '\\') + "/" + Path.GetFileName(f)).ToList();
        return Task.FromResult(files);
    }
}
