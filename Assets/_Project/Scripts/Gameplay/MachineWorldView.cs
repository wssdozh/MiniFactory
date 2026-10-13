using UnityEngine;

namespace MiniFactory.Gameplay
{
    public class MachineWorldView : MonoBehaviour
    {
        [SerializeField] private string machineId;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private MachineState _state;

        public void Initialize(MachineState state)
        {
            _state = state;
            if (_state != null)
            {
                _state.Changed += UpdateVisual;
            }
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null || _state == null)
            {
                return;
            }

            spriteRenderer.color = _state.IsUnlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.7f);
        }

        private void OnDisable()
        {
            if (_state != null)
            {
                _state.Changed -= UpdateVisual;
            }
        }
    }
}
