﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenMainMenuView
	 * 
	 * Main screen of the system
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMainMenuView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_MAIN_MENU";

		// ----------------------------------------------
		// SUBS
		// ----------------------------------------------	
		public const string SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP				= "SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP";
		public const string SUB_EVENT_SCREENMAIN_CONFIRMATION_CHANGE_NETWORK		= "SUB_EVENT_SCREENMAIN_CONFIRMATION_CHANGE_NETWORK";
		public const string SUB_EVENT_SCREENMAIN_CONFIRMATION_UNLOCK_BITCOIN_NETWORK = "SUB_EVENT_SCREENMAIN_CONFIRMATION_UNLOCK_BITCOIN_NETWORK";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string PAID_ACCESS_MAIN_BITCOIN_NETWORK = "PAID_ACCESS_MAIN_BITCOIN_NETWORK";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		public Sprite[] HelpImages;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private Dropdown m_networkBitcoin;
		private bool m_accessToMainBitcoinNetwork = false;
		private int m_indexCurrentNetwork = -1;
		private bool m_blockedAccessToChange = false;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.your.bitcoin.manager.title");

			m_container.Find("SendPayment").GetComponent<Button>().onClick.AddListener(OnSendPayment);
			m_container.Find("SendPayment/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.send.payment");

			m_container.Find("ReceivePayment").GetComponent<Button>().onClick.AddListener(OnReceivePayment);
			m_container.Find("ReceivePayment/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.receive.payment");

			m_container.Find("YourWallet").GetComponent<Button>().onClick.AddListener(OnYourWallet);
			UpdateBitcoinWallet();

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();

			m_container.Find("Exit").GetComponent<Button>().onClick.AddListener(ExitPressed);
			m_container.Find("Help").GetComponent<Button>().onClick.AddListener(HelpPressed);

			// CHANGE NETWORK
			m_networkBitcoin = m_container.Find("NetworkBitcoin").GetComponent<Dropdown>();
			m_networkBitcoin.options = new List<Dropdown.OptionData>();
			for (int i = 0; i < BitCoinController.OPTIONS_NETWORK.Length; i++)
			{
				m_networkBitcoin.options.Add(new Dropdown.OptionData(BitCoinController.OPTIONS_NETWORK[i]));
			}
			
			m_networkBitcoin.value = 1;
			m_networkBitcoin.value = 0;
			if (BitCoinController.Instance.IsMainNetwork)
			{
				m_networkBitcoin.value = 1;
			}
			else
			{
				m_networkBitcoin.value = 0;
			}
			m_indexCurrentNetwork = m_networkBitcoin.value;
			m_networkBitcoin.onValueChanged.AddListener(OnNetworkBitcoinChanged);

			// PAID ACCESS TO MAIN BITCOIN NETWORK
			m_accessToMainBitcoinNetwork = (PlayerPrefs.GetInt(PAID_ACCESS_MAIN_BITCOIN_NETWORK, 0) == 1);

			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;
			
			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
			GameObject.DestroyObject(this.gameObject);
			return false;
		}

		// -------------------------------------------
		/* 
		 * ExitPressed
		 */
		private void ExitPressed()
		{
			string warning = LanguageController.Instance.GetText("message.warning");
			string description = LanguageController.Instance.GetText("message.do.you.want.exit");
			ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_CONFIRMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP);
		}

		// -------------------------------------------
		/* 
		 * HelpPressed
		 */
		private void HelpPressed()
		{
			string title = LanguageController.Instance.GetText("message.info");
			List<PageInformation> pages = new List<PageInformation>();
			pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.features.page.1"), HelpImages[0], ""));
			pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.features.page.2"), HelpImages[1], ""));
			pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.features.page.3"), HelpImages[2], ""));
			pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.features.page.4"), HelpImages[3], ""));
			pages.Add(new PageInformation(title, LanguageController.Instance.GetText("screen.bitcoin.features.page.5"), HelpImages[4], ""));
			ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION_IMAGE, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, pages);
		}

		// -------------------------------------------
		/* 
		 * OnSendPayment
		 */
		private void OnSendPayment()
		{
			if (BitCoinController.Instance.PrivateKeys.Count > 0)
			{
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
				Invoke("OnRealSendPayment", 0.1f);
			}
			else
			{
				string warning = LanguageController.Instance.GetText("message.warning");
				string description = LanguageController.Instance.GetText("message.you.dont.have.private.key");
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * OnRealSendPayment
		 */
		public void OnRealSendPayment()
		{
			ScreenController.Instance.CreateNewScreen(ScreenBitcoinListKeysView.SCREEN_NAME, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true, ScreenBitcoinSendView.SCREEN_NAME, LanguageController.Instance.GetText("screen.list.select.wallet.used.to.pay"), ScreenController.Instance.SlotDisplayKeyPrefab, null);
		}

		// -------------------------------------------
		/* 
		 * OnReceivePayment
		 */
		private void OnReceivePayment()
		{
			if (BitCoinController.Instance.PrivateKeys.Count > 0)
			{
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
				Invoke("OnRealReceivePayment", 0.1f);
			}
			else
			{
				string warning = LanguageController.Instance.GetText("message.warning");
				string description = LanguageController.Instance.GetText("message.you.dont.have.private.key");
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * OnRealReceivePayment
		 */
		public void OnRealReceivePayment()
		{
			ScreenController.Instance.CreateNewScreen(ScreenBitcoinListKeysView.SCREEN_NAME, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true, ScreenBitcoinReceiveView.SCREEN_NAME, LanguageController.Instance.GetText("screen.list.select.wallet.to.receive.payment"), ScreenController.Instance.SlotDisplayKeyPrefab, null);
		}		

		// -------------------------------------------
		/* 
		 * OnYourWallet
		 */
		private void OnYourWallet()
		{
			ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
			Invoke("OnRealYourWallet", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * OnRealYourWallet
		 */
		public void OnRealYourWallet()
		{
			ScreenController.Instance.CreateNewScreen(ScreenBitcoinListKeysView.SCREEN_NAME, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true, ScreenBitcoinPrivateKeyView.SCREEN_NAME, LanguageController.Instance.GetText("screen.list.select.or.add.key"), ScreenController.Instance.SlotDisplayKeyPrefab, ScreenController.Instance.SlotAddKeyPrefab);
		}

		// -------------------------------------------
		/* 
		 * UpdateBitcoinWallet
		 */
		private void UpdateBitcoinWallet()
		{
			m_container.Find("YourWallet/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.main.menu.check.wallet");

			if ((BitCoinController.Instance.CurrentPrivateKey.Length > 0) 
				&& (BitCoinController.Instance.PrivateKeys.Count > 0)
				&& BitCoinController.Instance.PrivateKeys.Keys.Contains(BitCoinController.Instance.CurrentPrivateKey))
			{				
				m_container.Find("YourWallet/Text").GetComponent<Text>().text += "\n";
				decimal bitcoinsSelectedAccount = BitCoinController.Instance.PrivateKeys[BitCoinController.Instance.CurrentPrivateKey];
				decimal exchangeSelectedCurrency = BitCoinController.Instance.CurrenciesExchange[BitCoinController.Instance.CurrentCurrency];
				string labelCurrentAddress = BitCoinController.Instance.AddressToLabel(BitCoinController.Instance.CurrentPublicKey);
				m_container.Find("YourWallet/Text").GetComponent<Text>().text += LanguageController.Instance.GetText("screen.main.menu.selected.account");
				m_container.Find("YourWallet/Text").GetComponent<Text>().text += "\n";
				if (labelCurrentAddress != BitCoinController.Instance.CurrentPublicKey)
				{
					m_container.Find("YourWallet/Text").GetComponent<Text>().text += labelCurrentAddress;
					m_container.Find("YourWallet/Text").GetComponent<Text>().text += "\n";
				}
				m_container.Find("YourWallet/Text").GetComponent<Text>().text += Utilities.Trim(bitcoinsSelectedAccount.ToString()) + " BTC";
				m_container.Find("YourWallet/Text").GetComponent<Text>().text += "\n";
				m_container.Find("YourWallet/Text").GetComponent<Text>().text += Utilities.Trim((bitcoinsSelectedAccount * exchangeSelectedCurrency).ToString()) + " " + BitCoinController.Instance.CurrentCurrency;
			}
		}


		// -------------------------------------------
		/* 
		 * OnNetworkBitcoinChanged
		 */
		private void OnNetworkBitcoinChanged(int _index)
		{
			if (m_blockedAccessToChange) return;
			m_blockedAccessToChange = true;

			#if !ENABLE_IAP
				m_accessToMainBitcoinNetwork = true;
			#endif

			if (!m_accessToMainBitcoinNetwork)
			{
				if (_index == 1)
				{
					m_networkBitcoin.value = 0;
					string warning = LanguageController.Instance.GetText("screen.main.access.main.unlock.bitcoin.power");
					string description = LanguageController.Instance.GetText("screen.main.access.main.network.pay.iap");
					string okButton = LanguageController.Instance.GetText("screen.main.access.main.unlock.iap");
					string cancelButton = LanguageController.Instance.GetText("message.cancel");
					ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_UNLOCK_BITCOIN, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENMAIN_CONFIRMATION_UNLOCK_BITCOIN_NETWORK, okButton, cancelButton);
				}
			}
			else
			{
				string warning = LanguageController.Instance.GetText("message.warning");
				string description = LanguageController.Instance.GetText("screen.main.change.network.and.restart");
				string okButton = LanguageController.Instance.GetText("screen.main.change.network.confirmation");
				string cancelButton = LanguageController.Instance.GetText("message.cancel");
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_CHANGE_NETWORK, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENMAIN_CONFIRMATION_CHANGE_NETWORK, okButton, cancelButton);
			}
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_SELECTED_PRIVATE_KEY)
			{
				UpdateBitcoinWallet();
			}

			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == ScreenInformationView.EVENT_SCREENINFORMATION_CONFIRMATION_POPUP)
			{
				string subEvent = (string)_list[2];
				if (subEvent == SUB_EVENT_SCREENMAIN_CONFIRMATION_EXIT_APP)
				{
					if ((bool)_list[1])
					{
						Application.Quit();
						ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.close.the.app"), null, "");
					}
				}
				if (subEvent == SUB_EVENT_SCREENMAIN_CONFIRMATION_UNLOCK_BITCOIN_NETWORK)
				{
					if ((bool)_list[1])
					{
						// WAIT MESSAGE
						ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");

						// BUY IAP
#if ENABLE_IAP
						IAPController.Instance.UnlockAccessMainBitcoinNetwork();
#endif
					}
					else
					{
						m_networkBitcoin.value = m_indexCurrentNetwork;
						m_blockedAccessToChange = false;
					}
				}
				if (subEvent == SUB_EVENT_SCREENMAIN_CONFIRMATION_CHANGE_NETWORK)
				{
					if ((bool)_list[1])
					{
						m_indexCurrentNetwork = m_networkBitcoin.value;
						if (BitCoinController.OPTION_NETWORK_MAIN == m_networkBitcoin.options[m_networkBitcoin.value].text)
						{
							BitCoinController.Instance.IsMainNetwork = true;
						}
						else
						{
							BitCoinController.Instance.IsMainNetwork = false;
						}
						ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.close.the.app"), null, "");
						Application.Quit();
					}
					else
					{
						m_networkBitcoin.value = m_indexCurrentNetwork;
						m_blockedAccessToChange = false;
					}
				}
			}
#if ENABLE_IAP
			if (_nameEvent == IAPController.EVENT_IAP_SUCCESS_PURCHASE)
			{
				BasicEventController.Instance.DispatchBasicEvent(ScreenInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_POPUP);
				if ((bool)_list[0])
				{
					// IAP
					m_blockedAccessToChange = false;
					m_accessToMainBitcoinNetwork = true;
					PlayerPrefs.SetInt(PAID_ACCESS_MAIN_BITCOIN_NETWORK, 1);
					string warning = LanguageController.Instance.GetText("message.info");
					string description = LanguageController.Instance.GetText("screen.main.access.main.success.unlock.iap.confirmation");
					ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, "");
				}
				else
				{
					m_networkBitcoin.value = m_indexCurrentNetwork;
					m_blockedAccessToChange = false;
					m_accessToMainBitcoinNetwork = false;
					string warning = LanguageController.Instance.GetText("message.error");
					string description = LanguageController.Instance.GetText("screen.main.access.main.failure.unlock.iap.confirmation");
					ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, "");
				}
			}
#endif
			if (_nameEvent == ScreenController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				ExitPressed();
			}
		}
	}
}