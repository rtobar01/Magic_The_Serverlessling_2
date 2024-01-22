
using Amazon.S3.Model;
using SDL2;
using System.Text.Json.Serialization;

namespace MagicTheServerlessling
{
    public enum zone
    {
        LIBRARY,
        HAND,
        STACK,
        BATTLEFIELD,
        GRAVEYARD,
        EXILE
    }
    public class Card
    {
        public string cardType { get; set; }
        public string cardName { get; set; }
        public int tapped { get; set; }
        public int blue { get; set; }
        public int red { get; set; }
        public int black { get; set; }
        public int white { get; set; }
        public int green { get; set; }
        public int colorless { get; set; }
        public int hasTarget { get; set; }
        public uint power { get; set; }
        public uint toughness { get; set; }
        public int temporalPower { get; set; }
        public int temporalToughness { get; set; }
        public int attacking { get; set; }
        public int blocking { get; set; }
        public int summoningSickness { get; set; }
        public int flying { get; set; }
        public int lifelink { get; set; }
        public int deathtouch { get; set; }
        public int trample { get; set; }
        public int id { get; set; }
        public int cardId { get; set; }
        public Player owner;
        public Card blocker;
        public Card attackerBlocked;
        public zone cardZone;
        public uint damage;
        public int indestructible;
        public Card target;
        public bool[] typesOfValidTargets;
        public bool canCancell = true;
        public bool token;
        public bool focused = false;
        public struct cost
        {
            public uint totalMana;
            public uint[] mana;
        }


        IntPtr texture;
        IntPtr redBorder;
        IntPtr blueBorder;
        IntPtr purpleBorder;
        public uint x, y, w, h;

        [JsonConstructor]
        public Card(string cardName) {
            redBorder = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/cartaRoja.png");
            blueBorder = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/cartaAzul.png");
            purpleBorder = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/cartaMorada.png");
            cardZone = zone.HAND;
            switch (cardName)
            {
                case "SWAMP":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/swamp.png");
                    break;
                case "MOUNTAIN":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/mountain.png");
                    break;
                case "FOREST":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/forest.png");
                    break;
                case "PLAINS":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/plains.png");
                    break;
                case "ISLAND":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/island.png");
                    break;
                case "HAWK":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/vampire_nighthawk.png");
                    break;
                case "GIFTED":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/gifted_aetherborn.png");
                    break;
                case "GARY":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/gary.png");
                    break;
                case "DARK":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/dark_ritual.png");
                    break;
                case "DEMON_OF_C":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/demon_of_catastrophes.png");
                    break;
                case "FAIRY":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/fairy.png");
                    break;
                case "HYMN":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/hymn_to_tourach.png");
                    break;
                case "INFERNAL":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/infernal_grasp.png");
                    break;
                case "MURDER":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/murder.png");
                    break;
                case "SIGN":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/sign_in_blood.png");
                    break;
                case "RATS":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/typhoid_rats.png");
                    break;
                case "PASCAL":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/pascal_lord_of_midnight.png");
                    break;


                case "LLANOWAR":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/llanowar_elves.png");
                    break;
                case "MYSTIC":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/elvish_mystic.png");
                    break;
                case "KALONIAN":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/kalonian_tusker.png");
                    break;
                case "BALOTH":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/leatherback_baloth.png");
                    break;
                case "GIANT":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/giant_growth.png");
                    break;
                case "ASPECT":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/aspect_of_hydra.png");
                    break;
                case "BLOSSOM":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/wall_of_blossom.png");
                    break;
                case "SUMMER":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/summer_bloom.png");
                    break;
                case "TERRA":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/terra_stomper.png");
                    break;
                case "POLETTI":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/poletti_greenleaf_assassin.png");
                    break;
                case "FDI":
                    texture = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/cards/fdi.png");
                    break;
            }

        }
        public void render(uint x_, uint y_, uint w_, uint h_)
        {
            if (focused) visualizeRender();
            SDL.SDL_Rect sRect;
            sRect.x = 0;
            sRect.y = 0;
            sRect.w = 2000; //353
            sRect.h = 2000; // 500
            x = x_;
            y = y_;
            w = w_;
            h = h_;
            SDL.SDL_Rect dRect;
            if (attackerBlocked == null)
            {
                dRect.x = (int)x;
                dRect.y = (int)y;
                dRect.w = (int)w;
                dRect.h = (int)h;
                x = (uint)dRect.x;
                y = (uint)dRect.y;
            }
            else
            {
                dRect.x = (int)attackerBlocked.x;
                dRect.y = (int)(attackerBlocked.y+h/2);
                dRect.w = (int)w;
                dRect.h = (int)h;
                x = (uint)dRect.x;
                y = (uint)dRect.y;
            }

            var centro = new SDL.SDL_Point();
            centro.x = (int)(x + (w / 2));
            centro.y = (int)(y + (h / 2));
            if (summoningSickness == 1 && cardZone == zone.BATTLEFIELD)
            {
                SDL.SDL_Rect pBorder;

                pBorder.w = (int)(w + Program.getWUnit() * 2);
                pBorder.h = (int)(h + Program.getHUnit() * 2);
                pBorder.x = (int)(x - Program.getWUnit());
                pBorder.y = (int)(y - Program.getHUnit());
                SDL.SDL_RenderCopy(Program.renderer, purpleBorder, ref sRect, ref pBorder);
            }
            if (attacking == 1)
            {
                SDL.SDL_Rect rBorder;
                centro.x = (int)(w / 2);
                centro.y = (int)(h / 2);
                rBorder.w = (int)(w + Program.getHUnit() * 2);
                rBorder.h = (int)(h + Program.getHUnit() * 2);
                rBorder.x = (int)(x + Program.getHUnit());
                rBorder.y = (int)(y - Program.getHUnit());
                SDL.SDL_RenderCopyEx(Program.renderer, redBorder, ref sRect, ref rBorder, 90, ref centro, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
            }
            if (blocking == 1) 
            {
                SDL.SDL_Rect bBorder;

                bBorder.w = (int)(w + Program.getWUnit() * 2);
                bBorder.h = (int)(h + Program.getHUnit() * 2);
                bBorder.x = (int)(x - Program.getWUnit());
                bBorder.y = (int)(y - Program.getHUnit());
                SDL.SDL_RenderCopy(Program.renderer, blueBorder, ref sRect, ref bBorder);
            }
            if (temporalPower != 0) 
            {
                SDL.SDL_Rect sRectStats;
                sRectStats.x = 570 / 5 * (temporalPower % 10); ;
                sRectStats.y = 0;
                sRectStats.w = 570/5; //353
                sRectStats.h = 130; // 500
                if (sRectStats.x >= 570)
                {
                    sRectStats.x -= 570;
                    sRectStats.y = 130;
                }
                SDL.SDL_Rect dRectStats;
                dRectStats.x = (int)(x+w/4);
                dRectStats.y = (int)(y+h);
                dRectStats.w = (int)(w/4);
                dRectStats.h = (int)(h/4);
                SDL.SDL_RenderCopy(Program.renderer, Program.game.ui.numeros, ref sRectStats, ref dRectStats);

                sRectStats.x = 570 / 5 * (temporalPower / 10); ;
                sRectStats.y = 0;
                sRectStats.w = 570 / 5; //353
                sRectStats.h = 130; // 500
                if (sRectStats.x >= 570)
                {
                    sRectStats.x -= 570;
                    sRectStats.y = 130;
                }
                dRectStats.x = (int)(x);
                dRectStats.y = (int)(y+h);
                dRectStats.w = (int)(w / 4);
                dRectStats.h = (int)(h / 4);
                SDL.SDL_RenderCopy(Program.renderer, Program.game.ui.numeros, ref sRectStats, ref dRectStats);
            }
            if (temporalToughness != 0)
            {
                SDL.SDL_Rect sRectStats;
                sRectStats.x = 570 / 5 * (temporalToughness % 10); ;
                sRectStats.y = 0;
                sRectStats.w = 570 / 5; //353
                sRectStats.h = 130; // 500
                if (sRectStats.x >= 570)
                {
                    sRectStats.x -= 570;
                    sRectStats.y = 130;
                }
                SDL.SDL_Rect dRectStats;
                dRectStats.x = (int)(x + 3*w / 4);
                dRectStats.y = (int)(y + h);
                dRectStats.w = (int)(w / 4);
                dRectStats.h = (int)(h / 4);
                SDL.SDL_RenderCopy(Program.renderer, Program.game.ui.numeros, ref sRectStats, ref dRectStats);

                sRectStats.x = 570 / 5 * (temporalToughness / 10); ;
                sRectStats.y = 0;
                sRectStats.w = 570 / 5; //353
                sRectStats.h = 130; // 500
                if (sRectStats.x >= 570)
                {
                    sRectStats.x -= 570;
                    sRectStats.y = 130;
                }
                dRectStats.x = (int)(x+2*w/4);
                dRectStats.y = (int)(y + h);
                dRectStats.w = (int)(w / 4);
                dRectStats.h = (int)(h / 4);
                SDL.SDL_RenderCopy(Program.renderer, Program.game.ui.numeros, ref sRectStats, ref dRectStats);
            }
            if (tapped == 0 || cardZone == zone.HAND)
                SDL.SDL_RenderCopy(Program.renderer, texture, ref sRect, ref dRect);
            else
            {
                centro.x = (int)(w / 2);
                centro.y = (int)(h / 2);
                var beta = w;
                w = h;
                h = beta;
                SDL.SDL_RenderCopyEx(Program.renderer, texture, ref sRect, ref dRect, 90, ref centro, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
            }
        }
        public void visualizeRender()
        {
            SDL.SDL_Rect dRect;
            dRect.x = (int)(80 * Program.getWUnit());
            dRect.y = 0;
            dRect.w = (int)(20 * Program.getWUnit());
            dRect.h = (int)(40 * Program.getHUnit());
            SDL.SDL_RenderCopy(Program.renderer, texture, IntPtr.Zero, ref dRect);
        }
        // Función que lanza la carta elegida. Las comprobaciones de timing y prioridad se hacen en X
        public void playCard(Player player)
        {
            using (StreamWriter writer = new StreamWriter("input.txt"))
            {
                writer.WriteLine("PLAY_CARD");
                writer.WriteLine(Program.gameId);
                writer.WriteLine(player.playerId);
                writer.WriteLine(cardId);
                writer.Close();
            }
            Program.UploadFileAsync("input.txt", "playCard/" + Program.gameId + ".txt").Wait();
        }
        public void activateAbility(Player player,bool owner_)
        {
            string action;
            if (!owner_ && Program.game.getPhase() == phase.BLOCK) action = "BLOCK_ATTACKER";
            else if (!owner_) action = "TARGET_ENEMY";
            else action = "TARGET_ALLY";
                using (StreamWriter writer = new StreamWriter("input.txt"))
                {
                    writer.WriteLine("ACTIVATE_ABILITY");
                    writer.WriteLine(Program.gameId);
                    writer.WriteLine(player.playerId);
                    writer.WriteLine(cardId);
                    writer.WriteLine(action);
                    writer.Close();
                }
                Program.UploadFileAsync("input.txt", "activateAbility/" + Program.gameId + ".txt").Wait();
        }
        public string getCardType()
        {
            return cardType;
        }
        public void destroy()
        {
            cardZone = zone.GRAVEYARD;
            owner.battlefield.Remove(this);
        }
        public void discard()
        {
            cardZone = zone.GRAVEYARD;
            owner.hand.Remove(this);
            owner.cardsInHand--;
        }
    }
}
