using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using SEModAPIInternal.API.Common;
using SEModAPIInternal.API.Server;

using SEModAPIExtensions.API;

using Sandbox.Common.ObjectBuilders;

namespace MotdPlugin
{
	public partial class MotdPluginConfigForm : Form
	{

		#region "Attributes"

		private Dictionary<System.Timers.Timer, Adverts.Advert> m_timers = new Dictionary<System.Timers.Timer, Adverts.Advert>();

		private static bool m_isDebugging;

		private FileIOManager m_fileManager;

		Adverts m_adverts = new Adverts();

		private static bool m_newAdvert;

		#endregion

		#region "Constructors And Initalizers"

		public MotdPluginConfigForm(FileIOManager fileManager)
		{
			InitializeComponent();

			m_fileManager = fileManager;

			m_isDebugging = SandboxGameAssemblyWrapper.IsDebugging;

			this.SetHelp();

			foreach (Adverts.Advert advert in Adverts.Instance.AdvertList)
			{
				ListViewItem item = new ListViewItem(advert.Name);
				item.SubItems.Add(advert.Time.ToString());
				item.SubItems.Add(advert.Text);
				LST_AdvertsConfig_Adverts.Items.Add(item);
				item.Tag = (bool)advert.Active;

				if (m_fileManager.AdvertsActive)
				{
					if(advert.Active)
						this.ParseAdvert(advert);
				}
			}

			CHK_MotdPlugin_Adverts_Active.Checked = m_fileManager.AdvertsActive;
			TXT_MotdPlugin_Motd_Body.Lines = m_fileManager.MotdLines.ToArray();
			TXT_MotdPlugin_Motd_Title.Text = m_fileManager.MotdTitle;
			CHK_MotdPlugin_Motd_Active.Checked = m_fileManager.MotdActive;
			Console.WriteLine("MotdPlugin - ConfigForm - Loading Data");

			WEB_AdvertsHelp.IsWebBrowserContextMenuEnabled = false;
			WEB_MotdHelp.IsWebBrowserContextMenuEnabled = false;
		}

		#endregion

		#region "Properties"

		public MyConfigDedicatedData ServerConfig
		{
			get { return SandboxGameAssemblyWrapper.Instance.GetServerConfig(); }
		}

		#endregion

		#region "Event Handlers"

		private void MotdPluginConfigForm_Shown(object sender, EventArgs e) { }

		private void LST_AdvertsConfig_Adverts_MouseClick(object sender, MouseEventArgs e)
		{
			
		}

		#endregion

		#region "Methods"

		#region "Control"

		private void BTN_MotdPlugin_Motd_Save_Click(object sender, EventArgs e)
		{
			bool active = CHK_MotdPlugin_Motd_Active.Checked;
			string title = TXT_MotdPlugin_Motd_Title.Text;
			string[] body = TXT_MotdPlugin_Motd_Body.Lines;

			m_fileManager.MotdActive = active;
			m_fileManager.MotdTitle = title;
			m_fileManager.MotdLines = body;
			this.SaveChanges(String.Format("Saved MOTD with the title '{0}', {1} Lines, Enabled: {2}", title, body.Count(), active ? "Yes" : "No"));
		}

		private void BTN_MotdPlugin_Adverts_NewAdvert_Click(object sender, EventArgs e)
		{
			m_newAdvert = true;

			string name = "Advert Name";
			string text = "Advert Text";
			decimal time = 60;
			bool active = false;

			ListViewItem advertlst = new ListViewItem(name);
			advertlst.SubItems.Add(time.ToString());
			advertlst.SubItems.Add(text);
			advertlst.Tag = (bool)active;

			LST_AdvertsConfig_Adverts.Items.Add(advertlst);

			advertlst.Selected = true;

			LST_AdvertsConfig_Adverts.Enabled = false;
			BTN_MotdPlugin_Adverts_New.Enabled = false;
			BTN_MotdPlugin_Adverts_DeleteAdvert.Enabled = false;
			TXT_MotdPlugin_Adverts_Name.Enabled = true;
		}

		private void BTN_MotdPlugin_Adverts_Update_Click(object sender, EventArgs e)
		{
			if (m_newAdvert)
			{

				string name = TXT_MotdPlugin_Adverts_Name.Text;
				string text = TXT_MotdPlugin_Adverts_Text.Text;
				decimal time = NUM_MotdPlugin_Adverts_Time.Value;
				bool active = CHK_MotdPlugin_Adverts_EnableAdvert.Checked;

				ListView.SelectedListViewItemCollection adverts = this.LST_AdvertsConfig_Adverts.SelectedItems;
				foreach (ListViewItem item in adverts)
				{
					if (item.Selected)
					{

						item.Text = name;
						item.Tag = active;
						item.SubItems[1].Text = time.ToString();
						item.SubItems[2].Text = text;	
					}
				}

				Adverts.Advert advert = Adverts.Instance.AddAdvert(name, text, time, active);

				if (this.ParseAdvert(advert))
					this.SaveChanges(String.Format(String.Format("Created Advert '{0}' with a time of '{1}', Enabled: '{2}'. ", name, time, active ? "Yes" : "No")));

				LST_AdvertsConfig_Adverts.Enabled = true;
				BTN_MotdPlugin_Adverts_New.Enabled = true;
				BTN_MotdPlugin_Adverts_DeleteAdvert.Enabled = true;

				m_newAdvert = false;
				Adverts.Instance.UpdateAdvert(name, text, time, active);
			}
			else
			{
				ListView.SelectedListViewItemCollection adverts = this.LST_AdvertsConfig_Adverts.SelectedItems;
				foreach (ListViewItem item in adverts)
				{
					if (item.Selected)
					{
						try
						{
							string name = TXT_MotdPlugin_Adverts_Name.Text;
							string text = TXT_MotdPlugin_Adverts_Text.Text;
							decimal time = NUM_MotdPlugin_Adverts_Time.Value;
							bool active = CHK_MotdPlugin_Adverts_EnableAdvert.Checked;

							item.Text = name;
							item.Tag = active;
							item.SubItems[1].Text = time.ToString();
							item.SubItems[2].Text = text;

							Adverts.Advert advert = Adverts.Instance.UpdateAdvert(name, text, time, active);

							if (this.ParseAdvert(advert))
								this.SaveChanges(String.Format("Saved Advert '{0}' with a time of '{1}', Enabled: '{2}'. ", name, time, active ? "Yes" : "No"));

							item.Selected = false;
							TXT_MotdPlugin_Adverts_Name.Text = "";
							TXT_MotdPlugin_Adverts_Text.Text = "";
							NUM_MotdPlugin_Adverts_Time.Value = 60;
							CHK_MotdPlugin_Adverts_EnableAdvert.Checked = false;

							TXT_MotdPlugin_Adverts_Name.Enabled = true;
						}
						catch
						{
							return;
						}
					}
				}
			}
		}

		private void BTN_AdvertsConfig_CancelChanges_Click(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection adverts = this.LST_AdvertsConfig_Adverts.SelectedItems;
			foreach (ListViewItem item in adverts)
			{
				if (item.Selected )
				{
					item.Selected = false;
					TXT_MotdPlugin_Adverts_Name.Text = "";
					TXT_MotdPlugin_Adverts_Text.Text = "";
					NUM_MotdPlugin_Adverts_Time.Value = 60;
					CHK_MotdPlugin_Adverts_EnableAdvert.Checked = false;

					TXT_MotdPlugin_Adverts_Name.Enabled = true;
					LST_AdvertsConfig_Adverts.Enabled = true;
					BTN_MotdPlugin_Adverts_New.Enabled = true;
					BTN_MotdPlugin_Adverts_DeleteAdvert.Enabled = true;

					if(m_newAdvert)
						item.Remove();
				}

				
			}
		}

		private void BTN_MotdPlugin_Adverts_DeleteAdvert_Click(object sender, EventArgs e)
		{
			ListView.SelectedListViewItemCollection adverts = this.LST_AdvertsConfig_Adverts.SelectedItems;
			foreach (ListViewItem item in adverts)
			{
				if (item.Selected)
				{				
					TXT_MotdPlugin_Adverts_Name.Text = "";
					TXT_MotdPlugin_Adverts_Text.Text = "";
					NUM_MotdPlugin_Adverts_Time.Value = 60;
					CHK_MotdPlugin_Adverts_EnableAdvert.Checked = false;

					foreach (System.Timers.Timer timer in m_timers.Keys)
					{
						if (item.Name == m_timers[timer].Name)
						{
							timer.Enabled = false;
							timer.Stop();
						}

					}
			
					TXT_MotdPlugin_Adverts_Name.Enabled = true;
	
					Adverts.Instance.DeleteAdvertByName(item.Text);
					this.SaveChanges("Deleted Advert: " + item.Text);
					item.Remove();
				}
			}
		}

		private void LST_AdvertsConfig_Adverts_SelectedIndexChanged(object sender, EventArgs e)
		{
			TXT_MotdPlugin_Adverts_Name.Enabled = false;

			ListView.SelectedListViewItemCollection adverts = this.LST_AdvertsConfig_Adverts.SelectedItems;

			foreach (ListViewItem item in adverts)
			{
				if (item.Selected)
				{
					MotdConfig_Status.Text = "Selected '" + item.Text + "'";
					TXT_MotdPlugin_Adverts_Name.Enabled = false;
					BTN_MotdPlugin_Adverts_New.Enabled = false;

					TXT_MotdPlugin_Adverts_Name.Text = item.Text;
					NUM_MotdPlugin_Adverts_Time.Value = decimal.Parse(item.SubItems[1].Text);
					TXT_MotdPlugin_Adverts_Text.Text = item.SubItems[2].Text;
					CHK_MotdPlugin_Adverts_EnableAdvert.Checked = (bool)item.Tag;

				}
				BTN_MotdPlugin_Adverts_New.Enabled = true;
			}
		}

		#endregion

		#region "General"

		private bool ParseAdvert(Adverts.Advert advert)
		{
			try
			{
				if (m_fileManager.AdvertsActive)
				{
					if (m_timers.ContainsValue(advert))
					{
						foreach (System.Timers.Timer timer in m_timers.Keys)
						{
							if (m_timers[timer].Name == advert.Name)
							{
								m_timers[timer].Text = advert.Text;

								timer.Interval = 1000 * Convert.ToDouble(advert.Time);
							}
						}

					}
					else
					{
						// Setup the timers.
						System.Timers.Timer time = new System.Timers.Timer();
						time.AutoReset = true;
						time.Interval = 1000 * Convert.ToDouble(advert.Time);
						m_timers.Add(time, advert);

						time.Elapsed += delegate(object sender, System.Timers.ElapsedEventArgs eventArgs)
						{
							System.Timers.Timer t = (System.Timers.Timer)sender;

							ChatManager.Instance.SendPublicChatMessage(ReplaceFormatting(m_timers[t].Text));

							if (!m_timers[t].Active)
							{
								t.Enabled = false;
								t.Stop();
							}
						};

						if (advert.Active)
						{
							if (!time.Enabled)
							{
								time.Enabled = true;
								time.Start();
							}

						}
						else
						{
							time.Enabled = false;
							time.Stop();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Motd Plugin - Advert Parsing Error: " + ex.ToString());
				return false;
			}
			return true;
		}

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

		public string ReplaceFormatting(string stringtoparse)
		{
			string parsedstring = "";

			parsedstring = stringtoparse.Replace("%time%", (DateTime.Now.TimeOfDay.Hours + ":" + DateTime.Now.TimeOfDay.Minutes + ":" + DateTime.Now.TimeOfDay.Seconds).ToString());
			parsedstring = parsedstring.Replace("%date%", (DateTime.Today.Month + "/" + DateTime.Today.Day + "/" + DateTime.Today.Year).ToString());
			parsedstring = parsedstring.Replace("%servername%", ServerConfig.ServerName);
			parsedstring = parsedstring.Replace("%gamemode%", ServerConfig.SessionSettings.GameMode.ToString());
			parsedstring = parsedstring.Replace("%scenario%", ServerConfig.Scenario.SubtypeId);
			parsedstring = parsedstring.Replace("%worldsize%", ServerConfig.SessionSettings.WorldSizeKm.ToString());
			parsedstring = parsedstring.Replace("%playercount%", ServerNetworkManager.Instance.GetConnectedPlayers().Count().ToString());
			parsedstring = parsedstring.Replace("%maxplayers%", ServerConfig.SessionSettings.MaxPlayers.ToString());
			parsedstring = parsedstring.Replace("%hostility%", ServerConfig.SessionSettings.EnvironmentHostility.ToString());
			parsedstring = parsedstring.Replace("%autohealing%", ServerConfig.SessionSettings.AutoHealing ? "On": "Off");
			parsedstring = parsedstring.Replace("%weapons%", ServerConfig.SessionSettings.WeaponsEnabled ? "On" : "Off");
			parsedstring = parsedstring.Replace("%thrusterdmg%", ServerConfig.SessionSettings.ThrusterDamage ? "On" : "Off");
			parsedstring = parsedstring.Replace("%permadeath%", Convert.ToBoolean(ServerConfig.SessionSettings.PermanentDeath) ? "On" : "Off");
			parsedstring = parsedstring.Replace("%clientsaving%", ServerConfig.SessionSettings.ClientCanSave ? "On" : "Off");
			
			return parsedstring;
		}

		private void SaveChanges(string status)
		{
			m_fileManager.SaveConfig();
			WriteLineSetStatus(status);
		}

		public void WriteLineSetStatus(string status)
		{
			MotdConfig_Status.Text = status;
			Console.WriteLine("Motd Plugin - " + status);

			if(WEB_AdvertsHelp.DocumentText == "")
				this.SetHelp();

			if (WEB_MotdHelp.DocumentText == "")
				this.SetHelp();
		}

		public void SetHelp()
		{
			WEB_MotdHelp.DocumentText =
				"<html><body>" +
				"<div><strong>&nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp;Psymonkey Coders</strong></div>" +
				"<div>&nbsp;</div>" +
				"<div>Check the Available Formatting Replacers down at the bottom</div>" +
				"<div>you can use any of these anywhere in the motd tab.</div>" +
				"<div>&nbsp;</div>" +
				"<div>(Note: Currently you can safely set 7 lines with 45 characters per line for it to appear correctly in game.)</div>" +
				"<div>&nbsp;</div>" +
				"<div><strong>&nbsp;Changing the Message Of The Day:</strong></div>" +
				"<ol><li>Set the &#39;Title&#39; for your message of the day, it appears on it&#39;s own line above the motd body.</li>" +
				"<li>Set your &#39;Message of the day in the Textbox below.</li>" +
				"<li>&nbsp;</li>" +
				"<li>Check the Enable Motd button to allow your clients to use &#39;/motd&#39; to read the message of the day.</li>" +
				"<li>Click Save Motd to save it!</li>" +
				"</ol><div><strong>Available formatting replacers:</strong></div>" +
				"<div>&nbsp;</div><div>" +
				"<div>%time% - Returns the clients local time in 24hr.</div>" +
				"<div>%date% - Returns the clients local date.</div>" +
				"<div>%servername% - Returns the name of the server.</div>" +
				"<div>%gamemode% - Returns the current gamemode.</div>" +
				"<div>%scenario% - Returns the scenario.</div>" +
				"<div>%worldsize% - Returns the size of the world in km.</div>" +
				"<div>%playercount% - Returns the current player count</div>" +
				"<div>%maxplayers% - Returns the max player count</div>" +
				"<div>%hostility% - Returns the hostility of the world</div>" +
				"<div>%autohealing% - Returns if Auto Healing is On or Off</div>" +
				"<div>%weapons% - Returns if Weapons are On or Off</div>" +
				"<div>%thrusterdmg% - Returns if Thruster Damage is On or Off</div>" +
				"<div>%permadeath% - Returns if Permament Death is On or Off</div>" +
				"<div>%clientsaving% - Returns if clients can save as On or Off</div>" +
				"</div><div>&nbsp;</div></body></html>";

			WEB_AdvertsHelp.DocumentText =
				"<html><body>" +
				"<div><strong>&nbsp;Psymonkey Coders</strong></div>" +
				"<div>&nbsp;</div>" +
				"<div>Check the Available Formatting Replacers down at the bottom</div>" +
				"<div>You can use any of &nbsp;these in the &#39;Text&#39; textbox</div>" +
				"<div>&nbsp;</div>" +
				"<div><strong><span style=\"font-size:16px;\">Creating an Advert:</span></strong></div>" +
				"<ol>" +
				"<li>Click on the &#39;New Advert&#39; button to create a template.</li>" +
				"<li>Fill out the &#39;Name&#39; Textbox. (Cannot use a previously used name).</li>" +
				"<li>Change the &#39;Time&#39; (Limited to from 60 to 1000 seconds)</li>" +
				"<li>Fill out the &#39;Text&#39; with what you want your players to see after the time has elapsed</li>" +
				"<li>Check the &#39;Enable Advert&#39; Checkbox to enable it!</li>" +
				"<li>Click &#39;Save Changes&#39; to save and you&#39;re done!</li>" +
				"</ol>" +
				"<div>&nbsp;</div>" +
				"<div><strong>Updating an Advert:</strong></div>" +
				"<div>(Note: you cannot change the name of an advert once its created,</div>" +
				"<div>to change the name, delete the advert and make a new one)</div>" +
				"<ol>" +
				"<li>Select an Advert from the Adverts list.</li>" +
				"<li>You can change the &#39;Time&#39;, &#39;Text&#39; and Enable or Disable the advert with the checkbox.</li>" +
				"<li>Click Save changes to save and you&#39;re done updating your advert!</li>" +
				"<li>If you change your mind, just click the &#39;Cancel Changes&#39; button and it will clear the boxes out!</li>" +
				"</ol>" +
				"<div>&nbsp;</div>" +
				"<div><strong>Deleting an advert:</strong></div>" +
				"<ol>" +
				"<li>Select an Advert from the Adverts List</li>" +
				"<li>Click the Delete Advert button</li>" +
				"<li>The advert is now gone!</li>" +
				"</ol>" +
				"<div><strong>Available formatting replacers:</strong></div>" +
				"<div>&nbsp;</div>" +
				"<div>" +
				"<div>%time% - Returns the clients local time in 24hr.</div>" +
				"<div>%date% - Returns the clients local date.</div>" +
				"<div>%servername% - Returns the name of the server.</div>" +
				"<div>%gamemode% - Returns the current gamemode.</div>" +
				"<div>%scenario% - Returns the scenario.</div>" +
				"<div>%worldsize% - Returns the size of the world in km.</div>" +
				"<div>%playercount% - Returns the current player count</div>" +
				"<div>%maxplayers% - Returns the max player count</div>" +
				"<div>%hostility% - Returns the hostility of the world</div>" +
				"<div>%autohealing% - Returns if Auto Healing is On or Off</div>" +
				"<div>%weapons% - Returns if Weapons are On or Off</div>" +
				"<div>%thrusterdmg% - Returns if Thruster Damage is On or Off</div>" +
				"<div>%permadeath% - Returns if Permament Death is On or Off</div>" +
				"<div>%clientsaving% - Returns if clients can save as On or Off</div>" +
				"</div>" +
				"<p>&nbsp;</p>" +
				"</body></html>";
		}

		#endregion

		#endregion
	}
}
