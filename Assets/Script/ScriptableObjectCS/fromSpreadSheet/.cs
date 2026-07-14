using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "", menuName = "ScriptableObject/")]
public class  : ScriptableObject
{
    [SerializeField] public List<TestData> TestDataList;
}


