using System.Diagnostics;
using System.Text;
using Debug = UnityEngine.Debug;

namespace UnityTortoiseGitMenu.Editor
{
	internal static class Command
	{
		public static void Execute(string fileName, string command)
		{
			var proc = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = command,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			Process.Start(proc);
		}

		public static void Execute(string fileName, string command, out string output)
		{
			//Debug.Log($"Executing: {fileName} {command}");
			var proc = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = command,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				StandardOutputEncoding = Encoding.UTF8
			};

			var process = Process.Start(proc);
			if (process != null)
			{
				var reader = process.StandardOutput;
				output = reader.ReadToEnd();
				process.WaitForExit();
				//Debug.Log($"Output: {output}");
			}
			else
			{
				output = null;
			}
		}
		
		public static void Execute(string fileName, string command, string workingDirectory, out string output)
		{
			//Debug.Log($"Executing: {fileName} {command}");
			var proc = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = command,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				StandardOutputEncoding = Encoding.UTF8,
				WorkingDirectory = workingDirectory,
			};

			var process = Process.Start(proc);
			if (process != null)
			{
				var reader = process.StandardOutput;
				output = reader.ReadToEnd();
				process.WaitForExit();
				//Debug.Log($"Output: {output}");
			}
			else
			{
				output = null;
			}
		}
	}
}