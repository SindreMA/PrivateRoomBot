using System;
using Discord;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Addons.EmojiTools;
using System.Text.RegularExpressions;

namespace TestEasyBot
{
    class CommandHandler
    {
        private DiscordSocketClient _client;
        private CommandService _service;
        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += _client_MessageReceived;
            _client.ReactionAdded += _client_ReactionAdded;
        }

        private async Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var messageid = arg1;
            var channel = arg2;
            var socketuser = arg3;
            var msg = channel.GetMessageAsync(socketuser.MessageId).Result;

            SocketGuild AddingGuild = null;
            if (!socketuser.User.Value.IsBot)
            {
                var firstsplit = SplitOnString(msg.Content, " on ");
                var secsplit = firstsplit[0].Split('(');
                var thirdsplit = firstsplit[1].Split('(');
                var channelid = secsplit[1].Replace(")","");
                var guildid = thirdsplit[1].Replace(")?","");
                
                AddingGuild = _client.GetGuild(ulong.Parse(guildid));

                if (channelid != null || AddingGuild != null)
                {
                    if (socketuser.Emote.Name == "✅")
                    {
                        var AllowPerm = new OverwritePermissions(
               readMessages: PermValue.Allow,
               readMessageHistory: PermValue.Allow,
               sendMessages: PermValue.Allow,
               attachFiles: PermValue.Allow,
               embedLinks: PermValue.Allow,
               useExternalEmojis: PermValue.Allow,
               addReactions: PermValue.Allow,
               mentionEveryone: PermValue.Inherit,
               manageMessages: PermValue.Deny,
               sendTTSMessages: PermValue.Inherit,
               managePermissions: PermValue.Deny,
               createInstantInvite: PermValue.Deny,
               manageChannel: PermValue.Deny,
               manageWebhooks: PermValue.Deny
               );
                        var PChannel = AddingGuild.GetTextChannel(ulong.Parse(channelid));
                        await PChannel.AddPermissionOverwriteAsync(socketuser.User.Value, AllowPerm);
                        await socketuser.Channel.SendMessageAsync("You should have permission now!");

                    }
                    if (socketuser.Emote.Name == "🚫")
                    {
                        await socketuser.Channel.SendMessageAsync("oh well, if you ever change your mind, you can join with the command \".join (ChannelName)\" on the server");

                    }
                }
                else
                {
                    await channel.SendMessageAsync("This is a bit akward, i could not find the spesified channel." + Environment.NewLine + "Please use .join (channelname) on a channel in the server.");
                }
            }

        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            var msg = arg as SocketUserMessage;
            if (msg == null) return;

            var context = new SocketCommandContext(_client, msg);
            int argPost = 0;
            if (msg.HasCharPrefix('.', ref argPost))
            {
                var result = _service.ExecuteAsync(context, argPost);
                if (!result.Result.IsSuccess && result.Result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.Result.ErrorReason);
                }
                Program.Log("Invoked " + msg + " in " + context.Channel + " with " + result.Result, ConsoleColor.Magenta);
            }
            else
            {
                Program.Log(context.Channel + "-" + context.User.Username + " : " + msg, ConsoleColor.White);
            }


        }
        public string StringFinder(string Input, string Split)
        {
            Match match = Regex.Match(Input, Split, RegexOptions.IgnoreCase);
            string value = "";
            if (match.Success)
            {
                value = match.Groups[1].Value; //result here
            }
            return value;
        }
        string all = @"(?<text>[^\]]*)";
        string all2 = @"\s*(.+?)\s*";
        public string[] SplitOnString(string input, string Spliton)
        {
            return input.Split(new string[] { Spliton }, StringSplitOptions.None);
        }

    }
}
