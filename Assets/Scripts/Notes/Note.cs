namespace NoteEditor.Notes
{
    public class Note
    {
        // 定义音符的位置
        public NotePosition position = NotePosition.None;
        // 定义音符的类型
        public NoteTypes type = NoteTypes.Single;
        // 定义音符的下一个位置
        public NotePosition next = NotePosition.None;
        // 定义音符的上一个位置
        public NotePosition prev = NotePosition.None;

        // 构造函数，传入音符的位置、类型、下一个位置和上一个位置
        public Note(NotePosition position, NoteTypes type, NotePosition next, NotePosition prev)
        {
            this.position = position;
            this.type = type;
            this.next = next;
            this.prev = prev;
        }

        // 构造函数，传入音符的位置和类型
        public Note(NotePosition position, NoteTypes type)
        {
            this.position = position;
            this.type = type;
        }

        // 构造函数，传入音符的位置
        public Note(NotePosition position)
        {
            this.position = position;
        }

        // 构造函数，传入一个音符对象
        public Note(Note note)
        {
            this.position = note.position;
            this.type = note.type;
            this.next = note.next;
            this.prev = note.prev;
        }

        // 无参构造函数
        public Note() { }


        // 重写Equals方法，判断两个音符对象是否相等
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var c = (Note)obj;

            return position.Equals(c.position) &&
                type == c.type &&
                next.Equals(c.next) &&
                prev.Equals(c.prev);
        }

        // 重写GetHashCode方法，返回音符对象的哈希码
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
