using SaveRedirect.Package;

if (args.Length == 0)
{
    return Usage();
}

try
{
    Dictionary<string, string> options = ParseOptions(args.Skip(1));
    switch (args[0])
    {
        case "pack":
            ArchiveContract.Create(
                Required(options, "--plugin"),
                Required(options, "--output"),
                Directory.GetCurrentDirectory(),
                Required(options, "--version")
            );
            break;
        case "validate":
            ArchiveContract.Validate(
                Required(options, "--archive"),
                Required(options, "--version")
            );
            break;
        default:
            return Usage();
    }
}
catch (Exception exception) when (exception is ArgumentException or IOException)
{
    Console.Error.WriteLine($"SaveRedirect package error: {exception.Message}");
    return 2;
}

return 0;

static Dictionary<string, string> ParseOptions(IEnumerable<string> values)
{
    string[] items = values.ToArray();
    if (items.Length % 2 != 0)
    {
        throw new ArgumentException("Package options must use name/value pairs.");
    }

    Dictionary<string, string> options = new(StringComparer.Ordinal);
    for (int index = 0; index < items.Length; index += 2)
    {
        if (!items[index].StartsWith("--", StringComparison.Ordinal) || !options.TryAdd(items[index], items[index + 1]))
        {
            throw new ArgumentException("Package options are invalid or duplicated.");
        }
    }

    return options;
}

static string Required(IReadOnlyDictionary<string, string> options, string name)
{
    return options.TryGetValue(name, out string? value) && !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new ArgumentException($"Required option is missing: {name}");
}

static int Usage()
{
    Console.Error.WriteLine(
        "Usage: SaveRedirect.Package pack --plugin <dll> --output <zip> --version <version>"
    );
    Console.Error.WriteLine(
        "       SaveRedirect.Package validate --archive <zip> --version <version>"
    );
    return 2;
}
