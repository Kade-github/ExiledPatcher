using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Loader
{
	public class ModLoader
	{
		// Token: 0x06002341 RID: 9025 RVA: 0x00002FD1 File Offset: 0x000011D1
		public static void InitMods()
		{
		}

		// Token: 0x06002342 RID: 9026 RVA: 0x000B10B4 File Offset: 0x000AF2B4
		public static byte[] ReadFile(string path)
		{
			FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				fileStream.CopyTo(memoryStream);
				result = memoryStream.ToArray();
			}
			fileStream.Close();
			return result;
		}

		// Token: 0x06002343 RID: 9027 RVA: 0x00020440 File Offset: 0x0001E640
		public static void LoadBoi()
		{
			ModLoader.Loadxd();
		}

		// Token: 0x06002344 RID: 9028 RVA: 0x000B1100 File Offset: 0x000AF300
		public static void Loadxd()
		{
			if (ModLoader.loaded)
			{
				return;
			}
			ServerConsole.AddLog("Hello, yes, EXILED is loading..");
			try
			{
				ModLoader.loaded = true;
				string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EXILED");
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				if (File.Exists(Path.Combine(text, "EXILED.dll")))
				{
					byte[] rawAssembly = ModLoader.ReadFile(Path.Combine(text, "EXILED.dll"));
					try
					{
						MethodInfo methodInfo = Assembly.Load(rawAssembly).GetTypes().SelectMany((Type p) => p.GetMethods()).FirstOrDefault((MethodInfo f) => f.Name == "EntryPointForLoader");
						if (methodInfo != null)
						{
							methodInfo.Invoke(null, null);
						}
					}
					catch (Exception arg)
					{
						ServerConsole.AddLog(string.Format("EXILED load error: {0}", arg));
					}
				}
			}
			catch (Exception ex)
			{
				ServerConsole.AddLog(ex.ToString());
			}
		}

		// Token: 0x04001FEB RID: 8171
		private static bool loaded;
	}
}
