using UnityEngine;

namespace neuneu9.WindowPanel
{
    /// <summary>
    /// アニメーター演出ウインドウパネル
    /// </summary>
    public class AnimatedWindowPanel : WindowPanel
    {
        [SerializeField]
        private Animator _animator = null;

        [SerializeField]
        private string _openStateName = "Open";

        [SerializeField]
        private string _closeStateName = "Close";


        private int _layerIndex = 0;


        protected override void Awake()
        {
            base.Awake();

            if (_state == State.Opened)
            {
                _animator.CrossFadeInFixedTime(_openStateName, 0f, _layerIndex, 1f);
            }
        }


        protected override void OpenAction(float progress)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(_layerIndex);
            if (!stateInfo.IsName(_openStateName))
            {
                _animator.Play(_openStateName, _layerIndex, 0f);
            }
            else
            {
                _animator.Update((progress - stateInfo.normalizedTime) * stateInfo.length);
            }
        }

        protected override void CloseAction(float progress)
        {
            var stateInfo = _animator.GetCurrentAnimatorStateInfo(_layerIndex);
            if (!stateInfo.IsName(_closeStateName))
            {
                _animator.Play(_closeStateName, _layerIndex, 0f);
            }
            else
            {
                _animator.Update((progress - stateInfo.normalizedTime) * stateInfo.length);
            }
        }
    }
}
