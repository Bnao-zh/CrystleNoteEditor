﻿using NoteEditor.DTO;
using NoteEditor.Notes;
using NoteEditor.Presenter;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace NoteEditor.Model
{
    public class EditDataSerializer
    {
        // 将EditData对象序列化为json字符串
        public static string Serialize()
        {
            Debug.Log("开始序列化数据");
            var dto = new MusicDTO.EditData();
            dto.BPM = EditData.BPM.Value;
            dto.maxBlock = EditData.MaxBlock.Value;
            dto.offset = EditData.OffsetSamples.Value;
            dto.name = Path.GetFileNameWithoutExtension(EditData.Name.Value);

            var sortedNoteObjects = EditData.Notes.Values
                .Where(note => !(note.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(note.note.prev)))
                .OrderBy(note => note.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value));

            //List<NoteObject> sortedNoteObjects = new List<NoteObject>(EditData.Notes.Values.Where(note => !(note.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(note.note.prev))));

            // List<NoteObject> sortedNoteObjects = new List<NoteObject>();
            // foreach (NoteObject iN in EditData.Notes.Values) if (!(iN.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(iN.note.prev))) sortedNoteObjects.Add(iN);
            // //sortedNoteObjects.Sort((a, b) => a.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value).CompareTo(b.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value)));


            dto.notes = new List<MusicDTO.Note>();
            Debug.Log(new List<NoteObject>(sortedNoteObjects)[0]);
            foreach (var noteObject in sortedNoteObjects)
            {
                Debug.Log("成功第一步");
                var note = ToDTO(noteObject, EditData.BPM.Value);

                if (noteObject.note.type == NoteTypes.Long || noteObject.note.type == NoteTypes.Dragline)
                {
                    var current = noteObject;
                    while (EditData.Notes.ContainsKey(current.note.next))
                    {
                        var nextObj = EditData.Notes[current.note.next];
                        note.notes.Add(ToDTO(nextObj, EditData.BPM.Value));
                        current = nextObj;
                    }
                }

                dto.notes.Add(note);
            }

            return JsonUtility.ToJson(dto);
        }

        public static string Serializesocket()
        {
            Debug.Log("开始序列化数据");
            var dto = new MusicDTO.EditData();
            dto.BPM = EditData.BPM.Value;
            dto.maxBlock = EditData.MaxBlock.Value;
            dto.offset = EditData.OffsetSamples.Value;
            dto.name = Path.GetFileNameWithoutExtension(EditData.Name.Value);

            /*
            var sortedNoteObjects = EditData.Notes.Values
                .Where(note => !(note.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(note.note.prev)))
                .OrderBy(note => note.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value));
            */

            //List<NoteObject> sortedNoteObjects = new List<NoteObject>(EditData.Notes.Values.Where(note => !(note.note.type == NoteTypes.Long && EditData.Notes.ContainsKey(note.note.prev))));

            List<NoteObject> sortedNoteObjects = new List<NoteObject>();
            foreach (NoteObject iN in EditData.Notes.Values) if (!((iN.note.type == NoteTypes.Long || iN.note.type == NoteTypes.Dragline) && EditData.Notes.ContainsKey(iN.note.prev))) sortedNoteObjects.Add(iN);
            //sortedNoteObjects.Sort((a, b) => a.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value).CompareTo(b.note.position.ToSamples(Audio.Source.clip.frequency, EditData.BPM.Value)));


            dto.notes = new List<MusicDTO.Note>();
            Debug.Log(new List<NoteObject>(sortedNoteObjects)[0]);
            foreach (var noteObject in sortedNoteObjects)
            {
                Debug.Log("成功第一步");
                var note = ToDTO(noteObject, EditData.BPM.Value);

                if (noteObject.note.type == NoteTypes.Long || noteObject.note.type == NoteTypes.Dragline)
                {
                    var current = noteObject;
                    while (EditData.Notes.ContainsKey(current.note.next))
                    {
                        var nextObj = EditData.Notes[current.note.next];
                        note.notes.Add(ToDTO(nextObj, EditData.BPM.Value));
                        current = nextObj;
                    }
                }

                dto.notes.Add(note);
            }

            return JsonUtility.ToJson(dto);
        }


        // 将json字符串反序列化为MusicDTO.EditData对象
        public static void Deserialize(string json)
        {
            // 从json字符串中反序列化出EditData对象
            var editData = JsonUtility.FromJson<MusicDTO.EditData>(json);
            // 获取EditNotesPresenter的实例
            var notePresenter = EditNotesPresenter.Instance;

            // 将EditData中的BPM、maxBlock、offset赋值给EditData中的对应属性
            EditData.BPM.Value = editData.BPM;
            EditData.MaxBlock.Value = editData.maxBlock;
            EditData.OffsetSamples.Value = editData.offset;

            // 遍历EditData中的notes
            foreach (var note in editData.notes)
            {
                // 根据note.type的值，将note转换为NoteObject并添加到notePresenter中
                if (note.type == 1 || note.type == 3 || note.type == 5)
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
            switch (noteObject.note.type)
            {
                case NoteTypes.Single:
                    note.type = 1;
                    break;
                case NoteTypes.Long:
                    note.type = 2;
                    break;
                case NoteTypes.Drag:
                    note.type = 3;
                    break;
                case NoteTypes.Dragline:
                    note.type = 4;
                    break;
                case NoteTypes.Flick:
                    note.type = 5;
                    break;
            }
            note.notes = new List<MusicDTO.Note>();
            return note;
        }

        // 将MusicDTO.Note对象转换为Note对象
        public static Note ToNoteObject(MusicDTO.Note musicNote)
        {
            // 创建Note对象，传入NotePosition和NoteTypes参数
            return new Note(
                new NotePosition(musicNote.LPB, musicNote.num, musicNote.block),
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
