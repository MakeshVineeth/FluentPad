using System.Collections.Generic;

namespace FluentPad
{
    public class DefinitionClass
    {
        public string Definition { get; set; }
        public string Example { get; set; }
    }

    public class Meaning
    {
        public string PartOfSpeech { get; set; }
        public List<DefinitionClass> Definitions { get; set; }
    }

    public class Root
    {
        public List<Meaning> Meanings { get; set; }
    }
}
