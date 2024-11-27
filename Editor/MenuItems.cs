using UnityEditor;

namespace UnityTortoiseGitMenu.Editor
{
	static class MenuItems
	{
		[MenuItem("Git/Scan Git Repositories")]
		static void ScanGitRepositories()
		{
			Driver.ScanGitRepositories();
		}
	}
}