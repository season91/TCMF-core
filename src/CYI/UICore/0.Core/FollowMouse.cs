using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    private FollowMouse instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Camera.main != null)
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = 0f;
            
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            worldPosition.z = transform.position.z;

            transform.position = worldPosition;
        }
    }
}
