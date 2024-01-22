
using SDL2;
using System.ComponentModel.Design;

namespace MagicTheServerlessling
{
    public enum phase
    {
        UPKEEP,
        DRAW,
        MAIN,
        ATTACK,
        BLOCK,
        DAMAGE,
        SECONDMAIN,
        END
    }
    public class Game
    {
        public UI ui;
        public Player player, opponent;
        public Card waitingForTarget = null;
        int activePlayer;
        phase phaseId = phase.MAIN;
        public Card currentlyBlocking;
        public Card lastFocusedCard;
        public Card lastSpellCastedopp;
        public Card lastSpellCasted;
        public Game()
        {
            player = new Player(Program.localPlayerId);
            if (Program.localPlayerId == 0) 
            {
                opponent = new Player(1);
            }
            else opponent = new Player(0);
            activePlayer = 0;
            ui = new UI();
        }
        public void render()
        {
            ui.render();
            player.render(false);
            opponent.render(true);
            if (lastSpellCasted != null)
            {
                lastSpellCasted.render((uint)(22 *Program.getWUnit()), 0, (uint)(10 * Program.getWUnit()/1.4), (uint)(10 * Program.getHUnit()));
            }
            if (lastSpellCastedopp != null)
            {
                lastSpellCastedopp.render((uint)(55 * Program.getWUnit()), 0, (uint)(10 * Program.getWUnit()/1.4), (uint)(10 * Program.getHUnit()));
            }
        }

        public bool nextPhase()
        {
                using (StreamWriter writer = new StreamWriter("input.txt"))
                {
                    writer.WriteLine("NEXT_PHASE");
                    writer.WriteLine(Program.gameId);
                    writer.WriteLine(player.playerId);
                    writer.Close();
                }
                Program.UploadFileAsync("input.txt", "nextPhase/" + Program.gameId + ".txt").Wait();
            return true;
        }
        public bool updatePhase(int newPhase)
        {
            phaseId = (phase)newPhase;
            if (phaseId == phase.UPKEEP)
            {
                if (activePlayer == Program.localPlayerId)
                foreach (Card creature in player.battlefield)
                {
                    creature.summoningSickness = 0;
                }
                else
                foreach (Card creature in opponent.battlefield)
                {
                    creature.summoningSickness = 0;
                }
            }
            else if (phaseId == phase.DAMAGE)
            {
                foreach (Card creature in player.battlefield)
                {
                    creature.blocking = 0;
                    creature.attackerBlocked = null;
                    creature.blocker = null;
                }
                foreach (Card creature in opponent.battlefield)
                {
                    creature.blocking = 0;
                    creature.attackerBlocked = null;
                    creature.blocker = null;
                }
            }
            else if (phaseId == phase.SECONDMAIN)
            {
                foreach (Card creature in player.battlefield)
                {
                    creature.attacking = 0;
                }
                foreach (Card creature in opponent.battlefield)
                {
                    creature.attacking = 0;
                }
            }
            return true;
        }
        public void changeActivePlayer(int newActivePlayer)
        {
            activePlayer = newActivePlayer;
        }
        public void handleEvents(SDL.SDL_Event e)
        {
            int x, y;
            SDL.SDL_GetMouseState(out x, out y);
            if (waitingForTarget != null)
            {
                for (int i = 0; i < player.battlefield.Count; i++)
                {
                    if (x >= player.battlefield[i].x && x <= player.battlefield[i].x + player.battlefield[i].w && y >= player.battlefield[i].y && y <= player.battlefield[i].y + player.battlefield[i].h)
                    {
                        if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                        {
                            lastFocusedCard.focused = false;
                            player.battlefield[i].focused = true;
                            lastFocusedCard = player.battlefield[i];
                            return;
                        }
                    }
                }
                for (int i = 0; i < opponent.battlefield.Count; i++)
                {
                    if (x >= opponent.battlefield[i].x && x <= opponent.battlefield[i].x + opponent.battlefield[i].w && y >= opponent.battlefield[i].y && y <= opponent.battlefield[i].y + opponent.battlefield[i].h)
                    {
                        if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                        {
                            lastFocusedCard.focused = false;
                            opponent.battlefield[i].focused = true;
                            lastFocusedCard = opponent.battlefield[i];
                            return;
                        }
                        return;
                    }
                }
            }
            // Observar el ultimo hechizo lanzado por el jugador
            if (lastSpellCasted != null)
            {
                if (x >= lastSpellCasted.x && x <= lastSpellCasted.x + lastSpellCasted.w && y >= lastSpellCasted.y && y <= lastSpellCasted.y + lastSpellCasted.h)
                {
                    if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                    {
                        lastFocusedCard.focused = false;
                        lastSpellCasted.focused = true;
                        lastFocusedCard = lastSpellCasted;
                        return;
                    }
                }
            }
            // Observar el ultimo hechizo lanzado por el oponente
            if (lastSpellCastedopp != null)
            {
                if (x >= lastSpellCastedopp.x && x <= lastSpellCastedopp.x + lastSpellCastedopp.w && y >= lastSpellCastedopp.y && y <= lastSpellCastedopp.y + lastSpellCastedopp.h)
                {
                    if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                    {
                        lastFocusedCard.focused = false;
                        lastSpellCastedopp.focused = true;
                        lastFocusedCard = lastSpellCastedopp;
                        return;
                    }
                }
            }
            // Play card
            for (int i = player.handIndex; i < player.hand.Count; i++)
                {
                    if (x >= player.hand[i].x && x <= player.hand[i].x + player.hand[i].w && y >= player.hand[i].y && y <= player.hand[i].y + player.hand[i].h)
                    {
                        if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                        {
                            lastFocusedCard.focused = false;
                            player.hand[i].focused =true;
                            lastFocusedCard = player.hand[i];
                            return;
                        }
                            player.hand[i].playCard(player);
                        return;
                    }
                }
            // Interact with your cards
            for (int i = 0; i < player.battlefield.Count; i++)
            {
                if (x >= player.battlefield[i].x && x <= player.battlefield[i].x + player.battlefield[i].w && y >= player.battlefield[i].y && y <= player.battlefield[i].y + player.battlefield[i].h)
                {
                    if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                    {
                            lastFocusedCard.focused = false;
                            player.battlefield[i].focused = true;
                            lastFocusedCard = player.battlefield[i];
                            return;
                    }
                    player.battlefield[i].activateAbility(player,true);
                    return;
                }
            }
            // Block opponent
            for (int i = 0; i < opponent.battlefield.Count; i++)
            {
                if (x >= opponent.battlefield[i].x && x <= opponent.battlefield[i].x + opponent.battlefield[i].w && y >= opponent.battlefield[i].y && y <= opponent.battlefield[i].y + opponent.battlefield[i].h)
                {
                    if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
                    {
                        lastFocusedCard.focused = false;
                        opponent.battlefield[i].focused = true;
                        lastFocusedCard = opponent.battlefield[i];
                        return;
                    }
                    opponent.battlefield[i].activateAbility(player,false);
                    return;
                }
            }
            if (e.type == SDL.SDL_EventType.SDL_MOUSEMOTION)
            {
                lastFocusedCard.focused = false;
                return;
            }
            ui.handleEvents(e);
        }
        public phase getPhase()
        {
            return phaseId;
        }
        public int getActivePlayer()
        {
            return activePlayer;
        }
    }
}
