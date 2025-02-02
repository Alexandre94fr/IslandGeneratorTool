using System;

namespace IslandGenerator
{
    // NOTE : AllowMultiple and Inherited are respectively set at 'false' and 'true' by default
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class EditorHeaderAttribute : Attribute
    {
        public string HeaderText { get; }


        public EditorHeaderAttribute(string p_headerText)
        {
            HeaderText = p_headerText;
        }
    }
}