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
            GameplayEvents.MachineUnlocked += OnMachineUnlocked;
            UpdateVisual();
        }

        private void OnMachineUnlocked(string id)
        {
            if (_state != null && id == _state.MachineId)
            {
                UpdateVisual();
            }
        }

        private void UpdateVisual()
        {
            if (spriteRenderer == null || _state == null)
            {
                return;
            }

            if (_state.IsUnlocked)
            {
                spriteRenderer.color = Color.white;
            }
            else
            {
                spriteRenderer.color = new Color(0.35f, 0.35f, 0.35f, 0.7f);
            }
        }

        private void OnDisable()
        {
            GameplayEvents.MachineUnlocked -= OnMachineUnlocked;
        }
    }
}
