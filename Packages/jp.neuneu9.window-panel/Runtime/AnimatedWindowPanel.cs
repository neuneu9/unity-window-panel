using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

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
                JumpTo(_openStateName, _layerIndex, 1f);
            }
            if (_state == State.Closed)
            {
                JumpTo(_closeStateName, _layerIndex, 1f);
            }
        }

        private void JumpTo(string stateName, int layerIndex, float progress)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var controller = _animator.runtimeAnimatorController as AnimatorController;
                var state = controller.layers[layerIndex].stateMachine.states.SingleOrDefault(x => x.state.name.Equals(stateName));
                var clip = state.state.motion as AnimationClip;
                clip.SampleAnimation(_animator.gameObject, clip.length * progress);
                return;
            }
#endif
            _animator.CrossFade(stateName, 0f, layerIndex, progress);
        }

        protected override void OpenAction(float progress)
        {
            if (!Application.isPlaying)
            {
                JumpTo(_openStateName, _layerIndex, progress);
                return;
            }

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
            if (!Application.isPlaying)
            {
                JumpTo(_closeStateName, _layerIndex, progress);
                return;
            }

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
