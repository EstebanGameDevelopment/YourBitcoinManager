using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenBitcoinSignedVerificationView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinSignedVerificationView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_VERIFICATION";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private InputField m_publicAddressInput;
		private string m_publicAddressToSend;
		private bool m_validPublicAddressToUseForVerification = false;
		private GameObject m_saveAddress;
		private GameObject m_validAddress;

		private InputField m_signDataInput;

		public bool ValidPublicKeyToSend
		{
			set
			{
				m_validPublicAddressToUseForVerification = value;
				string labelAddress = BitCoinController.Instance.AddressToLabel(m_publicAddressToSend);
				if (labelAddress != m_publicAddressToSend)
				{
					m_container.Find("Address/Label").GetComponent<Text>().text = labelAddress;
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.red;
				}
				else
				{
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.black;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(OnBackButton);

			// PUBLIC KEY TO SEND
			m_container.Find("Address/Label").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bicoin.sign.write.public.address.verify");
			m_publicAddressInput = m_container.Find("Address/PublicKey").GetComponent<InputField>();
			m_publicAddressInput.onValueChanged.AddListener(OnValuePublicKeyChanged);
			m_container.Find("Address/SelectAddress").GetComponent<Button>().onClick.AddListener(OnSelectAddress);			
			m_container.Find("Address/SelectAddress/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.addresses");
			m_publicAddressInput.text = "";

			m_saveAddress = m_container.Find("Address/SaveAddress").gameObject;
			m_saveAddress.GetComponent<Button>().onClick.AddListener(OnSaveAddress);
			m_saveAddress.SetActive(false);

			m_validAddress = m_container.Find("Address/ValidAddress").gameObject;
			m_validAddress.GetComponent<Button>().onClick.AddListener(OnAddressValid);
			m_validAddress.SetActive(false);

			m_container.Find("Data/Label").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.sign.write.data.signed.to.verify");
			m_signDataInput = m_container.Find("Data/SignData").GetComponent<InputField>();

			m_container.Find("Button_Verify").GetComponent<Button>().onClick.AddListener(OnVerifySignedData);
			m_container.Find("Button_Verify/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.start.verification");

			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);

			m_container.Find("Network").GetComponent<Text>().text = LanguageController.Instance.GetText("text.network") + BitCoinController.Instance.Network.ToString();
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;
			BasicEventController.Instance.DispatchBasicEvent(ScreenController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnValuePublicKeyChanged
		 */
		private void OnValuePublicKeyChanged(string _newValue)
		{
			if ((_newValue.Length > 0) && (BitCoinController.Instance.CurrentPublicKey != m_publicAddressToSend))
			{
				m_publicAddressToSend = m_publicAddressInput.text;
				ValidPublicKeyToSend = BitCoinController.Instance.ValidatePublicKey(m_publicAddressToSend);
				bool enableButtonSaveAddress = true;
				if (BitCoinController.Instance.ContainsAddress(m_publicAddressToSend))
				{
					enableButtonSaveAddress = false;
				}
				if (enableButtonSaveAddress)
				{
					m_saveAddress.SetActive(true);
				}
#if ENABLE_PARTIAL_WALLET
				m_saveAddress.SetActive(false);
#endif
				m_validAddress.SetActive(true);
				m_validAddress.transform.Find("IconValid").gameObject.SetActive(m_validPublicAddressToUseForVerification);
				m_validAddress.transform.Find("IconError").gameObject.SetActive(!m_validPublicAddressToUseForVerification);
				if (!m_validPublicAddressToUseForVerification)
				{
					m_saveAddress.SetActive(false);
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnCheckWallet
		 */
		private void OnCheckWallet()
		{
			ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
			Invoke("OnRealCheckWallet", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * OnBackButton
		 */
		private void OnBackButton()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnScanQRCode
		 */
		private void OnSelectAddress()
		{
			ScreenController.Instance.CreateNewScreen(ScreenSelectAddressFromView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, false);
		}

		// -------------------------------------------
		/* 
		 * OnSaveAddress
		 */
		private void OnSaveAddress()
		{
			ScreenController.Instance.CreateNewScreen(ScreenEnterEmailView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, LanguageController.Instance.GetText("screen.enter.new.label.address"));
		}

		// -------------------------------------------
		/* 
		 * OnAddressValid
		 */
		private void OnAddressValid()
		{
			string description = "";
			if (m_validPublicAddressToUseForVerification)
			{
				description = LanguageController.Instance.GetText("screen.bitcoin.send.valid.address");
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), description, null, "");
			}
			else
			{
				description = LanguageController.Instance.GetText("screen.bitcoin.send.invalid.address");
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), description, null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * OnVerifySignedData
		 */
		private void OnVerifySignedData()
		{
			string signedData = m_signDataInput.text;

			if ((signedData.Length > 0) && m_validPublicAddressToUseForVerification)
			{
				Destroy();
				BasicEventController.Instance.DelayBasicEvent(ScreenBitcoinElementsToSignView.EVENT_SCREENELEMENTSTOSIGN_START_VERIFICATION, 0.1f, m_publicAddressToSend, signedData);
			}
			else
			{
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("message.data.missing"), null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * OnBitcoinEvent
		 */
		private void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY)
			{
				string publicKeyAddress = (string)_list[0];
				m_publicAddressInput.text = publicKeyAddress;
				m_publicAddressToSend = publicKeyAddress;
				ValidPublicKeyToSend = BitCoinController.Instance.ValidatePublicKey(m_publicAddressToSend);
#if DEBUG_MODE_DISPLAY_LOG
				Debug.Log("EVENT_BITCOINCONTROLLER_SELECTED_PUBLIC_KEY::PUBLIC KEY ADDRESS=" + publicKeyAddress);
#endif
			}
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == ScreenEnterEmailView.EVENT_SCREENENTEREMAIL_CONFIRMATION)
			{
				string label = (string)_list[0];				
				BitCoinController.Instance.SaveAddresses(m_publicAddressToSend, label);
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.bitcoin.send.address.saved"), null, "");
				m_saveAddress.SetActive(false);
				if (label.Length > 0)
				{
					m_container.Find("Address/Label").GetComponent<Text>().text = label;
					m_container.Find("Address/Label").GetComponent<Text>().color = Color.red;
				}
			}

			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == ScreenController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				OnBackButton();
			}
		}
	}
}