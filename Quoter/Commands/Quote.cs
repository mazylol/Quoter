using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Quoter.Commands;

public class Quote : ApplicationCommandModule
{
    [SlashCommandGroup("quote", "Quote related commands")]
    public class QuoteGroup
    {
        private static readonly IMongoDatabase? Db = Program.DbClient?.GetDatabase("Quotes");

        [SlashCommand("add", "Add a quote")]
        public static async Task Add(InteractionContext ctx, [Option("title", "The title of the quote")] string title,
            [Option("content", "The content of the quote")]
            string content, [Option("author", "The author of the quote")] DiscordUser author)
        {
            var collection = Db?.GetCollection<QuoteModel>(ctx.Guild.Id.ToString());

            var cursor = collection.AsQueryable();

            if (Enumerable.Any(cursor, document => document.Title == title))
            {
                await ctx.CreateResponseAsync("A quote with that title already exists", true);
                return;
            }

            if (Enumerable.Any(cursor, document => document.Content == content))
            {
                await ctx.CreateResponseAsync("A quote with that content already exists", true);
                return;
            }

            var insert = new QuoteModel
            {
                CreatorId = ctx.User.Id,
                Title = title,
                Content = content,
                AuthorId = author.Id
            };

            collection?.InsertOneAsync(insert);

            await ctx.CreateResponseAsync("Quote added successfully");
        }

        [SlashCommand("get", "Get a quote")]
        public static async Task Get(InteractionContext ctx, [Option("title", "The title of the quote")] string title)
        {
            var collection = Db?.GetCollection<QuoteModel>(ctx.Guild.Id.ToString());

            var cursor = collection.AsQueryable();

            if (!Enumerable.Any(cursor, document => document.Title == title))
            {
                await ctx.CreateResponseAsync("A quote with that title does not exist", true);
                return;
            }

            var quote = cursor.First(document => document.Title == title);

            var creator = await ctx.Client.GetUserAsync(quote.CreatorId);
            var author = await ctx.Client.GetUserAsync(quote.AuthorId);

            var embed = new DiscordEmbedBuilder
            {
                Title = quote.Title,
                Description = $"*{quote.Content}* - {author.Mention}",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Submitted by {creator.Username}#{creator.Discriminator}",
                    IconUrl = creator.AvatarUrl
                },
                Color = DiscordColor.Blurple
            };

            await ctx.CreateResponseAsync(embed.Build());
        }

        [SlashCommand("list", "List all quotes from a creator")]
        public static async Task List(InteractionContext ctx,
            [Option("creator", "The creator of the quote")] DiscordUser? creator = null)
        {
            var collection = Db?.GetCollection<QuoteModel>(ctx.Guild.Id.ToString());

            var cursor = collection.AsQueryable();

            if (creator == null)
            {
                creator = ctx.User;
            }

            if (!Enumerable.Any(cursor, document => document.CreatorId == creator.Id))
            {
                await ctx.CreateResponseAsync("That user has not submitted any quotes", true);
                return;
            }

            if (cursor != null)
            {
                var quotes = cursor.Where(document => document.CreatorId == creator.Id).ToList();

                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Quotes submitted by {creator.Username}#{creator.Discriminator}",
                    Color = DiscordColor.Blurple
                };

                foreach (var quote in quotes)
                {
                    embed.AddField(quote.Title, $"*{quote.Content}*");
                }

                await ctx.CreateResponseAsync(embed.Build());
            }
        }

        [SlashCommand("delete", "Delete a quote")]
        public static async Task Delete(InteractionContext ctx,
            [Option("title", "The title of the quote")]
            string title)
        {
            var collection = Db?.GetCollection<QuoteModel>(ctx.Guild.Id.ToString());

            var cursor = collection.AsQueryable();

            if (!Enumerable.Any(cursor, document => document.Title == title))
            {
                await ctx.CreateResponseAsync("A quote with that title does not exist", true);
                return;
            }

            var quote = cursor.First(document => document.Title == title);

            if (ctx.User != await ctx.Client.GetUserAsync(quote.CreatorId))
            {
                await ctx.CreateResponseAsync("You are not the creator of this quote", true);
                return;
            }

            collection?.DeleteOneAsync(document => document.Title == title);

            await ctx.CreateResponseAsync("Quote deleted successfully", true);
        }
    }
}

internal class QuoteModel
{
    public ObjectId Id { get; set; }
    public ulong CreatorId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public ulong AuthorId { get; set; }
}