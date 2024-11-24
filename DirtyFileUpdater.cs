using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.GitMenu
{
	sealed class DirtyFileUpdater : Updater
	{
		static GUIStyle styleDirtyFlag;
		readonly HashSet<string> dirtyPaths = new();
		string dataPath;

		internal override void OnInitialize()
		{
			dataPath = Application.dataPath;
			MarkDirty();
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
			EditorApplication.projectChanged += MarkDirty;
		}

		internal override void OnUpdate()
		{
			if (Dirty)
			{
				Dirty = false;
				UpdateDirtyFiles();
			}
		}

		void OnProjectWindowItemGUI(string guid, Rect selectionRect)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path)) return;
			try
			{
				path = System.IO.Path.GetFullPath(path);
				path = path.Replace('\\', '/');
			}
			catch (Exception e)
			{
				Debug.LogError($"unexpected path {path}");
				Debug.LogException(e);
			}
			if (dirtyPaths.Contains(path))
			{
				styleDirtyFlag ??= new() { normal = new() { textColor = Color.red } };
				var rect = new Rect(selectionRect.xMin - 2, selectionRect.yMin, 16, 16);
				GUI.Label(rect, "●", styleDirtyFlag);
			}
		}

		void UpdateDirtyFiles()
		{
			var newDirtyPaths = new HashSet<string>();
			Execute("git", "config --global core.quotepath false");
			Execute("git", "status --porcelain", out var result);
			var lines = result.Split('\n');
			var assetPath = dataPath;
			foreach (var line in lines)
				if (line.Length > 3)
				{
					var filePath = line[3..].Trim();
					filePath = Manager.commitIdUpdater.TopLevel + '/' + filePath;
					if (filePath.StartsWith(assetPath, StringComparison.Ordinal))
					{
						filePath = filePath.Replace(assetPath, "");
						while (!string.IsNullOrEmpty(filePath))
						{
							newDirtyPaths.Add(assetPath + filePath);
							var index = filePath.LastIndexOf('/');
							if (index < 0) break;
							filePath = filePath[..index];
						}
					}
				}
			if (newDirtyPaths.Count > 0) newDirtyPaths.Add(assetPath);
			if (equals(dirtyPaths, newDirtyPaths)) return;
			dirtyPaths.Clear();
			foreach (var p in newDirtyPaths)
				dirtyPaths.Add(p);

			static bool equals(HashSet<string> a, HashSet<string> b)
			{
				foreach (var p in a)
					if (!b.Contains(p))
						return false;
				foreach (var p in b)
					if (!a.Contains(p))
						return false;
				return true;
			}
		}
	}
}
