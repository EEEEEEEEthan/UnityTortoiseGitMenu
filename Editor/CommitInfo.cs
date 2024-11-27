using System;
using System.IO;

namespace UnityTortoiseGitMenu.Editor
{
	internal struct CommitInfo
	{
		public string author;
		public DateTime time;
		public string message;
		public bool available;

		public bool NeedSerialize
		{
			get
			{
				if(!available) return false;
				if (string.IsNullOrEmpty(author)) return false;
				if (time == default) return false;
				if (string.IsNullOrEmpty(message)) return false;
				return true;
			}
		}

		public CommitInfo(BinaryReader reader)
		{
			available = reader.ReadBoolean();
			if (!available)
			{
				author = default;
				time = default;
				message = default;
			}
			author = reader.ReadString();
			time = new DateTime(reader.ReadInt64());
			message = reader.ReadString();
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(available);
			if (!NeedSerialize) return;
			writer.Write(author);
			writer.Write(time.Ticks);
			writer.Write(message);
		}
	}
}