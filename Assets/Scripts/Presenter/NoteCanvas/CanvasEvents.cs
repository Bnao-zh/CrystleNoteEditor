using NoteEditor.Model;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.Presenter
{
    public class CanvasEvents : MonoBehaviour
    {
        // 定义一个可观察的Subject，用于在鼠标抬起时发送鼠标位置信息
        public readonly Subject<Vector3> NotesRegionOnMouseUpObservable = new Subject<Vector3>();
        // 定义一个可观察的Subject，用于在鼠标移出时发送鼠标位置信息
        public readonly Subject<Vector3> NotesRegionOnMouseExitObservable = new Subject<Vector3>();
        // 定义一个可观察的Subject，用于在鼠标按下时发送鼠标位置信息
        public readonly Subject<Vector3> NotesRegionOnMouseDownObservable = new Subject<Vector3>();
        // 定义一个可观察的Subject，用于在鼠标进入时发送鼠标位置信息
        public readonly Subject<Vector3> NotesRegionOnMouseEnterObservable = new Subject<Vector3>();
        // 定义一个可观察的Subject，用于在垂直线按下时发送鼠标位置信息
        public readonly Subject<Vector3> VerticalLineOnMouseDownObservable = new Subject<Vector3>();
        // 定义一个可观察的Subject，用于在波形区域按下时发送鼠标位置信息
        public readonly Subject<Vector3> WaveformRegionOnMouseDownObservable = new Subject<Vector3>();
        // 定义一个可观察的Subject，用于在波形区域移出时发送鼠标位置信息
        public readonly Subject<Vector3> WaveformRegionOnMouseExitObservable = new Subject<Vector3>();
        // 定义一个可观察的Subject，用于在波形区域进入时发送鼠标位置信息
        public readonly Subject<Vector3> WaveformRegionOnMouseEnterObservable = new Subject<Vector3>();
        // 定义一个可观察的Subject，用于在鼠标滚轮滚动时发送滚轮值信息
        public readonly Subject<float> MouseScrollWheelObservable = new Subject<float>();

        // 在Awake方法中，订阅鼠标滚轮滚动事件，并将滚轮值发送到MouseScrollWheelObservable
        void Awake()
        {
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse ScrollWheel"))
                .Where(delta => delta != 0)
                .Subscribe(MouseScrollWheelObservable.OnNext);

            // 订阅NotesRegionOnMouseExitObservable，当鼠标移出时，将isMouseOver设置为false
            NotesRegionOnMouseExitObservable.Select(_ => false)
                .Merge(NotesRegionOnMouseEnterObservable.Select(_ => true))
                .Subscribe(isMouseOver => NoteCanvas.IsMouseOverNotesRegion.Value = isMouseOver);

            // 订阅WaveformRegionOnMouseExitObservable，当鼠标移出时，将isMouseOver设置为false
            WaveformRegionOnMouseExitObservable.Select(_ => false)
                .Merge(WaveformRegionOnMouseEnterObservable.Select(_ => true))
                .Subscribe(isMouseOver => NoteCanvas.IsMouseOverWaveformRegion.Value = isMouseOver);
        }

        // 在NotesRegionOnMouseUp方法中，将鼠标位置信息发送到NotesRegionOnMouseUpObservable
        public void NotesRegionOnMouseUp() { NotesRegionOnMouseUpObservable.OnNext(Input.mousePosition); }
        // 在NotesRegionOnMouseExit方法中，将鼠标位置信息发送到NotesRegionOnMouseExitObservable
        public void NotesRegionOnMouseExit() { NotesRegionOnMouseExitObservable.OnNext(Input.mousePosition); }
        // 在NotesRegionOnMouseDown方法中，将鼠标位置信息发送到NotesRegionOnMouseDownObservable
        public void NotesRegionOnMouseDown() { NotesRegionOnMouseDownObservable.OnNext(Input.mousePosition); }
        // 在NotesRegionOnMouseEnter方法中，将鼠标位置信息发送到NotesRegionOnMouseEnterObservable
        public void NotesRegionOnMouseEnter() { NotesRegionOnMouseEnterObservable.OnNext(Input.mousePosition); }
        // 在VerticalLineOnMouseDown方法中，将鼠标位置信息发送到VerticalLineOnMouseDownObservable
        public void VerticalLineOnMouseDown() { VerticalLineOnMouseDownObservable.OnNext(Input.mousePosition); }
        // 在WaveformRegionOnMouseDown方法中，将鼠标位置信息发送到WaveformRegionOnMouseDownObservable
        public void WaveformRegionOnMouseDown() { WaveformRegionOnMouseDownObservable.OnNext(Input.mousePosition); }
        // 在WaveformRegionOnMouseExit方法中，将鼠标位置信息发送到WaveformRegionOnMouseExitObservable
        public void WaveformRegionOnMouseExit() { WaveformRegionOnMouseExitObservable.OnNext(Input.mousePosition); }
        // 在WaveformRegionOnMouseEnter方法中，将鼠标位置信息发送到WaveformRegionOnMouseEnterObservable
        public void WaveformRegionOnMouseEnter() { WaveformRegionOnMouseEnterObservable.OnNext(Input.mousePosition); }
    }
}
