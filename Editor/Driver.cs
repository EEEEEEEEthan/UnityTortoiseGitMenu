using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace UnityTortoiseGitMenu.Editor
{
	[InitializeOnLoad]
	internal static class Driver
	{
		const string key = "UnityTortoiseGitMenu.repositoryRoots";
		public static readonly string applicationPath;
		public static readonly string temporaryCachePath;

		static readonly Dictionary<string, GitRepositoryRoot>
			repositories = new Dictionary<string, GitRepositoryRoot>();

		static Driver()
		{
			var focusedWindow = EditorWindow.focusedWindow;
			applicationPath = Application.dataPath;
			temporaryCachePath = Application.temporaryCachePath;
			var rawPaths = RawPaths;
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
				Command.Execute("git", "config --global core.quotepath false");
				ScanGitRepositories();
				while (true)
				{
					if (RawPaths != rawPaths)
					{
						paths = (rawPaths = RawPaths).Split(';');
						foreach (var path in paths)
							if (!repositories.ContainsKey(path))
								repositories[path] = new GitRepositoryRoot(path);
						foreach (var path in repositories.Keys)
							if (!paths.Contains(path))
							{
								repositories[path].Dispose();
								repositories.Remove(path);
							}
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

		public static string RawPaths { get; set; } = "";

		public static void ScanGitRepositories()
		{
			string root;
			var paths = new HashSet<string>();
			// 先检查applicationPath是不是git目录
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
			// 递归查找git目录
			var directory = new DirectoryInfo(root);
			foreach (var info in directory.GetDirectories(".git", SearchOption.AllDirectories))
				if (info.Parent != null)
					paths.Add(info.Parent.FullName.Replace("\\", "/"));
			RawPaths = string.Join(";", paths);
		}
	}
}