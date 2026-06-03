using System;

namespace PuruWorldUtils
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PWU_NoteAttribute : Attribute
    {
        public readonly string Text;
        public PWU_NoteAttribute(string text) { Text = text; }
    }
}
