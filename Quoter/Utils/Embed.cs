using DSharpPlus.Entities;

namespace Quoter.Utils;

public static class Embed
{
    public static DiscordEmbed Error(string description)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = description,
            Color = DiscordColor.Red
        };

        return embed.Build();
    }

    public static DiscordEmbed Success(string description)
    {
        var embed = new DiscordEmbedBuilder
        {
            Description = description,
            Color = DiscordColor.Green
        };

        return embed.Build();
    }
}