using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
	[InitializeOnLoad]
	internal static class Driver
	{
		const string keyRawPaths = "TortoiseGitMenu.repositoryRoots";
		const string keyMarkDirtyFiles = "TortoiseGitMenu.showDirtyFiles";
		const string keyShowLastCommit = "TortoiseGitMenu.showLastCommit";
		const string keyUseDoubaoAI = "TortoiseGitMenu.useDoubaoAI";
		const string keyDoubaoModelName = "TortoiseGitMenu.doubaoModelName";
		const string keyDoubaoAPIKey = "TortoiseGitMenu.doubaoAPIKey";
		const string keyPromptForCommit = "TortoiseGitMenu.promptForCommit";

		public const string defaultPromptForCommit =
			"你是专业的Unity游戏开发者，现你需要对下面的git diff输出，产生一个git提交日志。日志格式为不超过50字的一句话+回车+回车+200字以内的详细描述。";

		public static readonly string temporaryCachePath;
		static readonly string applicationPath;

		static readonly Dictionary<string, GitRepositoryRoot>
			repositories = new Dictionary<string, GitRepositoryRoot>();

		static Driver()
		{
			var focusedWindow = EditorWindow.focusedWindow;
			applicationPath = Application.dataPath;
			temporaryCachePath = Application.temporaryCachePath;
			RawPaths = PrefRawPaths;
			MarkDirtyFiles = PrefMarkDirtyFiles;
			ShowLastCommit = PrefShowLastCommit;
			if (string.IsNullOrEmpty(RawPaths))
				ScanGitRepositories();
			var paths = RawPaths.Split(';');
			var availablePaths = new List<string>();
			foreach (var path in paths)
			{
				if (string.IsNullOrEmpty(path)) continue;
				try
				{
					repositories[path] = new GitRepositoryRoot(path);
					availablePaths.Add(path);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
			RawPaths = string.Join(";", availablePaths);

			Task.Run(Thread);
			EditorApplication.update += Update;
			return;

			void Update()
			{
				var newFocused = EditorWindow.focusedWindow;
				if (newFocused == focusedWindow) return;
				focusedWindow = newFocused;
				foreach (var repository in repositories.Values.Where(repository => !repository.Disposed))
					repository.OnFocusChanged();
			}

			void Thread()
			{
				var tobeRemoved = new List<string>();
				var rawPaths = RawPaths;
				while (true)
				{
					if (rawPaths != RawPaths)
					{
						paths = (rawPaths = RawPaths).Split(';');
						foreach (var path in paths)
							if (!repositories.ContainsKey(path))
								repositories[path] = new GitRepositoryRoot(path);
						foreach (var path in repositories.Keys.Where(path => !paths.Contains(path)))
						{
							repositories[path].Dispose();
							tobeRemoved.Add(path);
						}

						foreach (var path in tobeRemoved)
							repositories.Remove(path);
						tobeRemoved.Clear();
					}

					foreach (var repository in repositories.Values.Where(repository => !repository.Disposed))
						try
						{
							repository.UpdateThreaded();
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}

					System.Threading.Thread.Sleep(100);
				}
				// ReSharper disable once FunctionNeverReturns
			}
		}

		public static bool Enabled => MarkDirtyFiles || ShowLastCommit;
		public static bool MarkDirtyFiles { get; private set; }
		public static bool ShowLastCommit { get; private set; }

		public static bool PrefMarkDirtyFiles
		{
			get => EditorUserSettings.GetConfigValue(keyMarkDirtyFiles) != "false";
			set
			{
				MarkDirtyFiles = value;
				EditorUserSettings.SetConfigValue(keyMarkDirtyFiles, value ? "true" : "false");
			}
		}

		public static bool PrefShowLastCommit
		{
			get => EditorUserSettings.GetConfigValue(keyShowLastCommit) != "false";
			set
			{
				ShowLastCommit = value;
				EditorUserSettings.SetConfigValue(keyShowLastCommit, value ? "true" : "false");
			}
		}

		public static bool UseDoubaoAI
		{
			get => EditorUserSettings.GetConfigValue(keyUseDoubaoAI) == "true";
			set => EditorUserSettings.SetConfigValue(keyUseDoubaoAI, value ? "true" : "false");
		}

		public static string DoubaoModelName
		{
			get => EditorUserSettings.GetConfigValue(keyDoubaoModelName) ?? "";
			set => EditorUserSettings.SetConfigValue(keyDoubaoModelName, value);
		}

		public static string DoubaoAPIKey
		{
			get => EditorUserSettings.GetConfigValue(keyDoubaoAPIKey) ?? "";
			set => EditorUserSettings.SetConfigValue(keyDoubaoAPIKey, value);
		}

		public static string PromptForCommit
		{
			get => EditorUserSettings.GetConfigValue(keyPromptForCommit) ?? defaultPromptForCommit;
			set => EditorUserSettings.SetConfigValue(keyPromptForCommit, value);
		}

		static string RawPaths { get; set; }

		static string PrefRawPaths
		{
			get => EditorUserSettings.GetConfigValue(keyRawPaths) ?? "";
			set
			{
				RawPaths = value;
				EditorUserSettings.SetConfigValue(keyRawPaths, value);
			}
		}

		public static void ScanGitRepositories()
		{
			string root;
			var paths = new HashSet<string>();
			Command.Execute("git", "config --global core.quotepath false");
			Command.Execute("git", "rev-parse --show-toplevel", out var toplevel);
			toplevel = toplevel.Trim();
			if (string.IsNullOrEmpty(toplevel))
			{
				root = applicationPath;
			}
			else
			{
				root = toplevel;
				paths.Add(root);
			}

			var directory = new DirectoryInfo(root);
			foreach (var info in directory.GetDirectories(".git", SearchOption.AllDirectories))
				if (info.Parent != null)
					paths.Add(info.Parent.FullName.Replace("\\", "/"));
			PrefRawPaths = string.Join(";", paths);
			Debug.Log($"Scanned git repositories:\n{string.Join("\n", paths)}");
		}

		public static void Refresh()
		{
			foreach (var repository in repositories.Values)
				repository.Refresh();
		}
	}
}