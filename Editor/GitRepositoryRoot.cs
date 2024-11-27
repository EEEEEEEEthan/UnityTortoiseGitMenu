using System;
using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
	internal class GitRepositoryRoot : IDisposable
	{
		readonly string path;
		readonly DirtyMarker dirtyMarker;
		readonly CommitInfoUpdater commitInfoUpdater;

		string lastCommitId;
		DirtyFlags dirtyFlags;

		public GitRepositoryRoot(string path)
		{
			this.path = path;
			dirtyMarker = new DirtyMarker(path);
			commitInfoUpdater = new CommitInfoUpdater(path);
			Command.Execute("git", "rev-parse --show-toplevel", path, out var toplevel);
			toplevel = toplevel.Trim().Replace("\\", "/");
			EditorApplication.projectChanged += () => dirtyFlags |= DirtyFlags.DirtyFiles;
			if (string.IsNullOrEmpty(toplevel))
			{
				Debug.LogError($"Failed to get git repository root: {path}");
				Disposed = true;
				return;
			}
			dirtyFlags |= DirtyFlags.CommitId;
			dirtyFlags |= DirtyFlags.DirtyFiles;
		}

		public bool Disposed { get; private set; }

		public void Dispose()
		{
			Disposed = true;
		}

		public void OnFocusChanged()
		{
			dirtyFlags |= DirtyFlags.CommitId;
		}

		public void UpdateThreaded()
		{
			if (Disposed) throw new Exception("Disposed");
			if (dirtyFlags.HasFlag(DirtyFlags.CommitId))
			{
				dirtyFlags &= ~DirtyFlags.CommitId;
				UpdateLastCommitId();
			}
			if (dirtyFlags.HasFlag(DirtyFlags.DirtyFiles))
			{
				dirtyFlags &= ~DirtyFlags.DirtyFiles;
				dirtyMarker.UpdateDirtyFiles();
			}
			commitInfoUpdater.Update(lastCommitId);
		}

		void UpdateLastCommitId()
		{
			Command.Execute("git", "rev-parse --short HEAD", path, out var lastCommitId);
			if (this.lastCommitId != lastCommitId)
			{
				this.lastCommitId = lastCommitId;
				dirtyFlags |= DirtyFlags.DirtyFiles;
			}
		}

		[Flags]
		enum DirtyFlags
		{
			CommitId = 1 << 0,
			DirtyFiles = 1 << 1
		}
	}
}