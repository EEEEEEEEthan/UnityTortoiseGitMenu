using UnityEditor;

namespace TortoiseGitMenu.Editor
{
    internal static class MenuItems
    {
        private static string Path => !Selection.activeObject ? "" : AssetDatabase.GetAssetPath(Selection.activeObject);

        [MenuItem("Assets/Git/Scan Git Repositories", priority = 100)]
        private static void ScanGitRepositories()
        {
            Driver.ScanGitRepositories();
        }


        [MenuItem("Assets/Git/Pull", priority = 0)]
        private static void Pull()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:pull /path:\"{Path}\"");
        }

        [MenuItem("Assets/Git/Push", priority = 1)]
        private static void Push()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:push /path:\"{Path}\"");
        }

        [MenuItem("Assets/Git/Switch|CheckOut", priority = 2)]
        private static void Switch()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:switch /path:\"{Path}\"");
        }

        [MenuItem("Assets/Git/Commit", priority = 11)]
        private static void Commit()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:commit /path:\"{Path}\"");
        }

        [MenuItem("Assets/Git/Diff", priority = 12)]
        private static void Diff()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:diff /path:\"{Path}\"");
        }

        [MenuItem("Assets/Git/ShowLog", priority = 13)]
        private static void ShowLog()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:log /path:\"{Path}\"");
        }

        [MenuItem("Assets/Git/Revert", priority = 14)]
        private static void Revert()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:revert /path:\"{Path}\"");
        }


        [MenuItem("Assets/Git/Merge", priority = 7)]
        private static void Merge()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:merge /path:\"{Path}\"");
        }
    }
}
