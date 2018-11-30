using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using YourBitcoinController;
using YourCommonTools;

namespace YourBitcoinManager
{

	/******************************************
	 * 
	 * MenusScreenController
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
	public class MenusScreenController : ScreenBitcoinController
	{
		public const string IAP_ACCESS_MAIN_NETWORK = "yourbitcoinmanager.access.main.bitcoin.network";
		
		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static MenusScreenController _instance;

		public static MenusScreenController MainInstance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(MenusScreenController)) as MenusScreenController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject SlotDisplayKeyPrefab;
		public GameObject SlotAddKeyPrefab;
		public Sprite SignedDataOK;
		public Sprite SignedDataFailed;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private bool m_hasIAPBeenInitialized = false;

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
		public override void Start()
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

#if !ENABLE_IAP			
			m_hasIAPBeenInitialized = true;
#endif

			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);

			// Debug.LogError("///////////////////////////////////////////////////////////////////////////////////////////");
			LanguageController.Instance.Initialize();
			CreateNewInformationScreen(ScreenInformationView.SCREEN_INITIAL_CONNECTION, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, LanguageController.Instance.GetText("message.your.bitcoin.manager.title"), LanguageController.Instance.GetText("message.connecting.to.blockchain"), null, null);

			Invoke("InitializeBitcoinCustom", 0.1f);
		}

		// -------------------------------------------
		/* 
		 * InitializeBitcoinCustom
		 */
		public void InitializeBitcoinCustom()
		{
			InitializeBitcoin();
		}

		// -------------------------------------------
		/* 
		 * InitializeBitcoin
		 */
		public override void InitializeBitcoin(string _screenToLoad = "", params object[] _optionalParams)
		{
			BitCoinController.Instance.Init();
#if ENABLE_IAP
			if (GameObject.FindObjectOfType<IAPController>()!=null)
			{
				IAPController.Instance.Init(IAP_ACCESS_MAIN_NETWORK);
			}			
#endif
		}

		// -------------------------------------------
		/* 
		 * Destroy all references
		 */
		public override void Destroy()
		{
            DestroyScreensFromLayerPool();

			if (_instance != null)
			{
				UIEventController.Instance.UIEvent -= OnUIEvent;
				BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;
				LanguageController.Instance.Destroy();
				CommController.Instance.Destroy();
				BitCoinController.Instance.Destroy();
				Destroy(_instance);
				_instance = null;
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected override void OnBitcoinEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_ALL_DATA_COLLECTED)
			{
				if (!m_hasBeenInitialized)
				{
					m_hasBeenInitialized = true;
					BitCoinController.Instance.LoadPrivateKeys(true);					
				}
				LoadMainScreen();
			}
		}

		// -------------------------------------------
		/* 
		 * Will load the main screen when all the initial data has been synchronized
		 */
		private void LoadMainScreen()
		{			
			if (m_hasBeenInitialized && m_hasIAPBeenInitialized)
			{				
				CreateNewScreen(ScreenToLoad, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected override void OnUIEvent(string _nameEvent, params object[] _list)
		{
			base.OnUIEvent(_nameEvent, _list);

#if ENABLE_IAP			
			if (_nameEvent == IAPController.EVENT_IAP_INITIALIZED)
			{
				m_hasIAPBeenInitialized = true;
				LoadMainScreen();
			}
#endif			
		}
	}
}