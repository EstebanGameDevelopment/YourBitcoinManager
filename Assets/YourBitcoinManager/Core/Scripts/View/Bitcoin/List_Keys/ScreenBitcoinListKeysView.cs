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
	 * ScreenBitcoinListKeysView
	 * 
	 * It will show a list with the available keys
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinListKeysView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_LIST_KEYS";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENBITCOINLISTKEYS_CURRENCY_CHANGED	= "EVENT_SCREENBITCOINLISTKEYS_CURRENCY_CHANGED";
		public const string EVENT_SCREENBITCOINLISTKEYS_UPDATE_ACCOUNT_DATA = "EVENT_SCREENBITCOINLISTKEYS_UPDATE_ACCOUNT_DATA";
		public const string EVENT_SCREENBITCOINLISTKEYS_NEW_CURRENCY_SELECTED = "EVENT_SCREENBITCOINLISTKEYS_NEW_CURRENCY_SELECTED";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private GameObject m_root;
		private Transform m_container;
		private Transform m_listKeys;
		private string m_targetNameScreen;
		private Dropdown m_currencies;
		private GameObject m_prefabSlotKey;
		private GameObject m_prefabSlotNew;
		private string m_excludeAddress = "";
		private bool m_hasBeenPressed = false;
		
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_targetNameScreen = (string)_list[0];
			string description = (string)_list[1];
			m_prefabSlotKey = (GameObject)_list[2];
			m_prefabSlotNew = (GameObject)_list[3];
			if (_list.Length >= 5)
			{
				m_excludeAddress = (string)_list[4];
			}
			else
			{
				m_excludeAddress = "";
			}			

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.your.bitcoin.manager.title");

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);
			m_container.Find("Button_Refresh").GetComponent<Button>().onClick.AddListener(RefreshPressed);
			
			m_container.Find("ListItems/Title").GetComponent<Text>().text = description;
			m_listKeys = m_container.Find("ListItems");
			UpdateListItems();

			// CURRENCIES
			m_currencies = m_container.Find("Currency").GetComponent<Dropdown>();
			m_currencies.onValueChanged.AddListener(OnCurrencyChanged);
			m_currencies.options = new List<Dropdown.OptionData>();
			int indexCurrentCurrency = -1;
			for (int i = 0; i < BitCoinController.CURRENCY_CODE.Length; i++)
			{
				if (BitCoinController.Instance.CurrentCurrency == BitCoinController.CURRENCY_CODE[i])
				{
					indexCurrentCurrency = i;
				}
				m_currencies.options.Add(new Dropdown.OptionData(BitCoinController.CURRENCY_CODE[i]));
			}

			m_currencies.value = 1;
			m_currencies.value = 0;
			if (indexCurrentCurrency != -1)
			{
				m_currencies.value = indexCurrentCurrency;
			}			

			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();

			BasicEventController.Instance.DispatchBasicEvent(ScreenInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			if (m_listKeys!=null) m_listKeys.GetComponent<SlotManagerView>().Destroy();
			m_listKeys = null;

			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
			BasicEventController.Instance.DispatchBasicEvent(ScreenController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnCurrencyChanged
		 */
		private void OnCurrencyChanged(int _index)
		{
			BitCoinController.Instance.CurrentCurrency = m_currencies.options[_index].text;
			BasicEventController.Instance.DispatchBasicEvent(EVENT_SCREENBITCOINLISTKEYS_CURRENCY_CHANGED);
		}

		// -------------------------------------------
		/* 
		 * UpdateListItems
		 */
		private void UpdateListItems()
		{
			m_listKeys.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);			
			m_listKeys.GetComponent<SlotManagerView>().Initialize(4, BitCoinController.Instance.GetListPrivateKeys(m_excludeAddress), m_prefabSlotKey, m_prefabSlotNew);
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
		* RefreshPressed
		*/
		private void RefreshPressed()
		{
			if (!m_hasBeenPressed)
			{
				m_hasBeenPressed = true;
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");

				Invoke("RefreshRealPressed", 0.1f);
			}			
		}

		// -------------------------------------------
		/* 
		* RefreshRealPressed
		*/
		public void RefreshRealPressed()
		{
			BitCoinController.Instance.RefreshBalancePrivateKeys();
			m_container.Find("Button_Refresh").gameObject.SetActive(false);
			BasicEventController.Instance.DispatchBasicEvent(ScreenInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT);
		}

		// -------------------------------------------
		/* 
		* OnRealLoadNetScreen
		*/
		public void OnRealLoadNetScreen()
		{
			ScreenController.Instance.CreateNewScreen(m_targetNameScreen, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true, true);
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_SCREENBITCOINLISTKEYS_UPDATE_ACCOUNT_DATA)
			{
				UpdateListItems();
			}
			if (_nameEvent == EVENT_SCREENBITCOINLISTKEYS_NEW_CURRENCY_SELECTED)
			{
				BitCoinController.Instance.CurrentCurrency = (string)_list[0];
				int indexCurrentCurrency = -1;
				for (int i = 0; i < BitCoinController.CURRENCY_CODE.Length; i++)
				{
					if (BitCoinController.Instance.CurrentCurrency == BitCoinController.CURRENCY_CODE[i])
					{
						indexCurrentCurrency = i;
					}
				}

				m_currencies.value = 1;
				m_currencies.value = 0;
				if (indexCurrentCurrency != -1)
				{
					m_currencies.value = indexCurrentCurrency;
				}
			}

			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == AddKeyView.EVENT_ADD_KEY_SELECTED)
			{
				BitCoinController.Instance.CurrentPrivateKey = "";
				ScreenController.Instance.CreateNewScreen(m_targetNameScreen, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true, true);
			}
			if (_nameEvent == SlotKeyView.EVENT_SLOT_SELECTED)
			{
				if (m_targetNameScreen.Length > 0)
				{
					BitCoinController.Instance.CurrentPrivateKey = (string)_list[0];
					if (m_targetNameScreen == ScreenBitcoinPrivateKeyView.SCREEN_NAME)
					{
						ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
						Invoke("OnRealLoadNetScreen", 0.1f);
					}
					else
					{
						ScreenController.Instance.CreateNewScreen(m_targetNameScreen, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true);
					}
				}
				else
				{
					BasicEventController.Instance.DispatchBasicEvent(BitCoinController.EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY, BitCoinController.Instance.GetPublicKey((string)_list[0]));
					Destroy();
				}
			}
			if (_nameEvent == ScreenController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				Destroy();
			}
		}
	}
}