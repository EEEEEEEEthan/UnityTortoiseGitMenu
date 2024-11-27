using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
		public static readonly string applicationPath;
		public static readonly string temporaryCachePath;

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
			foreach (var path in paths)
			{
				if (string.IsNullOrEmpty(path)) continue;
				repositories[path] = new GitRepositoryRoot(path);
			}
			new Thread(thread).Start();
			EditorApplication.update += update;

			void update()
			{
				var newFocused = EditorWindow.focusedWindow;
				if (newFocused != focusedWindow)
				{
					focusedWindow = newFocused;
					foreach (var repository in repositories.Values)
						if (!repository.Disposed)
							repository.OnFocusChanged();
				}
			}

			void thread()
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
						foreach (var path in repositories.Keys)
							if (!paths.Contains(path))
							{
								repositories[path].Dispose();
								tobeRemoved.Add(path);
							}
						foreach (var path in tobeRemoved)
							repositories.Remove(path);
						tobeRemoved.Clear();
					}
					foreach (var repository in repositories.Values)
					{
						if (repository.Disposed) continue;
						try
						{
							repository.UpdateThreaded();
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}
					}
					Thread.Sleep(100);
				}
				// ReSharper disable once FunctionNeverReturns
			}
		}

		public static bool Enabled => MarkDirtyFiles || ShowLastCommit;

		public static string RawPaths { get; private set; }
		public static bool MarkDirtyFiles { get; private set; }
		public static bool ShowLastCommit { get; private set; }

		public static bool PrefMarkDirtyFiles
		{
			get => EditorPrefs.GetBool(keyMarkDirtyFiles, true);
			set
			{
				MarkDirtyFiles = value;
				if (value)
					EditorPrefs.DeleteKey(keyMarkDirtyFiles);
				else
					EditorPrefs.SetBool(keyMarkDirtyFiles, MarkDirtyFiles = false);
			}
		}

		public static string PrefRawPaths
		{
			get => EditorPrefs.GetString(keyRawPaths, "");
			set
			{
				RawPaths = value;
				if (string.IsNullOrEmpty(value))
					EditorPrefs.DeleteKey(keyRawPaths);
				else
					EditorPrefs.SetString(keyRawPaths, RawPaths = value);
			}
		}

		public static bool PrefShowLastCommit
		{
			get => EditorPrefs.GetBool(keyShowLastCommit, true);
			set
			{
				ShowLastCommit = value;
				if (value)
					EditorPrefs.DeleteKey(keyShowLastCommit);
				else
					EditorPrefs.SetBool(keyShowLastCommit, ShowLastCommit = false);
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
	}
}