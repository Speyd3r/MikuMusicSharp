﻿using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Entities;
using System.Linq;
using System.Collections.Generic;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using MikuMusicSharp.BotClass.BotNew;

namespace BetaPlush.Commands
{
    class VoiceNew : BaseCommandModule
    {
        [Command("join"), Description("Bot join voicechannel")]
        public async Task Join(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            var con = Bot.guit[0].LLinkCon;
            if (chn == null || pos == -1 || bot.VoiceState?.Channel == chn) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            Bot.guit[pos].LLGuild = await con.ConnectAsync(chn);
            Bot.guit[pos].LLGuild.PlaybackFinished += Bot.guit[pos].audioEvents.PlayFin;
            Bot.guit[pos].LLGuild.TrackException += Bot.guit[pos].audioEvents.PlayErr;
            Bot.guit[pos].LLGuild.TrackStuck += Bot.guit[pos].audioEvents.PlayStu;
            Console.WriteLine($"[{ctx.Guild.Id}] Joined");
            await ctx.RespondAsync("Heya!");
            await Task.CompletedTask;
        }
        [Command("leave"), Description("Bot leaves voicechannel")]
        public async Task Leave(CommandContext ctx, string LeaveOptions = null)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            var con = Bot.guit[0].LLinkCon;
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            if (LeaveOptions == "reset" || LeaveOptions == "r") {
                await Task.Run(() => Bot.guit[pos].audioFunc.Leave(pos));
                Bot.guit[pos].LLGuild.Disconnect();
                Bot.guit[pos].LLGuild.PlaybackFinished -= Bot.guit[pos].audioEvents.PlayFin;
                Bot.guit[pos].LLGuild.TrackException -= Bot.guit[pos].audioEvents.PlayErr;
                Bot.guit[pos].LLGuild.TrackStuck -= Bot.guit[pos].audioEvents.PlayStu;
                Bot.guit[pos].LLGuild = null;
                Bot.guit[pos].queue.Clear();
                Bot.guit[pos].playnow = null;
            } else {
                await Task.Run(() => Bot.guit[pos].audioFunc.Leave(pos));
                Bot.guit[pos].LLGuild.Disconnect();
                Bot.guit[pos].LLGuild.PlaybackFinished -= Bot.guit[pos].audioEvents.PlayFin;
                Bot.guit[pos].LLGuild.TrackException -= Bot.guit[pos].audioEvents.PlayErr;
                Bot.guit[pos].LLGuild.TrackStuck -= Bot.guit[pos].audioEvents.PlayStu;
                Bot.guit[pos].LLGuild = null;
            }
            await ctx.RespondAsync("Bye bye~! uwu");
            Console.WriteLine($"[{ctx.Guild.Id}] Left VC");
            await Task.CompletedTask;
        }
        [Command("pause"), Description("pause playback")]
        public async Task Pause(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            if (Bot.guit[pos].paused == true) {
                await Task.Run(() => Bot.guit[pos].audioFunc.Resume(pos));
                await ctx.RespondAsync($"**Resumed**");
                Console.WriteLine($"[{ctx.Guild.Id}] Resumed");
            } else {
                await Task.Run(() => Bot.guit[pos].audioFunc.Pause(pos));
                await ctx.RespondAsync($"**Paused**");
                Console.WriteLine($"[{ctx.Guild.Id}] Paused");
            }
            await Task.CompletedTask;
        }
        [Command("resume"), Aliases("unpause")]
        public async Task Resume(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            if (Bot.guit[pos].playing) await Task.Run(() => Bot.guit[pos].audioFunc.Resume(pos));
            else Bot.guit[pos].audioPlay.QueueLoop(pos, ctx);
            await ctx.RespondAsync($"**Resumed**");
            Console.WriteLine($"[{ctx.Guild.Id}] Resumed");
            await Task.CompletedTask;
        }
        [Command("repeat"), Aliases("r")]
        public async Task Repeat(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            await Task.Run(() => Bot.guit[pos].audioFunc.Repeat(pos));
            await ctx.RespondAsync($"Repeat set to {Bot.guit[pos].repeat}");
            Console.WriteLine($"[{ctx.Guild.Id}] Repeat set to {Bot.guit[pos].repeat}");
            await Task.CompletedTask;
        }
        [Command("repeatall"), Aliases("ra")]
        public async Task RepeatAll(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            await Task.Run(() => Bot.guit[pos].audioFunc.RepeatAll(pos));
            await ctx.RespondAsync($"Repeat all set to {Bot.guit[pos].repeatAll}");
            Console.WriteLine($"[{ctx.Guild.Id}] RepeatAll set to {Bot.guit[pos].repeatAll}");
            await Task.CompletedTask;
        }
        [Command("shuffle"), Aliases("s")]
        public async Task Shuffle(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            await Task.Run(() => Bot.guit[pos].audioFunc.Shuffle(pos));
            await ctx.RespondAsync($"Shuffle set to {Bot.guit[pos].shuffle}");
            Console.WriteLine($"[{ctx.Guild.Id}] Shuffle set to {Bot.guit[pos].shuffle}");
            await Task.CompletedTask;
        }
        [Command("playlist"), Aliases("pp", "pl")]
        public async Task Playlist(CommandContext ctx, string uri)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            var con = Bot.guit[0].LLinkCon;
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            if (!uri.StartsWith("https://") && !uri.StartsWith("http://")) {
                await ctx.RespondAsync("no valid playlist link");
                return;
            }
            var datrack = await con.GetTracksAsync(new Uri(uri));
            int couldadd = datrack.Tracks.Count();
            foreach (var dracks in datrack.Tracks) {
                if (dracks.Author == null) {
                    couldadd--;
                    continue;
                }
                Bot.guit[pos].queue.Add(new Gsets2 {
                    LavaTrack = dracks,
                    requester = ctx.Member,
                    addtime = DateTime.Now
                });
            }
            await ctx.RespondAsync($"Added {datrack.Tracks.Count()} songs to queue ({Bot.guit[pos].queue.Count} in queue now)");
            if (couldadd != datrack.Tracks.Count()) {
                await ctx.RespondAsync("Not all Songs were loaded, this could be due to an error or the video being blocked");
            }
            Console.WriteLine($"[{ctx.Guild.Id}] Playlist loaded {uri}");
            await Task.CompletedTask;
        }
        [Command("queueclear"), Aliases("qc")]
        public async Task QueueClear(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            if (Bot.guit[pos].playing) {
                Bot.guit[pos].queue.RemoveRange(1, Bot.guit[pos].queue.Count - 1);
            } else {
                Bot.guit[pos].queue.Clear();
            }
            Console.WriteLine($"[{ctx.Guild.Id}] Queue cleard");
            await ctx.RespondAsync("Cleared Queue");
        }
        [Command("queue"), Aliases("q")]
        public async Task Queue(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            await Task.Run(() => Bot.guit[pos].audioQueue.QueueList(pos, ctx));
            Console.WriteLine($"[{ctx.Guild.Id}] Showing queue");
            await Task.CompletedTask;
        }
        [Command("volume"), Aliases("vol")]
        public async Task Volume(CommandContext ctx, int vol = 100)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            var con = Bot.guit[0].LLinkCon;
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            if (vol > 150) vol = 150;
            await Task.Run(() => Bot.guit[pos].LLGuild.SetVolume(vol));
            await ctx.RespondAsync($"Volume changed to **{vol}** (150 is max)");
            Console.WriteLine($"[{ctx.Guild.Id}] Volume changed {vol}");
            await Task.CompletedTask;
        }
        [Command("queueremove"), Aliases("qr")]
        public async Task QueueRemove(CommandContext ctx, int r)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            var con = Bot.guit[0].LLinkCon;
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null || r > Bot.guit[pos].queue.Count - 1) {
                await Task.CompletedTask;
                return;
            }
            int pos2 = ctx.Member.Roles.ToList().FindIndex(x => x.CheckPermission(DSharpPlus.Permissions.ManageMessages) == DSharpPlus.PermissionLevel.Allowed);
            int pos3 = ctx.Member.Roles.ToList().FindIndex(x => x.CheckPermission(DSharpPlus.Permissions.Administrator) == DSharpPlus.PermissionLevel.Allowed);
            if (ctx.Member == Bot.guit[pos].queue[r].requester || pos2 != -1 || pos3 != -1) {
                await ctx.RespondAsync($"Removed: **{Bot.guit[pos].queue[r].LavaTrack.Title}** by **{Bot.guit[pos].queue[r].LavaTrack.Author}**");
                await Task.Run(() => Bot.guit[pos].audioQueue.queueRemove(pos, r));
            } else {
                await ctx.RespondAsync("You need the manage messages permission to delete others tracks");
            }
            Console.WriteLine($"[{ctx.Guild.Id}] Song Removed");
            await Task.CompletedTask;
        }
        [Command("nowplaying"), Aliases("np")]
        public async Task NowPlaying(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            var con = Bot.guit[0].LLinkCon;
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            }
            Bot.guit[pos].cmdChannel = ctx.Channel.Id;
            var eb = new DiscordEmbedBuilder{
                Color = new DiscordColor("#68D3D2"),
                Title = "Now Playing",
                Description = "**__Current Song:__**",
                ThumbnailUrl = ctx.Client.CurrentUser.AvatarUrl
            };
            var que = Bot.guit[pos].playnow;
            if (que.LavaTrack.Uri.ToString().Contains("youtu")) {
                try {
                    var youtubeService = new YouTubeService(new BaseClientService.Initializer(){
                        ApiKey = "AIzaSyDbj184qjOOS8fE6PlHcuyasA8VB_gr_f0",
                        ApplicationName = this.GetType().ToString()
                    });
                    var searchListRequest = youtubeService.Search.List("snippet");
                    searchListRequest.Q = que.LavaTrack.Title + " " + que.LavaTrack.Author;
                    searchListRequest.MaxResults = 1;
                    searchListRequest.Type = "video";
                    string time1 = "";
                    string time2 = "";
                    if (que.LavaTrack.Length.Hours < 1) {
                        time1 = Bot.guit[pos].LLGuild.CurrentState.PlaybackPosition.ToString(@"mm\:ss");
                        time2 = que.LavaTrack.Length.ToString(@"mm\:ss");
                    } else {
                        time1 = Bot.guit[pos].LLGuild.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss");
                        time2 = que.LavaTrack.Length.ToString(@"hh\:mm\:ss");
                    }
                    var searchListResponse = await searchListRequest.ExecuteAsync();
                    eb.AddField($"{que.LavaTrack.Title} ({time1}/{time2})", $"[Video Link]({que.LavaTrack.Uri})\n" +
                        $"[{que.LavaTrack.Author}](https://www.youtube.com/channel/" + searchListResponse.Items[0].Snippet.ChannelId + ")");
                    if (searchListResponse.Items[0].Snippet.Description.Length > 500) eb.AddField("Description", searchListResponse.Items[0].Snippet.Description.Substring(0, 500) + "...");
                    else eb.AddField("Description", searchListResponse.Items[0].Snippet.Description);
                    eb.WithImageUrl(searchListResponse.Items[0].Snippet.Thumbnails.High.Url);
                } catch {
                    eb.AddField($"{que.LavaTrack.Title} ({que.LavaTrack.Length})", $"By {que.LavaTrack.Author}\n[Link]({que.LavaTrack.Uri})\nRequested by {que.requester.Mention}");
                }
            } else {
                string time1 = "";
                string time2 = "";
                if (que.LavaTrack.Length.Hours < 1) {
                    time1 = Bot.guit[pos].LLGuild.CurrentState.PlaybackPosition.ToString(@"mm\:ss");
                    time2 = que.LavaTrack.Length.ToString(@"mm\:ss");
                } else {
                    time1 = Bot.guit[pos].LLGuild.CurrentState.PlaybackPosition.ToString(@"hh\:mm\:ss");
                    time2 = que.LavaTrack.Length.ToString(@"hh\:mm\:ss");
                }
                eb.AddField($"{que.LavaTrack.Title} ({time1}/{time2})", $"By {que.LavaTrack.Author}\n[Link]({que.LavaTrack.Uri})\nRequested by {que.requester.Mention}");
            }
            await ctx.RespondAsync(embed: eb.Build());
            Console.WriteLine($"[{ctx.Guild.Id}] NowPlaying");
            await Task.CompletedTask;
        }
        [Command("stop")]
        public async Task Stop(CommandContext ctx, string Options = null)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            var con = Bot.guit[0].LLinkCon;
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            } if (Bot.guit[pos].playnow.LavaTrack.IsStream) {
                Bot.guit[pos].playnow.sstop = true;
            } if (Options == "reset") {
                Bot.guit[pos].repeat = false;
                Bot.guit[pos].repeatAll = false;
                Bot.guit[pos].shuffle = false;
                if (Bot.guit[pos].playing) {
                    Bot.guit[pos].queue.RemoveRange(1, Bot.guit[pos].queue.Count - 1);
                } else {
                    Bot.guit[pos].queue.Clear();
                }
            }
            var stop = Bot.guit[pos].audioFunc.Stop(pos);
            stop.Wait();
            await Task.Delay(2500);
            Bot.guit[pos].stoppin = false;
            Console.WriteLine($"[{ctx.Guild.Id}] Stopped");
            await ctx.RespondAsync("Stopped!");
            await Task.CompletedTask;
        }
        [Command("play"),Aliases("p"), Cooldown(1, 5, CooldownBucketType.Guild)]
        public async Task Play(CommandContext ctx, [RemainingText] string Song = null){
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            var con = Bot.guit[0].LLinkCon;
            if (chn == null || pos == -1) {
                await Task.CompletedTask;
                return;
            }
            if (bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null || !Bot.guit[pos].LLGuild.IsConnected){
                if (Bot.guit[pos].LLGuild == null)
                {
                    Bot.guit[pos].LLGuild = await con.ConnectAsync(chn);
                    Bot.guit[pos].LLGuild.PlaybackFinished += Bot.guit[pos].audioEvents.PlayFin;
                    Bot.guit[pos].LLGuild.TrackException += Bot.guit[pos].audioEvents.PlayErr;
                    Bot.guit[pos].LLGuild.TrackStuck += Bot.guit[pos].audioEvents.PlayStu;
                }
                else
                {
                    Bot.guit[pos].LLGuild = await con.ConnectAsync(chn);
                }
            }
            var QueueCount = Bot.guit[pos].queue.Count;
            var B = Bot.guit[pos];
            if(QueueCount != 0 && !B.playing && Song == null) {
                Console.WriteLine($"[{ctx.Guild.Id}] Continuing queue");
                await ctx.RespondAsync("continouing queue/starting preloaded playlist");
                Bot.guit[pos].audioPlay.QueueLoop(pos, ctx);
            }
            else if(QueueCount == 0 && !B.playing && Song == null) {
                Console.WriteLine($"[{ctx.Guild.Id}] No Song Proviuded");
                await ctx.RespondAsync("Please provide a songname or URL");
            }
            else if(QueueCount == 0 && !B.playing && Song != null) {
                Console.WriteLine($"[{ctx.Guild.Id}] Playing {Song}");
                Bot.guit[pos].audioPlay.PlaySong(pos, ctx, Song);
            }
            else {
                Console.WriteLine($"[{ctx.Guild.Id}] Adding {Song}");
                var Q = Bot.guit[pos].audioPlay.QueueSong(pos, ctx, Song);
                Q.Wait();
            }
            await Task.CompletedTask;
        }
        [Command("skip")]
        public async Task Skip(CommandContext ctx)
        {
            var chn = ctx.Member.VoiceState?.Channel;
            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            var pos = Bot.guit.FindIndex(x => x.GID == ctx.Guild.Id);
            if (chn == null || pos == -1 || bot.VoiceState?.Channel != chn || Bot.guit[pos].LLGuild == null) {
                await Task.CompletedTask;
                return;
            } if (Bot.guit[pos].playnow.LavaTrack.IsStream) {
                Bot.guit[pos].playnow.sstop = true;
            }
            var stop = Bot.guit[pos].audioFunc.Skip(pos);
            stop.Wait();
            Console.WriteLine($"[{ctx.Guild.Id}] Skipped");
            await ctx.RespondAsync("Skipped!");
        }
    }
}
