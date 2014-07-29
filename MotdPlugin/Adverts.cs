using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SEModAPI.API.Definitions;

namespace MotdPlugin
{
	public class Adverts
	{
		#region "AdvertClass"

		public class Advert
		{
			public string Name { get; set; }
			public string Text { get; set; }
			public decimal Time { get; set; }
			public bool Active { get; set; }

			public Advert() { }			
			
			public Advert(string name, string text, decimal time, bool active)
			{
				Name = name;
				Text = text;
				Time = time;
				Active = active;
			}
		}

		#endregion

		#region "Attributes"

		private static Adverts m_instance;

		private static List<Advert> m_advertsList = new List<Advert>();

		private static Dictionary<string, Advert> m_advertsNameList = m_advertsList.ToDictionary(advert => advert.Name, advert => advert);

		#endregion 

		#region "Constructors And Initalizers"

		public Adverts()
		{
			m_instance = this;
		}

		#endregion

		#region "Properties"

		public static Adverts Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = new Adverts();
				}
				return m_instance;
			}
		}

		public List<Advert> AdvertList
		{
			get 
			{
				List<Advert> copy = new List<Advert>(m_advertsList.ToArray());
				return copy;
			}
		}

		#endregion

		#region "Event Handlers"


		#endregion

		#region "Methods"

		public Adverts.Advert AddAdvert(string name, string text, decimal time, bool active)
		{
			if (m_advertsNameList.ContainsKey(name))
				return null;

			Adverts.Advert advert = new Advert(name, text, time, active);
			m_advertsList.Add(advert);
			m_advertsNameList.Add(name, advert);
			return advert;		
		}

		public Adverts.Advert UpdateAdvert(string name, string text, decimal time, bool active)
		{
			Adverts.Advert returnadvert = null;

			// Loop through the adverts to find the one with the specified name.
			foreach(Advert advert in m_advertsList)
			{
				// If it cant find the advert with the name in the list
				if (!m_advertsNameList.ContainsKey(name))
					return returnadvert;

				if (advert.Name == name)
				{
					advert.Text = text;
					advert.Time = time;
					advert.Active = active;

					returnadvert = advert;
				}
			}
			return returnadvert;
		}

		public void DeleteAdvertByName(string name)
		{
			// Loop through the adverts to find the one with the specified name.
			List<Adverts.Advert> removelist = new List<Adverts.Advert>(m_advertsList.ToArray());
			foreach (Advert advert in removelist)
			{
				// If it cant find the advert with the name in the list
				if (!m_advertsNameList.ContainsKey(name))
					return;

				if (advert.Name == name)
				{
					m_advertsList.Remove(advert);
				}
			}
		}

		#endregion



	}
}
