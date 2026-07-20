using System;
using System.IO;

namespace SaveRedirect;

/// <summary>Framework-free lexical path confinement used by the Harmony boundary.</summary>
public static class SavePathPolicy
{
    /// <summary>Validate and canonicalize a launcher-provided save root.</summary>
    public static string NormalizeRoot(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !Path.IsPathRooted(value))
        {
            throw new ArgumentException("An absolute save root is required.", nameof(value));
        }

        string fullPath = Path.GetFullPath(value);
        string pathRoot = Path.GetPathRoot(fullPath)!;
        return string.Equals(fullPath, pathRoot, StringComparison.OrdinalIgnoreCase)
            ? pathRoot
            : fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>Resolve one Easy Save path and prove that it remains below the root.</summary>
    public static string Resolve(string root, string? requestedPath)
    {
        string normalizedRoot = NormalizeRoot(root);
        if (string.IsNullOrWhiteSpace(requestedPath) || Path.IsPathRooted(requestedPath))
        {
            throw new ArgumentException("A relative save path is required.", nameof(requestedPath));
        }

        string candidate = Path.GetFullPath(Path.Combine(normalizedRoot, requestedPath));
        string rootPrefix = normalizedRoot.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? normalizedRoot
            : normalizedRoot + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Save path escaped its instance root.", nameof(requestedPath));
        }

        return candidate;
    }
}
