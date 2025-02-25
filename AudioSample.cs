namespace OrdSpel2
{
    public class AudioSample
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public static AudioSample Empty { get; } = new AudioSample("", "");

        public AudioSample(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public bool IsEmpty()
        {
            if (Name == "")
                return true;

            return false;
        }
    };
}
