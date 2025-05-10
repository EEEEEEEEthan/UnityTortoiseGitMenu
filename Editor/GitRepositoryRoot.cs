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
        private string repoAndBranchName;

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
            dirtyFlags |= DirtyFlags.BranchName;
        }

        public bool Disposed { get; private set; }
        
        public void Refresh()
        {
            dirtyFlags |= DirtyFlags.CommitId;
            dirtyFlags |= DirtyFlags.DirtyFiles;
            dirtyFlags |= DirtyFlags.BranchName;
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

            if (dirtyFlags.HasFlag(DirtyFlags.BranchName))
            {
                dirtyFlags &= ~DirtyFlags.BranchName;
                UpdateRepositoriesAndBranchName();
            }

            commitInfoUpdater.Update(lastCommitId, repoAndBranchName);
        }

        private void UpdateLastCommitId()
        {
            if (!Driver.Enabled) return;
            Command.Execute("git", "rev-parse --short HEAD", path, out var lastCommitId);
            if (this.lastCommitId == lastCommitId) return;
            this.lastCommitId = lastCommitId;
            dirtyFlags |= DirtyFlags.DirtyFiles;
        }

        private void UpdateRepositoriesAndBranchName()
        {
            if (!Driver.Enabled) return;
            var branchName = GetBranchName();
            
            var repoName = GetRepositoryName();
            var repoAndBranchName = string.Empty;
            if (!string.IsNullOrEmpty(repoName))
            {
                repoAndBranchName = $"{repoName}({branchName})";
            }
            else
            {
                repoAndBranchName = branchName;
            }
            if (this.repoAndBranchName == repoAndBranchName) return;
            this.repoAndBranchName = repoAndBranchName;
            dirtyFlags |= DirtyFlags.DirtyFiles;
        }

        private string GetBranchName()
        {
            var command = $"rev-parse --abbrev-ref HEAD";
            Command.Execute("git.exe", command, path, out var branchName);
            branchName = branchName.Trim();
            
            return branchName;
        }
        
        private string GetRepositoryName()
        {
            var command = "config --get remote.origin.url";
            Command.Execute("git.exe", command, path, out var remoteUrl);
            remoteUrl = remoteUrl.Trim();
            
            if (string.IsNullOrEmpty(remoteUrl))
                return string.Empty;
                
            // 从URL中提取仓库名
            string repoName;
            if (remoteUrl.EndsWith(".git"))
                remoteUrl = remoteUrl.Substring(0, remoteUrl.Length - 4);
            
            var lastSlashIndex = remoteUrl.LastIndexOf('/');
            if (lastSlashIndex >= 0)
                repoName = remoteUrl.Substring(lastSlashIndex + 1);
            else
                repoName = remoteUrl;
                
            return repoName;
        }

        [Flags]
        private enum DirtyFlags
        {
            CommitId = 1 << 0,
            DirtyFiles = 1 << 1,
            BranchName = 1 << 2,
        }
    }
}
