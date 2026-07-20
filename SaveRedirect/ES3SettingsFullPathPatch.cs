using System;
using System.IO;

using HarmonyLib;

namespace SaveRedirect;

/// <summary>Confines persistent Easy Save 3 file paths to the selected instance root.</summary>
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
