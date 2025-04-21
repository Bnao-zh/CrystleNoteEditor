using NoteEditor.Model;
using NoteEditor.Presenter;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace NoteEditor.SoundEffect
{
    public class ClapSoundPlayer : MonoBehaviour
    {
        [SerializeField]
        AudioSource clapAudioSource = default;

        // 当脚本被唤醒时调用
        void Awake()
        {
            // 获取EditNotesPresenter的实例
            var editPresenter = EditNotesPresenter.Instance;
            // 设置clapOffsetSamples的值为1800
            var clapOffsetSamples = 1800;

            // 创建一个可观察对象，当音频正在播放时，如果EditData.OffsetSamples、editPresenter.RequestForEditNote、editPresenter.RequestForRemoveNote、editPresenter.RequestForAddNote中的任何一个被触发，则返回false
            var editedDuringPlaybackObservable = Observable.Merge(
                    EditData.OffsetSamples.Select(_ => false),
                    editPresenter.RequestForEditNote.Select(_ => false),
                    editPresenter.RequestForRemoveNote.Select(_ => false),
                    editPresenter.RequestForAddNote.Select(_ => false))
                .Where(_ => Audio.IsPlaying.Value);

            // 当音频正在播放时，如果isPlaying为true，则返回true，否则返回false
            Audio.IsPlaying.Where(isPlaying => isPlaying)
                // 将editedDuringPlaybackObservable与Audio.IsPlaying.Merge()合并
                .Merge(editedDuringPlaybackObservable)
                // 将EditData.Notes.Values中的noteObject.note.position转换为样本数，并加上EditData.OffsetSamples的值，然后去重，并按样本数排序，最后加上clapOffsetSamples的值
                .Select(_ =>
                    new Queue<int>(
                        EditData.Notes.Values
                            .Select(noteObject => noteObject.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value))
                            .Distinct()
                            .Select(samples => samples + EditData.OffsetSamples.Value)
                            .Where(samples => Audio.Source.timeSamples <= samples)
                            .OrderBy(samples => samples)
                            .Select(samples => samples - clapOffsetSamples)))
                // 将可观察对象转换为可观察队列
                .SelectMany(samplesQueue =>
                    this.LateUpdateAsObservable()
                        .TakeWhile(_ => Audio.IsPlaying.Value)
                        // 当editedDuringPlaybackObservable.Skip(1)被触发时，停止可观察队列
                        .TakeUntil(editedDuringPlaybackObservable.Skip(1))
                        // 返回可观察队列
                        .Select(_ => samplesQueue))
                // 如果可观察队列中的元素个数大于0，则返回true，否则返回false
                .Where(samplesQueue => samplesQueue.Count > 0)
                // 如果可观察队列中的第一个元素小于等于Audio.Source.timeSamples，则返回true，否则返回false
                .Where(samplesQueue => samplesQueue.Peek() <= Audio.Source.timeSamples)
                // 从可观察队列中移除第一个元素
                .Do(samplesQueue => samplesQueue.Dequeue())
                // 如果EditorState.ClapSoundEffectEnabled的值为true，则返回true，否则返回false
                .Where(_ => EditorState.ClapSoundEffectEnabled.Value)
                // 播放clapAudioSource的clip
                .Subscribe(_ => clapAudioSource.PlayOneShot(clapAudioSource.clip, 1));
        }
    }
}
