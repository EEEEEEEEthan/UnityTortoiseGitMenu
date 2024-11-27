using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
	static class MenuItems
	{
		static string Path
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

		[MenuItem("Assets/TortoiseGit/Scan Git Repositories", priority = 100)]
		static void ScanGitRepositories()
		{
			Driver.ScanGitRepositories();
		}


		[MenuItem("Assets/Git/Pull", priority = 0)]
		static void Pull()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:pull /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/Push", priority = 1)]
		static void Push()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:push /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/Switch|CheckOut", priority = 2)]
		static void Switch()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:switch /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/Commit", priority = 11)]
		static void Commit()
		{
			Debug.LogError(Path);
			Command.Execute("TortoiseGitProc.exe", $"/command:commit /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/Diff", priority = 12)]
		static void Diff()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:diff /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/ShowLog", priority = 13)]
		static void ShowLog()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:log /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/Revert", priority = 14)]
		static void Revert()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:revert /path:\"{Path}\"");
		}


		[MenuItem("Assets/Git/Merge", priority = 7)]
		static void Merge()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:merge /path:\"{Path}\"");
		}
	}
}
