using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Game.Scripts.GitMenu
{
	sealed class CommitInfoUpdater : Updater
	{
		readonly struct CommitInfo
		{
			public readonly string commitId;
			public readonly string author;
			public readonly DateTime time;
			public readonly string message;

			public readonly bool available;

			public CommitInfo(string commitId, string author, DateTime time, string message)
			{
				this.commitId = commitId;
				this.author = author;
				this.time = time;
				this.message = message;
				available = !(commitId == default || author == default || time == default || message == default);
			}

			public CommitInfo(BinaryReader reader)
			{
				available = reader.ReadBoolean();
				if (!available)
				{
					commitId = default;
					author = default;
					time = default;
					message = default;
				}
				commitId = reader.ReadString();
				author = reader.ReadString();
				time = new(reader.ReadInt64());
				message = reader.ReadString();
			}

			public void Serialize(BinaryWriter writer)
			{
				writer.Write(available);
				if (!available) return;
				writer.Write(commitId);
				writer.Write(author);
				writer.Write(time.Ticks);
				writer.Write(message);
			}
		}

		static string cacheFile;

		static CommitInfo Query(string fullPath)
		{
			var command = $"log -1 --pretty=format:\"%H|%an|%ad|%s\" --date=iso \"{fullPath}\"";
			try
			{
				Execute("git.exe", command, out var result);
				Debug.Log(result);
				var bytes = Encoding.UTF8.GetBytes(result);
				var parts = result.Split('|');
				return new(parts[0], parts[1], DateTime.Parse(parts[2]), parts[3]);
			}
			catch
			{
				return default;
			}
		}

		readonly Dictionary<string, CommitInfo> cache = new();
		readonly List<string> pending = new();
		readonly StringBuilder builder = new();
		string commitId;
		GUIStyle styleLastCommit;
		bool serializeChanged;
		DateTime lastSerializeTime;

		public CommitInfoUpdater()
		{
			cacheFile = Path.Combine(Application.temporaryCachePath, "commitInfo.data");
		}

		internal override void OnInitialize()
		{
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
			Deserialize();
		}

		internal override void OnUpdate()
		{
			if (commitId != Manager.commitIdUpdater.CommitId)
			{
				serializeChanged = true;
				cache.Clear();
				Manager.Repaint();
			}
			commitId = Manager.commitIdUpdater.CommitId;
			if (pending.Count > 0)
			{
				var path = pending[^1];
				pending.RemoveAt(pending.Count - 1);
				cache[path] = Query(path);
				serializeChanged = true;
				Manager.Repaint();
			}
			var span = DateTime.Now - lastSerializeTime;
			if ((serializeChanged && span.TotalSeconds > 1) || span.TotalSeconds < 0)
			{
				serializeChanged = false;
				lastSerializeTime = DateTime.Now;
				Serialize();
			}
		}

		void Serialize()
		{
			try
			{
				using var file = File.Open(cacheFile, FileMode.Create);
				using var writer = new BinaryWriter(file);
				writer.Write(commitId);
				var index = (int)writer.BaseStream.Position;
				writer.Write(cache.Count);
				var count = 0;
				foreach (var (path, info) in cache)
				{
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

		void Deserialize()
		{
			try
			{
				using var file = File.Open(cacheFile, FileMode.Open);
				using var reader = new BinaryReader(file);
				var commitId = reader.ReadString();
				if (commitId != Manager.commitIdUpdater.CommitId)
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

		void OnProjectWindowItemGUI(string guid, Rect selectionRect)
		{
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
				// 靠右
				styleLastCommit ??= new() { normal = new() { textColor = Color.gray }, alignment = TextAnchor.UpperRight };
				var commit = GetCommitInfo(path);
				if (commit.time != default)
				{
					builder.Clear();
					if (selectionRect.width > 200)
						builder.Append(commit.author);
					if (selectionRect.width > 500) builder.Append(", " + commit.message.Replace("\n", " "));
					if (selectionRect.width > 300)
					{
						var time = commit.time;
						var span = DateTime.Now - time;
						string timeText;
						if (span.TotalMinutes < 1)
							timeText = $", {(int)span.TotalSeconds} seconds ago";
						else if (span.TotalHours < 1)
							timeText = $", {(int)span.TotalMinutes} minutes ago";
						else if (span.TotalDays < 1)
							timeText = $", {(int)span.TotalHours} hours ago";
						else if (span.TotalDays < 7)
							timeText = $", {(int)span.TotalDays} days ago";
						else
							timeText = ", " + time.ToString("yyyy-MM-dd");
						builder.Append(timeText);
					}
					GUI.Label(selectionRect, builder.ToString(), styleLastCommit);
				}
			}
		}

		CommitInfo GetCommitInfo(string path)
		{
			if (cache.TryGetValue(path, out var commitInfo)) return commitInfo;
			commitInfo = default;
			cache[path] = commitInfo;
			pending.Add(path);
			return commitInfo;
		}
	}
}
