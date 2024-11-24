using UnityEditor;

namespace Game.Scripts.GitMenu
{
	sealed class CommitIdUpdater : Updater
	{
		bool focused;
		public string TopLevel { get; private set; }
		public string CommitId { get; private set; }

		internal override void OnInitialize()
		{
			UpdateCommitId();
			EditorApplication.focusChanged += OnFocusChanged;
		}

		internal override void OnUpdate()
		{
			if (Dirty)
			{
				Dirty = false;
				UpdateCommitId();
			}
		}

		public void Refresh()
		{
			Dirty = true;
			CommitId = "";
		}

		void OnFocusChanged(bool focused)
		{
			if (this.focused != focused)
			{
				this.focused = focused;
				if (focused) Dirty = true;
			}
		}

		void UpdateCommitId()
		{
			Execute("git", "rev-parse --short HEAD", out var commitId);
			Execute("git", "rev-parse --show-toplevel", out var toplevel);
			TopLevel = toplevel[..^1];
			if (commitId != CommitId)
			{
				CommitId = commitId;
				Manager.dirtyFileUpdater.MarkDirty();
				Manager.commitInfoUpdater.MarkDirty();
			}
		}
	}
}
