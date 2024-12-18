using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
	internal class DirtyMarker
	{
		readonly HashSet<string> dirtyFiles = new HashSet<string>();
		readonly string path;
		bool projectDirty;
		GUIStyle styleDirtyFlag;

		public DirtyMarker(string path)
		{
			this.path = path;
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
			EditorApplication.update += Update;
		}

		public void UpdateDirtyFiles()
		{
			if (!Driver.MarkDirtyFiles) return;
			var newDirtyFiles = new HashSet<string>();
			Command.Execute("git", "status --porcelain", path, out var result);
			result = result.Replace("\"", "");
			var lines = result.Split('\n');
			foreach (var line in lines)
			{
				if (line.Length <= 3)
					continue;
				var prefix = line.Substring(0, 3);
				switch (prefix)
				{
					case "R  ": // renamed
					{
						var content = line.Substring(3);
						var index = content.IndexOf(" -> ", StringComparison.Ordinal);
						var oldPath = content.Substring(0, index).Replace("\\", "/");
						var newPath = content.Substring(index + 4).Replace("\\", "/");
						addDirtyFile(oldPath);
						addDirtyFile(newPath);
						break;
					}
					default:
					{
						var filePath = line.Substring(3);
						addDirtyFile(filePath);
						break;
					}
				}
			}

			if (ContentEquals(dirtyFiles, newDirtyFiles)) return;
			dirtyFiles.Clear();
			foreach (var path in newDirtyFiles)
				dirtyFiles.Add(path);
			projectDirty = true;
			return;

			void addDirtyFile(string filePath)
			{
				filePath = filePath.Replace("\\", "/");
				var fullPath = this.path + "/" + filePath;
				var path = fullPath;
				while (true)
				{
					if (!newDirtyFiles.Add(path)) break;
					if (path == this.path) break;
					try
					{
						path = Path.GetDirectoryName(path);
					}
					catch (Exception e)
					{
                        Debug.LogError($"unexpected path {path}");
						Debug.LogException(e);
						break;
					}

					if (string.IsNullOrEmpty(path)) break;
					path = path.Replace("\\", "/");
				}
			}

			static bool ContentEquals(ICollection<string> a, ICollection<string> b) =>
				a.All(b.Contains) && b.All(a.Contains);
		}

		void Update()
		{
			if (!Driver.MarkDirtyFiles) return;
			if (projectDirty)
			{
				projectDirty = false;
				EditorApplication.RepaintProjectWindow();
			}
		}

		void OnProjectWindowItemGUI(string guid, Rect selectionRect)
		{
			if (!Driver.MarkDirtyFiles) return;
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path)) return;
			try
			{
				path = Path.GetFullPath(path);
				path = path.Replace('\\', '/');
			}
			catch (Exception e)
			{
				Debug.LogError($"unexpected path {path}");
				Debug.LogException(e);
			}

			if (dirtyFiles.Contains(path) || dirtyFiles.Contains(path + ".meta"))
			{
				styleDirtyFlag ??= new GUIStyle { normal = new GUIStyleState { textColor = Color.red } };
				var rect = new Rect(selectionRect.xMin - 2, selectionRect.yMin, 16, 16);
				GUI.Label(rect, "●", styleDirtyFlag);
			}
		}
	}
}