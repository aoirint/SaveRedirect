using System.IO.Compression;

using Mono.Cecil;

using SaveRedirect;
using SaveRedirect.Package;

if (args.Length != 1 || !File.Exists(args[0]))
{
    Console.Error.WriteLine("Pass the built SaveRedirect plugin DLL.");
    return 2;
}

string pluginPath = Path.GetFullPath(args[0]);
const string version = "0.1.0";
RunPathPolicyTests();
ArchiveContract.ValidatePlugin(File.ReadAllBytes(pluginPath), version);
RunArchiveContractTests(pluginPath, version);
return 0;

static void RunPathPolicyTests()
{
    string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "save-redirect-tests"));
    string[] compatiblePaths =
    [
        "LCSaveFile1",
        "LCSaveFile16",
        "LGU_1.json",
        "TempFile1",
        "LGUTempFile1",
        Path.Combine("nested", "LCSaveFile1"),
    ];
    foreach (string relativePath in compatiblePaths)
    {
        Equal(Path.Combine(root, relativePath), SavePathPolicy.Resolve(root, relativePath));
    }

    Equal(root, SavePathPolicy.NormalizeRoot(root + Path.DirectorySeparatorChar));
    string volumeRoot = Path.GetPathRoot(root)!;
    Equal(volumeRoot, SavePathPolicy.NormalizeRoot(volumeRoot));
    foreach (string rejected in new[] { "", "..\\normal-save", Path.GetFullPath("normal-save") })
    {
        Throws<ArgumentException>(() => SavePathPolicy.Resolve(root, rejected));
    }
}

static void RunArchiveContractTests(string pluginPath, string version)
{
    string root = Path.Combine(Path.GetTempPath(), "save-redirect-package-tests-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(root);
    try
    {
        string valid = Path.Combine(root, "valid.zip");
        WriteArchive(valid, RequiredEntries(File.ReadAllBytes(pluginPath)));
        ArchiveContract.Validate(valid, version);

        Reject(root, "missing.zip", entries => entries.Remove("README.md"), pluginPath, version);
        Reject(root, "unexpected.zip", entries => entries.Add("unexpected.txt", []), pluginPath, version);
        Reject(root, "extra-dll.zip", entries => entries.Add("other.dll", []), pluginPath, version);
        Reject(root, "corrupt.zip", entries => entries[ArchiveContract.PluginFileName] = [1, 2, 3], pluginPath, version);
        Reject(root, "wrong-assembly.zip", entries => entries[ArchiveContract.PluginFileName] = MutatePlugin(pluginPath, assembly => assembly.Name.Name = "Wrong"), pluginPath, version);
        Reject(root, "wrong-guid.zip", entries => entries[ArchiveContract.PluginFileName] = MutateAttribute(pluginPath, "BepInEx.BepInPlugin", 0, "wrong.guid"), pluginPath, version);
        Reject(root, "wrong-name.zip", entries => entries[ArchiveContract.PluginFileName] = MutateAttribute(pluginPath, "BepInEx.BepInPlugin", 1, "Wrong"), pluginPath, version);
        Reject(root, "wrong-version.zip", entries => entries[ArchiveContract.PluginFileName] = MutateAttribute(pluginPath, "BepInEx.BepInPlugin", 2, "9.9.9"), pluginPath, version);
        Reject(root, "wrong-process.zip", entries => entries[ArchiveContract.PluginFileName] = MutateAttribute(pluginPath, "BepInEx.BepInProcess", 0, "Other.exe"), pluginPath, version);
        Reject(root, "missing-attribute.zip", entries => entries[ArchiveContract.PluginFileName] = MutatePlugin(pluginPath, assembly => GetPluginType(assembly).CustomAttributes.Remove(GetAttribute(assembly, "BepInEx.BepInProcess"))), pluginPath, version);

        string duplicate = Path.Combine(root, "duplicate.zip");
        using (ZipArchive archive = ZipFile.Open(duplicate, ZipArchiveMode.Create))
        {
            foreach ((string name, byte[] bytes) in RequiredEntries(File.ReadAllBytes(pluginPath)))
            {
                WriteEntry(archive, name, bytes);
            }
            WriteEntry(archive, "README.md", []);
        }
        Throws<InvalidDataException>(() => ArchiveContract.Validate(duplicate, version));

        RejectUnsafe(root, "traversal.zip", "../README.md", pluginPath, version);
        RejectUnsafe(root, "absolute.zip", "/README.md", pluginPath, version);
        RejectUnsafe(root, "backslash.zip", "folder\\README.md", pluginPath, version);

        string oversized = Path.Combine(root, "oversized.zip");
        Dictionary<string, byte[]> oversizedEntries = RequiredEntries(File.ReadAllBytes(pluginPath));
        oversizedEntries["README.md"] = new byte[33 * 1024 * 1024];
        WriteArchive(oversized, oversizedEntries, CompressionLevel.NoCompression);
        Throws<InvalidDataException>(() => ArchiveContract.Validate(oversized, version));

        string symlink = Path.Combine(root, "symlink.zip");
        using (ZipArchive archive = ZipFile.Open(symlink, ZipArchiveMode.Create))
        {
            foreach ((string name, byte[] bytes) in RequiredEntries(File.ReadAllBytes(pluginPath)))
            {
                ZipArchiveEntry entry = WriteEntry(archive, name, bytes);
                if (name == "README.md")
                {
                    entry.ExternalAttributes = 0xA000 << 16;
                }
            }
        }
        Throws<InvalidDataException>(() => ArchiveContract.Validate(symlink, version));
    }
    finally
    {
        Directory.Delete(root, recursive: true);
    }
}

static Dictionary<string, byte[]> RequiredEntries(byte[] plugin)
{
    return new Dictionary<string, byte[]>(StringComparer.Ordinal)
    {
        [ArchiveContract.PluginFileName] = plugin,
        ["README.md"] = "readme"u8.ToArray(),
        ["CHANGELOG.md"] = "changelog"u8.ToArray(),
        ["LICENSE"] = "license"u8.ToArray(),
    };
}

static void Reject(
    string root,
    string name,
    Action<Dictionary<string, byte[]>> mutate,
    string pluginPath,
    string version
)
{
    Dictionary<string, byte[]> entries = RequiredEntries(File.ReadAllBytes(pluginPath));
    mutate(entries);
    string path = Path.Combine(root, name);
    WriteArchive(path, entries);
    Throws<InvalidDataException>(() => ArchiveContract.Validate(path, version));
}

static void RejectUnsafe(string root, string name, string unsafeName, string pluginPath, string version)
{
    Dictionary<string, byte[]> entries = RequiredEntries(File.ReadAllBytes(pluginPath));
    entries.Remove("README.md");
    entries.Add(unsafeName, []);
    string path = Path.Combine(root, name);
    WriteArchive(path, entries);
    Throws<InvalidDataException>(() => ArchiveContract.Validate(path, version));
}

static void WriteArchive(
    string path,
    IEnumerable<KeyValuePair<string, byte[]>> entries,
    CompressionLevel compressionLevel = CompressionLevel.Optimal
)
{
    using ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create);
    foreach ((string name, byte[] bytes) in entries)
    {
        WriteEntry(archive, name, bytes, compressionLevel);
    }
}

static ZipArchiveEntry WriteEntry(
    ZipArchive archive,
    string name,
    byte[] bytes,
    CompressionLevel compressionLevel = CompressionLevel.Optimal
)
{
    ZipArchiveEntry entry = archive.CreateEntry(name, compressionLevel);
    using Stream stream = entry.Open();
    stream.Write(bytes);
    return entry;
}

static byte[] MutateAttribute(string pluginPath, string typeName, int index, string value)
{
    return MutatePlugin(
        pluginPath,
        assembly =>
        {
            CustomAttribute attribute = GetAttribute(assembly, typeName);
            attribute.ConstructorArguments[index] = new CustomAttributeArgument(
                attribute.ConstructorArguments[index].Type,
                value
            );
        }
    );
}

static byte[] MutatePlugin(string pluginPath, Action<AssemblyDefinition> mutate)
{
    using AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(pluginPath);
    mutate(assembly);
    using MemoryStream output = new();
    assembly.Write(output);
    return output.ToArray();
}

static TypeDefinition GetPluginType(AssemblyDefinition assembly)
{
    return assembly.MainModule.Types.Single(type => type.FullName == "SaveRedirect.Plugin");
}

static CustomAttribute GetAttribute(AssemblyDefinition assembly, string typeName)
{
    return GetPluginType(assembly).CustomAttributes.Single(
        attribute => attribute.AttributeType.FullName == typeName
    );
}

static void Equal(string expected, string actual)
{
    if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}

static void Throws<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
