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
					var showRepoAndBranchName = EditorGUILayout.Toggle(new GUIContent("Show Repo and Branch Name", "Display the repository name and branch name in the Assets directory and repository root directory."), Driver.PrefShowRepoAndBranchName);

					var useAI = EditorGUILayout.Toggle("Use AI", Driver.UseAI);
					if (useAI)
					{
						var aiProvider = (Driver.AIProvider)EditorGUILayout.EnumPopup("AI Provider", Driver.provider);
						var model = EditorGUILayout.TextField("Model", Driver.ModelName);
						var APIKey = EditorGUILayout.TextField("API Key", Driver.APIKey);
						if (!Driver.provider.Equals(aiProvider))
						{
							Driver.provider = aiProvider;
							changed = true;
							model = EditorGUILayout.TextField("Model", Driver.ModelName);
							APIKey = EditorGUILayout.TextField("API Key", Driver.APIKey);
						}
						EditorGUILayout.LabelField("Prompt for Commit");
						var promptForCommit = EditorGUILayout.TextArea(Driver.PromptForCommit, GUILayout.Height(100));
						if (GUILayout.Button("revert")) promptForCommit = Driver.defaultPromptForCommit;
						if (Driver.ModelName != model)
						{
							Driver.ModelName = model;
							changed = true;
						}

						if (Driver.APIKey != APIKey)
						{
							Driver.APIKey = APIKey;
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

					if (Driver.PrefShowRepoAndBranchName != showRepoAndBranchName)
					{
						Driver.PrefShowRepoAndBranchName = showRepoAndBranchName;
						changed = true;
					}

					if (Driver.UseAI != useAI)
					{
						Driver.UseAI = useAI;
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
					"Commit",
					"BranchName"
				}
			};

			return provider;
		}
	}
}