using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;

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
	public class ScreenSelectAddressFromView : ScreenBaseView, IBasicScreenView
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
		public void Initialize(params object[] _list)
		{
			m_excludeCurrentAddress = true;
			if (_list.Length > 0)
			{
				m_excludeCurrentAddress = (bool)_list[0];
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
			BasicEventController.Instance.DispatchBasicEvent(ScreenController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

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
				ScreenController.Instance.CreateNewScreen(ScreenBitcoinListAddressesView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, BitCoinController.Instance.CurrentPublicKey);
			}
			else
			{
				ScreenController.Instance.CreateNewScreen(ScreenBitcoinListAddressesView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false);
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
				ScreenController.Instance.CreateNewScreen(ScreenBitcoinListKeysView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, "", LanguageController.Instance.GetText("screen.bitcoin.select.wallet.to.send"), ScreenController.Instance.SlotDisplayKeyPrefab, null, BitCoinController.Instance.CurrentPrivateKey);
			}
			else
			{
				ScreenController.Instance.CreateNewScreen(ScreenBitcoinListKeysView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, "", LanguageController.Instance.GetText("screen.bitcoin.select.wallet.to.send"), ScreenController.Instance.SlotDisplayKeyPrefab, null);
			}
		}

		// -------------------------------------------
		/* 
		 * OnQRCode
		 */
		private void OnQRCode()
		{
			Destroy();
			ScreenController.Instance.CreateNewScreen(ScreenQRCodeScanView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false);
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
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == ScreenController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				OnCancel();
			}
		}
	}
}