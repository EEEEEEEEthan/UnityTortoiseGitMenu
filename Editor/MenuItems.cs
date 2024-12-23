using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WebSocketSharp;

namespace TortoiseGitMenu.Editor
{
	internal static class MenuItems
	{
		static string Root
		{
			get
			{
				var selectedObject = Selection.activeObject;
				var workDir = Directory.GetParent(Application.dataPath).FullName;
				var path = AssetDatabase.GetAssetPath(selectedObject);
				path = workDir + "/" + path;
				Command.Execute("git", $"rev-parse --show-toplevel {path}", out var toplevel);
				toplevel = toplevel.Trim().Split('\n').Last().Trim();
				if (string.IsNullOrEmpty(toplevel))
					return Application.dataPath;
				return toplevel.Trim();
			}
		}

		static string Path
		{
			get
			{
				var selectedObject = Selection.activeObject;
				var workDir = Directory.GetParent(Application.dataPath).FullName;
				var path = AssetDatabase.GetAssetPath(selectedObject);
				path = workDir + "/" + path;
				if (string.IsNullOrEmpty(path))
				{
					// toplevel
					Command.Execute("git", "rev-parse --show-toplevel", out var toplevel);
					if (string.IsNullOrEmpty(toplevel))
						return workDir;
					return toplevel.Trim();
				}
				return path;
			}
		}

		[MenuItem("Assets/TortoiseGit/Pull", priority = 0)]
		static void Pull()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:pull /path:\"{Path}\"");
		}

		[MenuItem("Assets/TortoiseGit/Push", priority = 1)]
		static void Push()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:push /path:\"{Path}\"");
		}

		[MenuItem("Assets/TortoiseGit/Switch|CheckOut", priority = 2)]
		static void Switch()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:switch /path:\"{Path}\"");
		}

		[MenuItem("Assets/TortoiseGit/Merge", priority = 3)]
		static void Merge()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:merge /path:\"{Path}\"");
		}

		[MenuItem("Assets/TortoiseGit/Commit", priority = 51)]
		static void Commit()
		{
			if (Driver.UseDoubaoAI)
				Volcengine.GetDiffMessage(Root, Path, message =>
				{
					if (string.IsNullOrEmpty(Path))
						Command.Execute("TortoiseGitProc.exe", $"/command:commit /logmsg:\"{message}\"");
					else
						Command.Execute("TortoiseGitProc.exe",
							$"/command:commit /logmsg:\"{message}\" /path:\"{Path}\"");
				});
			else
				Command.Execute("TortoiseGitProc.exe", $"/command:commit /path:\"{Path}\"");
		}

		[MenuItem("Assets/TortoiseGit/Diff", priority = 52)]
		static void Diff()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:diff /path:\"{Path}\"");
		}

		[MenuItem("Assets/TortoiseGit/ShowLog", priority = 53)]
		static void ShowLog()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:log /path:\"{Path}\"");
		}

		[MenuItem("Assets/TortoiseGit/Revert", priority = 54)]
		static void Revert()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:revert /path:\"{Path}\"");
		}

		[MenuItem("Assets/TortoiseGit/Refresh Project _F5", priority = 100)]
		static void RefreshProject()
		{
			Driver.ScanGitRepositories();
			Driver.Refresh();
		}
	}
}
