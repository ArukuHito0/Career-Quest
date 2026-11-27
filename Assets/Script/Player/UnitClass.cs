using UnityEngine;

public class UnitClass : MonoBehaviour
{
    public enum ClassType
    {
        Fighter,
        Tank,
        Shooter,
    }

    [SerializeField]
    private ClassType unitClass;

    public ClassType CurrentClass => unitClass;
}