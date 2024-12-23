using System;
using System.Text;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace TortoiseGitMenu.Editor
{
	internal class Volcengine
	{
		/// <summary>
		///     详情见https://www.volcengine.com/docs/82379/1298454
		/// </summary>
		struct Result
		{
			public struct Message
			{
				public string content;
			}

			public struct Choice
			{
				public Message message;
			}

			public Choice[] choices;
		}

		public static void GetDiffMessage(string root, string path, Action<string> callback)
		{
			const string url = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";
			Command.Execute("git", $"--git-dir={root}/.git --work-tree={path} diff", out var output);
			var msg = new
			{
				model = Driver.DoubaoModelName,
				messages = new[]
				{
					new
					{
						role = "system",
						content = Driver.PromptForCommit ?? "",
					},
					new
					{
						role = "user",
						content = output
					}
				}
			};
			var json = JsonConvert.SerializeObject(msg);
			var request = new UnityWebRequest(url, "POST");
			request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
			request.downloadHandler = new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Authorization", $"Bearer {Driver.DoubaoAPIKey}");
			var operation = request.SendWebRequest();
			EditorApplication.update += onUpdate;

			void onUpdate()
			{
				if (!operation.isDone) return;
				EditorApplication.update -= onUpdate;
				if (request.result != UnityWebRequest.Result.Success)
					callback?.Invoke("生成日志失败: " + request.result);
				var text = request.downloadHandler.text;
				var result = JsonConvert.DeserializeObject<Result>(text);
				callback?.Invoke(result.choices[0].message.content);
			}
		}
	}
}