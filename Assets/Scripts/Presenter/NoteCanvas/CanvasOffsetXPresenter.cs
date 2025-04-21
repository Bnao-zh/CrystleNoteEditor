using NoteEditor.Common;
using NoteEditor.Model;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class CanvasOffsetXPresenter : MonoBehaviour
    {
        [SerializeField]
        CanvasEvents canvasEvents = default;
        [SerializeField]
        RectTransform verticalLineRect = default;
        [SerializeField]
        RectTransform waveformRenderImage = default;

        void Awake()
        {
            // Initialize canvas offset x
            Audio.OnLoad.Subscribe(_ => NoteCanvas.OffsetX.Value = -Screen.width * 0.45f * NoteCanvas.ScaleFactor.Value);

            // 订阅canvasEvents.VerticalLineOnMouseDownObservable，当鼠标按下时，开始计算canvas的偏移量
            var operateCanvasOffsetXObservable = this.UpdateAsObservable()
                .SkipUntil(canvasEvents.VerticalLineOnMouseDownObservable)
                // 当鼠标松开时，停止计算canvas的偏移量
                .TakeWhile(_ => !Input.GetMouseButtonUp(0))
                // 获取鼠标位置
                .Select(_ => Input.mousePosition.x)
                // 将鼠标位置缓存2个，每次取最新的2个
                .Buffer(2, 1).Where(b => 2 <= b.Count)
                // 重复计算
                .RepeatSafe()
                // 计算鼠标位置的差值，并乘以canvas的缩放因子
                .Select(b => (b[1] - b[0]) * NoteCanvas.ScaleFactor.Value)
                // 将差值加上canvas的偏移量
                .Select(x => x + NoteCanvas.OffsetX.Value)
                // 计算canvas的最大偏移量
                .Select(x => new { x, max = Screen.width * 0.5f * 0.95f * NoteCanvas.ScaleFactor.Value })
                // 将偏移量限制在最大偏移量范围内
                .Select(v => Mathf.Clamp(v.x, -v.max, v.max))
                // 当偏移量发生变化时，触发订阅
                .DistinctUntilChanged();

            // 订阅operateCanvasOffsetXObservable，将计算得到的偏移量赋值给NoteCanvas.OffsetX
            operateCanvasOffsetXObservable.Subscribe(x => NoteCanvas.OffsetX.Value = x);

            // 当鼠标松开时，将当前和之前的偏移量缓存起来，并触发订阅
            operateCanvasOffsetXObservable.Buffer(this.UpdateAsObservable().Where(_ => Input.GetMouseButtonUp(0)))
                .Where(b => 2 <= b.Count)
                .Select(x => new { current = x.Last(), prev = x.First() })
                .Subscribe(x => EditCommandManager.Do(
                    new Command(
                        () => NoteCanvas.OffsetX.Value = x.current,
                        () => NoteCanvas.OffsetX.Value = x.prev)));

            // 当NoteCanvas.OffsetX发生变化时，更新verticalLineRect和waveformRenderImage的位置
            NoteCanvas.OffsetX.Subscribe(x =>
            {
                var pos = verticalLineRect.localPosition; //  获取垂直线矩形的位置
                var pos2 = waveformRenderImage.localPosition; //  获取波形渲染图像的位置
                pos.x = pos2.x = x; //  将两个位置设置为相同的x坐标
                verticalLineRect.localPosition = pos; //  更新垂直线矩形的位置
                waveformRenderImage.localPosition = pos2; //  更新波形渲染图像的位置
            });
        }
    }
}
