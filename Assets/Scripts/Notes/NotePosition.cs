using NoteEditor.DTO;
using NoteEditor.Model;
using UnityEngine;

namespace NoteEditor.Notes
{
    public struct NotePosition
    {
        // 定义音符位置结构体
        public int LPB, num, block;

        // 构造函数，初始化音符位置
        public NotePosition(int LPB, int num, int block)
        {
            this.LPB = LPB;
            this.num = num;
            this.block = block;
        }

        // 将音符位置转换为采样数
        public int ToSamples(int frequency, int BPM)
        {
            return Mathf.FloorToInt(num * (frequency * 60f / BPM / LPB));
        }

        // 将音符位置转换为精确时间
        public double ToExactTime()
        {
            return 60.0 / EditData.BPM.Value / LPB * (num - 1);
        }

        // 将音符位置转换为字符串
        public override string ToString()
        {
            return LPB + "-" + num + "-" + block;
        }

        // 判断两个音符位置是否相等
        public override bool Equals(object obj)
        {
            if (!(obj is NotePosition))
            {
                return false;
            }

            NotePosition target = (NotePosition)obj;
            return (
                Mathf.Approximately((float)num / LPB, (float)target.num / target.LPB) &&
                block == target.block);
        }

        // 获取音符位置的哈希值
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        // 定义一个空的音符位置
        public static NotePosition None
        {
            get { return new NotePosition(-1, -1, -1); }
        }

        // 将两个音符位置相加
        public NotePosition Add(int LPB, int num, int block)
        {
            return new NotePosition(this.LPB + LPB, this.num + num, this.block + block);
        }
    }
}
