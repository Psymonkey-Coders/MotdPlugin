using SEModAPIExtensions.API;
using SEModAPIExtensions.API.Plugin;
using SEModAPIExtensions.API.Plugin.Events;
using SEModAPIInternal.API.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Xml;
using System.Xml.Serialization;



namespace MotdPlugin
{
    public class MotdPluginCore : PluginBase
	{

		#region "Attributes"





		private static string m_assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		private static string m_dataFile = Path.Combine(m_assemblyFolder, "MotdPlugin_Settings.xml");

		MotdPluginConfigForm m_motdPluginForm = new MotdPluginConfigForm(new FileIOManager(m_dataFile));

        #endregion

        #region "Constructors and Initializers"

		public MotdPluginCore()
		{
			Console.WriteLine("Motd Plugin '" + Id.ToString() + "' initialized!");	
        }

        public override void Init()
        {
            Console.WriteLine("Motd Plugin '" + Id.ToString() + "' initialized!");
        }

        #endregion

        #region "Properties"


		[Category("Motd Plugin")]
		[Description("Shows the Config Dialog")]
		[Browsable(true)]
		[ReadOnly(false)]
		public bool ShowConfig
		{
			get { return m_motdPluginForm.Visible; }
			set { m_motdPluginForm.Show(); }
		}

        #endregion

        #region "EventHandlers"

        public override void Update()
        {

        }

		// Called when the server is shutdown, or plugin is unloaded.
		public override void Shutdown()
		{
			m_motdPluginForm.ClearAdvertTimers();
		}

		#endregion
	}
}
