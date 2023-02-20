using DotNetEnv;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace Quoter
{
    internal abstract class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            Env.TraversePath().Load();
            var discordToken = Env.GetString("DEV_TOKEN");
            var guildId = Convert.ToUInt64(Env.GetString("GUILD_ID"));

            var discord = new DiscordClient(new DiscordConfiguration
            {
                Token = discordToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                MinimumLogLevel = LogLevel.Debug
            });

            discord.Ready += DiscordReady;

            var slash = discord.UseSlashCommands();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task DiscordReady(DiscordClient client, ReadyEventArgs e)
        {
            await client.UpdateStatusAsync(new DiscordActivity("Cheese is good - George Washington"),
                UserStatus.Online);
        }
    }
}