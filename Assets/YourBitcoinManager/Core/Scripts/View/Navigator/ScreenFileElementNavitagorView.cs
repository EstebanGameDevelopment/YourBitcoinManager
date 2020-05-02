using System;
using System.Collections.Generic;
using System.IO;
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
	 * ScreenFileElementNavitagorView
	 * 
	 * It will show a list with the available keys
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenFileElementNavitagorView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_FILEELEMENTS_NAVIGATOR";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public const string EVENT_SCREENSYSTEMNAVIGATOR_FINAL_SELECTION = "EVENT_SCREENSYSTEMNAVIGATOR_FINAL_SELECTION";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------
		public const string SEARCH_IMAGES = "*.png|*.jpg|*.jpeg";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GameObject FileElementSlot;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private GameObject m_root;
		private Transform m_container;
		private Transform m_listItems;

		private string m_currentFileSelection;
		
		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.navigator.file.system");

			m_container.Find("Button_Back").GetComponent<Button>().onClick.AddListener(BackPressed);
			m_container.Find("Button_Accept").GetComponent<Button>().onClick.AddListener(AcceptPressed);
			m_container.Find("Button_Accept/Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.navigator.select.a.file");

			m_listItems = m_container.Find("ListItems");
			UpdateListItems(FileSystemController.Instance.PathLastSearch);

			BasicSystemEventController.Instance.BasicSystemEvent += new BasicSystemEventHandler(OnBasicEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;
			
			if (m_listItems!=null) m_listItems.GetComponent<FileManagerView>().Destroy();
			m_listItems = null;

			BasicSystemEventController.Instance.BasicSystemEvent -= OnBasicEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		 * UpdateListItems
		 */
		private void UpdateListItems(DirectoryInfo _path)
		{
			m_listItems.GetComponent<FileManagerView>().ClearCurrentGameObject(true);
			List<ItemMultiObjectEntry> items = FileSystemController.Instance.GetFileList(_path, "");
			m_listItems.GetComponent<FileManagerView>().Initialize(15, items, FileElementSlot);
		}

		// -------------------------------------------
		/* 
		* BackPressed
		*/
		private void BackPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		* RefreshPressed
		*/
		private void AcceptPressed()
		{
			UIEventController.Instance.DispatchUIEvent(EVENT_SCREENSYSTEMNAVIGATOR_FINAL_SELECTION, true, m_currentFileSelection);
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == FileItemView.EVENT_FILE_ITEM_SELECTED)
			{
				ItemMultiObjectEntry item = (ItemMultiObjectEntry)_list[0];
				m_currentFileSelection = ((FileInfo)item.Objects[1]).FullName;
				if (FileSystemController.IsFileImage(m_currentFileSelection))
				{
					Texture2D loadedTexture = ImageUtils.LoadTexture2D(m_currentFileSelection, 600);
					byte[] dataImage = loadedTexture.EncodeToJPG(75);
                    List<object> listKeyParams = new List<object>();
                    listKeyParams.Add((long)-1);
                    listKeyParams.Add(dataImage);
                    UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_OPEN_GENERIC_SCREEN, ScreenSingleImageView.SCREEN_IMAGE, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, false, listKeyParams);
                }
			}
			if (_nameEvent == FileItemView.EVENT_FILE_FOLDER_SELECTED)
			{
				ItemMultiObjectEntry item = (ItemMultiObjectEntry)_list[0];
				UpdateListItems((DirectoryInfo)item.Objects[1]);
			}
			if (_nameEvent == UIEventController.EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON)
			{
				Destroy();
			}
		}
	}
}