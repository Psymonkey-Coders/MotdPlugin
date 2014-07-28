using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using SEModAPIExtensions.API;
using SEModAPIExtensions.API.Plugin;
using SEModAPIExtensions.API.Plugin.Events;
using SEModAPIInternal.API.Common;
using System.IO;
using System.Reflection;
using System.Timers;


namespace MotdPlugin
{
	public partial class MotdPluginConfigForm : Form, IChatEventHandler
	{

		#region "Attributes"


		private Dictionary<System.Timers.Timer, string> m_timers = new Dictionary<System.Timers.Timer, string>();

		private static bool m_isDebugging;

		private FileIOManager m_fileManager;

		Adverts m_adverts = new Adverts();

		#endregion

		#region "Constructors And Initalizers"

		public MotdPluginConfigForm(FileIOManager fileManager)
		{
			InitializeComponent();

			m_fileManager = fileManager;

			m_isDebugging = SandboxGameAssemblyWrapper.IsDebugging;



			foreach (Adverts.Advert advert in Adverts.Instance.AdvertList)
			{
				ListViewItem item = new ListViewItem(advert.Name);
				item.SubItems.Add(advert.Time.ToString());
				item.SubItems.Add(advert.Text);
				LST_AdvertsConfig_Adverts.Items.Add(item);
			}

			if (m_fileManager.AdvertsActive)
			{
				this.ParseAdverts();
			}
			else
			{
				this.ClearAdvertTimers();
			}

			CHK_MotdPlugin_Adverts_Active.Checked = m_fileManager.AdvertsActive;
			TXT_MotdPlugin_Motd_Body.Lines = m_fileManager.MotdLines.ToArray();
			TXT_MotdPlugin_Motd_Title.Text = m_fileManager.MotdTitle;
			CHK_MotdPlugin_Motd_Active.Checked = m_fileManager.MotdActive;
			Console.WriteLine("MotdPlugin - ConfigForm - Loading Data");
	
		}

		#endregion

		#region "Properties"


		#endregion

		#region "Event Handlers"

		// Called when a client says something in chat.
		public void OnChatReceived(ChatManager.ChatEvent client)
		{
			ulong id = client.sourceUserId;

			if (!m_fileManager.MotdActive)
				return;

			try
			{
				// If they said /motd                
				if (client.message.Substring(0, 5).Contains(("/motd")))
				{
					ChatManager.Instance.SendPrivateChatMessage(id, ReplaceFormatting(m_fileManager.MotdTitle));

					foreach (string line in m_fileManager.MotdLines)
					{
						ChatManager.Instance.SendPrivateChatMessage(id, ReplaceFormatting(line));
					}

				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Motd Plugin - Chat Error: " + ex.ToString());
			}
		}

		public void OnChatSent(ChatManager.ChatEvent events)
		{
		}
		#endregion

		#region "Control"

		private void BTN_MotdPlugin_Motd_Save_Click(object sender, EventArgs e)
		{
			m_fileManager.MotdActive = CHK_MotdPlugin_Motd_Active.Checked;
			m_fileManager.MotdTitle = TXT_MotdPlugin_Motd_Title.Text;
			m_fileManager.MotdLines = TXT_MotdPlugin_Motd_Body.Lines;
		}

		private void BTN_MotdPlugin_Adverts_NewAdvert_Click(object sender, EventArgs e)
		{
			string name = TXT_MotdPlugin_Adverts_Name.Text;
			string text = TXT_MotdPlugin_Adverts_Text.Text;
			int time = (int)NUM_MotdPlugin_Adverts_Time.Value;
			bool active = CHK_MotdPlugin_Adverts_EnableAdvert.Checked;

			Adverts.Instance.AddAdvert(name, text, time, active);

			ListViewItem advertlst = new ListViewItem(name);
			advertlst.SubItems.Add(time.ToString());
			advertlst.SubItems.Add(text);

			LST_AdvertsConfig_Adverts.Items.Add(advertlst);

			this.ParseAdverts();

		}

		private void BTN_MotdPlugin_Adverts_Update_Click(object sender, EventArgs e)
		{
			if (LST_AdvertsConfig_Adverts.SelectedItems[0].Selected)
			{
				ListViewItem item = LST_AdvertsConfig_Adverts.SelectedItems[0];
				item.Text = TXT_MotdPlugin_Adverts_Name.Text;
				item.SubItems[1].Text = NUM_MotdPlugin_Adverts_Time.ToString();
			}

			this.ParseAdverts();
		}

		private void BTN_MotdPlugin_Adverts_DeleteAdvert_Click(object sender, EventArgs e)
		{
			this.ParseAdverts();
		}

		private void LST_AdvertsConfig_Adverts_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.ParseAdverts();
		}

		#endregion

		#region "Methods"

		public void ClearAdvertTimers()
		{
			// Disable and Stop all timers in the Dictonary
			foreach (System.Timers.Timer timer in m_timers.Keys)
			{
				timer.Enabled = false;
				timer.Stop();
			}

			// Clear out the Dictionary and AdvertList list.
			m_timers.Clear();
		}

		private string ReplaceFormatting(string stringtoparse)
		{
			string parsedstring = "";

			parsedstring = stringtoparse.Replace("%time%", DateTime.Now.TimeOfDay.ToString());
			parsedstring = parsedstring.Replace("%date%", DateTime.Today.Date.ToString());


			return parsedstring;
		}

		private void ParseAdverts()
		{
			try
			{

				if (m_isDebugging)
					Console.WriteLine("Motd Plugin - Parsed {0} Adverts.", Adverts.Instance.AdvertList.Count());

				if (m_fileManager.AdvertsActive)
				{
					// For each advert in the list;
					// Create a timer, name it with the advert strings.
					// When the time is up for each advert it prints to Chat.
					Adverts.Instance.AdvertList.ForEach(delegate(Adverts.Advert advert)
					{

						// Setup the timers.
						System.Timers.Timer time = new System.Timers.Timer();
						time.AutoReset = true;
						time.Interval = 1000 * advert.Time;
						m_timers.Add(time, ReplaceFormatting(advert.Text));

						time.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs eventArgs)
						{
							System.Timers.Timer t = (System.Timers.Timer)sender;

							if (m_isDebugging)
								Console.WriteLine(m_timers[t]);

							ChatManager.Instance.SendPublicChatMessage(m_timers[t]);
						};

						if (advert.Active)
						{
							time.Enabled = true;
							time.Start();
						}
						else
						{
							time.Enabled = false;
							time.Stop();
						}

					});
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Motd Plugin - Advert Parsing Error: " + ex.Message);
			}

		}


		#endregion

	}
}
