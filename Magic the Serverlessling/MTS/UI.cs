using SDL2;

namespace MagicTheServerlessling
{
    public class UI
    {
        IntPtr BGTex;
        IntPtr BGHandTex;
        IntPtr BGInfoTex;
        public IntPtr numeros;
        IntPtr[] phasesTex;
        IntPtr opponentIcon;
        IntPtr opponentActiveIcon;
        IntPtr playerIcon;
        IntPtr playerActiveIcon;
        IntPtr redBorder;
        Button phaseButton;
        Button cancelButton;
        Button handRActiveButton;
        Button handRInactiveButton;
        Button handLActiveButton;
        Button handLInactiveButton;
        Button[] buttons;
        public UI()
        {
            BGTex = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/backgroundUI.png");
            BGHandTex = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/backgroundHandUI.png");
            BGInfoTex = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/backgroundInfoUI.png");
            numeros = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/numeros.png");
            playerIcon = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/you.png");
            playerActiveIcon = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/your_turn.png");
            opponentIcon = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/opponent.png");
            opponentActiveIcon = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/opponent_turn.png");
            redBorder = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/cartaRoja.png");
            phaseButton = new Button("../assets/images/ui/phaseButton.png",90,60,10,10,buttonType.PHASE);
            cancelButton = new Button("../assets/images/ui/cancelButton.png", 80, 60, 10, 10, buttonType.CANCEL);
            handRActiveButton = new Button("../assets/images/ui/handRButtonAct.png",90,70,10,7, buttonType.RIGHT_ON);
            handRInactiveButton = new Button("../assets/images/ui/handRButtonInact.png",90,70,10,7, buttonType.RIGHT_OFF);
            handLActiveButton = new Button("../assets/images/ui/handLButtonAct.png",80,70,10,7, buttonType.LEFT_ON);
            handLInactiveButton = new Button("../assets/images/ui/handLButtonInact.png",80,70,10,7, buttonType.LEFT_OFF);
            buttons = new Button[6];
            buttons[0] = phaseButton;
            buttons[1] = cancelButton;
            buttons[2] = handRActiveButton;
            buttons[3] = handRInactiveButton;
            buttons[4] = handLActiveButton;
            buttons[5] = handLInactiveButton;
            phasesTex = new IntPtr[8];
            phasesTex[0] = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/upkeep.png");
            phasesTex[1] = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/draw.png");
            phasesTex[2] = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/main.png");
            phasesTex[3] = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/attack.png");
            phasesTex[4] = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/block.png");
            phasesTex[5] = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/damage.png");
            phasesTex[6] = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/main2.png");
            phasesTex[7] = SDL_image.IMG_LoadTexture(Program.renderer, "../assets/images/ui/end.png");
        }
        public void render()
        {

            float wUnit = Program.getWUnit();
            float hUnit = Program.getHUnit();
            SDL.SDL_Rect sRect;
            sRect.x = 0;
            sRect.y = 0;
            sRect.w = 0;
            sRect.h = 0;
            SDL.SDL_Rect dRect;
            dRect.x = 0;
            dRect.y = 0;
            dRect.w = (int)(wUnit * 100);
            dRect.h = (int)(hUnit * 100);

            // Background general 
            SDL.SDL_RenderCopy(Program.renderer, BGTex, IntPtr.Zero, ref dRect);
            // Background mano
            dRect.y = (int)(hUnit * 70);
            dRect.h = (int)(hUnit * 30);
            SDL.SDL_RenderCopy(Program.renderer, BGHandTex, IntPtr.Zero, ref dRect);
            // Background info
            dRect.x = (int)(wUnit * 80);
            dRect.y = 0;
            dRect.w = (int)(wUnit * 20);
            dRect.h = (int)(hUnit * 75);
            SDL.SDL_RenderCopy(Program.renderer, BGInfoTex, IntPtr.Zero, ref dRect);
            foreach (Button button in buttons)
            {
                button.render();
            }


            // Fase
            sRect.x = 0;
            sRect.y = 0;
            sRect.w = 0;
            sRect.h = 0;

            dRect.x = (int)wUnit*39;
            dRect.y = 0;
            dRect.w = (int)(wUnit * 9);
            dRect.h = (int)(hUnit * 9);

            SDL.SDL_RenderCopy(Program.renderer, phasesTex[(int)Program.getGame().getPhase()], IntPtr.Zero, ref dRect);
            // Jugador activo
            sRect.x = 0;
            sRect.y = 0;
            sRect.w = 0;
            sRect.h = 0;

            dRect.x = (int)wUnit * 29;
            dRect.y = 0;
            dRect.w = (int)(wUnit * 10);
            dRect.h = (int)(hUnit * 10);
            if (Program.getGame().getActivePlayer() == Program.localPlayerId)
            {
                SDL.SDL_RenderCopy(Program.renderer, playerActiveIcon, IntPtr.Zero, ref dRect);
                dRect.x = (int)wUnit * 47;
                SDL.SDL_RenderCopy(Program.renderer, opponentIcon, IntPtr.Zero, ref dRect);
            }
            else
            {
                SDL.SDL_RenderCopy(Program.renderer, playerIcon, IntPtr.Zero, ref dRect);
                dRect.x = (int)wUnit * 47;
                SDL.SDL_RenderCopy(Program.renderer, opponentActiveIcon, IntPtr.Zero, ref dRect);
            }
            
            // Vidas jugador 570 260
            for (int i = 0; i < 2; i++)
            {
                int vidasPlayer;
                if (i == 1)
                { 
                    vidasPlayer = Program.getGame().opponent.getLife(); 
                }
                else
                {
                    vidasPlayer = Program.getGame().player.getLife();
                }
                sRect.x = 570 / 5 * (vidasPlayer % 10);
                sRect.y = 0;
                sRect.w = 570 / 5;
                sRect.h = 130;
                if (sRect.x >= 570)
                {
                    sRect.x -= 570;
                    sRect.y = 130;
                }
                dRect.x = (int)(wUnit * 90);
                dRect.y = (int)((hUnit * 50)-(i*10*hUnit));
                dRect.w = (int)(wUnit * 10);
                dRect.h = (int)(hUnit * 10);
                SDL.SDL_RenderCopy(Program.renderer, numeros, ref sRect, ref dRect);

                sRect.x = 570 / 5 * (int)(vidasPlayer / 10);
                sRect.y = 0;
                sRect.w = 570 / 5;
                sRect.h = 130;
                if (sRect.x >= 570)
                {
                    sRect.x -= 570;
                    sRect.y = 130;
                }
                dRect.x = (int)(wUnit * 80);
                dRect.y = (int)((hUnit * 50)-(i*10*hUnit));
                dRect.w = (int)(wUnit * 10);
                dRect.h = (int)(hUnit * 10);
                SDL.SDL_RenderCopy(Program.renderer, numeros, ref sRect, ref dRect);
            }

        }
        public void handleEvents(SDL.SDL_Event e)
        {
            foreach (Button button in buttons)
            {
                button.handleEvents(e);
            }
        }
    }
}
