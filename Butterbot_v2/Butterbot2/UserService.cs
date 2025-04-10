using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Butterbot2
{
    public class UserService
    {
        public struct Info
        {
            public DateTime LastSoundCommand;
            public int FailedOtzAttempts;
            public DateTime LastFailedOtzAttempt;
            public DateTime LastCatCommand;

            public Info()
            {
                LastSoundCommand = DateTime.MinValue;
                FailedOtzAttempts = 0;
                LastFailedOtzAttempt = DateTime.MinValue;
                LastCatCommand = DateTime.MinValue;
            }
        }

        public readonly Dictionary<ulong, Info> Users = [];

        public readonly int MaxOtzAttempts = 3;
        public readonly TimeSpan OtzCooldown = TimeSpan.FromMinutes(60);
        public readonly TimeSpan SoundCommandCooldown = TimeSpan.FromMinutes(30);
        public readonly TimeSpan CatCooldown = TimeSpan.FromSeconds(10);
    }
}
