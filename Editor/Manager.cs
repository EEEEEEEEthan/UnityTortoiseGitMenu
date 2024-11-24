using System;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace UnityTortoiseGitMenu.Editor
{
	static class Manager
	{
		public static readonly CommitIdUpdater commitIdUpdater;
		public static readonly DirtyFileUpdater dirtyFileUpdater;
		public static readonly CommitInfoUpdater commitInfoUpdater;

		static readonly Updater[] updaters;
		static readonly int randomId = new System.Random((int)DateTime.Now.Ticks).Next();

		static bool projectDirty;

		static string Path
		{
			get
			{
				var objects = Selection.objects;
				if (objects.Length != 1) return null;
				var selectedObject = objects[0];
				if (!selectedObject) return null;
				var path = AssetDatabase.GetAssetPath(selectedObject);
				return path;
			}
		}

		static Manager()
		{
			updaters = new Updater[]
			{
				commitIdUpdater = new(),
				dirtyFileUpdater = new(),
				commitInfoUpdater = new(),
			};
		}

		public static void Repaint()
		{
			projectDirty = true;
		}

		[InitializeOnLoadMethod]
		static void Initialize()
		{
			EditorApplication.update += Update;
			foreach (var updater in updaters)
				try
				{
					updater.OnInitialize();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			new Thread(Run) { IsBackground = true }.Start();
		}

		static void Run()
		{
			var id = randomId;
			while (randomId == id)
			{
				foreach (var updatable in updaters)
					try
					{
						updatable.OnUpdate();
					}
					catch (Exception e)
					{
						Debug.LogException(e);
					}
				Thread.Sleep(100);
			}
		}

		static void Update()
		{
			if (projectDirty)
			{
				projectDirty = false;
				EditorApplication.RepaintProjectWindow();
			}
		}

		[MenuItem("Assets/TortoiseGit/Pull", priority = -1)]
		static void Pull()
		{
			Updater.Execute("TortoiseGitProc.exe", "/command:pull");
		}

		[MenuItem("Assets/TortoiseGit/Push", priority = 2)]
		static void Push()
		{
			Updater.Execute("TortoiseGitProc.exe", "/command:push");
		}

		[MenuItem("Assets/TortoiseGit/Switch", priority = 3)]
		static void Switch()
		{
			Updater.Execute("TortoiseGitProc.exe", "/command:switch");
		}

		[MenuItem("Assets/TortoiseGit/Log", priority = 101)]
		static void Log()
		{
			Updater.Execute("TortoiseGitProc.exe", $"/command:log /path:{Path}");
		}

		[MenuItem("Assets/TortoiseGit/Log", true)]
		static bool ValidateLog()
		{
			return !string.IsNullOrEmpty(Path);
		}

		[MenuItem("Assets/TortoiseGit/Commit", priority = 102)]
		static void Commit()
		{
			Updater.Execute("TortoiseGitProc.exe", $"/command:commit /path:{Path}");
		}

		[MenuItem("Assets/TortoiseGit/Commit", true)]
		static bool ValidateCommit()
		{
			return !string.IsNullOrEmpty(Path);
		}

		[MenuItem("Assets/TortoiseGit/Blame", priority = 102)]
		static void Blame()
		{
			Updater.Execute("TortoiseGitProc.exe", $"/command:blame /path:{Path}");
		}

		[MenuItem("Assets/TortoiseGit/Blame", true)]
		static bool ValidateBlame()
		{
			return !string.IsNullOrEmpty(Path);
		}

		[MenuItem("Assets/TortoiseGit/Refresh", priority = 201)]
		static void Refresh()
		{
			commitIdUpdater.Refresh();
		}
	}
}
