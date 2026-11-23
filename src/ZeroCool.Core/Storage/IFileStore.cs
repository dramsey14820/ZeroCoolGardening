namespace ZeroCool.Core.Storage;

/// <summary>Text file storage addressed by relative paths such as "plans/abc.json".</summary>
public interface IFileStore
{
    Task<string?> ReadAsync(string path, CancellationToken ct = default);
    Task WriteAsync(string path, string content, CancellationToken ct = default);
    Task<bool> DeleteAsync(string path, CancellationToken ct = default);
    Task<IReadOnlyList<string>> ListAsync(string folder, CancellationToken ct = default);
}

