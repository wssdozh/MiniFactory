using UnityEngine;

namespace MiniFactory.Core
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class CameraResolutionFitter : MonoBehaviour
    {
        [SerializeField] private float targetAspectWidth = 9f;
        [SerializeField] private float targetAspectHeight = 16f;
        [SerializeField] private float baseOrthographicSize = 8f;

        private Camera _camera;
        private int _lastWidth;
        private int _lastHeight;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            UpdateViewport(Screen.width, Screen.height);
        }

        private void Start()
        {
            UpdateViewport(Screen.width, Screen.height);
        }

        private void Update()
        {
            if (Screen.width != _lastWidth || Screen.height != _lastHeight)
            {
                UpdateViewport(Screen.width, Screen.height);
            }
        }

        public void UpdateViewport(int width, int height)
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
                if (_camera == null)
                {
                    return;
                }
            }

            _lastWidth = width;
            _lastHeight = height;

            if (_lastWidth <= 0 || _lastHeight <= 0)
            {
                return;
            }

            float targetAspect = targetAspectWidth / targetAspectHeight;
            float currentAspect = (float)_lastWidth / _lastHeight;
            float variance = currentAspect / targetAspect;

            if (variance < 1.0f)
            {
                // Taller than 9:16 (tall phones like 19.5:9, 20:9)
                // Fullscreen viewport, adapt orthographic size to preserve horizontal framing
                _camera.rect = new Rect(0f, 0f, 1f, 1f);
                _camera.orthographicSize = baseOrthographicSize / variance;
            }
            else
            {
                // Wider than 9:16 (Free Aspect, 16:9, landscape, tablets)
                // Pillarbox to maintain exact mobile portrait aspect ratio
                float insetWidth = 1.0f / variance;
                float insetX = (1.0f - insetWidth) / 2.0f;
                _camera.rect = new Rect(insetX, 0f, insetWidth, 1f);
                _camera.orthographicSize = baseOrthographicSize;
            }
        }
    }
}
