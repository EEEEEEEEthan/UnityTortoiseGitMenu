using UnityEditor;

namespace UnityTortoiseGitMenu.Editor
{
	internal static class MenuItems
	{
		static string Path
		{
			get
			{
				if (!Selection.activeObject) return "";
				return AssetDatabase.GetAssetPath(Selection.activeObject);
			}
		}

		[MenuItem("Assets/Git/Scan Git Repositories", priority = 100)]
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

		[MenuItem("Assets/Git/Commit", priority = 2)]
		static void Commit()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:commit /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/Diff", priority = 3)]
		static void Diff()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:diff /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/ShowLog", priority = 4)]
		static void ShowLog()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:log /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/Revert", priority = 5)]
		static void Revert()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:revert /path:\"{Path}\"");
		}

		[MenuItem("Assets/Git/Switch|CheckOut", priority = 6)]
		static void Switch()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:switch /path:\"{Path}\"");
		}
		
		[MenuItem("Assets/Git/Merge", priority = 7)]
		static void Merge()
		{
			Command.Execute("TortoiseGitProc.exe", $"/command:merge /path:\"{Path}\"");
		}
	}
}