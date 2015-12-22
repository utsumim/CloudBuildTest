using UnityEngine;
using System.Collections;

public class SingletonMonoBehaviour<T> : MonoBehaviour {

    private static T instance;

    /// <summary>
    /// シングルトンのインスタンスを取得 ※インスタンスの参照設定はAwakeで行われるためAwakeメソッドでのインスタンスの参照は危険
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get{ return instance; }
    }

    /// <summary>
    /// 初期化
    /// </summary>
    protected virtual void Init()
    {
        // 継承先のクラスでoverrideして実装
    }

    private void Awake()
    {
        SetInstance ();
        Init ();

        // シーン遷移時にオブジェクトが破棄されないようにする
        DontDestroyOnLoad (this);
    }

    /// <summary>
    /// シングルトンのインスタンスを設定
    /// </summary>
    private void SetInstance()
    {
        instance = gameObject.GetComponent<T> ();
    }

}
