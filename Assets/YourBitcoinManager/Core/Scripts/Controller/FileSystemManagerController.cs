using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using YourBitcoinController;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * FileSystemManagerController
	 * 
	 * It allows us navigate through the filesystem
	 * 
	 * @author Esteban Gallardo
	 */
	public class FileSystemManagerController : MonoBehaviour
	{
		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------
		public const int ITEM_BACK		= 0;
		public const int ITEM_DRIVE		= 1;
		public const int ITEM_FOLDER	= 2;
		public const int ITEM_FILE		= 3;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------
		private static FileSystemManagerController _instance;

		public static FileSystemManagerController Instance
		{
			get
			{
				_instance = GameObject.FindObjectOfType(typeof(FileSystemManagerController)) as FileSystemManagerController;
				if (!_instance)
				{
					GameObject container = new GameObject();
					container.name = "FileSystemManagerController";
					_instance = container.AddComponent(typeof(FileSystemManagerController)) as FileSystemManagerController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------
		private DirectoryInfo m_pathLastSearch = null;

		public DirectoryInfo PathLastSearch
		{
			get { return m_pathLastSearch; }
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public void Destroy()
		{
			GameObject.Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * IsFileImage
		 */
		public static bool IsFileImage(string _filename)
		{
			return ((_filename.IndexOf(".png") != -1) || (_filename.IndexOf(".PNG") != -1)
					|| (_filename.IndexOf(".jpg") != -1) || (_filename.IndexOf(".JPG") != -1)
					|| (_filename.IndexOf(".jpeg") != -1) || (_filename.IndexOf(".JPEG") != -1));
		}

		// -------------------------------------------
		/* 
		 * Gets the list of items for the path
		 */
		public List<ItemMultiObjects> GetFileList(DirectoryInfo _directoryInfo, string _searchPattern = "")
		{
			List<ItemMultiObjects> output = new List<ItemMultiObjects>();
			try
			{
				DirectoryInfo directoryInfo = _directoryInfo;
				if (directoryInfo == null)
				{
					directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
				}
				
				// IF SHOW DRIVES
				if (directoryInfo.Parent == null)
				{
					string[] drives = System.IO.Directory.GetLogicalDrives();
					for (int i = 0; i < drives.Length; i++)
					{
						output.Add(new ItemMultiObjects(ITEM_DRIVE, new DirectoryInfo(drives[i])));
					}
				}

				// DIRECTORIES
				DirectoryInfo[] directories = directoryInfo.GetDirectories();
				for (int i = 0; i < directories.Length; i++)
				{
					output.Add(new ItemMultiObjects(ITEM_FOLDER, directories[i]));
				}

				// FILES
				FileInfo[] files = Utilities.GetFiles(directoryInfo, _searchPattern, SearchOption.TopDirectoryOnly);
				for (int i = 0; i < files.Length; i++)
				{
					output.Add(new ItemMultiObjects(ITEM_FILE, files[i]));
				}

				// IF SHOW DRIVES
				if (directoryInfo.Parent != null)
				{
					output.Insert(0, new ItemMultiObjects(ITEM_BACK, directoryInfo.Parent));
				}

				// SET LAST DIRECTORY VISITED
				m_pathLastSearch = directoryInfo;
			}
			catch (Exception err)
			{
				output.Clear();
				output.Add(new ItemMultiObjects(ITEM_BACK, m_pathLastSearch));
			}

			return output;
		}

	}
}