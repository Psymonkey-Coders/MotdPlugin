using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using System.IO;
using System.Windows.Forms;


using SEModAPIInternal.Support;

namespace MotdPlugin
{
	public class FileIOManager
	{
		[Serializable()]
		public class MotdPluginConfig
		{
			private bool motdActive;
			public bool MotdActive
			{
				get { return motdActive; }
				set { motdActive = value; }
			}

			private string motdTitle;
			public string MotdTitle
			{ 
				get { return motdTitle; }
				set { motdTitle = value; }
			}

			private string[] motdLines = new string[8];
			public string[] MotdLines 
			{
				get { return motdLines; }
				set { motdLines = value; }
			}

			private bool advertsActive;
			public bool AdvertsActive
			{
				get { return advertsActive; }
				set { advertsActive = value; }
			}

			private List<Adverts.Advert> advertsList = new List<Adverts.Advert>();
			public List<Adverts.Advert> AdvertsList 
			{
				get { return advertsList; }
				set { advertsList = value; }
			}

			public MotdPluginConfig() { }
		}

		private MotdPluginConfig Config;

		private XmlSerializer Serializer;

		private static string m_dataFile;

		public FileIOManager(string datafile)
		{
			Config = new MotdPluginConfig();
			Serializer = new XmlSerializer(typeof(MotdPluginConfig));

			m_dataFile = datafile;

			try
			{

				if (!File.Exists(m_dataFile))
				{

					Config.MotdActive = true;
					Config.AdvertsActive = true;

					Config.MotdTitle = "[Message Of The Day - %date%%time%";

					Config.MotdLines[0] = ("Welcome to %servername%.");
					Config.MotdLines[1] = ("Gamemode : %gamemode%. ");
					Config.MotdLines[2] = ("World: %worldname%.");
					Config.MotdLines[3] = ("Asteroids: %asteroidcount%");
					Config.MotdLines[4] = ("Players: %playercount%");
					Config.MotdLines[5] = (" ");
					Config.MotdLines[6] = ("You can Have up to 45 characters per line");
					Config.MotdLines[7] = ("and 8 lines  because of the current limitation");


					Adverts.Instance.AddAdvert("readmotd", "Use /motd to read the Message of the day!", 360, true);
					Adverts.Instance.AddAdvert("welcome", "Welcome to the server! Its %time12% on %date%!", 360, true);

					Config.AdvertsList.AddRange(Adverts.Instance.AdvertList);


					Console.WriteLine("Motd Plugin - FileManager - Default File Created");

					this.SaveConfig();
				}
				else
				{
					FileStream readFileStream = new FileStream(m_dataFile, FileMode.Open, FileAccess.Read, FileShare.Read);
					Config = (MotdPluginConfig)Serializer.Deserialize(readFileStream);
					readFileStream.Close();
					Console.WriteLine("Motd Plugin - FileManager - Loaded Data");
				}

			}
			catch (NullReferenceException nrefex)
			{
				LogManager.GameLog.WriteLineAndConsole("Motd Plugin - FileManager - Null Reference Error: " + nrefex.ToString());
			}
			catch (FileLoadException flex)
			{
				LogManager.GameLog.WriteLineAndConsole("Motd Plugin - FileManager - File Error: " + flex.ToString());
			}
			catch (Exception ex)
			{
				LogManager.GameLog.WriteLineAndConsole("Motd Plugin - FileManager - Error: " + ex.ToString());
			}
		}

		public bool MotdActive
		{
			get { return Config.MotdActive; }
			set 
			{ 
				Config.MotdActive = value;
				SaveConfig();
			}
		}

		public string MotdTitle
		{
			get { return Config.MotdTitle; }
			set
			{ 
				Config.MotdTitle = value;
				SaveConfig();
			}
		}

		public string[] MotdLines
		{
			get { return Config.MotdLines; }
			set 
			{ 
				Config.MotdLines = value;
				SaveConfig();
			}
		}

		public bool AdvertsActive
		{
			get { return Config.AdvertsActive; }
			set
			{ 
				Config.AdvertsActive = value;
				SaveConfig();
			}
		}
		
		public List<Adverts.Advert> AdvertsList
		{
			get { return Config.AdvertsList; }
			set 
			{ 
				Config.AdvertsList = value;
				SaveConfig();
			}
		}

		public void SaveConfig()
		{
			try
			{
				TextWriter textWriter = new StreamWriter(m_dataFile);
				Serializer.Serialize(textWriter, Config);
				textWriter.Close();

				Console.WriteLine("Motd Plugin- FileManager - Saved Data");
			}
			catch (FileLoadException flex)
			{
				LogManager.GameLog.WriteLineAndConsole("Motd Plugin - FileManager - File Error: " + flex.ToString());
			}
			catch (Exception ex)
			{
				LogManager.GameLog.WriteLineAndConsole("Motd Plugin - FileManager - Error: " + ex.ToString());
			}
		}

	}
}
