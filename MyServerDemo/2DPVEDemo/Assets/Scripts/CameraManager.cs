using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform localPlayer;
    [SerializeField] private float cameraHeight = 2.2f;
    [SerializeField] private float moveSpeed = 3f;
    private float cameraPositionLeft = -15f;
    private float cameraPositionRight = 35f;

    void LateUpdate()
    {
        if (localPlayer == null)
        {
            FindLocalPlayer();
            return;
        }

        if (localPlayer.position.x >= cameraPositionLeft && localPlayer.position.x <= cameraPositionRight)
        {
            MoveCamera(0);
        }
        else if (localPlayer.position.x < cameraPositionLeft)
        {
            MoveCamera(cameraPositionLeft);
        }
        else if (localPlayer.position.x > cameraPositionRight)
        {
            MoveCamera(cameraPositionRight);
        }
    }

    void FindLocalPlayer()
    {
        if (PlayerManager.instance != null)
            localPlayer = PlayerManager.instance.transform;
    }

    private void MoveCamera(float x)
    {
        float targetX = x != 0 ? x : localPlayer.position.x;
        Vector3 cameraPosition = new Vector3(
            targetX,
            cameraHeight,
            localPlayer.position.z - 10f
        );
        transform.position = Vector3.Lerp(transform.position, cameraPosition, moveSpeed * Time.deltaTime);
    }
}
