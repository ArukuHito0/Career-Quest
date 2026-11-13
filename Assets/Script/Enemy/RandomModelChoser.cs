using UnityEngine;

namespace CareerQuest.Enemy
{
    [DisallowMultipleComponent]
    public sealed class RandomModelChoser : MonoBehaviour
    {
        [SerializeField] GameObject[] _models;
        GameObject _activeModel;

        void OnEnable()
        {
            if (_models == null || _models.Length == 0) return;

            for (int i = 0; i < _models.Length; i++)
            {
                if (_models[i] != null)
                {
                    _models[i].SetActive(false);
                }
            }

            int randomI = Random.Range(0, _models.Length);
            _activeModel = _models[randomI];

            if (_activeModel != null)
            {
                _activeModel.SetActive(true);
            }
        }
    }
}