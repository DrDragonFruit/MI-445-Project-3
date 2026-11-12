using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform playerTransform;

    // Update is called once per frame
    void Update()
    {
        Vector3 playerCoords = new Vector3(playerTransform.position.x, playerTransform.position.y, -10);
        cameraTransform.position = playerCoords;
    }
}
