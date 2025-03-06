using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System.Windows.Input;

namespace BrainStorm
{
    /*
    
    // on startup:
    var zap = new CachedSound("zap.wav");
    var boom = new CachedSound("boom.wav");

    // later in the app...
    AudioPlaybackEngine.Instance.PlaySound(zap);
    AudioPlaybackEngine.Instance.PlaySound(boom);
    AudioPlaybackEngine.Instance.PlaySound("crash.wav");

    // on shutdown
    AudioPlaybackEngine.Instance.Dispose();

    */

    public class AudioHandler
    {
        public List<CachedSound> CachedSounds { get; set; } = new List<CachedSound>();
        public bool Initialized { get; private set; } = false;

        public AudioHandler()
        {
            try
            {
                Init();
                Initialized = true;
            }
            catch
            {
                Initialized = false;
            }
        }

        public void Init()
        {
            CachedSounds.Add(new CachedSound(".//wav//NATO//ALPHA.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//BRAVO.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//CHARLIE.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//DELTA.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//ECHO.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//FOXTROT.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//GOLF.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//HOTEL.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//INDIA.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//JULIET.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//KILO.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//LIMA.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//MIKE.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//NOVEMBER.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//OSCAR.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//PAPA.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//QUEBEC.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//ROMEO.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//SIERRA.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//TANGO.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//UNIFORM.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//VICTOR.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//WHISKEY.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//XRAY.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//YANKEE.wav"));
            CachedSounds.Add(new CachedSound(".//wav//NATO//ZULU.wav"));
        }

        public CachedSound GetSoundByName(string name)
        {
            foreach (CachedSound sound in CachedSounds)
            {
                if (sound.AudioFileName.Contains(name))
                    return sound;
            }

            return null;
        }

        public void PlayNATOAudio(Key key)
        {
            try
            {
                if (key == Key.A)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("ALPHA"));
                else if (key == Key.B)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("BETA"));
                else if (key == Key.C)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("CHARLIE"));
                else if (key == Key.D)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("DELTA"));
                else if (key == Key.E)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("ECHO"));
                else if (key == Key.F)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("FOXTROT"));
                else if (key == Key.G)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("GOLF"));
                else if (key == Key.H)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("HOTEL"));
                else if (key == Key.I)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("INDIA"));
                else if (key == Key.J)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("JULIET"));
                else if (key == Key.K)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("KILO"));
                else if (key == Key.L)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("LIMA"));
                else if (key == Key.M)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("MIKE"));
                else if (key == Key.N)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("NOVEMBER"));
                else if (key == Key.O)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("OSCAR"));
                else if (key == Key.P)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("PAPA"));
                else if (key == Key.Q)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("QUEBEC"));
                else if (key == Key.R)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("ROMEO"));
                else if (key == Key.S)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("SIERRA"));
                else if (key == Key.T)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("TANGO"));
                else if (key == Key.U)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("UNIFORM"));
                else if (key == Key.V)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("VICTOR"));
                else if (key == Key.W)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("WHISKEY"));
                else if (key == Key.X)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("XRAY"));
                else if (key == Key.Y)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("YANKEE"));
                else if (key == Key.Z)
                    AudioPlaybackEngine.Instance.PlaySound(GetSoundByName("ZULU"));
            }
            catch
            {
                ;
            }
        }

        public void PlayRandomSample(string path)   // TODO - play random sample from path
        {

        }
    }
}
