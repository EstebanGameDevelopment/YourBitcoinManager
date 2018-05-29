using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;
using YourCommonTools;

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
	public class ScreenOperationSignView : ScreenBaseView, IBasicView
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
		public override void Initialize(params object[] _list)
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

			UIEventController.Instance.UIEvent += new UIEventHandler(OnBasicEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			UIEventController.Instance.UIEvent -= OnBasicEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);
			GameObject.Destroy(this.gameObject);

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
				MenusScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_WAIT, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), LanguageController.Instance.GetText("message.please.wait"), null, "");
				UIEventController.Instance.DelayUIEvent(ScreenMainMenuView.EVENT_SCREENMAIN_LOAD_SCREEN_KEYS_FOR_SIGN, 0.1f);
			}
			else
			{
				string warning = LanguageController.Instance.GetText("message.warning");
				string description = LanguageController.Instance.GetText("message.you.dont.have.private.key");
				MenusScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, warning, description, null, "");
			}
		}

		// -------------------------------------------
		/* 
		 * OnAddTypeFile
		 */
		private void OnVerifyAuthenticity()
		{
			Destroy();
			MenusScreenController.Instance.CreateNewScreen(ScreenBitcoinElementsToSignView.SCREEN_NAME, UIScreenTypePreviousAction.HIDE_CURRENT_SCREEN, true, false);
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
			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				Destroy();
			}
		}
	}
}