using NoteEditor.GLDrawing;
using NoteEditor.Model;
using NoteEditor.Presenter;
using NoteEditor.Utility;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace NoteEditor.Notes
{
    public class NoteObject : IDisposable
    {
        // 定义一个Note类型的变量
        public Note note = new Note();
        // 定义一个ReactiveProperty类型的变量，用于表示是否被选中
        public ReactiveProperty<bool> isSelected = new ReactiveProperty<bool>();
        // 定义一个Subject类型的变量，用于表示LateUpdate事件
        public Subject<Unit> LateUpdateObservable = new Subject<Unit>();
        // 定义一个Subject类型的变量，用于表示OnClick事件
        public Subject<Unit> OnClickObservable = new Subject<Unit>();
        // 定义一个Color类型的属性，用于表示Note的颜色
        public Color NoteColor { get { return noteColor_.Value; } }
        // 定义一个ReactiveProperty类型的变量，用于表示Note的颜色
        ReactiveProperty<Color> noteColor_ = new ReactiveProperty<Color>();

        // 定义一个Color类型的变量，用于表示被选中的颜色
        Color selectedStateColor = new Color(255 / 255f, 0 / 255f, 255 / 255f);
        // 定义一个Color类型的变量，用于表示单音符的颜色
        Color singleNoteColor = new Color(175 / 255f, 255 / 255f, 78 / 255f);
        // 定义一个Color类型的变量，用于表示长音符的颜色
        Color longNoteColor = new Color(0 / 255f, 255 / 255f, 255 / 255f);
        Color dragNoteColor = new Color(0 / 255f, 255 / 255f, 122 / 255f);
        Color draglineNoteColor = new Color(0 / 255f, 255 / 255f, 122 / 255f);
        Color flickNoteColor = new Color(255 / 255f, 94 / 255f, 94 / 255f);
        // 定义一个Color类型的变量，用于表示无效状态的颜色
        Color invalidStateColor = new Color(255 / 255f, 0 / 255f, 0 / 255f);

        // 定义一个ReactiveProperty类型的变量，用于表示Note的类型
        ReactiveProperty<NoteTypes> noteType = new ReactiveProperty<NoteTypes>();
        // 定义一个CompositeDisposable类型的变量，用于管理可被释放的资源
        CompositeDisposable disposable = new CompositeDisposable();

        // 初始化方法
        public void Init()
        {
            // 将需要释放的资源添加到CompositeDisposable中
            disposable = new CompositeDisposable(
                isSelected,
                LateUpdateObservable,
                OnClickObservable,
                noteColor_,
                noteType);

            // 获取EditNotesPresenter的实例
            var editPresenter = EditNotesPresenter.Instance;
            // 将note.type的值转换为ReactiveProperty
            noteType = this.ObserveEveryValueChanged(_ => note.type).ToReactiveProperty();

            // 当noteType的值不为isSelected的值时，根据noteType的值设置noteColor的值
            disposable.Add(noteType.Where(_ => !isSelected.Value)
                .Merge(isSelected.Select(_ => noteType.Value))
                .Select(type => type)
                .Subscribe(type1 => {
                    switch(type1){
                        case NoteTypes.Single:
                            noteColor_.Value = singleNoteColor;
                            break;
                        case NoteTypes.Long:
                            noteColor_.Value = longNoteColor;
                            break;
                        case NoteTypes.Drag:
                            noteColor_.Value = dragNoteColor;
                            break;
                        case NoteTypes.Dragline:
                            noteColor_.Value = draglineNoteColor;
                            break;
                        case NoteTypes.Flick:
                            noteColor_.Value = flickNoteColor;
                            break;
                    }
                    }));

            // 当isSelected的值为true时，设置noteColor的值为selectedStateColor
            disposable.Add(isSelected.Where(selected => selected)
                .Subscribe(_ => noteColor_.Value = selectedStateColor));

            // 获取OnClickObservable的值，当NoteCanvas.ClosestNotePosition的值等于note.position时，根据EditState.NoteType的值进行操作
            var mouseDownObservable = OnClickObservable
                .Select(_ => EditState.NoteType.Value)
                .Where(_ => NoteCanvas.ClosestNotePosition.Value.Equals(note.position));

            // 当EditState.NoteType的值为NoteTypes.Single且noteType的值等于EditState.NoteType的值时，请求移除note
            disposable.Add(mouseDownObservable.Where(editType => editType == NoteTypes.Single)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ => editPresenter.RequestForRemoveNote.OnNext(note)));

            // 当EditState.NoteType的值为NoteTypes.Long且noteType的值等于EditState.NoteType的值时，进行长音符的操作
            disposable.Add(mouseDownObservable.Where(editType => editType == NoteTypes.Long)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ =>
                {
                    // 如果EditData.Notes包含EditState.LongNoteTailPosition的值且note.prev的值为NotePosition.None，则进行长音符的操作
                    if (EditData.Notes.ContainsKey(EditState.LongNoteTailPosition.Value) && note.prev.Equals(NotePosition.None))
                    {
                        var currentTailNote = new Note(EditData.Notes[EditState.LongNoteTailPosition.Value].note);
                        currentTailNote.next = note.position;
                        editPresenter.RequestForChangeNoteStatus.OnNext(currentTailNote);

                        var selfNote = new Note(note);
                        selfNote.prev = currentTailNote.position;
                        editPresenter.RequestForChangeNoteStatus.OnNext(selfNote);
                    }
                    // 否则，进行长音符的操作
                    else
                    {
                        // 如果EditData.Notes包含note.prev的值且不包含note.next的值，则设置EditState.LongNoteTailPosition的值为note.prev
                        if (EditData.Notes.ContainsKey(note.prev) && !EditData.Notes.ContainsKey(note.next))
                            EditState.LongNoteTailPosition.Value = note.prev;

                        // 请求移除note
                        editPresenter.RequestForRemoveNote.OnNext(new Note(note.position, EditState.NoteType.Value, note.next, note.prev));
                        // 移除链接
                        RemoveLink();
                    }
                }));

            var longNoteUpdateObservable = LateUpdateObservable
                .Where(_ => noteType.Value == NoteTypes.Long);

            disposable.Add(longNoteUpdateObservable
                // 监听长音符更新
                .Where(_ => EditData.Notes.ContainsKey(note.next))
                // 将长音符转换为画布位置
                .Select(_ => ConvertUtils.NoteToCanvasPosition(note.next))
                // 合并长音符
                .Merge(longNoteUpdateObservable
                    // 监听类型
                    .Where(_ => EditState.NoteType.Value == NoteTypes.Long)
                    // 监听尾部位置
                    .Where(_ => EditState.LongNoteTailPosition.Value.Equals(note.position))
                    // 将屏幕位置转换为画布位置
                    .Select(_ => ConvertUtils.ScreenToCanvasPosition(Input.mousePosition)))
                // 将画布位置转换为屏幕位置
                .Select(nextPosition => new Line(
                    ConvertUtils.CanvasToScreenPosition(ConvertUtils.NoteToCanvasPosition(note.position)),
                    ConvertUtils.CanvasToScreenPosition(nextPosition),
                    // 判断是否选中或长音符是否包含下一个音符
                    isSelected.Value || EditData.Notes.ContainsKey(note.next) && EditData.Notes[note.next].isSelected.Value ? selectedStateColor
                        // 判断下一个音符的位置是否在长音符的右侧
                        : 0 < nextPosition.x - ConvertUtils.NoteToCanvasPosition(note.position).x ? longNoteColor : invalidStateColor))
                // 绘制线段
                .Subscribe(line => GLLineDrawer.Draw(line)));

            disposable.Add(mouseDownObservable.Where(editType => editType == NoteTypes.Drag)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ => editPresenter.RequestForRemoveNote.OnNext(note)));

            disposable.Add(mouseDownObservable.Where(editType => editType == NoteTypes.Dragline)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ =>
                {
                    // 如果EditData.Notes包含EditState.LongNoteTailPosition的值且note.prev的值为NotePosition.None，则进行长音符的操作
                    if (EditData.Notes.ContainsKey(EditState.LongNoteTailPosition.Value) && note.prev.Equals(NotePosition.None))
                    {
                        var currentTailNote = new Note(EditData.Notes[EditState.LongNoteTailPosition.Value].note);
                        currentTailNote.next = note.position;
                        editPresenter.RequestForChangeNoteStatus.OnNext(currentTailNote);

                        var selfNote = new Note(note);
                        selfNote.prev = currentTailNote.position;
                        editPresenter.RequestForChangeNoteStatus.OnNext(selfNote);
                    }
                    // 否则，进行长音符的操作
                    else
                    {
                        // 如果EditData.Notes包含note.prev的值且不包含note.next的值，则设置EditState.LongNoteTailPosition的值为note.prev
                        if (EditData.Notes.ContainsKey(note.prev) && !EditData.Notes.ContainsKey(note.next))
                            EditState.LongNoteTailPosition.Value = note.prev;

                        // 请求移除note
                        editPresenter.RequestForRemoveNote.OnNext(new Note(note.position, EditState.NoteType.Value, note.next, note.prev));
                        // 移除链接
                        RemoveLink();
                    }
                }));

            // 获取LateUpdateObservable的值，当noteType的值为NoteTypes.Dragline时，进行长音符的操作
            var drawlineNoteUpdateObservable = LateUpdateObservable
            .Where(_ => noteType.Value == NoteTypes.Dragline);

            // 当EditData.Notes包含note.next的值时，根据note.next的值设置noteColor的值
            disposable.Add(drawlineNoteUpdateObservable
                // 监听长音符更新事件
                .Where(_ => EditData.Notes.ContainsKey(note.next))
                // 将长音符转换为画布位置
                .Select(_ => ConvertUtils.NoteToCanvasPosition(note.next))
                // 合并长音符更新事件
                .Merge(drawlineNoteUpdateObservable
                    // 监听长音符类型
                    .Where(_ => EditState.NoteType.Value == NoteTypes.Dragline)
                    // 监听长音符尾部位置
                    .Where(_ => EditState.LongNoteTailPosition.Value.Equals(note.position))
                    // 将屏幕位置转换为画布位置
                    .Select(_ => ConvertUtils.ScreenToCanvasPosition(Input.mousePosition)))
                // 将画布位置转换为屏幕位置
                .Select(nextPosition => new Line(
                    ConvertUtils.CanvasToScreenPosition(ConvertUtils.NoteToCanvasPosition(note.position)),
                    ConvertUtils.CanvasToScreenPosition(nextPosition),
                    // 判断是否选中或长音符是否包含下一个音符
                    isSelected.Value || EditData.Notes.ContainsKey(note.next) && EditData.Notes[note.next].isSelected.Value ? selectedStateColor
                        // 判断下一个音符的位置是否在长音符的右侧
                        : 0 < nextPosition.x - ConvertUtils.NoteToCanvasPosition(note.position).x ? draglineNoteColor : invalidStateColor))
                // 绘制线段
                .Subscribe(line => GLLineDrawer.Draw(line)));

            disposable.Add(mouseDownObservable.Where(editType => editType == NoteTypes.Flick)
                .Where(editType => editType == noteType.Value)
                .Subscribe(_ => editPresenter.RequestForRemoveNote.OnNext(note)));
        }

        // 移除链接的方法
        void RemoveLink()
        {
            // 如果EditData.Notes包含note.prev的值，则设置note.prev的next为note.next
            if (EditData.Notes.ContainsKey(note.prev))
                EditData.Notes[note.prev].note.next = note.next;

            // 如果EditData.Notes包含note.next的值，则设置note.next的prev为note.prev
            if (EditData.Notes.ContainsKey(note.next))
                EditData.Notes[note.next].note.prev = note.prev;
        }

        // 插入链接的方法
        void InsertLink(NotePosition position)
        {
            // 如果EditData.Notes包含note.prev的值，则设置note.prev的next为position
            if (EditData.Notes.ContainsKey(note.prev))
                EditData.Notes[note.prev].note.next = position;

            // 如果EditData.Notes包含note.next的值，则设置note.next的prev为position
            if (EditData.Notes.ContainsKey(note.next))
                EditData.Notes[note.next].note.prev = position;
        }

        // 设置状态的方法
        public void SetState(Note note)
        {
            // 如果note.type的值为NoteTypes.Single、NoteTypes.Drag或NoteTypes.Flick，则移除链接
            if (note.type == NoteTypes.Single || note.type == NoteTypes.Drag || note.type == NoteTypes.Flick)
            {
                RemoveLink();
            }

            // 设置note的值为传入的note
            this.note = note;

            // 如果note.type的值为NoteTypes.Long，则插入链接
            if (note.type == NoteTypes.Long || note.type == NoteTypes.Dragline)
            {
                InsertLink(note.position);
                // 设置EditState.LongNoteTailPosition的值为note.position或NotePosition.None
                EditState.LongNoteTailPosition.Value = EditState.LongNoteTailPosition.Value.Equals(note.prev)
                    ? note.position
                    : NotePosition.None;
            }
        }

        // 释放资源的方法
        public void Dispose()
        {
            disposable.Dispose();
        }
    }
}
