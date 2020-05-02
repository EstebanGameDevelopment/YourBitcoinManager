using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * ScreenEnterTextView
	 * 
	 * It allows the user to enter a text
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenEnterTextView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_ENTER_TEXT";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENENTERETEXT_CONFIRMATION = "EVENT_SCREENENTERETEXT_CONFIRMATION";
		public const string EVENT_SCREENENTERETEXT_MODIFICATION = "EVENT_SCREENENTERETEXT_MODIFICATION";		

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		private bool m_isModification;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
            object[] objectParams = (object[])_list[0];

			string initialText = "";
			m_isModification = false;
			if (objectParams.Length > 1)
			{
				initialText = (string)objectParams[1];
				m_isModification = true;
			}

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = (string)objectParams[0];
			m_container.Find("InputText").GetComponent<InputField>().text = initialText;

			m_container.Find("Button_Save").GetComponent<Button>().onClick.AddListener(OnConfirmEmail);
			m_container.Find("Button_Cancel").GetComponent<Button>().onClick.AddListener(OnCancelText);

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

			return false;
		}

		// -------------------------------------------
		/* 
		 * OnSaveEmail
		 */
		private void OnConfirmEmail()
		{
			string usersText = m_container.Find("InputText").GetComponent<InputField>().text;

			if (usersText.Length == 0)
			{
				string titleInfoError = LanguageController.Instance.GetText("message.error");
				string descriptionInfoError = LanguageController.Instance.GetText("screen.enter.email.empty.data");
                UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_INFORMATION_SCREEN, ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, titleInfoError, descriptionInfoError, null, "");
            }
			else
			{
				if (m_isModification)
				{
					UIEventController.Instance.DispatchUIEvent(EVENT_SCREENENTERETEXT_MODIFICATION, usersText);
				}
				else
				{
					UIEventController.Instance.DispatchUIEvent(EVENT_SCREENENTERETEXT_CONFIRMATION, usersText);
				}				
			}
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnCancelURL
		 */
		private void OnCancelText()
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
				OnCancelText();
			}
		}
	}
}