using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenBitcoinListAddressesView
	 * 
	 * It will show a list with the stored addresses
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinListAddressesView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_LIST_ADDRESSES";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GameObject PrefabSlotAddress;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private GameObject m_root;
		private Transform m_container;
		private Transform m_listAddresses;
		private Dropdown m_currencies;

		private string m_excludeAddress;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_excludeAddress = "";
			if (_list.Length > 0)
			{
				m_excludeAddress = (string)_list[0];
			}

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.your.bitcoin.manager.title");

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);

			m_container.Find("ListItems/Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.address.list");
			m_listAddresses = m_container.Find("ListItems");
			m_listAddresses.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);
			m_listAddresses.GetComponent<SlotManagerView>().Initialize(4, BitCoinController.Instance.GetListDataAddresses(true, m_excludeAddress), PrefabSlotAddress, null);

			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			if (m_listAddresses!=null) m_listAddresses.GetComponent<SlotManagerView>().Destroy();
			m_listAddresses = null;

			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
			BasicEventController.Instance.DispatchBasicEvent(ScreenController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		* BackPressed
		*/
		private void BackPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == SlotAddressView.EVENT_SLOT_ADDRESS_SELECTED)
			{
				string addressSelected = (string)_list[0];
				BasicEventController.Instance.DispatchBasicEvent(BitCoinController.EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY, addressSelected);
				Destroy();
			}
			if (_nameEvent == ScreenController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				Destroy();
			}
		}
	}
}