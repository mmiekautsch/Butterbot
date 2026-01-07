namespace Butterbot2
{
    public class UserService
    {
        public readonly int MaxOtzAttempts = 3;
        public readonly TimeSpan OtzCooldown = TimeSpan.FromMinutes(60);
        public readonly TimeSpan SoundCommandCooldown = TimeSpan.FromMinutes(30);
        public readonly TimeSpan CatCooldown = TimeSpan.FromSeconds(10);

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

        private readonly Dictionary<ulong, Info> Users = [];

        public Info GetUser(ulong id)
        {
            if (!Users.TryGetValue(id, out Info value))
            {
                Users[id] = new Info();
            }
            return value;
        }
    }
}
