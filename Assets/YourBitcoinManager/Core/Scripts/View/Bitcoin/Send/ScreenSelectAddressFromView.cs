using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;
using YourCommonTools;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenSelectAddressFromView
	 * 
	 * We give options to the user to choose how to select the destination
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenSelectAddressFromView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SELECT_ADDRESS";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private bool m_excludeCurrentAddress = true;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			m_excludeCurrentAddress = true;
			if (_list.Length > 0)
			{
                if (_list[0] != null)
                {
                    m_excludeCurrentAddress = (bool)_list[0];
                }				
			}

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.send.select.address");

			m_container.Find("AddressList").GetComponent<Button>().onClick.AddListener(OnAddressList);
			m_container.Find("AddressList/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.send.from.list.of.addresses");
			m_container.Find("YourAddresses").GetComponent<Button>().onClick.AddListener(OnYourAddresses);
			m_container.Find("YourAddresses/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.send.from.your.addresses");
			m_container.Find("Cancel").GetComponent<Button>().onClick.AddListener(OnCancel);
			m_container.Find("Cancel/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.cancel");
			
			m_container.Find("QRCode/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.send.scan.qr.code.address");
			m_container.Find("QRCode").GetComponent<Button>().onClick.AddListener(OnQRCode);
#if !ENABLE_QRCODE
			m_container.Find("QRCode").gameObject.SetActive(false);
#endif

			UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;
			
			UIEventController.Instance.UIEvent -= OnBasicEvent;
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnAddressList
		 */
		private void OnAddressList()
		{
			Destroy();
			if (m_excludeCurrentAddress)
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 2, null, ScreenBitcoinListAddressesView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, BitCoinController.Instance.CurrentPublicKey);
            }
			else
			{
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 2, null, ScreenBitcoinListAddressesView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false);
            }
		}

		// -------------------------------------------
		/* 
		 * OnYourAddresses
		 */
		private void OnYourAddresses()
		{
			Destroy();
			if (m_excludeCurrentAddress)
			{
                List<object> listKeyParams = new List<object>();
                listKeyParams.Add("");
                listKeyParams.Add(LanguageController.Instance.GetText("screen.bitcoin.select.wallet.to.send"));
                listKeyParams.Add(MenusScreenController.MainInstance.SlotDisplayKeyPrefab);
                listKeyParams.Add(null);
                listKeyParams.Add(BitCoinController.Instance.CurrentPrivateKey);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 2, null, ScreenBitcoinListKeysView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, false, listKeyParams.ToArray());
            }
            else
			{
                List<object> listKeyParams = new List<object>();
                listKeyParams.Add("");
                listKeyParams.Add(LanguageController.Instance.GetText("screen.bitcoin.select.wallet.to.send"));
                listKeyParams.Add(MenusScreenController.MainInstance.SlotDisplayKeyPrefab);
                listKeyParams.Add(null);
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 2, null, ScreenBitcoinListKeysView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, false, listKeyParams.ToArray());
            }
        }

		// -------------------------------------------
		/* 
		 * OnQRCode
		 */
		private void OnQRCode()
		{
			Destroy();
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_LAYER_GENERIC_SCREEN, 2, null, ScreenQRCodeScanView.SCREEN_NAME, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false);
        }

		// -------------------------------------------
		/* 
		 * OnCancel
		 */
		private void OnCancel()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnBitcoinEvent
		 */
		private void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY)
			{
				Destroy();
			}
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				OnCancel();
			}
		}
	}
}