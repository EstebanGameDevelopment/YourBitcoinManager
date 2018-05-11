using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YourBitcoinManager
{

	public enum TypePreviousActionEnum
	{
		DESTROY_ALL_SCREENS = 0x00,
		DESTROY_CURRENT_SCREEN = 0x01,
		KEEP_CURRENT_SCREEN = 0x02,
		HIDE_CURRENT_SCREEN = 0x03
	}

	/******************************************
	 * 
	 * ScreenController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal
	 * 
	 * 	To get Bitcoins in the Main Network:
	 *  
	 *  https://buy.blockexplorer.com/
	 *  
	 *  Or in the TestNet Network:
	 *  
	 *  https://testnet.manu.backend.hamburg/faucet
	 *
	 * @author Esteban Gallardo
	 */
	public class ScreenController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_SCREENMANAGER_OPEN_SCREEN			= "EVENT_SCREENMANAGER_OPEN_SCREEN";
		public const string EVENT_SCREENMANAGER_DESTROY_SCREEN		= "EVENT_SCREENMANAGER_DESTROY_SCREEN";
		public const string EVENT_SCREENMANAGER_POOL_DESTROY_LAST	= "EVENT_SCREENMANAGER_POOL_DESTROY_LAST";
		public const string EVENT_SCREENMANAGER_OVERLAY_DESTROY_LAST = "EVENT_SCREENMANAGER_OVERLAY_DESTROY_LAST";
		public const string EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON = "EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON";
		public const string EVENT_GENERIC_MESSAGE_INFO_OK_BUTTON	= "EVENT_GENERIC_MESSAGE_INFO_OK_BUTTON";

		public const int MAXIMUM_NUMBER_OF_STACKED_SCREENS = 20;

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static ScreenController _instance;

		public static ScreenController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(ScreenController)) as ScreenController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public TextAsset ReadMeFile;

		public string ScreenToLoad;
		public GameObject SlotDisplayKeyPrefab;
		public GameObject SlotAddKeyPrefab;

		[Tooltip("All the screens used by the application")]
		public GameObject[] ScreensPrefabs;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private List<GameObject> m_screensPool = new List<GameObject>();
		private List<GameObject> m_screensOverlay = new List<GameObject>();
		private bool m_enableScreens = true;
		private bool m_enableDebugTestingCode = false;

		private bool m_hasBeenInitialized = false;

		// ----------------------------------------------
		// GETTERS/SETTERS
		// ----------------------------------------------	
		public bool EnableDebugTestingCode
		{
			get { return m_enableDebugTestingCode; }
			set { m_enableDebugTestingCode = value; }
		}

		// -------------------------------------------
		/* 
		* Awake
		*/
		void Awake()
		{
			System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
			customCulture.NumberFormat.NumberDecimalSeparator = ".";
			System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
		}

		// -------------------------------------------
		/* 
		 * Initialitzation listener
		 */
		void Start()
		{
#if DEBUG_MODE_DISPLAY_LOG
            Debug.Log("YourVRUIScreenController::Start::First class to initialize for the whole system to work");
#endif

			Screen.autorotateToPortrait = false;
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToLandscapeLeft = false;
			Screen.autorotateToLandscapeRight = false;

			Screen.orientation = ScreenOrientation.Portrait;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        Screen.SetResolution(550, 900, false);
#endif

			BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);

			// Debug.LogError("///////////////////////////////////////////////////////////////////////////////////////////");
			LanguageController.Instance.LoadTextsXML();
			CreateNewInformationScreen(ScreenInformationView.SCREEN_INITIAL_CONNECTION, TypePreviousActionEnum.DESTROY_ALL_SCREENS, LanguageController.Instance.GetText("message.your.bitcoin.manager.title"), LanguageController.Instance.GetText("message.connecting.to.blockchain"), null, null);

			Invoke("InitializeBitcoin", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * InitializeBitcoin
		 */
		public void InitializeBitcoin()
		{
			BitCoinController.Instance.Init();
#if ENABLE_IAP
			IAPController.Instance.Init();
#endif
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public void Destroy()
		{
			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
			LanguageController.Instance.Destroy();
			CommController.Instance.Destroy();
			BitCoinController.Instance.Destroy();
			DestroyObject(_instance);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * Create a new screen
		 */
		public void CreateNewScreenNoParameters(string _nameScreen, TypePreviousActionEnum _previousAction)
		{
			CreateNewScreen(_nameScreen, _previousAction, true, null);
		}

		// -------------------------------------------
		/* 
		 * Create a new screen
		 */
		public void CreateNewScreenNoParameters(string _nameScreen, bool _hidePreviousScreens, TypePreviousActionEnum _previousAction)
		{
			CreateNewScreen(_nameScreen, _previousAction, _hidePreviousScreens, null);
		}

		// -------------------------------------------
		/* 
		 * Create a new screen
		 */
		public void CreateNewInformationScreen(string _nameScreen, TypePreviousActionEnum _previousAction, string _title, string _description, Sprite _image, string _eventData, string _okButtonText = "", string _cancelButtonText = "")
		{
			List<PageInformation> pages = new List<PageInformation>();
			pages.Add(new PageInformation(_title, _description, _image, _eventData, _okButtonText, _cancelButtonText));

			CreateNewScreen(_nameScreen, _previousAction, false, pages);
		}

		// -------------------------------------------
		/* 
		 * Create a new screen
		 */
		public void CreateNewInformationScreen(string _nameScreen, TypePreviousActionEnum _previousAction, List<PageInformation> _pages)
		{
			CreateNewScreen(_nameScreen, _previousAction, false, _pages);
		}

		// -------------------------------------------
		/* 
		 * Create a new screen
		 */
		public void CreateNewScreen(string _nameScreen, TypePreviousActionEnum _previousAction, bool _hidePreviousScreens, params object[] _list)
		{
			if (!m_enableScreens) return;

#if DEBUG_MODE_DISPLAY_LOG
        Debug.Log("EVENT_SCREENMANAGER_OPEN_SCREEN::Creating the screen[" + _nameScreen + "]");
#endif
			if (_hidePreviousScreens)
			{
				if (m_screensPool.Count > MAXIMUM_NUMBER_OF_STACKED_SCREENS)
				{
					string warning = LanguageController.Instance.GetText("message.warning");
					string description = LanguageController.Instance.GetText("message.location.reached.limit.screen.stacked");
					ScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, TypePreviousActionEnum.KEEP_CURRENT_SCREEN, warning, description, null, "");
					return;
				}
			}
			if (_hidePreviousScreens)
			{
				EnableAllScreens(false);
			}

			// PREVIOUS ACTION
			switch (_previousAction)
			{
				case TypePreviousActionEnum.HIDE_CURRENT_SCREEN:
					if (m_screensPool.Count > 0)
					{
						m_screensPool[m_screensPool.Count - 1].SetActive(false);
					}
					break;

				case TypePreviousActionEnum.KEEP_CURRENT_SCREEN:
					break;

				case TypePreviousActionEnum.DESTROY_CURRENT_SCREEN:
					if (m_screensPool.Count > 0)
					{
						GameObject sCurrentScreen = m_screensPool[m_screensPool.Count - 1];
						if (sCurrentScreen.GetComponent<IBasicScreenView>() != null)
						{
							sCurrentScreen.GetComponent<IBasicScreenView>().Destroy();
						}
						GameObject.Destroy(sCurrentScreen);
						m_screensPool.RemoveAt(m_screensPool.Count - 1);
					}
					break;

				case TypePreviousActionEnum.DESTROY_ALL_SCREENS:
					DestroyScreensPool();
					DestroyScreensOverlay();
					break;
			}

			// CREATE SCREEN
			GameObject currentScreen = null;
			for (int i = 0; i < ScreensPrefabs.Length; i++)
			{
				if (ScreensPrefabs[i].name == _nameScreen)
				{
					currentScreen = (GameObject)Instantiate(ScreensPrefabs[i]);
					currentScreen.name = _nameScreen;
					currentScreen.GetComponent<IBasicScreenView>().Initialize(_list);
					break;
				}
			}

			if (_hidePreviousScreens)
			{
				m_screensPool.Add(currentScreen);
			}
			else
			{
				m_screensOverlay.Add(currentScreen);
			}
		}

		// -------------------------------------------
		/* 
		 * Destroy all the screens in memory
		 */
		public void DestroyScreensPool()
		{
			for (int i = 0; i < m_screensPool.Count; i++)
			{
				if (m_screensPool[i] != null)
				{
					if (m_screensPool[i].GetComponent<IBasicScreenView>() != null)
					{
						m_screensPool[i].GetComponent<IBasicScreenView>().Destroy();
					}
					GameObject.Destroy(m_screensPool[i]);
					m_screensPool[i] = null;
				}
			}
			m_screensPool.Clear();
		}
		
		// -------------------------------------------
		/* 
		 * Destroy all the screens in memory
		 */
		public void DestroyScreensOverlay()
		{			
			for (int i = 0; i < m_screensOverlay.Count; i++)
			{
				if (m_screensOverlay[i] != null)
				{
					if (m_screensOverlay[i].GetComponent<IBasicScreenView>() != null)
					{
						m_screensOverlay[i].GetComponent<IBasicScreenView>().Destroy();
					}
					GameObject.Destroy(m_screensOverlay[i]);
					m_screensOverlay[i] = null;
				}
			}
			m_screensOverlay.Clear();
		}

		// -------------------------------------------
		/* 
		 * Changes the enable of the screens
		 */
		private void EnableScreens(bool _activation)
		{
			if (m_screensPool.Count > 0)
			{
				if (m_screensPool[m_screensPool.Count - 1] != null)
				{
					if (m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicScreenView>() != null)
					{
						m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicScreenView>().SetActivation(_activation);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Changes the enable of the screens
		 */
		private void EnableAllScreens(bool _activation)
		{
			for (int i = 0; i < m_screensPool.Count; i++)
			{
				if (m_screensPool[i] != null)
				{
					if (m_screensPool[i].GetComponent<IBasicScreenView>() != null)
					{
						m_screensPool[i].GetComponent<IBasicScreenView>().SetActivation(_activation);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Remove the screen from the list of screens
		 */
		private void DestroyGameObjectSingleScreen(GameObject _screen, bool _runDestroy)
		{
			if (_screen == null) return;

			for (int i = 0; i < m_screensPool.Count; i++)
			{
				GameObject screen = (GameObject)m_screensPool[i];
				if (_screen == screen)
				{
					if (_runDestroy)
					{
						screen.GetComponent<IBasicScreenView>().Destroy();
					}
					m_screensPool.RemoveAt(i);
					if (screen != null)
					{ 						
						GameObject.Destroy(screen);
					}
					return;
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_SCREENMANAGER_DESTROY_SCREEN)
			{
				m_enableScreens = true;
				GameObject screen = (GameObject)_list[0];
				DestroyGameObjectSingleScreen(screen, true);
				EnableScreens(true);
			}
			if (_nameEvent == ScreenInformationView.EVENT_SCREENINFORMATION_FORCE_DESTRUCTION_POPUP)
			{
				DestroyScreensOverlay();
			}
			if (_nameEvent == EVENT_SCREENMANAGER_POOL_DESTROY_LAST)
			{
				if (m_screensPool.Count > 0)
				{
					if (m_screensPool[m_screensPool.Count - 1] != null)
					{
						if (m_screensPool[m_screensPool.Count - 1].GetComponent<IBasicScreenView>() != null)
						{
							GameObject refObject = m_screensPool[m_screensPool.Count - 1];
							m_screensPool.RemoveAt(m_screensPool.Count - 1);
							refObject.GetComponent<IBasicScreenView>().Destroy();
							refObject = null;
							EnableScreens(true);
							return;
						}
					}
				}
			}
			if (_nameEvent == EVENT_SCREENMANAGER_OVERLAY_DESTROY_LAST)
			{
				if (m_screensOverlay.Count > 0)
				{
					if (m_screensOverlay[m_screensOverlay.Count - 1] != null)
					{
						if (m_screensOverlay[m_screensOverlay.Count - 1].GetComponent<IBasicScreenView>() != null)
						{
							GameObject refObject = m_screensOverlay[m_screensOverlay.Count - 1];
							m_screensOverlay.RemoveAt(m_screensOverlay.Count - 1);
							refObject.GetComponent<IBasicScreenView>().Destroy();
							refObject = null;
						}
					}
				}
			}
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_ALL_DATA_COLLECTED)
			{
				if (!m_hasBeenInitialized)
				{
					m_hasBeenInitialized = true;
					BitCoinController.Instance.LoadPrivateKeys(true);
					
					CreateNewScreenNoParameters(ScreenToLoad, TypePreviousActionEnum.DESTROY_ALL_SCREENS);
				}				
			}
		}

		// -------------------------------------------
		/* 
		 * Update
		 */
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				BasicEventController.Instance.DispatchBasicEvent(EVENT_SCREENMANAGER_ANDROID_BACK_BUTTON);
			}
		}
	}
}