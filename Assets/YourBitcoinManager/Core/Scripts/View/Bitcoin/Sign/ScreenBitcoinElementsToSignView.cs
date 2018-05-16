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
	 * ScreenBitcoinElementsToSignView
	 * 
	 * It will show a list the list of the elements we are going to sign
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenBitcoinElementsToSignView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_DOCUMENTS_SIGN";

		// ----------------------------------------------
		// PUBLIC EVENT
		// ----------------------------------------------
		public const string EVENT_SCREENELEMENTSTOSIGN_START_VERIFICATION = "EVENT_SCREENELEMENTSTOSIGN_START_VERIFICATION";

		// ----------------------------------------------
		// SUBEVENT
		// ----------------------------------------------
		public const string SUB_EVENT_SCREENBITCOINELEMENTOSIGN_CONFIRMATION_EXIT_WITHOUT_PROCESS = "SUB_EVENT_SCREENBITCOINELEMENTOSIGN_CONFIRMATION_EXIT_WITHOUT_PROCESS";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GameObject PrefabSlotElement;
		public GameObject PrefabSlotNew;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private GameObject m_root;
		private Transform m_container;
		private Transform m_listElements;
		private bool m_hasBeenPressed = false;

		private bool m_modeSignData = false;

		private List<ItemMultiObjects> m_documentsToSign = new List<ItemMultiObjects>();
		private int m_counterDocuments = 0;
		private string m_originalData;

		private bool m_hasChanged = false;

		public bool HasChanged
		{
			get { return m_hasChanged; }
			set
			{
				m_hasChanged = value;
			}
		}

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			if (_list.Length > 0)
			{
				m_modeSignData = (bool)_list[0];
			}
			else
			{
				m_modeSignData = true;
			}			

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			if (m_modeSignData)
			{
				m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.select.data.to.sign");
			}
			else
			{
				m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.select.data.to.verify");
			}
			

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);
			m_container.Find("Button_Sign").GetComponent<Button>().onClick.AddListener(SignPressed);
			string labelAddress = BitCoinController.Instance.AddressToLabel(BitCoinController.Instance.CurrentPublicKey);
			if (BitCoinController.ValidateBitcoinAddress(labelAddress))
			{
				labelAddress = LanguageController.Instance.GetText("message.selected");
			}
			if (m_modeSignData)
			{
				m_container.Find("Button_Sign/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.sign.data", labelAddress);
			}
			else
			{
				m_container.Find("Button_Sign/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.verify.data");
			}

			m_listElements = m_container.Find("ListItems");
			UpdateListItems();

			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);

			BasicEventController.Instance.DispatchBasicEvent(ScreenInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_WAIT);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			if (m_listElements!=null) m_listElements.GetComponent<SlotManagerView>().Destroy();
			m_listElements = null;

			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
			BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;
			BasicEventController.Instance.DispatchBasicEvent(ScreenController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * UpdateListItems
		 */
		private void UpdateListItems()
		{
			if (m_documentsToSign.Count > 0)
			{
				HasChanged = true;
			}			
			m_listElements.GetComponent<SlotManagerView>().ClearCurrentGameObject(true);			
			m_listElements.GetComponent<SlotManagerView>().Initialize(4, m_documentsToSign, PrefabSlotElement, PrefabSlotNew);
		}

		// -------------------------------------------
		/* 
		* SignPressed
		*/
		private void SignPressed()
		{
			if (m_documentsToSign.Count == 0)
			{
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION_IMAGE, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.bitcoin.sign.no.data.to.sign"), ScreenController.Instance.SignedDataFailed, "");
				return;
			}

			List<byte[]> documentsBytes = new List<byte[]>();

			int numberOfBytes = 0;
			for (int i = 0; i < m_documentsToSign.Count; i++)
			{
				int typeItem = (int)m_documentsToSign[i].Objects[1];
				byte[] dataItem = null;
				switch (typeItem)
				{
					case SlotElementView.TYPE_STRING:
						dataItem = Encoding.UTF8.GetBytes((string)m_documentsToSign[i].Objects[2]);
						documentsBytes.Add(dataItem);
						numberOfBytes += dataItem.Length;
						break;

					case SlotElementView.TYPE_FILE:
						dataItem = Utilities.LoadAllByteData((string)m_documentsToSign[i].Objects[2]);
						documentsBytes.Add(dataItem);
						numberOfBytes += dataItem.Length;
						break;
				}
				
				if (dataItem == null)
				{
					throw new Exception("The data can't be null");
				}
			}

			byte[] fullData = new byte[numberOfBytes];
			int pos = 0;
			for (int i = 0; i < documentsBytes.Count; i++)
			{
				byte[] document = documentsBytes[i];
				Array.Copy(document, 0, fullData, pos, document.Length);
				pos += document.Length;
			}

			m_originalData = Encoding.UTF8.GetString(fullData);
			string signedData = BitCoinController.Instance.SignTextData(m_originalData, BitCoinController.Instance.CurrentPrivateKey);

			if (m_modeSignData)
			{
				HasChanged = false;
				ScreenController.Instance.CreateNewScreen(ScreenEmailSignedDataView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, signedData);
			}
			else
			{
				ScreenController.Instance.CreateNewScreen(ScreenBitcoinSignedVerificationView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false);
			}
		}

		// -------------------------------------------
		/* 
		* BackPressed
		*/
		private void BackPressed()
		{
			if (!HasChanged)
			{
				Destroy();
			}
			else
			{
				string warning = LanguageController.Instance.GetText("message.warning");
				string description = LanguageController.Instance.GetText("message.exit.without.apply.changes");
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_CONFIRMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, SUB_EVENT_SCREENBITCOINELEMENTOSIGN_CONFIRMATION_EXIT_WITHOUT_PROCESS);
			}

		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		private void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (!this.gameObject.activeSelf) return;

			if (_nameEvent == AddElementView.EVENT_ADD_ELEMENT_SELECTED)
			{
				ScreenController.Instance.CreateNewScreen(ScreenTypeDataView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, LanguageController.Instance.GetText("screen.bitcoin.sign.write.text.you.want.signed"));				
			}
			if (_nameEvent == SlotElementView.EVENT_SLOT_ELEMENT_SELECTED)
			{
			}
			if (_nameEvent == SlotElementView.EVENT_SLOT_DELETED_SELECTED)
			{
				ItemMultiObjects itemToDelete = (ItemMultiObjects)_list[0];
				for (int i = 0; i < m_documentsToSign.Count; i++)
				{
					if (((int)m_documentsToSign[i].Objects[0]) == (int)itemToDelete.Objects[0])
					{
						m_documentsToSign.RemoveAt(i);
						break;
					}
				}
				UpdateListItems();
			}
			if (_nameEvent == ScreenFileElementNavitagorView.EVENT_SCREENSYSTEMNAVIGATOR_FINAL_SELECTION)
			{
				if ((bool)_list[0])
				{
					string filename = (string)_list[1];
					m_documentsToSign.Add(new ItemMultiObjects(m_counterDocuments, SlotElementView.TYPE_FILE, filename));
					m_counterDocuments++;
					UpdateListItems();
				}
			}
			if (_nameEvent == ScreenEnterTextView.EVENT_SCREENENTERETEXT_CONFIRMATION)
			{
				string textToSign = (string)_list[0];
				m_documentsToSign.Add(new ItemMultiObjects(m_counterDocuments, SlotElementView.TYPE_STRING, textToSign));
				m_counterDocuments++;
				UpdateListItems();
			}
			if (_nameEvent == EVENT_SCREENELEMENTSTOSIGN_START_VERIFICATION)
			{
				string publicKeyAddresToUse = (string)_list[0];
				string signedDataToVerify = (string)_list[1];
				HasChanged = false;
				try
				{
					if (BitCoinController.Instance.VerifySignedData(m_originalData, signedDataToVerify, publicKeyAddresToUse))
					{						
						ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION_IMAGE, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.bitcoin.sign.validation.success"), ScreenController.Instance.SignedDataOK, "");
					}
					else
					{
						ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION_IMAGE, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.bitcoin.sign.validation.failed"), ScreenController.Instance.SignedDataFailed, "");
					}
				}
				catch (Exception err)
				{
					ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION_IMAGE, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("screen.bitcoin.sign.validation.failed"), ScreenController.Instance.SignedDataFailed, "");
				}
			}
			if (_nameEvent == ScreenInformationView.EVENT_SCREENINFORMATION_CONFIRMATION_POPUP)
			{
				string subEvent = (string)_list[2];
				if (subEvent == SUB_EVENT_SCREENBITCOINELEMENTOSIGN_CONFIRMATION_EXIT_WITHOUT_PROCESS)
				{
					if ((bool)_list[1])
					{
						Destroy();
					}
				}
			}
			if (_nameEvent == ScreenController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				Destroy();
			}
		}
	}
}