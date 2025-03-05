using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.Windows.Input;

namespace BrainStorm
{
    public class AudioHandler
    {
        List<ISampleProvider> _selectedSamples = new List<ISampleProvider>();
        ConcatenatingSampleProvider? _fullAudio;
        WaveOutEvent _wavPlayer = new WaveOutEvent();

        public void HandleQWERTYAudio(Key key)
        {
            if (key == Key.A)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("AlphaSample", ".//wav//NATO//ALPHA.wav").Path));
            else if (key == Key.B)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("BravoSample", ".//wav//NATO//BRAVO.wav").Path));
            else if (key == Key.C)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("CharlieSample", ".//wav//NATO//CHARLIE.wav").Path));
            else if (key == Key.D)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("DeltaSample", ".//wav//NATO//DELTA.wav").Path));
            else if (key == Key.E)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("EchoSample", ".//wav//NATO//ECHO.wav").Path));
            else if (key == Key.F)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("FoxtrotSample", ".//wav//NATO//FOXTROT.wav").Path));
            else if (key == Key.G)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("GolfSample", ".//wav//NATO//GOLF.wav").Path));
            else if (key == Key.H)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("HotelSample", ".//wav//NATO//HOTEL.wav").Path));
            else if (key == Key.I)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("IndiaSample", ".//wav//NATO//INDIA.wav").Path));
            else if (key == Key.J)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("JulietSample", ".//wav//NATO//JULIET.wav").Path));
            else if (key == Key.K)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("KiloSample", ".//wav//NATO//KILO.wav").Path));
            else if (key == Key.L)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("LimaSample", ".//wav//NATO//LIMA.wav").Path));
            else if (key == Key.M)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("MikeSample", ".//wav//NATO//MIKE.wav").Path));
            else if (key == Key.N)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("NovemberSample", ".//wav//NATO//NOVEMBER.wav").Path));
            else if (key == Key.O)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("OscarSample", ".//wav//NATO//OSCAR.wav").Path));
            else if (key == Key.P)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("PapaSample", ".//wav//NATO//PAPA.wav").Path));
            else if (key == Key.Q)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("QuebecSample", ".//wav//NATO//QUEBEC.wav").Path));
            else if (key == Key.R)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("RomeoSample", ".//wav//NATO//ROMEO.wav").Path));
            else if (key == Key.S)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("SierraSample", ".//wav//NATO//SIERRA.wav").Path));
            else if (key == Key.T)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("TangoSample", ".//wav//NATO//TANGO.wav").Path));
            else if (key == Key.U)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("UniformSample", ".//wav//NATO//UNIFORM.wav").Path));
            else if (key == Key.V)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("VictorSample", ".//wav//NATO//VICTOR.wav").Path));
            else if (key == Key.W)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("WhiskeySample", ".//wav//NATO//WHISKEY.wav").Path));
            else if (key == Key.X)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("XraySample", ".//wav//NATO//XRAY.wav").Path));
            else if (key == Key.Y)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("YankeeSample", ".//wav//NATO//YANKEE.wav").Path));
            else if (key == Key.Z)
                _selectedSamples.Add(new AudioFileReader(new AudioSample("ZuluSample", ".//wav//NATO//ZULU.wav").Path));

            try
            {
                _fullAudio = new ConcatenatingSampleProvider(_selectedSamples);

                try
                {
                    _wavPlayer.Init(_fullAudio);
                    _wavPlayer.Play();
                }
                catch
                {
                    ;
                }
            }
            catch
            {
                ;
            }
        }

        public void PlayRandomSample(string name)   // TODO
        {
            _selectedSamples.Add(new AudioFileReader(new AudioSample(name, ".//wav//samples//" + name + ".wav").Path));

            Random rnd = new Random();
            int idx = rnd.Next(0, 5);

            var _oneSelectedSamples = new List<ISampleProvider>();
            var _selectedSample = _selectedSamples[idx];
            _oneSelectedSamples.Add(_selectedSample);

            _fullAudio = new ConcatenatingSampleProvider(_oneSelectedSamples);

            try
            {
                _wavPlayer.Init(_fullAudio);
                _wavPlayer.Play();
            }
            catch
            {
                ;
            }
        }
    }
}
