using Discord.Commands;

namespace Butterbot2
{
    public class CheckIfUserIsAdminAttribute : PreconditionAttribute
    {
        private static readonly ulong[] admins = [202861899098882048, 769230869373124638];

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (admins.Contains(context.User.Id))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else 
            {
                return Task.FromResult(PreconditionResult.FromError("Das darfst du leider nicht :("));
            }
        }
    }
}