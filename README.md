# Unity Window Panel

[English](README.md) | [日本語](README-ja.md) |  

## What’s this?

'Unity Window Panel' is simple and extensible window UI component for Unity.  

## Supported Unity versions

Unity 2020.3 or higher.  

## Installation

Via Package Manager.  
Open the Package Manager window in your Unity editor, select `Add package from git URL...` from the `+` button in the top left, enter following and press the `Add` button.  

```text
https://github.com/neuneu9/unity-window-panel.git?path=Packages/jp.neuneu9.window-panel
```

Or open the `Packages/manifest.json` file and add an item like the following to the `dependencies` field.  

```json
"jp.neuneu9.window-panel": "https://github.com/neuneu9/unity-window-panel.git?path=Packages/jp.neuneu9.window-panel",
```

## How to use

3 components are available by default:  

- `Fade Window Panel`: Fade style  
  ![fade](https://github.com/neuneu9/unity-window-panel/blob/images/fade.gif)  
- `Slide In Window Panel`: Slide-in style  
  ![slidein](https://github.com/neuneu9/unity-window-panel/blob/images/slidein.gif)  
- `Animated Window Panel`: Animation style  
  ![animated](https://github.com/neuneu9/unity-window-panel/blob/images/animated.gif)  

### Creation

Select `GameObject -> UI -> Window Panel -> *** Window Panel` in the menu to create a constructed GameObject.  

### Set initial open/close

Press `Default To Open` to set it open, and press `Default To Close` to set it close in the inspector.  

### Inspector exposed parameters

| Name | Explanation |
| - | - |
| Window | Reference to window object |
| Background | Reference to background object |
| Skip Opening On Clicked | Whether to complete the transition immediately if clicked while open transitioning |
| Close On Background Clicked | Whether to close if the background is clicked |
| Open Duration | Open transition time [s] |
| Close Duration | Close transition time [s] |
| On Pre Open | Called before starting the open transition |
| On Opened | Called after opened |
| On Pre Close | Called before starting the close transition |
| On Closed | Called after closed |

### Scripting APIs

| Method name | Explanation |
| - | - |
| `Open()`<br>`Open(UnityAction onCompleted)` | Open the window panel. If it's already open or about to be opened, nothing will be done.<br>`onCompleted`: Completion callback |
| `Close()`<br>`Close(UnityAction onCompleted)` | Close the window panel. If it's already closed or about to close, nothing will be done.<br>`onCompleted`: Completion callback |
| `OpenImmediately()` | Open the window panel instantly. If it's already open, nothing will be done. |
| `CloseImmediately()` | Close the window panel instantly. If it's already closed, nothing will be done. |
| `GetWindow<TWindow>()` | Get a component of the window object.<br>`TWindow`: Type of component to get |

### Customize animations of Animated Window Panel

1. Create open and close animation clips  
NOTE: The clip length is arbitrary, but Animated Window Panel normalizes it to 1 [s].  
To set the length of the open/close transition, use `Open Duration` and `Close Duration` in the inspector.  
1. Create an Animator Override Controller based on `Packages/jp.neuneu9.window-panel/Animations/WindowPanel.controller`.  
1. Set the open animation clip to `window_panel_open` and the close animation clip to `window_panel_close`.  

## Extension

Want to use your behavior? It’s easy.  

1. Create a class that inherits from `neuneu9.WindowPanel`.  
1. Override the following two methods:  
  (The argument `progress` is a transition rate between 0 and 1.)  
    - `void OpenAction(float progress)`: Code the open transition processing.  
    - `void CloseAction(float progress)`: Code the close transition processing.  
1. Use the class script instead of scripts such as Fade Window Panel.  

Below is an example implementation of a pop-up styled window using [DOTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676).  

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
