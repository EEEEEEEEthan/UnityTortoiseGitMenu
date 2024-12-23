using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
	internal static class Settings
	{
		[SettingsProvider]
		public static SettingsProvider CreateMyCustomSettingsProvider()
		{
			var provider = new SettingsProvider("Preferences/Tortoise Git Menu", SettingsScope.User)
			{
				label = "Tortoise Git Menu",
				guiHandler = searchContext =>
				{
					var changed = false;
					var markDirtyFiles = EditorGUILayout.Toggle("Mark Dirty Files", Driver.PrefMarkDirtyFiles);
					var showLastCommit = EditorGUILayout.Toggle("Show Last Commit", Driver.PrefShowLastCommit);
					var useDoubaoAI = EditorGUILayout.Toggle("Use Doubao AI", Driver.UseDoubaoAI);
					if (useDoubaoAI)
					{
						var doubaoModel = EditorGUILayout.TextField("Doubao Model", Driver.DoubaoModelName);
						var doubaoAPIKey = EditorGUILayout.TextField("Doubao API Key", Driver.DoubaoAPIKey);
						EditorGUILayout.LabelField("Prompt for Commit");
						var promptForCommit = EditorGUILayout.TextArea(Driver.PromptForCommit, GUILayout.Height(100));
						if (GUILayout.Button("revert")) promptForCommit = Driver.defaultPromptForCommit;

						if (Driver.DoubaoModelName != doubaoModel)
						{
							Driver.DoubaoModelName = doubaoModel;
							changed = true;
						}

						if (Driver.DoubaoAPIKey != doubaoAPIKey)
						{
							Driver.DoubaoAPIKey = doubaoAPIKey;
							changed = true;
						}

						if (Driver.PromptForCommit != promptForCommit)
						{
							Driver.PromptForCommit = promptForCommit;
							changed = true;
						}
					}

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

					if (Driver.UseDoubaoAI != useDoubaoAI)
					{
						Driver.UseDoubaoAI = useDoubaoAI;
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