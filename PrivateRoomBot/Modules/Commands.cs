using Discord;
using Discord.Addons.EmojiTools;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestEasyBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("Ping")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }
        [Command("Help")]
        [Alias(".?")]
        public async Task Help()
        {
            await Context.Channel.SendMessageAsync(
                "**.NewRoom [ChannelName]** (This lets you create a new private room)" + Environment.NewLine +
                "**.RemoveRoom** (Deletes the textchannel if its a private room)" + Environment.NewLine +
                "**.LeaveRoom** (Leaves the channel, if empty it will also delete the channel.)" + Environment.NewLine +
                "**.JoinRoom [ChannelName]** (Joins the spesified channel.[Must be used in a guild channel])" + Environment.NewLine +
                "**.Invite [Username or UserID] ** (Sends a invite message to the user)" 

                );
        }
        [Command("NewRoom")]
        [Alias("new")]
        public async Task CreateRoom(string Name)
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
                             managePermissions: PermValue.Allow,
                             createInstantInvite: PermValue.Deny,
                             manageChannel: PermValue.Allow,
                             manageWebhooks: PermValue.Deny
                             );
            var Channel = await Context.Guild.CreateTextChannelAsync(Name);
            await Channel.ModifyAsync((TextChannelProperties x) => x.Topic = "_Private-Chat_" + "Admins : " + Context.User.Id);

            await Channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, Discord.OverwritePermissions.DenyAll(Channel), Discord.RequestOptions.Default);

            await Channel.AddPermissionOverwriteAsync(Context.User, AllowPerm);


        }
        [Command("RemoveRoom")]
        [Alias("del")]
        public async Task DeleteRoom()
        {
            
            var topic = Context.Guild.GetTextChannel(Context.Channel.Id).Topic;
            if (topic.StartsWith("_Private-Chat_"))
            {
           
                if (topic.Contains(Context.User.Id.ToString()) || Context.Guild.GetUser(Context.User.Id).GuildPermissions.ManageChannels)
                {
                    await Context.Guild.GetTextChannel(Context.Channel.Id).DeleteAsync();
                }
                else
                {
                    await Context.Channel.SendMessageAsync("You dont have permission!");
                }

            }
            else
            {
                await Context.Channel.SendMessageAsync("Channel is not a private room");
            }
        }
        [Command("JoinRoom")]
        [Alias("join")]
        public async Task JoinRoom(string name)
        {
            SocketTextChannel Channel = null;
            int n;
            bool isNumeric = int.TryParse(name, out n);
            if (isNumeric)
            {
                Channel = Context.Guild.GetTextChannel(ulong.Parse(name));
            }
            else
            {

                foreach (var channel in Context.Guild.TextChannels)
                {
                    if (channel.Name == name && channel.Topic.StartsWith("_Private-Chat_"))
                    {
                        Channel = channel;
                    }
                }
            }
            if (Channel != null)
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
                await Channel.AddPermissionOverwriteAsync(Context.User, AllowPerm);
            }
            else
            {
                await Context.Channel.SendMessageAsync("Channel doesnt exist!");
            }
        }
        [Command("LeaveRoom")]
        [Alias("leave")]
        public async Task LeaveRoom()
        {
            var Channel = Context.Guild.GetTextChannel(Context.Channel.Id);

            if (Channel.Topic.StartsWith("_Private-Chat_"))
            {
                await Channel.RemovePermissionOverwriteAsync(Context.User);

                int i = 0;
                foreach (var permover in Channel.PermissionOverwrites)
                {
                    if (permover.TargetId != Context.Guild.EveryoneRole.Id)
                    {
                        i++;
                    }

                }
                if (i == 0)
                {
                    await Channel.DeleteAsync();
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("Channel is not a private room");
            }

        }
        [Command("InviteUser")]
        [Alias("invite")]
        public async Task SendInvite(string user)
        {
            var Channel = Context.Guild.GetTextChannel(Context.Channel.Id);
            SocketGuildUser Founduser = null;
            if (Channel.Topic != null)
            {


                if (Channel.Topic.StartsWith("_Private-Chat_"))
                {
                    Int64 n;
                    bool isNumeric = Int64.TryParse(user, out n);
                    if (isNumeric)
                    {
                        Founduser = Context.Guild.Users.Single(x => x.Id == ulong.Parse(user));
                    }
                    else
                    {
                        Founduser = Context.Guild.Users.Single(x => x.Username.ToLower() == user.ToLower());
                    }
                    if (Founduser != null)
                    {
               
                        var msg = await Founduser.SendMessageAsync(
                            "Hi there!" + Environment.NewLine +
                            " " + Environment.NewLine +
                            Context.User.Username + " from " + Context.Guild.Name + " have invited you to join channel " + Channel.Name + "!" + Environment.NewLine +
                            " " + Environment.NewLine +
                            "Do you want to join " + Channel.Name + "(" + Channel.Id + ")" + " on " + Channel.Guild.Name + "(" + Channel.Guild.Id + ")?"
                            
                          
                            );
                        Thread.Sleep(1000);
                        await msg.AddReactionAsync(EmojiExtensions.FromText("::white_check_mark::"));
                        await msg.AddReactionAsync(EmojiExtensions.FromText(":no_entry_sign:"));
                    }
                    else
                    {
                        await Channel.SendMessageAsync("Could'nt find user");
                    }



                }
                else
                {
                    await Channel.SendMessageAsync("Command only works for Private rooms");
                }
            }
            else
            {
                await Channel.SendMessageAsync("Command only works for Private rooms");
            }
        }




        [Command("info")]
        public async Task Info()
        {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync(
                $"{Format.Bold("Info")}\n" +
                $"- Author: SindreMA#9630\n" +
                $"- Library: Discord.Net ({DiscordConfig.Version})\n" +
                $"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n" +
                $"- Uptime: {GetUptime()}\n\n" +

                $"{Format.Bold("Stats")}\n" +
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}"
            );
        }
        [Command("setgame")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetGame([Remainder]string text)
        {

            await Context.Client.SetGameAsync(text);


        }
        private static string GetUptime()
           => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

    }
}
