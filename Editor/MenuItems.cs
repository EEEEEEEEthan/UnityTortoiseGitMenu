using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
    internal static class MenuItems
    {
        private static string Path
        {
            get
            {
                var selectedObject = Selection.activeObject;
                var path = AssetDatabase.GetAssetPath(selectedObject);
                if (string.IsNullOrEmpty(path))
                {
                    // toplevel
                    Command.Execute("git", "rev-parse --show-toplevel", out var toplevel);
                    if (string.IsNullOrEmpty(toplevel))
                        return Application.dataPath;
                    return toplevel.Trim();
                }

                return path;
            }
        }
        
        [MenuItem("Assets/TortoiseGit/Pull", priority = 0)]
        private static void Pull()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:pull /path:\"{Path}\"");
        }

        [MenuItem("Assets/TortoiseGit/Push", priority = 1)]
        private static void Push()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:push /path:\"{Path}\"");
        }

        [MenuItem("Assets/TortoiseGit/Switch|CheckOut", priority = 2)]
        private static void Switch()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:switch /path:\"{Path}\"");
        }

        [MenuItem("Assets/TortoiseGit/Merge", priority = 3)]
        private static void Merge()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:merge /path:\"{Path}\"");
        }

        [MenuItem("Assets/TortoiseGit/Commit", priority = 51)]
        private static void Commit()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:commit /path:\"{Path}\"");
        }

        [MenuItem("Assets/TortoiseGit/Diff", priority = 52)]
        private static void Diff()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:diff /path:\"{Path}\"");
        }

        [MenuItem("Assets/TortoiseGit/ShowLog", priority = 53)]
        private static void ShowLog()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:log /path:\"{Path}\"");
        }

        [MenuItem("Assets/TortoiseGit/Revert", priority = 54)]
        private static void Revert()
        {
            Command.Execute("TortoiseGitProc.exe", $"/command:revert /path:\"{Path}\"");
        }
        
        [MenuItem("Assets/TortoiseGit/Refresh Project _F5", priority = 100)]
        private static void RefreshProject()
        {
            Driver.ScanGitRepositories();
            Driver.Refresh();
        }
    }
}
