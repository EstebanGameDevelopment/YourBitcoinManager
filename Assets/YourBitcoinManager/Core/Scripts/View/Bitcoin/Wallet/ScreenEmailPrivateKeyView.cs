using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenEmailPrivateKeyView
	 * 
	 * It allows the user to enter an email address to send the private key
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenEmailPrivateKeyView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_EMAIL_PRIVATE_KEY";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENENTEREMAIL_PRIVATE_KEY_CONFIRMATION = "EVENT_SCREENENTEREMAIL_PRIVATE_KEY_CONFIRMATION";

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

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.warning");

			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.wallet.send.private.key.warning");

			m_container.Find("LabelEmail").GetComponent<Text>().text = LanguageController.Instance.GetText("message.email.address");
			
			m_container.Find("Button_Save").GetComponent<Button>().onClick.AddListener(OnConfirmEmail);
			m_container.Find("Button_Cancel").GetComponent<Button>().onClick.AddListener(OnCancelEmail);

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
		 * OnSaveEmail
		 */
		private void OnConfirmEmail()
		{
			string email = m_container.Find("Email").GetComponent<InputField>().text;

			if (email.Length == 0)
			{
				string titleInfoError = LanguageController.Instance.GetText("message.error");
				string descriptionInfoError = LanguageController.Instance.GetText("screen.enter.email.empty.data");
				ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, titleInfoError, descriptionInfoError, null, "");
			}
			else
			{
				BasicEventController.Instance.DispatchBasicEvent(EVENT_SCREENENTEREMAIL_PRIVATE_KEY_CONFIRMATION, email);
			}
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnCancelURL
		 */
		private void OnCancelEmail()
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
				OnCancelEmail();
			}
		}
	}
}