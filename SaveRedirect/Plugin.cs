using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace SaveRedirect;

/// <summary>Redirects Easy Save 3 files into a launcher-selected directory.</summary>
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Lethal Company.exe")]
public sealed class Plugin : BaseUnityPlugin
{
    /// <summary>Environment variable containing the absolute isolated save root.</summary>
    public const string SaveRootEnvironmentVariable = "SAVE_REDIRECT_ROOT";

    /// <summary>Log marker emitted after the path root and Harmony patch are ready.</summary>
    public const string ReadyMarker = "[SAVEREDIRECT] ready";

    internal static string SaveRoot { get; private set; } = string.Empty;

    private static ManualLogSource? PluginLogger { get; set; }

    private void Awake()
    {
        PluginLogger = Logger;
        try
        {
            SaveRoot = SavePathPolicy.NormalizeRoot(
                Environment.GetEnvironmentVariable(SaveRootEnvironmentVariable)
            );
            Directory.CreateDirectory(SaveRoot);
            new Harmony(MyPluginInfo.PLUGIN_GUID).PatchAll(Assembly.GetExecutingAssembly());

            using SHA256 sha256 = SHA256.Create();
            string rootHash = Convert.ToBase64String(
                sha256.ComputeHash(Encoding.UTF8.GetBytes(SaveRoot))
            );
            TryLogInfo(
                $"{ReadyMarker} version={MyPluginInfo.PLUGIN_VERSION} root_sha256={rootHash}"
            );
        }
        catch (Exception exception)
        {
            TryLogFatal($"[SAVEREDIRECT] failed error={exception.GetType().Name}");
            Application.Quit(86);
        }
    }

    internal static void ReportBlockedPath(Exception exception)
    {
        TryLogError($"[SAVEREDIRECT] blocked_path error={exception.GetType().Name}");
    }

    private static void TryLogInfo(string message)
    {
        try
        {
            PluginLogger?.LogInfo(message);
        }
        catch (Exception)
        {
            // Logging must never escape plugin startup.
        }
    }

    private static void TryLogError(string message)
    {
        try
        {
            PluginLogger?.LogError(message);
        }
        catch (Exception)
        {
            // Logging must never escape a Harmony callback.
        }
    }

    private static void TryLogFatal(string message)
    {
        try
        {
            PluginLogger?.LogFatal(message);
        }
        catch (Exception)
        {
            // Process termination remains the fail-closed path.
        }
    }
}

[HarmonyPatch(typeof(ES3Settings), nameof(ES3Settings.FullPath), MethodType.Getter)]
internal static class ES3SettingsFullPathPatch
{
    private static bool Prefix(ES3Settings __instance, ref string __result)
    {
        if (
            __instance.location != ES3.Location.File
            || __instance.directory != ES3.Directory.PersistentDataPath
        )
        {
            return true;
        }

        try
        {
            __result = SavePathPolicy.Resolve(Plugin.SaveRoot, __instance.path);
            Directory.CreateDirectory(Path.GetDirectoryName(__result)!);
            return false;
        }
        catch (Exception exception)
        {
            Plugin.ReportBlockedPath(exception);
            __result = Path.Combine(Plugin.SaveRoot, "blocked-path.es3");
            return false;
        }
    }
}

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
