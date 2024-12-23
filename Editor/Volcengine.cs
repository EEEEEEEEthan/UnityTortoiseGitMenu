using System;
using System.Text;
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
		[Serializable]
		struct Result
		{
			[Serializable]
			public struct Message
			{
				public string content;
			}

			[Serializable]
			public struct Choice
			{
				public Message message;
			}

			public Choice[] choices;
		}

		[Serializable]
		struct Request
		{
			[Serializable]
			public struct MessageParam
			{
				public string role;
				public string content;
			}

			public string model;
			public MessageParam[] messages;
		}

		public static void GetDiffMessage(string root, string path, Action<string> callback)
		{
			const string url = "https://ark.cn-beijing.volces.com/api/v3/chat/completions";
			Command.Execute("git", $"--git-dir={root}/.git --work-tree={path} diff", out var output);
			var msg = new Request
			{
				model = Driver.DoubaoModelName,
				messages = new[]
				{
					new Request.MessageParam
					{
						role = "system",
						content = Driver.PromptForCommit ?? ""
					},
					new Request.MessageParam
					{
						role = "user",
						content = output
					}
				}
			};
			//var json = JsonConvert.SerializeObject(msg);
			var json = JsonUtility.ToJson(msg);
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
				var result = JsonUtility.FromJson<Result>(text);
				callback?.Invoke(result.choices[0].message.content);
			}
		}
	}
}