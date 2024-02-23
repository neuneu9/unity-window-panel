# Unity Window Panel

[English](README.md) | [日本語](README-ja.md) |  

## このパッケージについて

Unity Window Panel は Unity 用の、シンプルかつ拡張可能なウインドウ UI です。

## サポートするバージョン

Unity 2020.3 以降

## インストール方法

Package Manager 経由でインストールできます。  
Unity エディタの Package Manager ウインドウを開き、左上の `+` ボタンから `Add package from git URL...` を選択し、  

```text
https://github.com/neuneu9/unity-window-panel.git?path=Packages/jp.neuneu9.window-panel
```

を入力して `Add` ボタンを押すか、  
`Packages/manifest.json` ファイルを開き、`dependencies` フィールドに次のような項目を追加してください。  

```json
"jp.neuneu9.window-panel": "https://github.com/neuneu9/unity-window-panel.git?path=Packages/jp.neuneu9.window-panel",
```

## 使い方

3 種類のコンポーネントがデフォルトで使えます。  

- `Fade Window Panel`: フェードタイプ  
  ![fade](https://github.com/neuneu9/unity-window-panel/blob/images/fade.gif)  
- `Slide In Window Panel`: スライドインタイプ  
  ![slidein](https://github.com/neuneu9/unity-window-panel/blob/images/slidein.gif)  
- `Animated Window Panel`: アニメーションタイプ  
  ![animated](https://github.com/neuneu9/unity-window-panel/blob/images/animated.gif)  

### 作成方法

メニューの `GameObject -> UI -> Window Panel -> *** Window Panel` を選択すると構築済みのゲームオブジェクトが生成されます。  

### 初期状態の変更

インスペクターの `Default To Open` を押すと開いた状態、`Default To Close` を押すと閉じた状態になります。  

### インスペクター項目

| 項目名 | 説明 |
| - | - |
| Window | ウインドウオブジェクトへの参照 |
| Background | 背面オブジェクトへの参照 |
| Skip Opening On Clicked | 開く途中でクリックされたら即開き終えるかどうか |
| Close On Background Clicked | 背面をクリックされたら閉じるかどうか |
| Open Duration | 開くトランジションの時間 [s] |
| Close Duration | 閉じるトランジションの時間 [s] |
| On Pre Open | 開くトランジションを開始する直前に呼ばれる |
| On Opened | 開き終えた直後に呼ばれる |
| On Pre Close | 閉じるトランジションを開始する直前に呼ばれる |
| On Closed | 閉じ終えた直後に呼ばれる |

### スクリプティング API

| 関数名 | 説明 |
| - | - |
| `Open()`<br>`Open(UnityAction onCompleted)` | ウインドウパネルを開きます。すでに開いている・開こうとしている場合は何もしません。<br>`onCompleted`: 完了コールバック |
| `Close()`<br>`Close(UnityAction onCompleted)` | ウインドウパネルを閉じます。すでに閉じている・閉じようとしている場合は何もしません。<br>`onCompleted`: 完了コールバック |
| `OpenImmediately()` | ウインドウパネルを瞬時に開きます。すでに開いている場合は何もしません。 |
| `CloseImmediately()` | ウインドウパネルを瞬時に閉じます。すでに閉じている場合は何もしません。 |
| `GetWindow<TWindow>()` | ウインドウオブジェクトのコンポーネントを取得します。<br>`TWindow`: 取得するコンポーネントの型 |

### Animated Window Panel のアニメーションをカスタマイズ

1. 開くアニメーションおよび閉じるアニメーションの Animation Clip を作成します。  
    ※クリップの長さは任意ですが、Animated Window Panel は 1 [s] に正規化して扱います。  
開閉トランジションの長さの変更はインスペクタの `Open Duration` および `Close Duration` でおこなってください。  
1. `Packages/jp.neuneu9.window-panel/Animations/WindowPanel.controller` をベースにした Animator Override Controller を作成します。  
1. 作成した Animator Override Controller の `window_panel_open` に開くアニメーションを、`window_panel_close` に閉じるアニメーションをセットしてください。  

## 拡張

自分で任意の動きを実装したい場合は、次の手順で簡単に拡張できます。  

1. `neuneu9.WindowPanel` を継承したクラスを作成します。  
1. 次の 2 つのメソッドをオーバーライドしてください。  
  引数の `progress` は 0 - 1 のトランジション割合です。  
    - `void OpenAction(float progress)`: 開くトランジション処理を記述します。  
    - `void CloseAction(float progress)`: 閉じるトランジション処理を記述します。  
1. 作成したクラススクリプトを Fade Window Panel などのスクリプトの代わりに使用します。  

例として、[DOTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676) を使ってポップアップスタイルのウインドウを実装するサンプルを掲載しておきます。  

![popup](https://github.com/neuneu9/unity-window-panel/blob/images/popup.gif)  

```PopUpWindowPanel.cs
using UnityEngine;
using DG.Tweening;
using neuneu9.WindowPanel;

public class PopUpWindowPanel : WindowPanel
{
    [SerializeField]
    private Ease _openEase = Ease.OutBack;

    [SerializeField]
    private Ease _closeEase = Ease.InQuart;

    [SerializeField]
    private Vector3 _scaleOnClosed = new Vector3(0.6f, 0.6f, 1f);


    protected override void OpenAction(float progress)
    {
        _window.alpha = Mathf.Lerp(0f, 1f, progress * 2f);
        _window.transform.localScale = Vector3.LerpUnclamped(_scaleOnClosed, Vector3.one, DOVirtual.EasedValue(0f, 1f, progress, _openEase));
    }

    protected override void CloseAction(float progress)
    {
        _window.alpha = Mathf.Lerp(1f, 0f, (progress - 0.5f) * 2f);
        _window.transform.localScale = Vector3.LerpUnclamped(Vector3.one, _scaleOnClosed, DOVirtual.EasedValue(0f, 1f, progress, _closeEase));
    }
}
```
