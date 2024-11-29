using System;
using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
    internal class GitRepositoryRoot : IDisposable
    {
        private readonly CommitInfoUpdater commitInfoUpdater;
        private readonly DirtyMarker dirtyMarker;
        private readonly string path;
        private DirtyFlags dirtyFlags;

        private string lastCommitId;

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
        
        public void Refresh()
        {
            dirtyFlags |= DirtyFlags.CommitId;
            dirtyFlags |= DirtyFlags.DirtyFiles;
            commitInfoUpdater.Refresh();
        }

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
            if (!Driver.Enabled) return;
            if (Disposed) throw new Exception("Disposed");
            if (dirtyFlags.HasFlag(DirtyFlags.CommitId))
            {
                dirtyFlags &= ~DirtyFlags.CommitId;
                UpdateLastCommitId();
            }

            if (dirtyFlags.HasFlag(DirtyFlags.DirtyFiles) && Driver.MarkDirtyFiles)
            {
                dirtyFlags &= ~DirtyFlags.DirtyFiles;
                dirtyMarker.UpdateDirtyFiles();
            }

            commitInfoUpdater.Update(lastCommitId);
        }

        private void UpdateLastCommitId()
        {
            if (!Driver.Enabled) return;
            Command.Execute("git", "rev-parse --short HEAD", path, out var lastCommitId);
            if (this.lastCommitId == lastCommitId) return;
            this.lastCommitId = lastCommitId;
            dirtyFlags |= DirtyFlags.DirtyFiles;
        }

        [Flags]
        private enum DirtyFlags
        {
            CommitId = 1 << 0,
            DirtyFiles = 1 << 1
        }
    }
}
