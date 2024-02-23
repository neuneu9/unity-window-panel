using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace neuneu9.WindowPanel
{
    /// <summary>
    /// ウインドウ付きパネルのベースクラス
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class WindowPanel : MonoBehaviour
    {
        /// <summary>
        /// 状態定義
        /// </summary>
        protected enum State
        {
            /// <summary>
            /// 閉じている
            /// </summary>
            Closed,
            /// <summary>
            /// 閉じ→開き移行中
            /// </summary>
            Opening,
            /// <summary>
            /// 開いている
            /// </summary>
            Opened,
            /// <summary>
            /// 開き→閉じ移行中
            /// </summary>
            Closing,
        }


        [SerializeField, HideInInspector]
        protected State _state = State.Opened;

        /// <summary>
        /// ウインドウ
        /// </summary>
        [SerializeField]
        protected CanvasGroup _window = null;

        /// <summary>
        /// 背面パネル
        /// </summary>
        [SerializeField]
        protected CanvasGroup _background = null;

        /// <summary>
        /// 開き演出中にクリックで即開くかどうか
        /// </summary>
        [SerializeField]
        protected bool _skipOpeningOnClicked = false;

        /// <summary>
        /// 背面パネルをクリックで閉じるかどうか
        /// </summary>
        [SerializeField]
        protected bool _closeOnBackgroundClicked = false;

        /// <summary>
        /// 開く演出にかける時間 [s]
        /// </summary>
        [SerializeField]
        private float _openDuration = 0.4f;

        /// <summary>
        /// 閉じる演出にかける時間 [s]
        /// </summary>
        [SerializeField]
        private float _closeDuration = 0.4f;

        [SerializeField]
        private UnityEvent _onPreOpen = new UnityEvent();

        [SerializeField]
        private UnityEvent _onOpened = new UnityEvent();

        [SerializeField]
        private UnityEvent _onPreClose = new UnityEvent();

        [SerializeField]
        private UnityEvent _onClosed = new UnityEvent();

        /// <summary>
        /// パネルを開く前の処理
        /// </summary>
        public UnityEvent OnPreOpen => _onPreOpen;

        /// <summary>
        /// パネルを開いた後の処理
        /// </summary>
        public UnityEvent OnOpened => _onOpened;

        /// <summary>
        /// パネルを閉じる前の処理
        /// </summary>
        public UnityEvent OnPreClose => _onPreClose;

        /// <summary>
        /// パネルを閉じた後の処理
        /// </summary>
        public UnityEvent OnClosed => _onClosed;

        private Coroutine _process = null;
        private CanvasGroup _canvasGroup = null;


        /// <summary>
        /// 開く動作定義
        /// </summary>
        /// <returns></returns>
        protected abstract void OpenAction(float progress);

        /// <summary>
        /// 閉じる動作定義
        /// </summary>
        /// <returns></returns>
        protected abstract void CloseAction(float progress);


        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            switch (_state)
            {
                case State.Closed:
                case State.Closing:
                    CompleteClose();
                    break;

                case State.Opened:
                case State.Opening:
                    CompleteOpen();
                    break;

                default:
                    throw new IndexOutOfRangeException();
            }

            // 背面クリックイベントの登録
            if (Application.isPlaying)
            {
                EventTrigger.Entry backgroundEntry = new EventTrigger.Entry {eventID = EventTriggerType.PointerClick};
                backgroundEntry.callback.AddListener((_) =>
                {
                    if (_closeOnBackgroundClicked && _state == State.Opened)
                    {
                        Close();
                    }
                    if (_skipOpeningOnClicked && _state == State.Opening)
                    {
                        if (_process != null)
                        {
                            StopCoroutine(_process);
                            _process = null;
                        }
                        OpenImmediately();
                    }
                });

                EventTrigger backgroundEventTrigger = _background.gameObject.AddComponent<EventTrigger>();
                backgroundEventTrigger.triggers.Add(backgroundEntry);
            }
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
        }


        /// <summary>
        /// 開く
        /// </summary>
        public void Open()
        {
            Open(null);
        }

        /// <summary>
        /// 閉じる
        /// </summary>
        public void Close()
        {
            Close(null);
        }

        /// <summary>
        /// 開く
        /// </summary>
        /// <param name="onCompleted">完了コールバック</param>
        public void Open(UnityAction onCompleted)
        {
            if (_state == State.Opening || _state == State.Opened)
            {
                Debug.LogWarning("Already open or about to open.");
                return;
            }

            if (_process != null)
            {
                StopCoroutine(_process);
            }

            _process = StartCoroutine(OpenAsync(onCompleted));
        }

        public IEnumerator OpenAsync(UnityAction onCompleted = null)
        {
            if (_state == State.Opening || _state == State.Opened)
            {
                Debug.LogWarning("Already open or about to open.");
                yield break;
            }

            _state = State.Opening;
            _onPreOpen.Invoke();

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _window.blocksRaycasts = false;

            yield return DoOpenAction();

            CompleteOpen();

            onCompleted?.Invoke();
        }

        /// <summary>
        /// 閉じる
        /// </summary>
        /// <param name="onCompleted">完了コールバック</param>
        public void Close(UnityAction onCompleted)
        {
            if (_state == State.Closing || _state == State.Closed)
            {
                Debug.LogWarning("Already close or about to close.");
                return;
            }

            if (_process != null)
            {
                StopCoroutine(_process);
            }

            _process = StartCoroutine(CloseAsync(onCompleted));
        }

        public IEnumerator CloseAsync(UnityAction onCompleted = null)
        {
            if (_state == State.Closing || _state == State.Closed)
            {
                Debug.LogWarning("Already close or about to close.");
                yield break;
            }

            _state = State.Closing;
            _onPreClose.Invoke();

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _window.blocksRaycasts = false;

            yield return DoCloseAction();

            CompleteClose();

            onCompleted?.Invoke();
        }

        /// <summary>
        /// ウインドウのコンポーネントを取得
        /// </summary>
        /// <typeparam name="TWindow"></typeparam>
        /// <returns></returns>
        public TWindow GetWindow<TWindow>() where TWindow : Component
        {
            return _window.GetComponent<TWindow>();
        }

        /// <summary>
        /// 即開く
        /// </summary>
        public void OpenImmediately()
        {
            if (_state == State.Opened)
            {
                return;
            }

            if (_state != State.Opening)
            {
                _onPreOpen.Invoke();
            }

            if (_process != null)
            {
                StopCoroutine(_process);
                _process = null;
            }

            OpenAction(1f);

            CompleteOpen();
        }

        /// <summary>
        /// 即閉じる
        /// </summary>
        public void CloseImmediately()
        {
            if (_state == State.Closed)
            {
                return;
            }

            if (_state != State.Closing)
            {
                _onPreClose.Invoke();
            }

            if (_process != null)
            {
                StopCoroutine(_process);
                _process = null;
            }

            CloseAction(1f);

            CompleteClose();
        }

        private IEnumerator DoOpenAction()
        {
            OpenAction(0f);
            _background.alpha = 0f;
            yield return null;

            float startTime = Time.unscaledTime;
            while (Time.unscaledTime < startTime + _openDuration)
            {
                float rate = Mathf.Clamp01((Time.unscaledTime - startTime) / _openDuration);
                OpenAction(rate);
                _background.alpha = rate;
                yield return null;
            }

            OpenAction(1f);
            _background.alpha = 1f;
            yield return null;
        }

        private IEnumerator DoCloseAction()
        {
            CloseAction(0f);
            _background.alpha = 1f;
            yield return null;

            float startTime = Time.unscaledTime;
            while (Time.unscaledTime < startTime + _closeDuration)
            {
                float rate = Mathf.Clamp01((Time.unscaledTime - startTime) / _closeDuration);
                CloseAction(rate);
                _background.alpha = 1f - rate;
                yield return null;
            }

            CloseAction(1f);
            _background.alpha = 0f;
            yield return null;
        }

        private void CompleteOpen()
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            _background.alpha = 1f;

            _window.blocksRaycasts = true;

            _state = State.Opened;
            _onOpened.Invoke();
        }

        private void CompleteClose()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            _background.alpha = 0f;

            _window.blocksRaycasts = false;

            _state = State.Closed;
            _onClosed.Invoke();
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(WindowPanel), true)]
        public class WindowPanelEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                var self = target as WindowPanel;

                EditorGUILayout.BeginHorizontal();

                bool isReady = self._background != null && self._window != null;
                using (new EditorGUI.DisabledScope(!isReady))
                {
                    if (EditorApplication.isPlaying)
                    {
                        EditorGUI.BeginDisabledGroup(self._state == State.Opening || self._state == State.Opened);
                        if (GUILayout.Button("Open"))
                        {
                            self.Open();
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUI.BeginDisabledGroup(self._state == State.Closing || self._state == State.Closed);
                        if (GUILayout.Button("Close"))
                        {
                            self.Close();
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUI.BeginDisabledGroup(self._state == State.Opened);
                        if (GUILayout.Button("Default To Open"))
                        {
                            Undo.RegisterFullObjectHierarchyUndo(self.gameObject, "Open " + self.name);

                            self.Awake();
                            self.OpenImmediately();
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUI.BeginDisabledGroup(self._state == State.Closed);
                        if (GUILayout.Button("Default To Close"))
                        {
                            Undo.RegisterFullObjectHierarchyUndo(self.gameObject, "Close " + self.name);

                            self.Awake();
                            self.CloseImmediately();
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (!isReady)
                {
                    EditorGUILayout.HelpBox($"{ObjectNames.NicifyVariableName(nameof(self._background))} and {ObjectNames.NicifyVariableName(nameof(self._window))} is required.", MessageType.Warning);
                }

                base.OnInspectorGUI();
            }
        }

        [MenuItem("GameObject/UI/Window Panel/Fade Window Panel", false, 30001)]
        public static void AddFadeWindowPanel(MenuCommand menuCommand)
        {
            AddWindowPanel(menuCommand, "Packages/jp.neuneu9.window-panel/Prefabs/FadeWindowPanel.prefab", "FadeWindowPanel");
        }

        [MenuItem("GameObject/UI/Window Panel/Animated Window Panel", false, 30002)]
        public static void AddAnimatedWindowPanel(MenuCommand menuCommand)
        {
            AddWindowPanel(menuCommand, "Packages/jp.neuneu9.window-panel/Prefabs/AnimatedWindowPanel.prefab", "AnimatedWindowPanel");
        }

        [MenuItem("GameObject/UI/Window Panel/Slide In Window Panel", false, 30003)]
        public static void AddSlideInWindowPanel(MenuCommand menuCommand)
        {
            AddWindowPanel(menuCommand, "Packages/jp.neuneu9.window-panel/Prefabs/SlideInWindowPanel.prefab", "SlideInWindowPanel");
        }

        private static void AddWindowPanel(MenuCommand menuCommand, string prefabPath, string instanceName)
        {
            Type type = Type.GetType("UnityEditor.UI.MenuOptions, UnityEditor.UI");
            MethodInfo methodInfo = type.GetMethod("PlaceUIElementRoot", BindingFlags.NonPublic | BindingFlags.Static);

            GameObject prefab = (GameObject) AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
            GameObject instance = Instantiate(prefab);
            instance.name = instanceName;

            methodInfo.Invoke(null, new object[] {instance, menuCommand});

            RectTransform rectTransform = instance.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }
#endif
    }
}
