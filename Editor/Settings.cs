using UnityEditor;

namespace TortoiseGitMenu.Editor
{
	public class Settings
	{
		[SettingsProvider]
		public static SettingsProvider CreateMyCustomSettingsProvider()
		{
			var provider = new SettingsProvider("Preferences/Tortoise Git Menu", SettingsScope.User)
			{
				label = "Tortoise Git Menu",
				guiHandler = searchContext =>
				{
					var markDirtyFiles = EditorGUILayout.Toggle("Mark Dirty Files", Driver.PrefMarkDirtyFiles);
					var showLastCommit = EditorGUILayout.Toggle("Show Last Commit", Driver.PrefShowLastCommit);
					var changed = false;
					if (Driver.PrefMarkDirtyFiles != markDirtyFiles)
					{
						Driver.PrefMarkDirtyFiles = markDirtyFiles;
						changed = true;
					}
					if (Driver.PrefShowLastCommit != showLastCommit)
					{
						Driver.PrefShowLastCommit = showLastCommit;
						changed = true;
					}
					if (changed)
						EditorApplication.RepaintProjectWindow();
				},
				keywords = new[]
				{
					"Tortoise",
					"Git",
					"Menu",
					"Mark",
					"Dirty",
					"Files",
					"Show",
					"Last",
					"Commit"
				}
			};

			return provider;
		}
	}
}