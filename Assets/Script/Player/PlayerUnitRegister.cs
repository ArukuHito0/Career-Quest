using UnityEngine;


// PlayerUnit‚ðŽ©“®“I‚ÉRegistry‚Ö“o˜^‚·‚é
public class PlayerUnitRegister : MonoBehaviour
{
    private void OnEnable()
    {
        PlayerUnitRegistry.Register(gameObject);
    }


    private void OnDisable()
    {
        PlayerUnitRegistry.Unregister(gameObject);
    }
}
