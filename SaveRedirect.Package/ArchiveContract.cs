using System.IO.Compression;

using Mono.Cecil;

namespace SaveRedirect.Package;

/// <summary>Creates and validates the host-neutral SaveRedirect package contract.</summary>
public static class ArchiveContract
{
    /// <summary>The only DLL allowed at the archive root.</summary>
    public const string PluginFileName = "com.aoirint.SaveRedirect.dll";

    private const string AssemblyName = "com.aoirint.SaveRedirect";
    private const string PluginGuid = "com.aoirint.SaveRedirect";
    private const string PluginName = "SaveRedirect";
    private const string ProcessName = "Lethal Company.exe";
    private const long MaximumEntryBytes = 32 * 1024 * 1024;
    private const long MaximumArchiveBytes = 64 * 1024 * 1024;
    private const long MaximumCompressionRatio = 1_000;

    private static readonly string[] RequiredFiles =
    [
        PluginFileName,
        "README.md",
        "CHANGELOG.md",
        "LICENSE",
    ];

    /// <summary>Create one deterministic-layout ZIP and validate the produced bytes.</summary>
    public static void Create(string plugin, string output, string repositoryRoot, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plugin);
        ArgumentException.ThrowIfNullOrWhiteSpace(output);
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        ValidatePlugin(File.ReadAllBytes(plugin), version);
        if (File.Exists(output))
        {
            throw new IOException("Package output already exists.");
        }

        string? outputDirectory = Path.GetDirectoryName(Path.GetFullPath(output));
        Directory.CreateDirectory(outputDirectory!);
        using (ZipArchive archive = ZipFile.Open(output, ZipArchiveMode.Create))
        {
            AddFile(archive, plugin, PluginFileName);
            foreach (string name in RequiredFiles.Where(name => name != PluginFileName))
            {
                AddFile(archive, Path.Combine(repositoryRoot, name), name);
            }
        }

        Validate(output, version);
    }

    /// <summary>Validate archive paths, the exact file set, and semantic plugin identity.</summary>
    public static void Validate(string archivePath, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);

        using ZipArchive archive = ZipFile.OpenRead(archivePath);
        ValidateEntries(archive.Entries);
        ZipArchiveEntry plugin = archive.GetEntry(PluginFileName)!;
        using Stream source = plugin.Open();
        using MemoryStream bytes = new();
        source.CopyTo(bytes);
        ValidatePlugin(bytes.ToArray(), version);
    }

    /// <summary>Validate a built plugin assembly independently of an archive.</summary>
    public static void ValidatePlugin(byte[] bytes, string version)
    {
        try
        {
            using MemoryStream stream = new(bytes, writable: false);
            using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(stream);
            if (!string.Equals(assembly.Name.Name, AssemblyName, StringComparison.Ordinal))
            {
                throw new InvalidDataException("Plugin assembly name does not match.");
            }

            CustomAttribute plugin = GetSingleAttribute(assembly, "BepInEx.BepInPlugin");
            RequireStringArgument(plugin, 0, PluginGuid, "plugin GUID");
            RequireStringArgument(plugin, 1, PluginName, "plugin name");
            RequireStringArgument(plugin, 2, version, "plugin version");

            CustomAttribute process = GetSingleAttribute(assembly, "BepInEx.BepInProcess");
            RequireStringArgument(process, 0, ProcessName, "process restriction");
        }
        catch (BadImageFormatException exception)
        {
            throw new InvalidDataException("Plugin DLL is not a valid managed assembly.", exception);
        }
    }

    private static void ValidateEntries(IReadOnlyCollection<ZipArchiveEntry> entries)
    {
        if (entries.Count > RequiredFiles.Length)
        {
            throw new InvalidDataException("Archive contains too many entries.");
        }

        HashSet<string> names = new(StringComparer.Ordinal);
        long totalBytes = 0;
        foreach (ZipArchiveEntry entry in entries)
        {
            string name = entry.FullName;
            string[] segments = name.Split('/');
            int unixMode = (entry.ExternalAttributes >> 16) & 0xFFFF;
            int unixType = unixMode & 0xF000;
            if (
                string.IsNullOrWhiteSpace(name)
                || name.Contains('\\')
                || name.StartsWith('/')
                || name.Contains(':')
                || segments.Any(segment => segment is "" or "." or "..")
                || unixType is not (0 or 0x8000)
            )
            {
                throw new InvalidDataException("Archive contains an unsafe entry.");
            }

            if (entry.Length > MaximumEntryBytes)
            {
                throw new InvalidDataException("Archive entry exceeds the size limit.");
            }

            totalBytes = checked(totalBytes + entry.Length);
            if (
                totalBytes > MaximumArchiveBytes
                || (entry.Length > 0 && entry.CompressedLength == 0)
                || (
                    entry.CompressedLength > 0
                    && entry.Length / entry.CompressedLength > MaximumCompressionRatio
                )
            )
            {
                throw new InvalidDataException("Archive expansion exceeds the resource limit.");
            }

            if (!names.Add(name))
            {
                throw new InvalidDataException("Archive contains a duplicate entry.");
            }
        }

        if (!names.SetEquals(RequiredFiles))
        {
            throw new InvalidDataException("Archive file set does not match the contract.");
        }

        if (names.Count(name => name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) != 1)
        {
            throw new InvalidDataException("Archive must contain exactly one plugin DLL.");
        }
    }

    private static CustomAttribute GetSingleAttribute(AssemblyDefinition assembly, string typeName)
    {
        CustomAttribute[] matches = assembly.MainModule.Types
            .SelectMany(type => type.CustomAttributes)
            .Where(attribute => string.Equals(attribute.AttributeType.FullName, typeName, StringComparison.Ordinal))
            .ToArray();
        return matches.Length == 1
            ? matches[0]
            : throw new InvalidDataException($"Plugin must contain exactly one {typeName} attribute.");
    }

    private static void RequireStringArgument(
        CustomAttribute attribute,
        int index,
        string expected,
        string label
    )
    {
        if (
            attribute.ConstructorArguments.Count <= index
            || attribute.ConstructorArguments[index].Value is not string actual
            || !string.Equals(actual, expected, StringComparison.Ordinal)
        )
        {
            throw new InvalidDataException($"Plugin {label} does not match.");
        }
    }

    private static void AddFile(ZipArchive archive, string source, string name)
    {
        if (!File.Exists(source))
        {
            throw new FileNotFoundException("Required package input is missing.", source);
        }

        archive.CreateEntryFromFile(source, name, CompressionLevel.Optimal);
    }
}
