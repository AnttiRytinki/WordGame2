namespace OrdSpel2
{
    public class AudioSample
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public AudioSample(string name, string path)
        {
            Name = name;
            Path = path;
        }
    };
}
