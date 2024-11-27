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
		const string key = "UnityTortoiseGitMenu.repositoryRoots";
		public static readonly string applicationPath;
		public static readonly string temporaryCachePath;

		static readonly Dictionary<string, GitRepositoryRoot>
			repositories = new Dictionary<string, GitRepositoryRoot>();

		static string rawPaths = "";

		static Driver()
		{
			var focusedWindow = EditorWindow.focusedWindow;
			applicationPath = Application.dataPath;
			temporaryCachePath = Application.temporaryCachePath;
			rawPaths = MainThreadRawPaths;
			if (string.IsNullOrEmpty(rawPaths))
				ScanGitRepositories();
			var paths = rawPaths.Split(';');
			foreach (var path in paths)
			{
				if (string.IsNullOrEmpty(path)) continue;
				repositories[path] = new GitRepositoryRoot(path);
			}
			new Thread(thread).Start();
			EditorApplication.update += update;

			void update()
			{
				rawPaths = MainThreadRawPaths;
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
				var rawPaths = Driver.rawPaths;
				while (true)
				{
					if (rawPaths != Driver.rawPaths)
					{
						paths = (rawPaths = Driver.rawPaths).Split(';');
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

		public static string MainThreadRawPaths
		{
			get => EditorPrefs.GetString(key, "");
			set => EditorPrefs.SetString(key, value);
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
			MainThreadRawPaths = string.Join(";", paths);
			Debug.Log($"Scanned git repositories:\n{string.Join("\n", paths)}");
		}
	}
}