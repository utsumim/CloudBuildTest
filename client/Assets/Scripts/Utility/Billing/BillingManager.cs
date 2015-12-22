using UnityEngine;
using System.Collections;
using Prime31;
using System.Collections.Generic;

public class BillingManager : MonoBehaviour {

    // APIの進行状態の種別
    public enum eApiState
    {
        RequestThrow,
        ResponseWait,
        End,
    }

    // 課金トランザクションの進行状態の種別
    public enum ePurchaseTransactionState
    {
        /// <summary>購入処理開始</summary>
        PurchaseStart,
        /// <summary>購入完了</summary>
        PurchaseComplete,
        /// <summary>付与完了</summary>
        GrantComplete,
    }

    // 課金処理完了時に呼び出すイベントを格納するためのデリゲート
    public delegate void GrantItemCompletedDelegate();

    // 課金開始前に呼び出すAPIを格納するためのデリゲート
    public delegate IEnumerator PurchaseStartApiDelegate ();
    // 課金アイテムを付与するAPIを格納するためのデリゲート
    public delegate IEnumerator GiveItemApiDelegate ();

    /// <summary>iOS用の課金を行うスクリプト</summary>
    [SerializeField] private StoreKitPlugin storeKitInstance;
    /// <summary>Android用の課金を行うスクリプト</summary>
    [SerializeField] private GoogleIABPlugin googleIABInstance;

    /// <summary>課金開始前に呼び出すAPI</summary>
    private PurchaseStartApiDelegate purchaseStartApi;
    /// <summary>課金開始前に呼び出すAPIの状態</summary>
    private eApiState purchaseStartApiState;
    /// <summary>課金アイテムを付与するAPI</summary>
    private GiveItemApiDelegate grantItemApi;
    /// <summary>課金アイテムを付与するAPIの状態</summary>
    private eApiState grantItemApiState;
    /// <summary>課金処理完了時に呼び出すイベント</summary>
    private GrantItemCompletedDelegate grantCompletedEvent;

    /// <summary>課金処理を自動で行うか否か</summary>
    private bool isAutoPurchase;
    /// <summary>購入対象の課金アイテムのID</summary>
    private string targetProductId;
    /// <summary>アプリケーションに登録されている課金アイテのIDリスト</summary>
    private string[] productIdList;
    /// <summary>課金管理クラスのインスタンス</summary>
    private static BillingManager instance;

    /// <summary>課金開始前に呼び出すAPIの状態</summary>
    public eApiState PurchaseStartApiState
    {
        get{ return purchaseStartApiState; }
        set{ purchaseStartApiState = value; }
    }

    /// <summary>課金アイテムを付与するAPIの状態</summary>
    public eApiState GrantItemApiState
    {
        get{ return grantItemApiState; }
        set{ grantItemApiState = value; }
    }

    /// <summary>アプリケーションに登録されている課金アイテのIDリスト</summary>
    public string[] ProductIdList
    {
        get{ return productIdList; }
        set{ productIdList = value; }
    }

    /// <summary>課金管理クラスのインスタンス ※Awakeで参照が設定されるので、他クラスのAwakeからアクセスする場合はタイミングに注意する</summary>
    public static BillingManager Instance
    {
        get { return instance; }
    }

    /// <summary>
    /// 課金を行う際に必要なAPIを設定
    /// </summary>
    /// <param name="purchaseStart">Purchase start.</param>
    /// <param name="giveItem">Give item.</param>
    public void SetApi(PurchaseStartApiDelegate purchaseStart, GiveItemApiDelegate grantItem)
    {
        purchaseStartApi = purchaseStart;
        grantItemApi = grantItem;
    }

    /// <summary>
    /// 購入処理終了時のコールバックを設定
    /// </summary>
    /// <param name="completedEvent">Completed event.</param>
    public void SetGrantCompletedCallback(GrantItemCompletedDelegate completedEvent)
    {
        grantCompletedEvent = completedEvent;
    }

    /// <summary>
    /// 課金アイテム付与処理終了時の処理
    /// </summary>
    private void OnGrantCompleted()
    {
        if (grantCompletedEvent != null) {
            grantCompletedEvent ();
        }
    }

    /// <summary>
    /// 中断された課金トランザクションの有無を調べる
    /// </summary>
    private void CheckSuspendedBillingTransaction()
    {
        //todo:utsumi ローカルファイルの存在チェック
    }

    /// <summary>
    /// 課金開始前APIを実行
    /// </summary>
    /// <returns>The start purchase API.</returns>
    private IEnumerator ExecuteStartPurchaseApi()
    {
        // 課金前APIを呼び出す
        // ※APIの内部でpurchaseStartApiStateの状態を管理する
        StartCoroutine (purchaseStartApi ());

        // APIの終了を待つ
        while (true) {
            if (purchaseStartApiState == BillingManager.eApiState.End) {
                break;
            }

            yield return null;
        }

        // todo:utsumi 課金トランザクション情報を保存
        BillingManager.Instance.SaveBillingTransaction (BillingManager.ePurchaseTransactionState.PurchaseStart, "");

        // 自動購入がONの場合は購入リクエストを投げる
        if (isAutoPurchase) {
            // 購入リクエストを発行
            ThrowPurchaseRequest ();
        }
    }

    /// <summary>
    /// 購入リクエストを発行
    /// </summary>
    private void ThrowPurchaseRequest()
    {
#if UNITY_IOS
        // 購入リクエストを発行
        storeKitInstance.PurchaseProduct (targetProductId, purchaseQuantity);
#elif UNITY_ANDROID
        PurchaseProduct(targetProductId);
#endif
    }

    /// <summary>
    /// 課金アイテム付与APIを実行
    /// </summary>
    /// <returns>The give item API.</returns>
    private IEnumerator ExecuteGrantItemApi()
    {
        // 課金アイテム付与APIを呼び出す
        // ※APIの内部でgiveItemApiStateの状態を管理する
        StartCoroutine (grantItemApi ());

        // APIの終了を待つ
        while (true) {
            if (grantItemApiState == BillingManager.eApiState.End) {
                break;
            }

            yield return null;
        }

        // 課金トランザクションを削除
        BillingManager.Instance.DeleteBillingTransaction ();

        OnGrantCompleted ();
    }


    private void SaveBillingTransaction(ePurchaseTransactionState transactionState)
    {
        //todo:utsumi 課金トランザクション情報を保存
    }

    private void SaveBillingTransaction(ePurchaseTransactionState transactionState, string transactionId)
    {
        //todo:utsumi 課金トランザクション情報を保存
    }

    /// <summary>
    /// 課金トランザクションを削除
    /// </summary>
    private void DeleteBillingTransaction()
    {
        //todo:utsumi 課金トランザクション削除処理を実装
    }

    /// <summary>
    /// 課金トランザクション情報を読み込む
    /// </summary>
    private PurchaseTransactionInfo LoadBillingTransactionInfo()
    {
        //todo:utsumi ローカルファイルから課金トランザクション情報を取得
        PurchaseTransactionInfo dummyInfo = new PurchaseTransactionInfo ("jp.co.craftegg.nanairorungirls.gem.0004", "transactionId", ePurchaseTransactionState.PurchaseStart);
        return null;//dummyInfo;
    }

    private void Awake()
    {
#if UNITY_IOS
        // 外部から設定されたコールバックが購入処理終了時に呼び出されるように設定
        storeKitInstance.PurchaseSuccessfulEvent = OnPurchseSucceeded;
        storeKitInstance.PurchaseFailedEvent = OnPurchseFailed;
        storeKitInstance.PurchaseCancelledEvent = OnPurchaseCancelled;
        // 外部から設定されたコールバックが課金アイテム情報取得処理終了時に呼び出されるように設定
        storeKitInstance.ProductListReceivedEvent = OnProductRequestSucceeded;
        storeKitInstance.ProductListFailedEvent = OnProductListRequestFailed;
        // 外部から設定されたコールバックが購入処理終了時に呼び出されるように設定
        storeKitInstance.GrantCompletedEvent = OnGrantCompleted;
#elif UNITY_ANDROID
        // 外部から設定されたコールバックが課金可否チェック終了時に呼び出されるように設定
        googleIABInstance.BillingSupportedEvent = OnSupportedBilling;
        googleIABInstance.BillingNotSupportedEvent = OnNotSupportedBilling;
        // 外部から設定されたコールバックが購入情報取得時に呼び出されるように設定
        googleIABInstance.QueryInventorySucceededEvent = OnQueryInventorySucceeded;
        googleIABInstance.QueryInventoryFailedEvent = OnQueryInventoryFailed;
        // 外部から設定されたコールバックが購入処理終了時に呼び出されるように設定
        googleIABInstance.PurchaseSucceededEvent = OnPurchaseSucceeded;
        googleIABInstance.PurchaseFailedEvent = OnPurchaseFailed;
        googleIABInstance.PurchaseCompleteEvent = OnPurchaseCompleted;
        // 外部から設定されたコールバックが消費リクエスト送信時に呼び出されるように設定
        googleIABInstance.ConsumeSucceededEvent = OnConsumeSucceeded;
        googleIABInstance.ConsumeFailedEvent = OnConsumeFailed;
#endif
        instance = gameObject.GetComponent<BillingManager> ();

        // APIの状態を初期化
        purchaseStartApiState = BillingManager.eApiState.RequestThrow;
        grantItemApiState = eApiState.RequestThrow;
    }

#if UNITY_IOS

    // 課金が許可されていない場合に呼び出すイベントを格納するゲリゲート
    public delegate void UnauthorizedBillingDelegate();

    /// <summary>外部から設定する課金成功時に呼び出す処理</summary>
    private StoreKitPlugin.PurchaseSuccessfulDelegate purchaseSucceededEvent;
    /// <summary>外部から設定する課金失敗時に呼び出すコールバック</summary>
    private StoreKitPlugin.FailedEventDelegate purchaseFailedEvent;
    /// <summary>外部から設定する課金キャンセル時に呼び出すコールバック</summary>
    private StoreKitPlugin.PurchaseCancelledDelegate purchaseCancelledEvent;
    /// <summary>外部から設定する課金アイテム情報取得成功時に呼び出すコールバック</summary>
    private StoreKitPlugin.ProductListReceivedDelegate productReqSucceededEvent;
    /// <summary>外部から設定する課金アイテム情報取得失敗時に呼び出すコールバック</summary>
    private StoreKitPlugin.FailedEventDelegate productReqFailedEvent;
    /// <summary>課金が許可されていない場合に呼び出すイベント</summary>
    private UnauthorizedBillingDelegate unauthorizedEvent;

    /// <summary>課金アイテムを購入する個数</summary>
    private int purchaseQuantity;

    /// <summary>
    /// 課金が許可されない設定になっていた場合のコールバックを設定
    /// </summary>
    /// <param name="unauthoried">Unauthoried.</param>
    public void SetUnauthorizeBillingCallback(UnauthorizedBillingDelegate unauthoried)
    {
        unauthorizedEvent = unauthoried;
    }

    /// <summary>
    /// 購入リクエストのコールバックを設定
    /// </summary>
    /// <param name="succeeded">Succeeded.</param>
    /// <param name="failedEvent">Failed event.</param>
    /// <param name="cancelledEvent">Cancelled event.</param>
    public void SetPurchaseCallback(StoreKitPlugin.PurchaseSuccessfulDelegate succeededEvent, StoreKitPlugin.FailedEventDelegate failedEvent, StoreKitPlugin.PurchaseCancelledDelegate cancelledEvent)
    {
        purchaseSucceededEvent = succeededEvent;
        purchaseFailedEvent = failedEvent;
        purchaseCancelledEvent = cancelledEvent;
    }

    /// <summary>
    /// 課金アイテム情報取得のコールバックを設定
    /// </summary>
    /// <param name="succeededEvent">Succeeded event.</param>
    /// <param name="failedEvent">Failed event.</param>
    public void SetProductListRequestCallback(StoreKitPlugin.ProductListReceivedDelegate succeededEvent, StoreKitPlugin.FailedEventDelegate failedEvent)
    {
        productReqSucceededEvent = succeededEvent;
        productReqFailedEvent = failedEvent;
    }

    /// <summary>
    /// 事前に設定したAPI、コールバックを使用して課金を行う
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    public void PurchaseProductAuto(string productId, int quantity = 1)
    {
        if (CanMakePayment ()) {
            targetProductId = productId;
            purchaseQuantity = quantity;
            isAutoPurchase = true;

            StartCoroutine (ExecuteStartPurchaseApi ());
        } else {
            // 課金が出来ない設定になっている場合の処理
            if (unauthorizedEvent != null) {
                unauthorizedEvent ();
            }
        }
    }

    /// <summary>
    /// 端末が課金可能な設定になっているかをチェック
    /// </summary>
    /// <returns><c>true</c> if this instance can make payment; otherwise, <c>false</c>.</returns>
    public bool CanMakePayment()
    {
        return StoreKitBinding.canMakePayments ();
    }

    /// <summary>
    /// 初期化
    /// </summary>
	private void Start()
    {
        // 外部から設定されたコールバックが購入処理終了時に呼び出されるように設定
        storeKitInstance.PurchaseSuccessfulEvent = OnPurchseSucceeded;
        storeKitInstance.PurchaseFailedEvent = OnPurchseFailed;
        storeKitInstance.PurchaseCancelledEvent = OnPurchaseCancelled;
        // 外部から設定されたコールバックが課金アイテム情報取得処理終了時に呼び出されるように設定
        storeKitInstance.ProductListReceivedEvent = OnProductRequestSucceeded;
        storeKitInstance.ProductListFailedEvent = OnProductListRequestFailed;
        // 外部から設定されたコールバックが購入処理終了時に呼び出されるように設定
        storeKitInstance.GrantCompletedEvent = OnGrantCompleted;

        // APIの状態を初期化
        purchaseStartApiState = BillingManager.eApiState.RequestThrow;
        grantItemApiState = eApiState.RequestThrow;
    }

    /// <summary>
    /// 課金成功時の処理
    /// </summary>
    /// <param name="transaction">Transaction.</param>
    private void OnPurchseSucceeded(StoreKitTransaction transaction)
    {
        if (purchaseSucceededEvent != null) {
            purchaseSucceededEvent (transaction);
        }

        // 自動処理がONなら付与APIを呼び出す
        if (isAutoPurchase) {
            StartCoroutine (ExecuteGrantItemApi ());
        }
    }

    /// <summary>
    /// 課金失敗時の処理
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnPurchseFailed(string error)
    {
        if (purchaseFailedEvent != null) {
            purchaseFailedEvent (error);
        }
    }

    /// <summary>
    /// 課金キャンセル時の処理
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnPurchaseCancelled(string error)
    {
        if (purchaseCancelledEvent != null) {
            purchaseCancelledEvent (error);
        }
    }

    /// <summary>
    /// 課金アイテム情報取得成功時の処理
    /// </summary>
    /// <param name="productList">Product list.</param>
    private void OnProductRequestSucceeded(List<StoreKitProduct> productList)
    {
        if (productReqSucceededEvent != null) {
            productReqSucceededEvent (productList);
        }
    }

    /// <summary>
    /// 課金アイテム情報取得失敗時の処理
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnProductListRequestFailed(string error)
    {
        if (productReqFailedEvent != null) {
            productReqFailedEvent (error);
        }
    }

#elif UNITY_ANDROID

    /// <summary>ユーザによる購入キャンセルを示すエラーコード</summary>
    private const int ERR_CODE_PURCHASE_CANCEL_BY_USER = -1005;
    /// <summary>購入していない課金アイテムの消費リクエストを送った際のエラーメッセージ</summary>
    private const string ERR_MSG_NOT_EXISTS_UNCONSUMED_PURCHASE = "";

    // 購入情報取得時に、消費リクエスト未送信のデータが存在しなかった場合の処理を格納するデリゲート
    public delegate void QueryInventoryNotExistsUnconsumedDelegate ();

    /// <summary>課金可否チェックで可能だった場合に呼び出すイベント</summary>
    private GoogleIABPlugin.BillingSupportedDelegate billingSupportedEvent;
    /// <summary>課金可否チェックで不可能だった場合に呼び出すイベント</summary>
    private GoogleIABPlugin.FailedEventDelegate billingNotSupportedEvent;
    /// <summary>課金アイテム購入状態取得成功時に呼び出すイベント</summary>
    private GoogleIABPlugin.QueryInventorySucceededDelegate queryInventorySucceededEvent;
    /// <summary>課金アイテム購入状態取得失敗時に呼び出すイベント</summary>
    private GoogleIABPlugin.FailedEventDelegate queryInventoryFailedEvent;
    /// <summary>購入処理の完了通知時に呼び出すイベント</summary>
    private GoogleIABPlugin.PurchaseCompleteAwaitingDelegate purchaseCompleteEvent;
    /// <summary>購入成功時に呼び出すイベント</summary>
    private GoogleIABPlugin.PurchaseSucceededDelegate purchaseSucceededEvent;
    /// <summary>購入失敗時に呼び出すイベント</summary>
    private GoogleIABPlugin.PurchaseFailedDelegate purchaseFailedEvent;
    /// <summary>消費リクエスト成功時に呼び出すイベント</summary>
    private GoogleIABPlugin.ConsumePurchaseSucceededDelegate consumeSucceededEvent;
    /// <summary>消費リクエスト失敗時に呼び出すイベント</summary>
    private GoogleIABPlugin.FailedEventDelegate consumeFailedEvent;
    /// <summary>購入情報取得時に、消費リクエスト未送信のデータが存在しなかった場合に呼び出すイベント</summary>
    private QueryInventoryNotExistsUnconsumedDelegate notExistsUnconsumedEvent;

    /// <summary>課金可否フラグ</summary>
    public bool IsSupportedBilling
    {
        get{ return googleIABInstance.IsSupportedBilling; }
    }

    /// <summary>
    /// 課金可否チェックのコールバックを設定
    /// </summary>
    /// <param name="supportedEvent">Supported event.</param>
    /// <param name="notSupportedEvent">Not supported event.</param>
    public void SetBillingSupportedCheckCallback(GoogleIABPlugin.BillingSupportedDelegate supportedEvent, GoogleIABPlugin.FailedEventDelegate notSupportedEvent)
    {
        billingSupportedEvent = supportedEvent;
        billingNotSupportedEvent = notSupportedEvent;
    }

    /// <summary>
    /// 購入情報取得処理のコールバックを設定
    /// </summary>
    /// <param name="succeededEvent">Succeeded event.</param>
    /// <param name="failedEvent">Failed event.</param>
    public void SetQueryInventoryCallback(GoogleIABPlugin.QueryInventorySucceededDelegate succeededEvent, GoogleIABPlugin.FailedEventDelegate failedEvent, QueryInventoryNotExistsUnconsumedDelegate unconsumedEvent)
    {
        queryInventorySucceededEvent = succeededEvent;
        queryInventoryFailedEvent = failedEvent;
        notExistsUnconsumedEvent = unconsumedEvent;
    }

    /// <summary>
    /// 購入処理のコールバックを設定
    /// </summary>
    /// <param name="">.</param>
    /// <param name="failedEvent">Failed event.</param>
    public void SetPurchaseCallback(GoogleIABPlugin.PurchaseSucceededDelegate succeededEvent, GoogleIABPlugin.PurchaseFailedDelegate failedEvent, GoogleIABPlugin.PurchaseCompleteAwaitingDelegate completedEvent)
    {
        purchaseSucceededEvent = succeededEvent;
        purchaseFailedEvent = failedEvent;
        purchaseCompleteEvent = completedEvent;
    }

    /// <summary>
    /// 消費リクエスト送信処理のコールバックを設定
    /// </summary>
    /// <param name="succeededEvent">Succeeded event.</param>
    /// <param name="failedEvent">Failed event.</param>
    public void SetConsumeCallback(GoogleIABPlugin.ConsumePurchaseSucceededDelegate succeededEvent, GoogleIABPlugin.FailedEventDelegate failedEvent)
    {
        consumeSucceededEvent = succeededEvent;
        consumeFailedEvent = failedEvent;
    }

    /// <summary>
    /// アプリケーションのパブリックキーを設定
    /// </summary>
    /// <param name="publicKey">Public key.</param>
    public void SetApplicationPublickKey(string publicKey)
    {
        googleIABInstance.AppPublicKey = publicKey;
    }

    /// <summary>
    /// GoogleIABの初期化を行う
    /// </summary>
    public void InitGoogleIAB()
    {
        googleIABInstance.InitGoogleIAB ();
    }

    /// <summary>
    /// 購入情報を取得
    /// </summary>
    /// <param name="sku">Sku.</param>
    public void QueryInventory(string[] sku)
    {
        GoogleIAB.queryInventory (sku);
    }

    /// <summary>
    /// 課金アイテムの購入
    /// </summary>
    /// <param name="sku">Sku.</param>
    public void PurchaseProduct(string sku)
    {
        targetProductId = sku;

        GoogleIAB.purchaseProduct(targetProductId);
    }

    /// <summary>
    /// 課金アイテムの消費リクエストを送信
    /// </summary>
    /// <param name="sku">Sku.</param>
    public void ConsumeProduct(string sku)
    {
        GoogleIAB.consumeProduct (sku);
    }

    /// <summary>
    /// 自動で課金を行う
    /// </summary>
    /// <param name="sku">Sku.</param>
    public void PurchaseProductAuto(string sku)
    {
        targetProductId = sku;
        isAutoPurchase = true;

        grantCompletedEvent = SendConsumeRequest;

        InitGoogleIAB ();
    }

    /// <summary>
    /// 課金可否チェックで可能だった場合の処理
    /// </summary>
    private void OnSupportedBilling()
    {
        if (billingSupportedEvent != null) {
            billingSupportedEvent ();
        }

        // 自動購入がONの場合、課金開始前APIを呼び出す
        if (isAutoPurchase) {
            PurchaseTransactionInfo transactionInfo = LoadBillingTransactionInfo ();

            if (transactionInfo != null) {
                if (transactionInfo.PurchaseState == ePurchaseTransactionState.PurchaseStart) {
                    // 課金開始APIは呼び出し済みなので未消費の課金アイテムがあるかをチェック
                    QueryInventory (productIdList);
                } else if (transactionInfo.PurchaseState == ePurchaseTransactionState.PurchaseComplete) {
                    // 購入まで完了しているので付与APIを呼び出す
                    StartCoroutine (ExecuteGrantItemApi ());
                } else if (transactionInfo.PurchaseState == ePurchaseTransactionState.GrantComplete) {
                    // 付与まで完了しているので、消費リクエスト未送信かをチェック
                    QueryInventory (new string[]{ transactionInfo.TargetProductId });
                }
            } else {
                // 課金トランザクションが存在しなければ課金開始
                StartCoroutine (ExecuteStartPurchaseApi ());
            }
        }
    }

    /// <summary>
    /// 課金可否チェックで不可能だった場合の処理
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnNotSupportedBilling(string error)
    {
        if (billingNotSupportedEvent != null) {
            billingNotSupportedEvent (error);
        }
    }

    /// <summary>
    /// 購入情報取得成功時の処理
    /// </summary>
    /// <param name="purchases">Purchases.</param>
    /// <param name="skus">Skus.</param>
    private void OnQueryInventorySucceeded(List<GooglePurchase> purchases, List<GoogleSkuInfo> skus)
    {
        if (queryInventorySucceededEvent != null) {
            queryInventorySucceededEvent (purchases, skus);
        }

        // 自動購入がONの場合、過去に途中で止まっていた処理を再開する
        if (isAutoPurchase) {
            // 処理が中断している処理があるかチェック
            if (purchases != null && purchases.Count > 0) {
                // 中断している課金アイテムのプロダクトIDを設定
                // ※課金が中断しているものがあったら先に処理するので複数の中断されたトランザクションは存在しない想定
                targetProductId = purchases [0].productId;
                // 付与APIを呼び出す
                StartCoroutine (ExecuteGrantItemApi ());
            } else {
                // 未消費の購入がないということは、"購入していない" or "付与まで完了している"ため課金トランザクションを削除
                DeleteBillingTransaction ();
                // 消費リクエスト未送信のデータが存在しない場合の処理を行う
                if (notExistsUnconsumedEvent != null) {
                    notExistsUnconsumedEvent ();
                }
            }
        }
    }

    /// <summary>
    /// 購入情報取得失敗時の処理
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnQueryInventoryFailed(string error)
    {
        if (queryInventoryFailedEvent != null) {
            queryInventoryFailedEvent (error);
        }
    }

    /// <summary>
    /// 購入成功時の処理
    /// </summary>
    /// <param name="purchase">Purchase.</param>
    private void OnPurchaseSucceeded(GooglePurchase purchase)
    {
        if (purchaseSucceededEvent != null) {
            purchaseSucceededEvent (purchase);
        }

        // 自動購入がONなら付与APIを呼び出す
        if (isAutoPurchase) {
            StartCoroutine (ExecuteGrantItemApi ());
        }
    }

    /// <summary>
    /// 購入失敗時の処理
    /// </summary>
    /// <param name="error">Error.</param>
    /// <param name="response">Response.</param>
    private void OnPurchaseFailed(string error, int response)
    {
        if (response != ERR_CODE_PURCHASE_CANCEL_BY_USER) {
            // ユーザにより購入がキャンセルされた場合はエラー処理は行わない
            if (purchaseFailedEvent != null) {
                purchaseFailedEvent (error, response);
            }
        }
    }

    /// <summary>
    /// 購入完了時の処理
    /// </summary>
    /// <param name="purchaseData">Purchase data.</param>
    /// <param name="signature">Signature.</param>
    private void OnPurchaseCompleted(string purchaseData, string signature)
    {
        if (purchaseCompleteEvent != null) {
            purchaseCompleteEvent (purchaseData, signature);
        }
    }

    /// <summary>
    /// 消費リクエスト成功時の処理
    /// </summary>
    /// <param name="purchase">Purchase.</param>
    private void OnConsumeSucceeded(GooglePurchase purchase)
    {
        if (consumeSucceededEvent != null) {
            consumeSucceededEvent (purchase);
        }

        DeleteBillingTransaction ();
    }

    /// <summary>
    /// 消費リクエスト失敗時の処理
    /// </summary>
    /// <param name="error">Error.</param>
    private void OnConsumeFailed(string error)
    {
        if (error == ERR_MSG_NOT_EXISTS_UNCONSUMED_PURCHASE) {
            // 消費リクエスト未送信の購入がない場合はエラーとして扱わず課金トランザクションを消す
            DeleteBillingTransaction ();
        } else {
            if (consumeFailedEvent != null) {
                consumeFailedEvent (error);
            }
        }
    }

    /// <summary>
    /// 消費リクエストを送信
    /// </summary>
    private void SendConsumeRequest()
    {
        // todo:utsumi プロダクトIDを指定
        ConsumeProduct (targetProductId);
    }

#endif

}
