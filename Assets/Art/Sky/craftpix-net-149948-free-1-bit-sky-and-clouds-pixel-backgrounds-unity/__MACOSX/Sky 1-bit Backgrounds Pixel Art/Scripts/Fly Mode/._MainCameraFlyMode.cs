using UnityEngine;

namespace Sky1bitBackgroundsPixelArt
{
    [ExecuteInEditMode]
    public class MainCameraFlyMode : MonoBehaviour
    {
        private Transform player;

        public bool smoothCamera = true;

        private void Start()
        {
            player = GameObject.FindWithTag("Player").GetComponent<Transform>();
        }

        private void Update()
        {
            Camera.main.orthographicSize = 5.0f;

            float smoothSpeed = 5.0f;

            Vector3 desiredPosition = new Vector3(player.position.x, transform.position.y, -10f);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            transform.position = smoothCamera ? smoothedPosition : desiredPosition;
        }
    }
}
