using NoteEditor.Notes;
using NoteEditor.Model;
using NoteEditor.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace NoteEditor.Presenter
{
    public class ToggleEditTypePresenter : MonoBehaviour
    {
        // 序列化字段，编辑类型切换按钮
        [SerializeField]
        Button editTypeToggleButton = default;
        // 序列化字段，长音符图标
        [SerializeField]
        Sprite iconLongNotes = default;
        // 序列化字段，单音符图标
        [SerializeField]
        Sprite iconSingleNotes = default;
        [SerializeField]
        Sprite iconDragNotes = default;
        [SerializeField]
        Sprite iconDraglineNotes = default;
        [SerializeField]
        Sprite iconFlickNotes = default;
        // 序列化字段，长音符状态按钮颜色
        [SerializeField]
        Color longTypeStateButtonColor = default;
        // 序列化字段，单音符状态按钮颜色
        [SerializeField]
        Color singleTypeStateButtonColor = default;
        [SerializeField]
        Color dragTypeStateButtonColor = default;
        [SerializeField]
        Color draglineTypeStateButtonColor = default;
        [SerializeField]
        Color flickTypeStateButtonColor = default;

        void Awake()
        {
            // 当编辑类型切换按钮被点击或Alt键被按下时，切换音符类型
            editTypeToggleButton.OnClickAsObservable()
                // 将UpdateAsObservable()和KeyInput.AltKeyDown()合并，当Alt键按下时，选择NoteTypes.Long或NoteTypes.Single
                .Merge(this.UpdateAsObservable().Where(_ => KeyInput.AltKeyDown()))
                // 选择NoteTypes.Long或NoteTypes.Single
                .Select(
                    _ =>
                    {
                        return EditState.NoteType.Value switch
                        {
                            NoteTypes.Single => NoteTypes.Long,
                            NoteTypes.Long => NoteTypes.Drag,
                            NoteTypes.Drag => NoteTypes.Dragline,
                            NoteTypes.Dragline => NoteTypes.Flick,
                            NoteTypes.Flick => NoteTypes.Single,
                            _ => NoteTypes.Single,
                        };

                    }
                )
                // 将选择的NoteTypes.Long或NoteTypes.Single赋值给EditState.NoteType.Value
                .Subscribe(editType => EditState.NoteType.Value = editType);

            // 获取编辑类型切换按钮的Image组件
            var buttonImage = editTypeToggleButton.GetComponent<Image>();

            // 根据音符类型切换按钮的图标和颜色
            EditState.NoteType.Select(_ => EditState.NoteType.Value)
                .Subscribe(NoteType =>
                {
                    // buttonImage.sprite = isLongType ? iconLongNotes : iconSingleNotes;
                    // buttonImage.color = isLongType ? longTypeStateButtonColor : singleTypeStateButtonColor;

                    switch (NoteType)
                    {
                        case NoteTypes.Single:
                            buttonImage.sprite = iconSingleNotes;
                            buttonImage.color = singleTypeStateButtonColor;
                            break;
                        case NoteTypes.Long:
                            buttonImage.sprite = iconLongNotes;
                            buttonImage.color = longTypeStateButtonColor;
                            break;
                        case NoteTypes.Drag:
                            buttonImage.sprite = iconDragNotes;
                            buttonImage.color = dragTypeStateButtonColor;
                            break;
                        case NoteTypes.Dragline:
                            buttonImage.sprite = iconDraglineNotes;
                            buttonImage.color = draglineTypeStateButtonColor;
                            break;
                        case NoteTypes.Flick:
                            buttonImage.sprite = iconFlickNotes;
                            buttonImage.color = flickTypeStateButtonColor;
                            break;
                    }
                });
        }
    }
}
