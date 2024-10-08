using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.UI;
using Microsoft.Xna.Framework;

namespace terraguardians.Companions.Leopold
{
    public class HeldByAdaBehavior : BehaviorBase
    {
        TerraGuardian Ada = null;
        ushort HeldTime = 0;

        public HeldByAdaBehavior(TerraGuardian companion)
        {
            if (!companion.IsSameID(CompanionDB.Ada))
            {
                Deactivate();
                return;
            }
            Ada = companion;
            HeldTime = (ushort)(Main.rand.Next(90, 180) * 60);
        }

        public override void ChangeLobbyDialogueOptions(MessageDialogue Message, out bool ShowCloseButton)
        {
            ShowCloseButton = true;
            Message.AddOption("I wanted to talk with " + Ada.GetNameColored() + ".", OnAskToTalkWithAda);
            Message.AddOption("Help him out.", OnHelpLeopoldOut);
        }

        void OnHelpLeopoldOut()
        {
            Dialogue.LobbyDialogue("*Thank you. She was holding me too tight.*");
            Deactivate();
        }

        void OnAskToTalkWithAda()
        {
            if (Ada != null)
            {
                GetOwner.SaySomething("*I thought you was going to help me here!*");
                Dialogue.StartDialogue(Ada);
            }
        }

        public override void Update(Companion companion)
        {
            if(Ada == null || !Ada.active || Ada.dead || Ada.KnockoutStates > KnockoutStates.Awake || companion.Owner != null)
            {
                Deactivate();
                return;
            }
            DrawOrderInfo.AddDrawOrderInfo(Ada, companion, DrawOrderInfo.DrawOrderMoment.InBetweenParent);
            AffectCompanion(Ada);
            Vector2 Position = Vector2.Zero;
            RunCombatBehavior = Ada.BodyFrameID != 23;
            switch(Ada.BodyFrameID)
            {
                default:
                    Position.X = (3 - 6) * Ada.direction;
                    Position.Y = -6 + 8;
                    break;
                case 23:
                    Position.X = (14 - 6) * Ada.direction;
                    Position.Y = 22 + 8;
                    break;
                case 24:
                case 26:
                    Position.X = (3 - 2) * Ada.direction;
                    Position.Y = -6 + 18;
                    break;
                case 31:
                    Position.X = -Ada.direction * (7 - 6);
                    Position.Y = 6 + 8;
                    break;
                case 30:
                    Position.X = Ada.direction * (-6);
                    Position.Y = 16 + 8;
                    break;
            }
            Position.Y = -64 + Position.Y;
            if (companion.itemAnimation <= 0)
                companion.direction = Ada.direction;
            companion.position = Ada.Bottom + Position * Ada.Scale - (Ada.Base.Scale - Ada.Scale) * new Vector2(0.1f, 1f) * 24;
            if (companion.direction < 0)
                companion.position.X -= companion.width;
            companion.position.Y += Ada.gfxOffY;
            companion.velocity.Y = 0;
            companion.velocity.X = 0;
            companion.gfxOffY = 0;
            if (Ada.whoAmI < companion.whoAmI)
            {
                companion.position += Ada.velocity;
            }
            companion.SetFallStart();
            companion.MoveUp = companion.MoveDown = companion.MoveLeft = companion.MoveRight = false;
            companion.ControlJump = false;
            HeldTime--;
            if (HeldTime == 0)
            {
                Deactivate();
            }
        }

        public override void UpdateAnimationFrame(Companion companion)
        {
            short FrameID = 29;
            switch(Ada.BodyFrameID)
            {
                case 23:
                    FrameID = 21;
                    break;
                case 26:
                    FrameID = 24;
                    break;
                case 31:
                    FrameID = 15;
                    break;
                case 30:
                    FrameID = 16;
                    break;
            }
            companion.BodyFrameID = FrameID;
            TerraGuardian tg = (TerraGuardian)companion;
            for(int i = 0; i < tg.ArmFramesID.Length; i++)
            {
                if (tg.HeldItems[i].ItemAnimation <= 0)
                {
                    tg.ArmFramesID[i] = FrameID;
                }
            }
        }

        public override void UpdateAffectedCompanionAnimationFrame(Companion companion)
        {
            AdaBase.ApplyHeldBunnyAnimation((TerraGuardian)companion, true);
        }

        public override void OnEnd()
        {
            if (Ada != null && GetOwner != null)
            {
                GetOwner.Teleport(Ada);
            }
        }

        public override void ChangeDrawMoment(Companion companion, ref CompanionDrawMomentTypes DrawMomentType)
        {
            if (Ada.IsBeingControlledBySomeone)
                DrawMomentType = CompanionDrawMomentTypes.DrawInBetweenOwner;
        }
    }
}