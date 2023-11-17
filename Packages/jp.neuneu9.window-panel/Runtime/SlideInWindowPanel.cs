using UnityEngine;

namespace neuneu9.WindowPanel
{
    /// <summary>
    /// 画面外からスライドインするウインドウパネル
    /// </summary>
    public class SlideInWindowPanel : WindowPanel
    {
        /// <summary>
        /// 画面外に収納されるときの配置タイプ種類
        /// </summary>
        private enum OuterType
        {
            Up,
            Down,
            Left,
            Right,
        }

        /// <summary>
        /// 入ってくる方向
        /// </summary>
        [SerializeField]
        private OuterType _entryOuter = OuterType.Right;

        /// <summary>
        /// 出ていく方向
        /// </summary>
        [SerializeField]
        private OuterType _exitOuter = OuterType.Right;

        /// <summary>
        /// イージング曲線
        /// </summary>
        [SerializeField]
        private AnimationCurve _easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private RectTransform _rectTransform = null;
        private RectTransform _windowRectTransform = null;

        /// <summary>
        /// 画面外に収納されるときのアンカーポジションを取得
        /// </summary>
        private Vector2 GetOuterAnchoredPosition(OuterType outerType)
        {
            switch (outerType)
            {
                case OuterType.Up:
                    return new Vector2(0f, _rectTransform.rect.height);
                case OuterType.Down:
                    return new Vector2(0f, -_rectTransform.rect.height);
                case OuterType.Left:
                    return new Vector2(-_rectTransform.rect.width, 0f);
                case OuterType.Right:
                    return new Vector2(_rectTransform.rect.width, 0f);
                default:
                    return Vector2.zero;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _rectTransform = GetComponent<RectTransform>();
            _windowRectTransform = _window.transform as RectTransform;
        }

        protected override void OpenAction(float progress)
        {
            Vector2 startPosition = GetOuterAnchoredPosition(_entryOuter);
            _windowRectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, Vector2.zero, _easingCurve.Evaluate(progress));
        }

        protected override void CloseAction(float progress)
        {
            Vector2 endPosition = GetOuterAnchoredPosition(_exitOuter);
            _windowRectTransform.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, endPosition, _easingCurve.Evaluate(progress));
        }
    }
}
