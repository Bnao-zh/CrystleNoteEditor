using System.Collections.Generic;

namespace NoteEditor.DTO
{
    [System.Serializable]
    public class SettingsDTO
    {
        public string workSpacePath;
        public int maxBlock;
        public List<int> noteInputKeyCodes;

        public static SettingsDTO GetDefaultSettings()
        {
            return new SettingsDTO
            {
                workSpacePath = "",
                maxBlock = 7,
                noteInputKeyCodes = new List<int> { 81, 87, 69, 82, 84, 89, 85 }
            };
        }
    }
}
