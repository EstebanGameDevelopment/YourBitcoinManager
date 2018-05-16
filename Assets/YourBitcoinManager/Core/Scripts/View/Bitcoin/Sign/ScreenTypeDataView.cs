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
	 * ScreenTypeDataView
	 * 
	 * It allows the users to select between a text and a file to enter
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenTypeDataView : ScreenBaseView, IBasicScreenView
	{
		public const string SCREEN_NAME = "SCREEN_TYPE_DATA";

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

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.sign.select.data");

			m_container.Find("Button_AddText").GetComponent<Button>().onClick.AddListener(OnAddTypeText);
			m_container.Find("Button_AddText/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.sign.type.data.add.text");
			m_container.Find("Button_AddFile").GetComponent<Button>().onClick.AddListener(OnAddTypeFile);
			m_container.Find("Button_AddFile/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.sign.type.data.add.file");

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
		private void OnAddTypeText()
		{			
			Destroy();
			ScreenController.Instance.CreateNewScreen(ScreenEnterTextView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false, LanguageController.Instance.GetText("screen.bitcoin.sign.write.text.you.want.signed"));
		}

		// -------------------------------------------
		/* 
		 * OnAddTypeFile
		 */
		private void OnAddTypeFile()
		{
			Destroy();
			ScreenController.Instance.CreateNewScreen(ScreenFileElementNavitagorView.SCREEN_NAME, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, false);
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