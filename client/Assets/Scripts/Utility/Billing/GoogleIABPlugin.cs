using UnityEngine;
using System.Collections;
using Prime31;
using System.Collections.Generic;

public class GoogleIABPlugin : MonoBehaviour {

#if UNITY_ANDROID

    // 課金可能時に呼び出すイベントを格納するためのデリゲート
    public delegate void BillingSupportedDelegate();
    // 失敗時に呼び出すイベントを格納するデリゲート
    public delegate void FailedEventDelegate (string error);
    // 課金アイテム購入状態取得成功時に呼び出すイベントを格納するデリゲート
    public delegate void QueryInventorySucceededDelegate(List<GooglePurchase> purchases, List<GoogleSkuInfo> skus);
    // 購入処理の完了通知時に呼び出すイベントを格納するデリゲート
    public delegate void PurchaseCompleteAwaitingDelegate(string purchaseData, string signature);
    // 購入成功時に呼び出すイベントを格納するデリゲート
    public delegate void PurchaseSucceededDelegate(GooglePurchase purchase);
    // 購入失敗時に呼び出すイベントを格納するデリゲート
    public delegate void PurchaseFailedDelegate(string error, int response);
    // 消費リクエスト成功時に呼び出すイベントを格納するデリゲート
    public delegate void ConsumePurchaseSucceededDelegate(GooglePurchase purchase);

    /// <summary>Googleデベロッパーコンソールで表示されているアプリケーションのパブリックキー</summary>
    private string appPublicKey = "";
    /// <summary>課金可否チェックで可能だった場合に呼び出すイベント</summary>
    private BillingSupportedDelegate billingSupportedEvent;
    /// <summary>課金可否チェックで不可能だった場合に呼び出すイベント</summary>
    private FailedEventDelegate billingNotSupportedEvent;
    /// <summary>課金アイテム購入状態取得成功時に呼び出すイベント</summary>
    private QueryInventorySucceededDelegate queryInventorySucceededEvent;
    /// <summary>課金アイテム購入状態取得失敗時に呼び出すイベント</summary>
    private FailedEventDelegate queryInventoryFailedEvent;
    /// <summary>購入処理の完了通知時に呼び出すイベント</summary>
    private PurchaseCompleteAwaitingDelegate purchaseCompleteEvent;
    /// <summary>購入成功時に呼び出すイベント</summary>
    private PurchaseSucceededDelegate purchaseSucceededEvent;
    /// <summary>購入失敗時に呼び出すイベント</summary>
    private PurchaseFailedDelegate purchaseFailedEvent;
    /// <summary>消費リクエスト成功時に呼び出すイベント</summary>
    private ConsumePurchaseSucceededDelegate consumeSucceededEvent;
    /// <summary>消費リクエスト失敗時に呼び出すイベント</summary>
    private FailedEventDelegate consumeFailedEvent;
    /// <summary>GoogleIABの初期化済みフラグ</summary>
    private bool isAlreadyInitialize;
    /// <summary>課金可否フラグ</summary>
    private bool isSupportedBilling;

    /// <summary>課金可否チェックで可能だった場合に呼び出すイベント</summary>
    public BillingSupportedDelegate BillingSupportedEvent
    {
        set{ billingSupportedEvent = value; }
    }

    /// <summary>課金可否チェックで不可能だった場合に呼び出すイベント</summary>
    public FailedEventDelegate BillingNotSupportedEvent
    {
        set{ billingNotSupportedEvent = value; }
    }

    /// <summary>課金アイテム購入状態取得成功時に呼び出すイベント</summary>
    public QueryInventorySucceededDelegate QueryInventorySucceededEvent
    {
        set{ queryInventorySucceededEvent = value; }
    }

    /// <summary>課金アイテム購入状態取得失敗時に呼び出すイベント</summary>
    public FailedEventDelegate QueryInventoryFailedEvent
    {
        set{ queryInventoryFailedEvent = value; }
    }

    /// <summary>購入処理の完了通知時に呼び出すイベント</summary>
    public PurchaseCompleteAwaitingDelegate PurchaseCompleteEvent
    {
        set{ purchaseCompleteEvent = value; }
    }

    /// <summary>購入成功時に呼び出すイベント</summary>
    public PurchaseSucceededDelegate PurchaseSucceededEvent
    {
        set{ purchaseSucceededEvent = value; }
    }

    /// <summary>購入失敗時に呼び出すイベント</summary>
    public PurchaseFailedDelegate PurchaseFailedEvent
    {
        set{ purchaseFailedEvent = value; }
    }

    /// <summary>消費リクエスト成功時に呼び出すイベント</summary>
    public ConsumePurchaseSucceededDelegate ConsumeSucceededEvent
    {
        set{ consumeSucceededEvent = value; }
    }

    /// <summary>消費リクエスト失敗時に呼び出すイベント</summary>
    public FailedEventDelegate ConsumeFailedEvent
    {
        set{ consumeFailedEvent = value; }
    }

    /// <summary>Googleデベロッパーコンソールで表示されているアプリケーションのパブリックキー</summary>
    public string AppPublicKey
    {
        set{ appPublicKey = value; }
    }

    /// <summary>課金可否フラグ</summary>
    public bool IsSupportedBilling
    {
        get{ return isSupportedBilling; }
    }

    /// <summary>
    /// GoogleIABの初期化を行う
    /// </summary>
    public void InitGoogleIAB()
    {
        GoogleIAB.init (appPublicKey);
    }

    /// <summary>
    /// 課金イベントを有効化
    /// </summary>
    public void EnableEvent()
    {
        GoogleIABManager.billingSupportedEvent += OnbillingSupported;
        GoogleIABManager.billingNotSupportedEvent += OnbillingNotSupported;
        GoogleIABManager.queryInventorySucceededEvent += OnQueryInventorySucceeded;
        GoogleIABManager.queryInventoryFailedEvent += OnQueryInventoryFailed;
        GoogleIABManager.purchaseCompleteAwaitingVerificationEvent += OnPurchaseCompleteAwaitingVerification;
        GoogleIABManager.purchaseSucceededEvent += OnPurchaseSucceeded;
        GoogleIABManager.purchaseFailedEvent += OnPurchaseFailed;
        GoogleIABManager.consumePurchaseSucceededEvent += OnConsumeSucceeded;
        GoogleIABManager.consumePurchaseFailedEvent += OnConsumeFailed;
    }

    /// <summary>
    /// 課金イベントを無効化
    /// </summary>
    public void DisableEvent()
    {
        GoogleIABManager.billingSupportedEvent -= OnbillingSupported;
        GoogleIABManager.billingNotSupportedEvent -= OnbillingNotSupported;
        GoogleIABManager.queryInventorySucceededEvent -= OnQueryInventorySucceeded;
        GoogleIABManager.queryInventoryFailedEvent -= OnQueryInventoryFailed;
        GoogleIABManager.purchaseCompleteAwaitingVerificationEvent -= OnPurchaseCompleteAwaitingVerification;
        GoogleIABManager.purchaseSucceededEvent -= OnPurchaseSucceeded;
        GoogleIABManager.purchaseFailedEvent -= OnPurchaseFailed;
        GoogleIABManager.consumePurchaseSucceededEvent -= OnConsumeSucceeded;
        GoogleIABManager.consumePurchaseFailedEvent -= OnConsumeFailed;
    }

    /// <summary>
    /// GoogleIABを終了
    /// </summary>
    public void UnbindService()
    {
        GoogleIAB.unbindService ();
    }

    private void Awake()
    {
        EnableEvent ();
    }

    private void OnApplicationQuit()
    {
        UnbindService ();
    }

    /// <summary>
    /// 課金可否チェック処理で課金可能だった場合の処理
    /// </summary>
    private void OnbillingSupported()
    {
        isSupportedBilling = true;

        if (billingSupportedEvent != null) {
            billingSupportedEvent ();
        }
    }

    /// <summary>
    /// 課金可否チェック処理で課金不可だった場合の処理
    /// </summary>
    private void OnbillingNotSupported(string error)
    {
        isSupportedBilling = false;

        if (billingNotSupportedEvent != null) {
            billingNotSupportedEvent (error);
        }
    }

    /// <summary>
    /// 課金アイテム購入状態取得成功
    /// </summary>
    /// <param name="purchases">Purchases.</param>
    /// <param name="skus">Skus.</param>
    private void OnQueryInventorySucceeded( List<GooglePurchase> purchases, List<GoogleSkuInfo> skus )
    {
        if (queryInventorySucceededEvent != null) {
            queryInventorySucceededEvent (purchases, skus);
        }
    }

    /// <summary>
    /// 課金アイテム購入状態取得失敗
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnQueryInventoryFailed( string error )
    {
        if (queryInventoryFailedEvent != null) {
            queryInventoryFailedEvent (error);
        }
    }

    /// <summary>
    /// 購入完了
    /// </summary>
    /// <param name="purchaseData">Purchase data.</param>
    /// <param name="signature">Signature.</param>
    private void OnPurchaseCompleteAwaitingVerification(string purchaseData, string signature)
    {
        if (purchaseCompleteEvent != null) {
            purchaseCompleteEvent (purchaseData, signature);
        }
    }

    /// <summary>
    /// 購入成功
    /// </summary>
    /// <param name="purchase">Purchase.</param>
    private void OnPurchaseSucceeded(GooglePurchase purchase)
    {
        if (purchaseSucceededEvent != null) {
            purchaseSucceededEvent (purchase);
        }
    }

    /// <summary>
    /// 課金失敗
    /// </summary>
    /// <param name="error">Error.</param>
    /// <param name="response">Response.</param>
    private void OnPurchaseFailed(string error, int response)
    {
        if (purchaseFailedEvent != null) {
            purchaseFailedEvent (error, response); 
        }
    }

    /// <summary>
    /// 消費リクエスト成功
    /// </summary>
    /// <param name="purchase">Purchase.</param>
    private void OnConsumeSucceeded(GooglePurchase purchase)
    {
        if (consumeSucceededEvent != null) {
            consumeSucceededEvent(purchase);
        }
    }

    /// <summary>
    /// 消費リクエスト失敗
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnConsumeFailed(string error)
    {
        if (consumeFailedEvent != null) {
            consumeFailedEvent (error);
        }
    }

#endif

}
