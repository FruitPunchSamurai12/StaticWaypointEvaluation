using UnityEngine;

public class InteractBox:MonoBehaviour
{
    [SerializeField] Transform center;
    [SerializeField] Vector3 halfExtends;
    [SerializeField] LayerMask interactMask;
    I_Interactable interactableInFront;

    public void LookForInteractables()
    {
        Collider[] interactable = Physics.OverlapBox(center.position, halfExtends, Quaternion.identity, interactMask);
        if (interactable.Length > 0)
        {
            Debug.Log(interactable[0].name);
            interactableInFront = interactable[0].GetComponent<I_Interactable>();
        }
        else
        {
            interactableInFront = null;
        }
    }

    public void Interact(Player player)
    {
        if (interactableInFront != null)
        {
            interactableInFront.Interact(player);
        }
        else
        {
            Debug.Log("no object in front");
        }
    }

    private void OnDrawGizmos()
    {
        if (center == null) return;
        Gizmos.DrawWireCube(center.position, halfExtends * 2f);
    }

}

