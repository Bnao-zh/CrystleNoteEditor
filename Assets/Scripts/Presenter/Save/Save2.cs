using NoteEditor.Model;
using UnityEngine;
using UnityEngine.UI;

public class Save2 : MonoBehaviour
    {
        [SerializeField]
        Button saveButton = default;
        void Start()
    {
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(Save);
        }
    }
        public void Save()
        {
            var json = EditDataSerializer.Serialize();
            Debug.Log("数据：" + json);
        }
    }

