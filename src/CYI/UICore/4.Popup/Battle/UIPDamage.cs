using UnityEngine;

public class UIPDamage : MonoBehaviour
{
     private UIDynamicObjectPool<UIWgDamage> dynamicDamagePool;
     [SerializeField] private Transform damageRoot;
     [SerializeField] private UIWgDamage originDamage;
     [SerializeField] private Canvas parentCanvas;

     private static readonly Vector2 BoundMin = new (-50, -100);
     private static readonly Vector2 BoundMax = new (50, 100);
     private const string MissText = "MISS";
     
     private void Reset()
     {
         damageRoot = transform.FindChildByName<Transform>("Group_Damage");
         originDamage = transform.FindChildByName<UIWgDamage>("GUI_Damage");
         parentCanvas = damageRoot.GetComponentInParent<Canvas>();
     }

     public void Initialize()
     {
         dynamicDamagePool = new UIDynamicObjectPool<UIWgDamage>(originDamage, damageRoot, 5);
         
         BattleManager.Instance.UI.OnShowDamage -= PopupDamage;
         BattleManager.Instance.UI.OnShowDamage += PopupDamage;
     }

     private void PopupDamage(int damage, Vector2 worldPos)
     {
         var uiDmg = dynamicDamagePool.Get();
         uiDmg.RegisterPool(dynamicDamagePool);
         string damageText = damage == -1 ? MissText : damage.ToString();
         
         // 위치 값 Screen상 위치로 변경
         RectTransformUtility.ScreenPointToLocalPointInRectangle(
             parentCanvas.transform as RectTransform,
             UIManager.Instance.MainCamera.WorldToScreenPoint(worldPos),
             parentCanvas.worldCamera,
             out var anchoredPos
         );
         
         // 랜덤 위치 포지션
         anchoredPos += new Vector2(
             Random.Range(BoundMin.x, BoundMax.y),
             Random.Range(BoundMin.x, BoundMax.y)
         );

         uiDmg.Show(damageText, anchoredPos);
     }
}