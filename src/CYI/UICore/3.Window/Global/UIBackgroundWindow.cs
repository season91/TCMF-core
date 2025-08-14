using UnityEngine;

public class UIBackgroundWindow : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRdr;

    private void Reset()
    {
        spriteRdr = GetComponentInChildren<SpriteRenderer>();
    }

    public void ChangeBg(string bgAdr)
    {
        // 해당 이름에 맞는 BG
        spriteRdr.sprite = ResourceManager.Instance.GetResource<Sprite>(bgAdr);
    }
}
