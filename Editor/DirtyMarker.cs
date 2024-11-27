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
        private readonly HashSet<string> dirtyFiles = new HashSet<string>();
        private readonly string path;
        private bool projectDirty;
        private GUIStyle styleDirtyFlag;

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
                if (line.Length > 3)
                {
                    var filePath = line.Substring(3).Replace("\\", "/");
                    var fullPath = path + "/" + filePath;
                    var p = fullPath;
                    while (true)
                    {
                        newDirtyFiles.Add(p);
                        if (p == path) break;
                        try
                        {
                            p = Path.GetDirectoryName(p);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            break;
                        }

                        if (string.IsNullOrEmpty(p)) break;
                        p = p.Replace("\\", "/");
                    }
                }

            if (ContentEquals(dirtyFiles, newDirtyFiles)) return;
            dirtyFiles.Clear();
            foreach (var path in newDirtyFiles)
                dirtyFiles.Add(path);
            projectDirty = true;

            return;

            static bool ContentEquals(ICollection<string> a, ICollection<string> b)
            {
                return a.All(b.Contains) && b.All(a.Contains);
            }
        }

        private void Update()
        {
            if (!Driver.MarkDirtyFiles) return;
            if (projectDirty)
            {
                projectDirty = false;
                EditorApplication.RepaintProjectWindow();
            }
        }

        private void OnProjectWindowItemGUI(string guid, Rect selectionRect)
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
