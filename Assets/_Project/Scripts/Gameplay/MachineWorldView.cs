using UnityEngine;

namespace MiniFactory.Gameplay
{
    public class MachineWorldView : MonoBehaviour
    {
        [SerializeField] private string machineId;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private MachineState _state;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        public void Initialize(MachineState state)
        {
            _state = state;
            if (_state != null)
            {
                _state.Changed += UpdateVisual;
            }
            UpdateVisual();
        }

        private void Update()
        {
            if (_state == null || _state.IsUnlocked == false)
            {
                if (transform.localScale != _baseScale)
                {
                    transform.localScale = _baseScale;
                }
                return;
            }

            float speed = 4f + _state.Level * 0.3f;
            float amplitude = Mathf.Min(0.04f + _state.Level * 0.012f, 0.14f);
            float bounce = Mathf.Sin(Time.time * speed) * amplitude;
            transform.localScale = new Vector3(_baseScale.x, _baseScale.y * (1f + bounce), _baseScale.z);
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

            transform.localScale = _baseScale;
        }
    }
}
