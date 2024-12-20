﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace TortoiseGitMenu.Editor
{
    [InitializeOnLoad]
    internal static class Driver
    {
        private const string keyRawPaths = "TortoiseGitMenu.repositoryRoots";
        private const string keyMarkDirtyFiles = "TortoiseGitMenu.showDirtyFiles";
        private const string keyShowLastCommit = "TortoiseGitMenu.showLastCommit";
        private static readonly string applicationPath;
        public static readonly string temporaryCachePath;

        private static readonly Dictionary<string, GitRepositoryRoot>
            repositories = new Dictionary<string, GitRepositoryRoot>();

        static Driver()
        {
            var focusedWindow = EditorWindow.focusedWindow;
            applicationPath = Application.dataPath;
            temporaryCachePath = Application.temporaryCachePath;
            RawPaths = PrefRawPaths;
            MarkDirtyFiles = PrefMarkDirtyFiles;
            ShowLastCommit = PrefShowLastCommit;
            if (string.IsNullOrEmpty(RawPaths))
                ScanGitRepositories();
            var paths = RawPaths.Split(';');
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                repositories[path] = new GitRepositoryRoot(path);
            }

            Task.Run(Thread); 
            EditorApplication.update += Update;
            return;

            void Update()
            {
                var newFocused = EditorWindow.focusedWindow;
                if (newFocused == focusedWindow) return;
                focusedWindow = newFocused;
                foreach (var repository in repositories.Values.Where(repository => !repository.Disposed))
                    repository.OnFocusChanged();
            }

            void Thread()
            {
                var tobeRemoved = new List<string>();
                var rawPaths = RawPaths;
                while (true)
                {
                    if (rawPaths != RawPaths)
                    {
                        paths = (rawPaths = RawPaths).Split(';');
                        foreach (var path in paths)
                            if (!repositories.ContainsKey(path))
                                repositories[path] = new GitRepositoryRoot(path);
                        foreach (var path in repositories.Keys.Where(path => !paths.Contains(path)))
                        {
                            repositories[path].Dispose();
                            tobeRemoved.Add(path);
                        }

                        foreach (var path in tobeRemoved)
                            repositories.Remove(path);
                        tobeRemoved.Clear();
                    }

                    foreach (var repository in repositories.Values.Where(repository => !repository.Disposed))
                        try
                        {
                            repository.UpdateThreaded();
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }

                    System.Threading.Thread.Sleep(100);
                }
                // ReSharper disable once FunctionNeverReturns
            }
        }

        public static bool Enabled => MarkDirtyFiles || ShowLastCommit;
        private static string RawPaths { get; set; }
        public static bool MarkDirtyFiles { get; private set; }
        public static bool ShowLastCommit { get; private set; }

        public static bool PrefMarkDirtyFiles
        {
            get => EditorUserSettings.GetConfigValue(keyMarkDirtyFiles) != "false";
            set
            {
                MarkDirtyFiles = value;
                EditorUserSettings.SetConfigValue(keyMarkDirtyFiles, value ? "true" : "false");
            }
        }

        private static string PrefRawPaths
        {
            get => EditorUserSettings.GetConfigValue(keyRawPaths) ?? "";
            set
            {
                RawPaths = value;
                EditorUserSettings.SetConfigValue(keyRawPaths, value);
            }
        }

        public static bool PrefShowLastCommit
        {
            get => EditorUserSettings.GetConfigValue(keyShowLastCommit) != "false";
            set
            {
                ShowLastCommit = value;
                EditorUserSettings.SetConfigValue(keyShowLastCommit, value ? "true" : "false");
            }
        }

        public static void ScanGitRepositories()
        {
            string root;
            var paths = new HashSet<string>();
            Command.Execute("git", "config --global core.quotepath false");
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

            var directory = new DirectoryInfo(root);
            foreach (var info in directory.GetDirectories(".git", SearchOption.AllDirectories))
                if (info.Parent != null)
                    paths.Add(info.Parent.FullName.Replace("\\", "/"));
            PrefRawPaths = string.Join(";", paths);
            Debug.Log($"Scanned git repositories:\n{string.Join("\n", paths)}");
        }

        public static void Refresh()
        {
            foreach (var repository in repositories.Values)
                repository.Refresh();
        }
    }
}
