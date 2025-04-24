using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace TortoiseGitMenu.Editor
{
	[InitializeOnLoad]
	internal static class Driver
	{
		const string keyRawPaths = "TortoiseGitMenu.repositoryRoots";
		const string keyMarkDirtyFiles = "TortoiseGitMenu.showDirtyFiles";
		const string keyShowLastCommit = "TortoiseGitMenu.showLastCommit";
		const string keyUseAI = "TortoiseGitMenu.useAI";
		const string keyAIProvider = "TortoiseGitMenu.aiProvider";
		const string keyDoubaoModelName = "TortoiseGitMenu.doubaoModelName";
		const string keyDoubaoAPIKey = "TortoiseGitMenu.doubaoAPIKey";
		const string keyOpenAIModelName = "TortoiseGitMenu.OpenAIModelName";
		const string keyOpenAIAPIKey = "TortoiseGitMenu.OpenAIAPIKey";
		const string keyDeepSeekModelName = "TortoiseGitMenu.deepSeekModelName";
		const string keyDeepSeekAPIKey = "TortoiseGitMenu.deepSeekAPIKey";
		const string keyPromptForCommit = "TortoiseGitMenu.promptForCommit";
		
		public enum AIProvider
		{
			DouBao,
			DeepSeek,
			OpenAI
		}

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
				Assert.IsTrue(System.Threading.Thread.CurrentThread.ManagedThreadId != 1);
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
		
		public static bool UseAI
		{
			get => EditorUserSettings.GetConfigValue(keyUseAI) == "true";
			set => EditorUserSettings.SetConfigValue(keyUseAI, value ? "true" : "false");
		}

		public static AIProvider provider
		{
			get
			{
				return (AIProvider)Enum.Parse(typeof(AIProvider), EditorUserSettings.GetConfigValue(keyAIProvider) ?? AIProvider.DouBao.ToString());
			}
			set => EditorUserSettings.SetConfigValue(keyAIProvider, value.ToString());
		}
		
		public static string ModelName
		{
			get
			{
				switch (provider)
				{
					case AIProvider.DouBao:
						return EditorUserSettings.GetConfigValue(keyDoubaoModelName) ?? "";
						break;
					case AIProvider.DeepSeek:
						return EditorUserSettings.GetConfigValue(keyDeepSeekModelName) ?? "";
						break;
					case AIProvider.OpenAI:
						return EditorUserSettings.GetConfigValue(keyOpenAIModelName) ?? "";
						break;
				}
				return string.Empty;
			}
			set
			{
				switch (provider)
				{
					case AIProvider.DouBao:
						EditorUserSettings.SetConfigValue(keyDoubaoModelName, value);
						break;
					case AIProvider.DeepSeek:
						EditorUserSettings.SetConfigValue(keyDeepSeekModelName, value);
						break;
					case AIProvider.OpenAI:
						EditorUserSettings.SetConfigValue(keyOpenAIModelName, value);
						break;
				}
			}
		}
		
		public static string APIKey
		{
			get
			{
				switch (provider)
				{
					case AIProvider.DouBao:
						return EditorUserSettings.GetConfigValue(keyDoubaoAPIKey) ?? "";
						break;
					case AIProvider.DeepSeek:
						return EditorUserSettings.GetConfigValue(keyDeepSeekAPIKey) ?? "";
						break;
					case AIProvider.OpenAI:
						return EditorUserSettings.GetConfigValue(keyOpenAIAPIKey) ?? "";
						break;
				}
				return string.Empty;
			}
			set
			{
				switch (provider)
				{
					case AIProvider.DouBao:
						EditorUserSettings.SetConfigValue(keyDoubaoAPIKey, value);
						break;
					case AIProvider.DeepSeek:
						EditorUserSettings.SetConfigValue(keyDeepSeekAPIKey, value);
						break;
					case AIProvider.OpenAI:
						EditorUserSettings.SetConfigValue(keyOpenAIAPIKey, value);
						break;
				}
			}
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