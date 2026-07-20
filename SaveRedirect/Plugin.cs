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
