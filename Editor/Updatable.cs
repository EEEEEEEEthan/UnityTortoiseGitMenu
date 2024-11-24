using System.Diagnostics;
using System.Text;

namespace Game.Scripts.GitMenu
{
	abstract class Updater
	{
		public static void Execute(string fileName, string command)
		{
			var proc = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = command,
				UseShellExecute = false,
				CreateNoWindow = true,
			};
			Process.Start(proc);
		}

		public static void Execute(string fileName, string command, out string output)
		{
			var proc = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = command,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true,
				StandardOutputEncoding = Encoding.UTF8,
			};

			var process = Process.Start(proc);
			if (process != null)
			{
				var reader = process.StandardOutput;
				output = reader.ReadToEnd();
				process.WaitForExit();
			}
			else
			{
				output = null;
			}
		}

		public bool Dirty { get; protected set; }
		internal abstract void OnInitialize();
		internal abstract void OnUpdate();

		public void MarkDirty()
		{
			Dirty = true;
		}
	}
}
