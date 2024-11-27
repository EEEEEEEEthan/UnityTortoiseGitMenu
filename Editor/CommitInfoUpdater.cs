using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
    internal class CommitInfoUpdater
    {
        private static string cacheFile;
        private readonly StringBuilder builder = new StringBuilder();

        private readonly Dictionary<string, CommitInfo> cache = new Dictionary<string, CommitInfo>();
        private readonly string path;
        private readonly List<string> pending = new List<string>();
        private string commitId;
        private DateTime lastSerializeTime;
        private bool projectDirty;
        private bool serializeChanged;
        private GUIStyle styleLastCommit;

        public CommitInfoUpdater(string path)
        {
            this.path = path;
            cacheFile = Path.Combine(Driver.temporaryCachePath, $"{path.GetHashCode()}.data");
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            EditorApplication.update += () =>
            {
                if (!projectDirty) return;
                projectDirty = false;
                EditorApplication.RepaintProjectWindow();
            };
            Deserialize();
        }

        public void Update(string newestCommitId)
        {
            if (!Driver.ShowLastCommit) return;
            if (newestCommitId != commitId)
            {
                serializeChanged = true;
                cache.Clear();
                projectDirty = true;
            }

            commitId = newestCommitId;
            {
                string next = null;
                lock (pending)
                {
                    if (pending.Count > 0)
                    {
                        next = pending[pending.Count - 1];
                        pending.RemoveAt(pending.Count - 1);
                    }
                }

                if (!string.IsNullOrEmpty(next))
                {
                    cache[next] = Query(next);
                    serializeChanged = true;
                    projectDirty = true;
                }
            }
            var span = DateTime.Now - lastSerializeTime;
            if ((serializeChanged && span.TotalSeconds > 1) || span.TotalSeconds < 0)
            {
                serializeChanged = false;
                lastSerializeTime = DateTime.Now;
                Serialize();
            }
        }

        private CommitInfo Query(string fullPath)
        {
            var command = $"log -1 --pretty=format:\"%H|%an|%ad|%s\" --date=iso \"{fullPath}\"";
            try
            {
                Command.Execute("git.exe", command, path, out var result);
                var parts = result.Split('|');
                return new CommitInfo
                {
                    //commitId = parts[0],
                    author = parts[1],
                    time = DateTime.Parse(parts[2]),
                    message = parts[3],
                    available = true
                };
            }
            catch (Exception)
            {
                return default;
            }
        }

        private void Serialize()
        {
            if (!Driver.ShowLastCommit) return;
            try
            {
                if (commitId is null) return;
                using var file = File.Open(cacheFile, FileMode.Create);
                using var writer = new BinaryWriter(file);
                writer.Write(commitId);
                var index = (int)writer.BaseStream.Position;
                writer.Write(cache.Count);
                var count = 0;
                foreach (var pair in cache)
                {
                    var path = pair.Key;
                    var info = pair.Value;
                    if (!info.available) continue;
                    writer.Write(path);
                    info.Serialize(writer);
                    ++count;
                }

                writer.Seek(index, SeekOrigin.Begin);
                writer.Write(count);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void Deserialize()
        {
            if (!Driver.ShowLastCommit) return;
            if (!File.Exists(cacheFile)) return;
            try
            {
                using var file = File.Open(cacheFile, FileMode.Open);
                using var reader = new BinaryReader(file);
                var commitId = reader.ReadString();
                if (commitId != this.commitId && !string.IsNullOrEmpty(this.commitId))
                    return;
                this.commitId = commitId;
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var path = reader.ReadString();
                    var info = new CommitInfo(reader);
                    cache[path] = info;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            if (!Driver.ShowLastCommit) return;
            if (selectionRect.height <= 20)
            {
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
                    return;
                }

                styleLastCommit ??= new GUIStyle
                    { normal = new GUIStyleState { textColor = Color.gray }, alignment = TextAnchor.UpperRight };
                var commit = GetCommitInfo(path);
                builder.Clear();
                if (commit.time != default)
                {
                    // 计算author长度
                    string timeText;
                    var time = commit.time;
                    var span = DateTime.Now - time;
                    if (span.TotalMinutes < 1)
                        timeText = $"{(int)span.TotalSeconds} seconds ago";
                    else if (span.TotalHours < 1)
                        timeText = $"{(int)span.TotalMinutes} minutes ago";
                    else if (span.TotalDays < 1)
                        timeText = $"{(int)span.TotalHours} hours ago";
                    else if (span.TotalDays < 7)
                        timeText = $"{(int)span.TotalDays} days ago";
                    else
                        timeText = time.ToString("yyyy-MM-dd");
                    var timeLength = GUI.skin.label.CalcSize(new GUIContent(timeText)).x;
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                    var fileLength = GUI.skin.label.CalcSize(new GUIContent(fileNameWithoutExtension)).x;
                    if (selectionRect.width - fileLength > timeLength)
                    {
                        builder.Append(timeText);
                        var authorText = commit.author + ", ";
                        var authorLength = GUI.skin.label.CalcSize(new GUIContent(authorText)).x;
                        if (selectionRect.width - fileLength > timeLength + authorLength)
                        {
                            builder.Insert(0, authorText);
                            var commitText = commit.message.Replace("\n", " ") + ", ";
                            // 把commit多余的部分变成省略号
                            var maxLength = selectionRect.width - fileLength - timeLength - authorLength;
                            var commitLength = GUI.skin.label.CalcSize(new GUIContent(commitText)).x;
                            var cnt = commitText.Length;
                            while (commitLength > maxLength && cnt > 0)
                            {
                                commitText = commitText.Substring(0, cnt /= 2) + "..., ";
                                commitLength = GUI.skin.label.CalcSize(new GUIContent(commitText)).x;
                            }

                            builder.Insert(0, commitText);
                        }
                    }

                    GUI.Label(selectionRect, builder.ToString(), styleLastCommit);
                }
            }
        }

        private CommitInfo GetCommitInfo(string path)
        {
            if (!path.StartsWith(this.path)) return default;
            if (cache.TryGetValue(path, out var commitInfo)) return commitInfo;
            commitInfo = default;
            cache[path] = commitInfo;
            lock (pending)
            {
                var index = pending.IndexOf(path);
                if (index >= 0) pending.RemoveAt(index);
                pending.Add(path);
            }

            return commitInfo;
        }
    }
}
