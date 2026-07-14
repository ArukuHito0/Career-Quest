using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BlockUI : MonoBehaviour
{
    public MapEditor editor;
    public GameObject buttonPrefab;
    public Transform container;

    void Start()
    {
        for (int i = 0; i < editor.blockLibrary.Count; i++)
        {
            int index = i;
            GameObject btn = Instantiate(buttonPrefab, container);

            // UI上のボタン画像の更新
#if UNITY_EDITOR
            Texture2D tex = AssetPreview.GetAssetPreview(editor.blockLibrary[i].surfacePrefab);
            if (tex != null)
            {
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                btn.GetComponent<Image>().sprite = sprite;
            }
#endif
            btn.GetComponent<Button>().onClick.AddListener(() => editor.currentBlockIndex = index);
        }
    }
}