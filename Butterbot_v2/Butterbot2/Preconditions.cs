using Discord.Commands;
using Discord.Rest;
using Microsoft.Extensions.DependencyInjection;

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

    public class CheckUserInfo : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            UserService userService = services.GetRequiredService<UserService>();
            UserService.Info info = userService.GetUser(context.User.Id);

            switch (command.Name)
            {
                case "otz":
                    if (info.FailedOtzAttempts < userService.MaxOtzAttempts)
                    {
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }
                    else
                    {
                        if (DateTime.Now - info.LastFailedOtzAttempt > userService.OtzCooldown)
                        {
                            info.FailedOtzAttempts = 0;
                            return Task.FromResult(PreconditionResult.FromSuccess());
                        }
                        else
                        {
                            TimeSpan timeLeft = userService.OtzCooldown - (DateTime.Now - info.LastFailedOtzAttempt);
                            return Task.FromResult(PreconditionResult.FromError($"Deine mom is ne otze, otz in {timeLeft.Minutes}min {timeLeft.Seconds}s wieder"));
                        }
                    }

                case "machwas":
                    if (DateTime.Now - info.LastSoundCommand > userService.SoundCommandCooldown)
                    {
                        info.LastSoundCommand = DateTime.Now;
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }
                    else
                    {
                        TimeSpan timeLeft = userService.SoundCommandCooldown - (DateTime.Now - info.LastSoundCommand);
                        return Task.FromResult(PreconditionResult.FromError($"Immer mit der Ruhe du kleiner Pisser. In {timeLeft.Minutes}min {timeLeft.Seconds} kannste wieder"));
                    }

                case "cat":
                    if (DateTime.Now - info.LastCatCommand > userService.CatCooldown)
                    {
                        info.LastCatCommand = DateTime.Now;
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }
                    else
                    {
                        TimeSpan timeLeft = userService.CatCooldown - (DateTime.Now - info.LastCatCommand);
                        return Task.FromResult(PreconditionResult.FromError($"In {timeLeft.Minutes}min {timeLeft.Seconds} kannste wieder :)"));
                    }
                default:
                    return Task.FromResult(PreconditionResult.FromError($"Error: check für {command.Name} is unbehandelt!!"));
            }
        }
    }
}