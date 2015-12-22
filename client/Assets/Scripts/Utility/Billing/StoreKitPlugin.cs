using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;
using System.Text;

public class StoreKitPlugin : MonoBehaviour {

	#if UNITY_IOS

	// 購入成功時に呼び出すイベントを格納するためのデリゲート
	public delegate void PurchaseSuccessfulDelegate(StoreKitTransaction transaction);
	// 課金処理が失敗した際に呼び出すをイベントを格納するための汎用デリゲート
	public delegate void FailedEventDelegate(string error);
	// 購入キャンセル時に呼び出すイベントを格納するためのデリゲート
	public delegate void PurchaseCancelledDelegate(string error);
	// 課金アイテム情報取得成功時に呼び出すイベントを格納するためのデリゲート
	public delegate void ProductListReceivedDelegate(List<StoreKitProduct> productList);

	/// <summary>購入成功時に呼び出すイベント</summary>
	private PurchaseSuccessfulDelegate purchaseSuccessfulEvent;
	/// <summary>購入失敗時に呼び出すイベント</summary>
	private FailedEventDelegate purchaseFailedEvent;
	/// <summary>購入キャンセル時に呼び出すイベント</summary>
	private PurchaseCancelledDelegate purchaseCancelledEvent;
	/// <summary>課金アイテム情報取得成功時に呼び出すイベント</summary>
	private ProductListReceivedDelegate productListReceivedEvent;
	/// <summary>課金アイテム情報取得失敗時に呼び出すイベント</summary>
	private FailedEventDelegate productListFailedEvent;
	/// <summary>課金処理完了時に呼び出すイベント</summary>
	private BillingManager.GrantItemCompletedDelegate grantCompletedEvent;
	/// <summary>購入リクエストを発行する際に課金アイテム情報を取得するか否か</summary>
	private bool isAutoProductRequest;
	/// <summary>購入対象の課金アイテムのID</summary>
	private string targetProductId;
	/// <summary>課金アイテムを購入する個数</summary>
	private int purchaseQuantity;

	/// <summary>購入成功時に呼び出すイベント</summary>
	public PurchaseSuccessfulDelegate PurchaseSuccessfulEvent
	{
		set{ purchaseSuccessfulEvent = value; }
	}

	/// <summary>購入失敗時に呼び出すイベント</summary>
	public FailedEventDelegate PurchaseFailedEvent
	{
		set{ purchaseFailedEvent = value; }
	}

	/// <summary>購入キャンセル時に呼び出すイベント</summary>
	public PurchaseCancelledDelegate PurchaseCancelledEvent
	{
		set{ purchaseCancelledEvent = value; }
	}

	/// <summary>課金アイテム情報取得成功時に呼び出すイベント</summary>
	public ProductListReceivedDelegate ProductListReceivedEvent
	{
		set{ productListReceivedEvent = value; }
	}

	/// <summary>課金アイテム情報取得失敗時に呼び出すイベント</summary>
	public FailedEventDelegate ProductListFailedEvent
	{
		set{ productListFailedEvent = value; }
	}

	/// <summary>課金トランザクション後に呼び出すイベント</summary>
	public BillingManager.GrantItemCompletedDelegate GrantCompletedEvent
	{
		set{ grantCompletedEvent = value; }
	}

	/// <summary>購入リクエストを発行する際に課金アイテム情報を取得するか否か</summary>
	public bool IsAutoProductRequest
	{
		set{ isAutoProductRequest = value; }
	}

	/// <summary>購入対象の課金アイテムのID</summary>
	public string TargetProductId
	{
		set{ targetProductId = value; }
	}

	/// <summary>課金アイテムを購入する個数</summary>
	public int PurchaseQuantity
	{
		set{ purchaseQuantity = value; }
	}

	/// <summary>
	/// アプリで購入可能な課金アイテムの情報を取得
	/// </summary>
	/// <param name="productIdentifiers">Product identifiers.</param>
	public void GetProductList()
	{
		// 課金アイテムのIDが存在したらリクエストを投げる
		if (BillingManager.Instance.ProductIdList != null && BillingManager.Instance.ProductIdList.Length > 0) {
			StoreKitBinding.requestProductData (BillingManager.Instance.ProductIdList);
		}
	}

	/// <summary>
	/// 課金アイテムの購入を行う
	/// </summary>
	/// <param name="productId">Product identifier.</param>
	public void PurchaseProduct(string productId, int quantity = 1)
	{
		// 購入情報を保存
		targetProductId = productId;
		purchaseQuantity = quantity;

		if (isAutoProductRequest) {
			// 自動で購入リクエストを投げるための情報を設定
			SetAutoPurchaseInfo (productId, quantity);

			// 課金アイテムの情報を取得
			GetProductList ();
		} else {
			// 指定した課金アイテムの購入リクエストを投げる
			PurchaseProduct ();
		}
	}

	private void Awake()
	{
		// 課金時のイベントを有効化
		EnableEvent();
		// 購入リクエストを送る前に自動で課金アイテムを取得するように設定
		isAutoProductRequest = true;
	}

	/// <summary>
	/// appleに購入リクエストを投げる
	/// </summary>
	private void PurchaseProduct()
	{
		// 指定した課金アイテムの購入リクエストを投げる
		StoreKitBinding.purchaseProduct (targetProductId, purchaseQuantity);
	}

	/// <summary>
	/// 課金成功
	/// </summary>
	/// <param name="transaction">Transaction.</param>
	private void OnSucceededPurchase(StoreKitTransaction transaction)
	{
		if(purchaseSuccessfulEvent != null)
		{
			purchaseSuccessfulEvent (transaction);
		}
	}

	/// <summary>
	/// 課金失敗
	/// </summary>
	/// <param name="error">Error.</param>
	private void OnFailedPurchase(string error)
	{
		if (purchaseFailedEvent != null) {
			purchaseFailedEvent (error);
		}
	}

	/// <summary>
	/// 課金キャンセル
	/// </summary>
	/// <param name="error">Error.</param>
	private void OnCancelledPurchase(string error)
	{
		if (purchaseCancelledEvent != null) {
			purchaseCancelledEvent (error);
		}
	}

	/// <summary>
	/// 課金アイテム情報取得成功
	/// </summary>
	/// <param name="productList">Product list.</param>
	private void OnSucceededProductListRequest(List<StoreKitProduct> productList)
	{
		if (productListReceivedEvent != null) {
			productListReceivedEvent (productList);
		}
	}

	/// <summary>
	/// 課金アイテム情報取得失敗
	/// </summary>
	/// <param name="error">Error.</param>
	private void OnFailedProductListRequest(string error)
	{
		if (productListFailedEvent != null) {
			productListFailedEvent (error);
		}
	}

	/// <summary>
	/// iOSの課金イベント有効化
	/// </summary>
	private void EnableEvent()
	{
		StoreKitManager.purchaseSuccessfulEvent += OnSucceededPurchase;
		StoreKitManager.purchaseFailedEvent += OnFailedPurchase;
		StoreKitManager.purchaseCancelledEvent += OnCancelledPurchase;
		StoreKitManager.productListReceivedEvent += OnSucceededProductListRequest;
		StoreKitManager.productListRequestFailedEvent += OnFailedProductListRequest;
	}

	/// <summary>
	/// iOSの課金イベント無効化
	/// </summary>
	private void   DisableEvent()
	{
		StoreKitManager.purchaseSuccessfulEvent -= OnSucceededPurchase;
		StoreKitManager.purchaseFailedEvent -= OnFailedPurchase;
		StoreKitManager.purchaseCancelledEvent -= OnCancelledPurchase;
		StoreKitManager.productListReceivedEvent -= OnSucceededProductListRequest;
		StoreKitManager.refreshReceiptFailedEvent -= OnFailedProductListRequest;
	}

	/// <summary>
	/// 課金アイテム情報取得後に自動で購入リクエストを投げるための情報を設定
	/// </summary>
	/// <param name="productId">Product identifier.</param>
	/// <param name="quantity">Quantity.</param>
	private void SetAutoPurchaseInfo(string productId, int quantity)
	{
		// 課金アイテム情報取得後に自動で購入リクエ  トを    るように設定
		productListReceivedEvent =  ExecuteAutoPurchase;
		// 情報取得に失敗したら課金エラーと同じ処理を実行        productListFailedEvent = p  rchaseFailedEvent;
	}

	/// <summary>
	/// 課金アイテム情報取得後に自動で購入リクエストを投げる処理
	/// </summary>
	/// <param name="productList">Product list.</param>
	private void ExecuteAutoPurchase(List<StoreKitProduct> productList)
	{
		// 指定した課金アイテムの購入リクエストを投  る
		PurchaseProduct ();
	}

	/// <summary>
	/// 課金アイテム付与終了時の処理
	/// </summary>
	private void   OnGrantCompleted()
	{
		//todo:utsumi 削除処理を実装

		//外部から設定された付与終了時の処理を実行
		if (grantCompletedEvent != null) {
			grantCompletedEvent ();
		}
	}

	#endif

}