using NoteEditor.DTO;
using NoteEditor.Notes;
using NoteEditor.Presenter;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NoteEditor.Model
{
    public class EditDataSerializer
    {
        public static string Serialize()
        {
            var dto = new MusicDTO.EditData();
            dto.BPM = EditData.BPM.Value;
            dto.maxBlock = EditData.MaxBlock.Value;
            dto.offset = EditData.OffsetSamples.Value;
            dto.name = Path.GetFileNameWithoutExtension(EditData.Name.Value);

            var sortedNoteObjects = EditData.Notes.Values
                .Where(note => !(note.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(note.note.prev)))
                .OrderBy(note => note.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value));

            dto.notes = new List<MusicDTO.Note>();

            foreach (var noteObject in sortedNoteObjects)
            {
                if (noteObject.note.type == NoteTypes.Single)
                {
                    dto.notes.Add(ToDTO(noteObject, EditData.BPM.Value));
                }
                else if (noteObject.note.type == NoteTypes.Long)
                {
                    var current = noteObject;
                    var note = ToDTO(noteObject, EditData.BPM.Value);

                    while (EditData.Notes.ContainsKey(current.note.next))
                    {
                        var nextObj = EditData.Notes[current.note.next];
                        note.notes.Add(ToDTO(nextObj, EditData.BPM.Value));
                        current = nextObj;
                    }

                    dto.notes.Add(note);
                }
                else if (noteObject.note.type == NoteTypes.Drag)
                {
                    dto.notes.Add(ToDTO(noteObject, EditData.BPM.Value));
                }
                else if (noteObject.note.type == NoteTypes.Dragline)
                {
                    var current = noteObject;
                    var note = ToDTO(noteObject, EditData.BPM.Value);

                    while (EditData.Notes.ContainsKey(current.note.next))
                    {
                        var nextObj = EditData.Notes[current.note.next];
                        note.notes.Add(ToDTO(nextObj, EditData.BPM.Value));
                        current = nextObj;
                    }

                    dto.notes.Add(note);
                }
                else if (noteObject.note.type == NoteTypes.Flick)
                {
                    dto.notes.Add(ToDTO(noteObject, EditData.BPM.Value));
                }
            }

            return UnityEngine.JsonUtility.ToJson(dto);
        }

        // 将json字符串反序列化为MusicDTO.EditData对象
        public static void Deserialize(string json)
        {
            // 从json字符串中反序列化出EditData对象
            var editData = UnityEngine.JsonUtility.FromJson<MusicDTO.EditData>(json);
            // 获取EditNotesPresenter的实例
            var notePresenter = EditNotesPresenter.Instance;

            // 将EditData中的BPM、maxBlock、offset赋值给EditData中的对应属性
            EditData.BPM.Value = editData.BPM;
            EditData.MaxBlock.Value = editData.maxBlock;
            EditData.OffsetSamples.Value = editData.offset;

            // 遍历EditData中的notes
            foreach (var note in editData.notes)
            {
                // 如果note.type为1或3，则将其转换为NoteObject并添加到notePresenter中
                if (note.type == 1 && note.type == 3 && note.type == 5)
                {
                    notePresenter.AddNote(ToNoteObject(note));
                    continue;
                }

                // 将note和note.notes中的note转换为NoteObject并添加到notePresenter中，并将结果存储在longNoteObjects中
                var longNoteObjects = new[] { note }.Concat(note.notes)
                    .Select(note_ =>
                    {
                        notePresenter.AddNote(ToNoteObject(note_));
                        return EditData.Notes[ToNoteObject(note_).position];
                    })
                    .ToList();

                // 遍历longNoteObjects，将每个note的prev和next属性设置为前一个和后一个note的position
                for (int i = 1; i < longNoteObjects.Count; i++)
                {
                    longNoteObjects[i].note.prev = longNoteObjects[i - 1].note.position;
                    longNoteObjects[i - 1].note.next = longNoteObjects[i].note.position;
                }

                // 将EditState.LongNoteTailPosition设置为None
                EditState.LongNoteTailPosition.Value = NotePosition.None;
            }
        }

        static MusicDTO.Note ToDTO(NoteObject noteObject, int BPM)
        {
            var note = new MusicDTO.Note();
            note.num = noteObject.note.position.num;
            note.block = noteObject.note.position.block;
            note.LPB = noteObject.note.position.LPB;
            note.exactTime = 60.0 / BPM / noteObject.note.position.LPB * (noteObject.note.position.num - 1);
            note.type = noteObject.note.type == NoteTypes.Long ? 2 : 1;
            note.notes = new List<MusicDTO.Note>();
            return note;
        }

        // 将MusicDTO.Note对象转换为Note对象
        public static Note ToNoteObject(MusicDTO.Note musicNote)
        {
            // 创建Note对象，传入NotePosition和NoteTypes参数
            return new Note(
                // 创建NotePosition对象，传入LPB、num和block参数
                new NotePosition(musicNote.LPB, musicNote.num, musicNote.block),
                // 根据musicNote.type的值，返回NoteTypes枚举值
                musicNote.type switch
                {
                    1 => NoteTypes.Single,
                    2 => NoteTypes.Long,
                    3 => NoteTypes.Drag,
                    4 => NoteTypes.Dragline,
                    5 => NoteTypes.Flick,
                    _ => NoteTypes.Single
                }
            );
        }
    }
}
