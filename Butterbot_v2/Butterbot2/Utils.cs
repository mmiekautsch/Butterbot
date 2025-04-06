using System.Diagnostics;

#pragma warning disable CS8603 // Mögliche Nullverweisrückgabe.

namespace Butterbot2
{
    public class Utils
    {
        public static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });
        }

        public static string GetRandomSound()
        {
            string[] sounds = Directory.GetFiles("C:\\Users\\dasmo\\Documents\\Butterbot\\sounds", "*.mp3");
            return sounds[new Random().Next(sounds.Length)];
        }
    }
}
