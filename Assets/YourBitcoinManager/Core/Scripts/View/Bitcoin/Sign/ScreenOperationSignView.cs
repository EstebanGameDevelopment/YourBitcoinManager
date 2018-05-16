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
	 * ScreenOperationSignView
	 * 
	 * You can either sign data or verify data
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenOperationSignView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_SIGN_OPERATION";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public void Initialize(params object[] _list)
		{
			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.operation.sign.verification");
			
			m_container.Find("Button_SignData").GetComponent<Button>().onClick.AddListener(OnSignYourData);
			m_container.Find("Button_SignData/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.operation.sign.your.documents");
			m_container.Find("Button_Verify").GetComponent<Button>().onClick.AddListener(OnVerifyAuthenticity);
			m_container.Find("Button_Verify/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.operation.sign.verify.documents");
			m_container.Find("Button_Cancel").GetComponent<Button>().onClick.AddListener(OnCancel);
			m_container.Find("Button_Cancel/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("message.cancel");

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
		 * OnAddTypeText
		 */
		private void OnSignYourData()
		{
			Destroy();
			OnSignData();
		}

		// -------------------------------------------
		/* 
		* OnSignData
		*/
		private void OnSignData()
		{
			if (BitCoinController.Instance.PrivateKeys.Count > 0)
			{
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
				BasicEventController.Instance.DelayBasicEvent(ScreenMainMenuView.EVENT_SCREENMAIN_LOAD_SCREEN_KEYS_FOR_SIGN, 0.1f);
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
		 * OnAddTypeFile
		 */
		private void OnVerifyAuthenticity()
		{
			Destroy();
			ScreenController.Instance.CreateNewScreen(ScreenBitcoinElementsToSignView.SCREEN_NAME, TypePreviousActionEnum.HIDE_CURRENT_SCREEN, true, false);
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
				Destroy();
			}
		}
	}
}