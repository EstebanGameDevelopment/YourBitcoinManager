#if ENABLE_IAP
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine.Purchasing;

namespace YourBitcoinManager
{
	/******************************************
	 * 
	 * IAPController
	 * 
	 * It manages all the IAP with the different systems of payment.
	 * 
	 * @author Esteban Gallardo
	 */
	public class IAPController : MonoBehaviour, IStoreListener
	{
		// ----------------------------------------------
		// PRODUCTS
		// ----------------------------------------------
		// ANDROID
		public const string IAP_ACCESS_MAIN_NETWORK = "yourbitcoinmanager.access.main.bitcoin.network";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------
		public const string EVENT_IAP_RESPONSE			= "EVENT_IAP_RESPONSE";
		public const string EVENT_IAP_SUCCESS_PURCHASE	= "EVENT_IAP_SUCCESS_PURCHASE";
		
		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------
		private static IAPController _instance;

		public static IAPController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType<IAPController>();
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// MEMBERS
		// ----------------------------------------------
		// Unity IAP objects
		private static IStoreController m_StoreController;          // The Unity Purchasing system.
		private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

		private bool m_iapHasBeenInitialized = false;

		// ----------------------------------------------
		// CONSTRUCTOR
		// ----------------------------------------------	
		// -------------------------------------------
		/* 
		 * This will be called when Unity IAP has finished initialising.
		 */
		public void Init()
		{
			if (m_StoreController == null)
			{
				if (IsInitialized())
				{
					return;
				}
				m_iapHasBeenInitialized = true;

				BasicEventController.Instance.BasicEvent += new BasicEventHandler(OnBasicEvent);

				var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

				// DEFINE YOUR OWN IAPs
				builder.AddProduct(IAP_ACCESS_MAIN_NETWORK, ProductType.Consumable);

				UnityPurchasing.Initialize(this, builder);				
			}
		}

		// -------------------------------------------
		/* 
		 * IsInitialized
		 */
		private bool IsInitialized()
		{
			return m_StoreController != null && m_StoreExtensionProvider != null && m_iapHasBeenInitialized;
		}

		// -------------------------------------------
		/* 
		* UnlockAccessMainBitcoinNetwork
		*/
		public void UnlockAccessMainBitcoinNetwork()
		{
#if !UNITY_EDITOR
			BuyProductID(IAP_ACCESS_MAIN_NETWORK);
#else
			BasicEventController.Instance.DispatchBasicEvent(EVENT_IAP_SUCCESS_PURCHASE, true);
#endif
		}

		// -------------------------------------------
		/* 
		* Destroy
		*/
		public void Destroy()
		{
			BasicEventController.Instance.BasicEvent -= OnBasicEvent;
			Destroy(_instance.gameObject);
			_instance = null;
		}

		// -------------------------------------------
		/* 
		 * OnInitialized
		 */
		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			m_StoreController = controller;
			m_StoreExtensionProvider = extensions;

			// Purchasing has succeeded initializing. Collect our Purchasing references.
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("OnInitialized: PASS+++++++++++++++++");
			Debug.LogError("(m_StoreController!=null)[" + (m_StoreController != null) + "]");
			Debug.LogError("(m_StoreExtensionProvider!=null)[" + (m_StoreExtensionProvider != null) + "]");
#endif
		}

		// -------------------------------------------
		/* 
		 * OnInitializeFailed
		 */
		public void OnInitializeFailed(InitializationFailureReason error)
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("OnInitializeFailed InitializationFailureReason:" + error);
#endif
		}

		// -------------------------------------------
		/* 
		 * BuyProductID
		 */
		private void BuyProductID(string _productId)
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError("BuyProductID: _productId="+_productId+"+++++++++++++++++");
			Debug.LogError("BuyProductID: IsInitialized="+IsInitialized()+"+++++++++++++++++");
#endif
			if (IsInitialized())
			{
				Product product = m_StoreController.products.WithID(_productId);

				if (product != null && product.availableToPurchase)
				{
#if DEBUG_MODE_DISPLAY_LOG
					Debug.LogError(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
#endif
					m_StoreController.InitiatePurchase(product);
				}
				else
				{
#if DEBUG_MODE_DISPLAY_LOG
					Debug.LogError("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
#endif
				}
			}
			else
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.LogError("BuyProductID FAIL. Not initialized.");
#endif
			}
		}

		// -------------------------------------------
		/* 
		* RestorePurchases
		*/
		public void RestorePurchases()
		{
			if (!IsInitialized())
			{
#if DEBUG_MODE_DISPLAY_LOG
				Debug.LogError("RestorePurchases FAIL. Not initialized.");
#endif
				return;
			}

			// We are not running on an Apple device. No work is necessary to restore purchases.
#if DEBUG_MODE_DISPLAY_LOG
			Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
#endif
		}

		// -------------------------------------------
		/* 
		 * ProcessPurchase
		 */
		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError(string.Format("ProcessPurchase: SUCCESS. Product: '{0}'", args.purchasedProduct.definition.id));
#endif
			BasicEventController.Instance.DispatchBasicEvent(EVENT_IAP_RESPONSE, true, args.purchasedProduct.definition.id);
			return PurchaseProcessingResult.Complete;
		}


		// -------------------------------------------
		/* 
		 * OnPurchaseFailed
		 */
		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
		{
#if DEBUG_MODE_DISPLAY_LOG
			Debug.LogError(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
#endif
			BasicEventController.Instance.DispatchBasicEvent(EVENT_IAP_RESPONSE, false, product.definition.id);
		}

		// -------------------------------------------
		/* 
		 * OnBasicEvent
		 */
		private void OnBasicEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_IAP_RESPONSE)
			{
				bool success = (bool)_list[0];
				string iapID = (string)_list[1];
#if DEBUG_MODE_DISPLAY_LOG
				Debug.LogError("EVENT_IAP_CONFIRMATION::success[" + success + "]::iapID[" + iapID + "]");
#endif
				if (iapID.IndexOf(IAP_ACCESS_MAIN_NETWORK) != -1)
				{
					if (success)
					{
						BasicEventController.Instance.DispatchBasicEvent(EVENT_IAP_SUCCESS_PURCHASE, true);
					}
					else
					{
						BasicEventController.Instance.DispatchBasicEvent(EVENT_IAP_SUCCESS_PURCHASE, false);
					}
				}
			}
		}
	}
}
#endif