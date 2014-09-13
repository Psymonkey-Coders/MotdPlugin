using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using SEModAPI.API;

using SEModAPIExtensions.API;
using SEModAPIExtensions.API.Plugin;
using SEModAPIExtensions.API.Plugin.Events;

using SEModAPIInternal.Support;

namespace MotdPlugin
{
    public class MotdPluginCore : PluginBase
	{

		#region "Attributes"

		private static string m_assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		private static string m_dataFile = Path.Combine(m_assemblyFolder, "MotdPlugin_Settings.xml");

		private static FileIOManager m_fileManager = new FileIOManager(m_dataFile);
		MotdPluginConfigForm m_motdPluginForm = new MotdPluginConfigForm(m_fileManager);

        #endregion

        #region "Constructors and Initializers"

		public MotdPluginCore()
		{
			Console.WriteLine("Motd Plugin '" + Id.ToString() + "' Constructed!");

			ChatManager.ChatCommand motdCommand = new ChatManager.ChatCommand();
			motdCommand.command = "motd";
			motdCommand.callback = Command_Motd;
			motdCommand.requiresAdmin = true;

			ChatManager.Instance.RegisterChatCommand(motdCommand);
        }

        public override void Init()
        {
            Console.WriteLine("Motd Plugin '" + Id.ToString() + "' initialized!");
			Console.WriteLine("Motd Plugin - Set the ConfigForm to true to edit the motd and advert settings!");
        }

        #endregion

        #region "Properties"

		[Category("Motd Plugin")]
		[Description("Shows the Config Dialog,"+  
			"Change ConfigForm to true or press T after clicking on it to show the form for editing the Motd and Adverts.")]
		[Browsable(true)]
		[ReadOnly(false)]
		public bool ShowConfig
		{
			get { return m_motdPluginForm.Visible; }
			set { m_motdPluginForm.Show(); }
		}

        #endregion

        #region "EventHandlers"

		// Called when a client says something in chat.
		protected void Command_Motd(ChatManager.ChatEvent client)
		{
			ulong id = client.sourceUserId;

			if (!m_fileManager.MotdActive)
				return;

			try
			{
				Console.WriteLine("'{0}' used the command {1}.", id.ToString(), client.message.Substring(0, 5).Contains(("/motd")));
					
				ChatManager.Instance.SendPrivateChatMessage(id, m_motdPluginForm.ReplaceFormatting(m_fileManager.MotdTitle));

				if (id == 0)
					Console.WriteLine(m_motdPluginForm.ReplaceFormatting(m_fileManager.MotdTitle));

				foreach (string line in m_fileManager.MotdLines)
				{
					ChatManager.Instance.SendPrivateChatMessage(id, m_motdPluginForm.ReplaceFormatting(line));

					if (id == 0)
							Console.WriteLine(m_motdPluginForm.ReplaceFormatting(line));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Motd Plugin - Chat Error: " + ex.ToString());
			}
		}

        public override void Update()
        {
        }

		public override void Shutdown()
		{
			m_motdPluginForm.ClearAdvertTimers();
		}

		#endregion
	}
}
