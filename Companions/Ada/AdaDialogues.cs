using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using System.Collections.Generic;

namespace terraguardians.Companions
{
    public class AdaDialogues : CompanionDialogueContainer
    {
        public override string GreetMessages(Companion companion)
        {
            companion.SaySomething("Run GreetMessages",true);
            switch (Main.rand.Next(4))
            {
                case 0:
                    return "*Hello Terrarian. I'm a Werewolf? What is a Werewolf?*";//"\"Is... That... a Werewolf? I don't think so... It's a taller one?\"";
                case 1:
                    return "*Hi. I didn't expect to meet someone else here.*";//"\"As soon as I got closer to it, that... Wolf? Friendly waved at me.\"";
                case 2:
                    return "*Are you here for camping too? I love sitting by the fire sometimes.*"; //"\"She is asking me If I'm camping too.\"";
                default:
                    return "*Hello. I was about to set up a campfire here, want to join in?*"; //"\"She seems to be enjoying the bonfire, until I showed up.\"";
            }
        }
        
        // 对话触发策略  -  打开对话框时触发
        public override string NormalMessages(Companion companion)
        {
            companion.SaySomething("Run NormalMessages",true);
            List<string> Mes = new List<string>();
            List<string> boredomMessages = new List<string>
            {
                "*我好无聊啊……想玩个游戏，但似乎没有人合适。*",
                "*有人想玩吗？我有点坐不住了……*",
                "*我现在真的需要点乐趣。有人想一起玩吗？*",
                "*这里太无聊了。玩个游戏会让气氛活跃起来！*"
            };
            string message = boredomMessages[Main.rand.Next(boredomMessages.Count)];
            Mes.Add(message);
            return Mes[Main.rand.Next(Mes.Count)];
        }

        public override string TalkMessages(Companion companion)
        {
            companion.SaySomething("Run TalkMessages", true);
            Player player = MainMod.GetLocalPlayer;
            List<string> Mes = new List<string>();
            if (HasCompanionSummoned(0, ControlledToo: false))
                Mes.Add("*哦，你带了[gn:0]过来……你有“唠叨药水”吗？*");
            if (CanTalkAboutCompanion(0) && WorldMod.CompanionNPCs[WorldMod.GetCompanionNpcPosition(0)].Distance(player.Center) < 1024f)
                Mes.Add("*能把[gn:0]送到远一点的地方吗？*");
            if (CanTalkAboutCompanion(3) && WorldMod.CompanionNPCs[WorldMod.GetCompanionNpcPosition(3)].Distance(player.Center) >= 768f)
                Mes.Add("*说起来，[gn:3]也住在这里吧？我能搬到他附近吗？*");
            if (NPC.AnyNPCs(Terraria.ID.NPCID.WitchDoctor))
                Mes.Add("*告诉我，你最喜欢的毒药是什么？*");
            if (!HasCompanionSummoned(1, ControlledToo: false))
            {
                Mes.Add("*你觉得我房间的改造怎么样？*");
                Mes.Add("*我想和你一起环游世界。带我去吧。*");
                Mes.Add("*你能帮我搬些家具吗？*");
                Mes.Add("*这些跳蚤真让我受不了。你有杀跳蚤的药方吗？*");
            }
            else
            {
                if (NPC.AnyNPCs(Terraria.ID.NPCID.Stylist))
                    Mes.Add("*我想去拜访[nn:" + Terraria.ID.NPCID.Stylist + "]。*");
                if (Main.moonPhase == 0)
                {
                    if (!HasCompanion(3))
                    {
                        Mes.Add("*抱歉……我在想……某个人……*");
                    }
                    else
                    {
                        Mes.Add("*我总是喜欢满月，因为它让我想起[gn:3]。*");
                    }
                }
            }
            if (HasCompanionSummoned(2))
            {
                Mes.Add("*嘿，[gn:2]，想玩个游戏吗？*（[gn:2]现在有点慌。）*");
            }
            if (HasCompanionSummoned(3))
            {
                Mes.Add("*见到你真好，[gn:3]……*（她看起来很高兴见到[gn:3]，但也有些伤感。）*");
            }
            else if (HasCompanion(3))
            {
                Mes.Add("*我在想……有没有办法让[gn:3]恢复以前的样子？*");
            }
            if (CanTalkAboutCompanion(3))
                Mes.Add("*我承认，我最初来到这个世界是为了找[gn:3]，但看到这里的环境如此美丽，我决定多待一段时间。既然[gn:3]也在这里，我们就可以待得更久。*");
            Mes.Add("*想去购物吗，[nickname]？那……你能借我一些钱吗？*");
            if (Main.bloodMoon)
                Mes.Add("*我现在气得想杀人！真高兴外面有这么多选择。*");
            return Mes[Main.rand.Next(Mes.Count)];
        }
        
        // 对话触发策略  - 点击我想聊聊时触发
        public override string TalkAboutOtherTopicsMessage(Companion companion, TalkAboutOtherTopicsContext context)
        {
            companion.SaySomething("Run TalkAboutOtherTopicsMessage",true);
            switch(context)
            {
                case TalkAboutOtherTopicsContext.FirstTimeInThisDialogue:
                    return "*哦，你想聊些什么呢？*";
                case TalkAboutOtherTopicsContext.AfterFirstTime:
                    return "*还有什么想聊的吗？*";
                case TalkAboutOtherTopicsContext.Nevermind:
                    return "*好的，那想聊点别的吧？*";
            }
            return base.TalkAboutOtherTopicsMessage(companion, context);
        }

        // 对话触发策略  -  点击你有什么委托吗触发
        public override string RequestMessages(Companion companion, RequestContext context)
        {
            companion.SaySomething("Run RequestMessages",true);
            switch(context)
            {
                case RequestContext.NoRequest:
                    if (Main.rand.NextDouble() < 0.5)
                        return "*我现在不需要任何东西。稍后再来，我可能会需要一些帮助。*";
                    return "*不，我现在不需要任何东西。*";
                case RequestContext.HasRequest:
                    if (Main.rand.NextDouble() < 0.5)
                        return "*我很高兴你问了。我真的有件事情需要处理，但我现在正忙着，如果你能帮我... 这是我的问题，如果你问的话：[objective]*";
                    return "*我很高兴你问了！这里，看看这个：“[objective]”。你能帮我吗？*";
                case RequestContext.Completed:
                    if (Main.rand.NextDouble() < 0.5)
                        return "*我很高兴你完成了这个。谢谢你，[nickname]。*";
                    return "*我真高兴，差点想亲你一下。谢谢！*（她一边微笑一边摇着尾巴）";
                case RequestContext.Failed:
                    return "*我真的很失望，你居然没能完成我的请求。不过没关系... 这没事。*";
                case RequestContext.Accepted:
                    return "*在执行我的请求时要小心哦，[nickname]。*";
                case RequestContext.Rejected:
                    return "*哦... 好吧... 那我就把这个清单留着，以后再做吧。*";
                case RequestContext.TooManyRequests:
                    return "*你无法专注于我的请求，因为你有太多其他请求了。*";
                case RequestContext.PostponeRequest:
                    return "*你觉得这个请求太难了，还是现在无法完成？*";
                case RequestContext.AskIfRequestIsCompleted:
                    return "*你完成我要求的事情了吗？*";
                case RequestContext.RemindObjective:
                    return "*我让你做的是 [objective]。*";
                case RequestContext.CancelRequestAskIfSure:
                    return "*什么？！你想取消我的请求？你确定吗？*";
                case RequestContext.CancelRequestYes:
                    return "*哦.. 好吧.. 完成了...* （现在她的脸上充满了愤怒。快跑吧，[nickname]，快跑！）";
                case RequestContext.CancelRequestNo:
                    return "*呼... （她把爪子放在胸口，松了一口气）你差点吓到我了... 那么，想聊点别的吗？*";
            }
            return base.RequestMessages(companion, context);
        }

        // 对话触发策略  -  加入队伍触发触发
        public override string JoinGroupMessages(Companion companion, JoinMessageContext context)
        {
            companion.SaySomething("Run JoinGroupMessages",true);
            switch(context)
            {
                case JoinMessageContext.Success:
                    return "*我正好厌倦待在家里，走吧，去冒险吧!*";
                case JoinMessageContext.Fail:
                    return "*我现在不想去冒险。*";
                case JoinMessageContext.FullParty:
                    return "*I dislike crowds.*";
            }
            return base.JoinGroupMessages(companion, context);
        }

        // 对话触发策略  -  离开队伍触发触发
        public override string LeaveGroupMessages(Companion companion, LeaveMessageContext context)
        {
            companion.SaySomething("Run LeaveGroupMessages",true);
            switch(context)
            {
                case LeaveMessageContext.Success:
                    return "*再见了，[nickname]。记得去找我寻找的另一只狼同伴。*";
                case LeaveMessageContext.Fail:
                    return "*我现在不打算去别的地方。*";
                case LeaveMessageContext.AskIfSure:
                    return "*你真的想把我留在这里吗？*";
                case LeaveMessageContext.DangerousPlaceYesAnswer:
                    return "*好吧，我想在回家的路上砍砍乐子。到时候见，[nickname]。*";
                case LeaveMessageContext.DangerousPlaceNoAnswer:
                    return "*呼...*";
            }
            return base.LeaveGroupMessages(companion, context);
        }

        public override string MountCompanionMessage(Companion companion, MountCompanionContext context)
        {
            companion.SaySomething("Run MountCompanionMessage",true);
            switch(context)
            {
                case MountCompanionContext.Success:
                    return "*只要你不弄乱我的头发，我不介意。*";
                case MountCompanionContext.SuccessMountedOnPlayer:
                    return "*你要给我搭个便车吗？我的脚谢谢你。*";
                case MountCompanionContext.Fail:
                    return "*不，我不这么认为。*";
                case MountCompanionContext.NotFriendsEnough:
                    return "*没门。你可能会弄乱我的头发。*";
                case MountCompanionContext.SuccessCompanionMount:
                    return "*好吧，只要[target]不弄乱我的头发。*";
                case MountCompanionContext.AskWhoToCarryMount:
                    return "*这要看你想让我背谁。*";
            }
            return base.MountCompanionMessage(companion, context);
        }

        public override string DismountCompanionMessage(Companion companion, DismountCompanionContext context)
        {
            companion.SaySomething("Run DismountCompanionMessage",true);
            switch(context)
            {
                case DismountCompanionContext.SuccessMount:
                    return "*我希望你没有弄乱我的头发。*";
                case DismountCompanionContext.SuccessMountOnPlayer:
                    return "*那我们就回去走路吧。*";
                case DismountCompanionContext.Fail:
                    return "*这似乎不是个好主意。*";
            }
            return base.DismountCompanionMessage(companion, context);
        }

        public override string AskCompanionToMoveInMessage(Companion companion, MoveInContext context)
        {
            companion.SaySomething("Run AskCompanionToMoveInMessage",true);
            switch(context)
            {
                case MoveInContext.Success:
                    return "*当然可以。我喜欢这个世界的环境，还有这里的人。*";
                case MoveInContext.Fail:
                    return "*不.. 我现在不想住在这里..*";
                case MoveInContext.NotFriendsEnough:
                    return "*虽然我喜欢这个地方，但我对你还不够了解。*";
            }
            return base.AskCompanionToMoveInMessage(companion, context);
        }

        public override string AskCompanionToMoveOutMessage(Companion companion, MoveOutContext context)
        {
            companion.SaySomething("Run AskCompanionToMoveOutMessage",true);
            switch(context)
            {
                case MoveOutContext.Success:
                    return "*哎呀.. 我本来很喜欢住在这里的...*";
                case MoveOutContext.Fail:
                    return "*抱歉，我现在不想搬走。*";
                case MoveOutContext.NoAuthorityTo:
                    return "*我对你了解不多。让我搬进来的那个人至少是我的朋友。*";
            }
            return base.AskCompanionToMoveOutMessage(companion, context);
        }

        public override string OnToggleShareChairMessage(Companion companion, bool Share)
        {
            companion.SaySomething("Run OnToggleShareChairMessage",true);
            if (Share) return "*我觉得这样没什么坏处..*";
            return "*我觉得这个主意也不错。*";
        }

        public override string OnToggleShareBedsMessage(Companion companion, bool Share)
        {
            companion.SaySomething("Run OnToggleShareBedsMessage",true);
            if(Share)
            {
                if(MainMod.GetLocalPlayer.Male)
                    return "*只要是为了睡觉，我不介意。*";
                return "*我更喜欢自己一个人睡，但好吧，我可以和你分享。*";
            }
            return "*我对此没有异议。*";
        }

        public override string TacticChangeMessage(Companion companion, TacticsChangeContext context)
        {
            companion.SaySomething("Run TacticChangeMessage",true);
            switch(context)
            {
                case TacticsChangeContext.OnAskToChangeTactic:
                    return "*我打得不好吗？那你有什么建议？*";
                case TacticsChangeContext.ChangeToCloseRange:
                    return "*那我就近身缠住敌人。*";
                case TacticsChangeContext.ChangeToMidRanged:
                    return "*我会准备好我的剑，以防有什么靠近。*";
                case TacticsChangeContext.ChangeToLongRanged:
                    return "*他们根本不知道会发生什么。*";
                case TacticsChangeContext.Nevermind:
                    return "*你想聊点别的吗？*";
                case TacticsChangeContext.FollowAhead:
                    return "*我可以做到。*";
                case TacticsChangeContext.FollowBehind:
                    return "*好吧，那你带路吧。*";
                case TacticsChangeContext.AvoidCombat:
                    return "*我希望你知道你在让我做什么，不过我会照做的。*";
                case TacticsChangeContext.PartakeInCombat:
                    return "*太好了，我开始想念撕扯东西的感觉了。*";
            }
            return base.TacticChangeMessage(companion, context);
        }

        public override string SleepingMessage(Companion companion, SleepingMessageContext context)
        {
            companion.SaySomething("Run SleepingMessage",true);
            switch(context)
            {
                case SleepingMessageContext.WhenSleeping:
                    {
                        List<string> Mes = new List<string>();
                        if (!PlayerMod.PlayerHasCompanion(MainMod.GetLocalPlayer, CompanionDB.Zacks))
                        {
                            Mes.Add("*你在哪里.... 我想你.... 你为什么离开我.... 呼噜....*");
                            Mes.Add("*不... 回来... 别走.... 呼噜...*");
                            Mes.Add("(你可以看到[name]脸上有些泪水。)");
                        }
                        else
                        {
                            Mes.Add("(她似乎睡得很好。)");
                            Mes.Add("(她在睡梦中看起来有点担心。)");
                            Mes.Add("(她似乎在做关于[gn:"+3+" ]的梦。)");
                        }
                        if (PlayerMod.PlayerHasCompanion(MainMod.GetLocalPlayer, CompanionDB.Sardine))
                            Mes.Add("*你跑得再快... 我也会抓到你.... 咬咬咬...* (她一定是在梦里和[gn:"+CompanionDB.Sardine+" ]玩。)");
                        Mes.Add("(她似乎在梦到和其他人露营。)");
                        return Mes[Main.rand.Next(Mes.Count)];
                    }
                case SleepingMessageContext.OnWokeUp:
                    {
                        switch (Main.rand.Next(3))
                        {
                            case 0:
                                return "*嗯？ 你想要什么？ 希望是重要的事情。*";
                            case 1:
                                return "*那么... 你想要的事情，不能等我醒来再说吗？*";
                            case 2:
                                return "*打哈欠~... 你想要什么，[nickname]?*";
                        }
                    }
                    break;
            }
            return base.SleepingMessage(companion, context);
        }

        public override string CompanionMetPartyReactionMessage(Companion WhoReacts, Companion WhoJoined, out float Weight)
        {
            WhoReacts.SaySomething("Run CompanionMetPartyReactionMessage",true);
            if(WhoJoined.ModID == MainMod.mod.Name)
            {
                if(WhoJoined.ID == CompanionDB.Zacks)
                {
                    Weight = 1.5f;
                    return "*太好了，我们终于找到了你，"+WhoJoined.GetNameColored()+"。*";
                }
            }
            Weight = 1f;
            return "*哇，真棒，一个新朋友！*";
        }

        // 对话触发策略  -  加入队伍触发触发
        public override string CompanionJoinPartyReactionMessage(Companion WhoReacts, Companion WhoJoined, out float Weight)
        {
            WhoReacts.SaySomething("Run CompanionJoinPartyReactionMessage",true);
            if (WhoJoined.ModID == MainMod.mod.Name)
            {
                switch (WhoJoined.ID)
                {
                    case CompanionDB.Zacks:
                        Weight = 1.5f;
                        return "*你要加入我们，"+WhoJoined.GetNameColored()+"？我很高兴有你在这里。*";
                    case CompanionDB.Rococo:
                        Weight = 1.2f;
                        return "*你也要加入...太好了...*";
                    case CompanionDB.Sardine:
                        Weight = 1.2f;
                        return "*太完美了，我的牙齿正好需要咬点东西。*";
                }
            }
            Weight = 1f;
            return "*你好。*";
        }

        public override string ControlMessage(Companion companion, ControlContext context)
        {
            companion.SaySomething("Run ControlMessage",true);
            switch(context)
            {
                case ControlContext.SuccessTakeControl:
                    return "*我会把自己交给你。尽量别让我们出事。*";
                case ControlContext.SuccessReleaseControl:
                    return "*好的。希望这对你有帮助。*";
                case ControlContext.FailTakeControl:
                    return "*我现在可不想这么做。*";
                case ControlContext.FailReleaseControl:
                    return "*我觉得现在解除合并不是个好主意。*";
                case ControlContext.NotFriendsEnough:
                    return "*什么？不！我对你几乎一无所知。*";
                case ControlContext.ControlChatter:
                    switch(Main.rand.Next(3))
                    {
                        default:
                            return "*记住，我一直在关注一切，[nickname]。*";
                        case 1:
                            return "*你需要我帮忙吗？*";
                        case 2:
                            return "*即使合并在一起，我依然很漂亮。你不觉得吗？*";
                    }
                case ControlContext.GiveCompanionControl:
                    return "*好的，谢谢。希望你不是为了偷懒才这么做的。*";
                case ControlContext.TakeCompanionControl:
                    return "*好了，尽量别让我们出事。*";
            }
            return base.ControlMessage(companion, context);
        }

        public override string UnlockAlertMessages(Companion companion, UnlockAlertMessageContext context)
        {
            companion.SaySomething("Run UnlockAlertMessages",true);
            switch(context)
            {
                case UnlockAlertMessageContext.MoveInUnlock:
                    return "";
                case UnlockAlertMessageContext.ControlUnlock:
                    return "*[nickname]，我把和我合并的控制权交给你，但要小心使用我的身体哦。*";
                case UnlockAlertMessageContext.FollowUnlock:
                    return "";
                case UnlockAlertMessageContext.MountUnlock:
                    return "*我有个好消息要告诉你，[nickname]，你不再需要走路了，直接跳到我肩膀上吧。只要你不弄乱我的头发，我都不介意。*";
                case UnlockAlertMessageContext.RequestsUnlock:
                    return "";
                case UnlockAlertMessageContext.BuddiesModeUnlock:
                    return "*你好，[nickname]，我一直在想……我觉得你值得信任。如果你想让我成为你的好朋友，我会很高兴接受。*";
                case UnlockAlertMessageContext.BuddiesModeBenefitsMessage:
                    return "*哦，[nickname]，我忘了告诉你一件事。既然我们现在是好朋友，就没有理由不信任你让我做的任何事情，所以如果你需要我做什么，尽管开口。*";
            }
            return base.UnlockAlertMessages(companion, context);
        }

        // 对话触发策略  -  点击你能帮我个忙吗时触发
        public override string InteractionMessages(Companion companion, InteractionMessageContext context)
        {
            companion.SaySomething("Run InteractionMessages",true);
            switch(context)
            {
                case InteractionMessageContext.OnAskForFavor:
                    return "*你需要我帮忙吗？*";
                case InteractionMessageContext.Accepts:
                    return "*我可以做到。*";
                case InteractionMessageContext.Rejects:
                    return "*没门。*";
                case InteractionMessageContext.Nevermind:
                    return "*你不需要我的帮助了？还是只是想问问美容秘诀？*";
            }
            return base.InteractionMessages(companion, context);
        }

        public override string ChangeLeaderMessage(Companion companion, ChangeLeaderContext context)
        {
            companion.SaySomething("Run ChangeLeaderMessage",true);
            switch(context)
            {
                case ChangeLeaderContext.Success:
                    return "*那我就来带头了。*";
                case ChangeLeaderContext.Failed:
                    return "*抱歉，没门。*";
            }
            return "";
        }

        public override string BuddiesModeMessage(Companion companion, BuddiesModeContext context)
        {
            companion.SaySomething("Run BuddiesModeMessage",true);
            switch(context)
            {
                case BuddiesModeContext.AskIfPlayerIsSure:
                    return "*你想让我成为你的好朋友吗？*";
                case BuddiesModeContext.PlayerSaysYes:
                    return "*当然可以，我很荣幸能成为你的好朋友。*";
                case BuddiesModeContext.PlayerSaysNo:
                    return "*这可不是开玩笑的事，[nickname]。*";
                case BuddiesModeContext.NotFriendsEnough:
                    return "*我对你还不够了解呢。*";
                case BuddiesModeContext.Failed:
                    return "*不行。*";
                case BuddiesModeContext.AlreadyHasBuddy:
                    return "*可是你已经有朋友了，记得吗？*";
            }
            return "";
        }

        public override string InviteMessages(Companion companion, InviteContext context)
        {
            companion.SaySomething("Run InviteMessages",true);
            switch(context)
            {
                case InviteContext.Success:
                    return "*好的，我马上就到。*";
                case InviteContext.SuccessNotInTime:
                    return "*现在不行，因为有点晚了。明天我会来。*";
                case InviteContext.Failed:
                    return "*抱歉，不行。*";
                case InviteContext.CancelInvite:
                    return "*你不想让我来了吗？没关系。*";
                case InviteContext.ArrivalMessage:
                    return "*我来了，[nickname]。你有什么事想让我帮忙的吗？*";
            }
            return "";
        }

        public override string GetOtherMessage(Companion companion, string Context)
        {
            companion.SaySomething("Run GetOtherMessage",true);
            switch(Context)
            {
                case MessageIDs.LeopoldEscapedMessage:
                    return "*你为什么让他跑了？他太可爱了。*";
                case MessageIDs.VladimirRecruitPlayerGetsHugged:
                    return "*你们两个认识吗？*";
                //Alexander
                case MessageIDs.AlexanderSleuthingStart:
                    return "*让我看看...*";
                case MessageIDs.AlexanderSleuthingProgress:
                    return "*她真美...*";
                case MessageIDs.AlexanderSleuthingNearlyDone:
                    return "*也许我该约她出去... 不，等一下... 我得专注...*";
                case MessageIDs.AlexanderSleuthingFinished:
                    return "*好了... 完成了。也许可以为将来做个计划...*";
                case MessageIDs.AlexanderSleuthingFail:
                    return "*啊... 不... 你不是在想那个吧！*";
            }
            return base.GetOtherMessage(companion, Context);
        }

        public override string ReviveMessages(Companion companion, Player target, ReviveContext context)
        {
            companion.SaySomething("Run ReviveMessages",true);
            switch(context)
            {
                case ReviveContext.HelpCallReceived:
                    return "*我听到你的呼喊了，让我来帮你。*";
                case ReviveContext.RevivingMessage:
                    {
                        List<string> Mes = new List<string>();
                        bool IsPlayer = !(target is Companion);
                        bool GotMessage = false;
                        if (!IsPlayer)
                        {
                            Companion t2 = target as Companion;
                            if (companion.ModID == t2.ModID)
                            {
                                GotMessage = true;
                                switch (t2.ID)
                                {
                                    default:
                                        GotMessage = false;
                                        break;
                                    case CompanionDB.Zacks:
                                        {
                                            Mes.Add("*不！我差点失去你！别再这样了！*");
                                            Mes.Add("*我不知道这是否有效，请你站起来！*");
                                            Mes.Add("*我不能再失去你了，求你了！*");
                                        }
                                        break;
                                    case CompanionDB.Sardine:
                                        {
                                            Mes.Add("*被击倒可不好玩。*");
                                            Mes.Add("*如果你不醒来，我就咬你！……他还是昏迷不醒。*");
                                            Mes.Add("*好吧，我保证如果你醒来就不追着咬你。请，快醒来！*");
                                        }
                                        break;
                                }
                            }
                        }
                        if (!GotMessage)
                        {
                            Mes.Add("*别担心，你很快就会好的。*");
                            Mes.Add("*来，握住我的手。现在站起来！*");
                            Mes.Add("*我在这里陪着你，休息一下，我来帮你。*");
                        }
                        return Mes[Main.rand.Next(Mes.Count)];
                    }
                case ReviveContext.OnComingForFallenAllyNearbyMessage:
                    return "*别担心，我来了！*";
                case ReviveContext.ReachedFallenAllyMessage:
                    return "*你在我这里是安全的。*";
                case ReviveContext.RevivedByItself:
                    return "*我现在没事了，如果有人在关心的话。*";
                case ReviveContext.ReviveWithOthersHelp:
                    return "*谢谢大家的帮助。*";
            }
            return base.ReviveMessages(companion, target, context);
        }
    }
}
