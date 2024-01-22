using System;
using SDL2;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicTheServerlessling
{
    enum buttonType
    {
        PHASE,
        CANCEL,
        RIGHT_ON,
        RIGHT_OFF,
        LEFT_ON,
        LEFT_OFF
    }
    class Button
    {
        IntPtr tex;
        int x, y, w, h;
        buttonType type;
        public Button(string tex_,int x_,int y_, int w_, int h_,buttonType type_)
        {
            tex = SDL_image.IMG_LoadTexture(Program.renderer, tex_);
            x = x_; y = y_; w = w_; h = h_;
            type = type_;
        }
        public void render()
        {
            if (type == buttonType.CANCEL && Program.getGame().waitingForTarget == null) return;
            if (type == buttonType.RIGHT_ON && Program.getGame().player.handIndex+7 >= Program.getGame().player.cardsInHand) return;
            if (type == buttonType.RIGHT_OFF && Program.getGame().player.handIndex+7 < Program.getGame().player.cardsInHand) return;
            if (type == buttonType.LEFT_ON && Program.getGame().player.handIndex == 0) return;
            if (type == buttonType.LEFT_OFF && Program.getGame().player.handIndex != 0) return;
            // Boton fase
            float wUnit = Program.getWUnit();
            float hUnit = Program.getHUnit();
            SDL.SDL_Rect sRect;
            SDL.SDL_Rect dRect;
            sRect.x = 0;
            sRect.y = 0;
            sRect.w = 0;
            sRect.h = 0;
            dRect.x = (int)(wUnit * x);
            dRect.y = (int)(hUnit * y);
            dRect.w = (int)(wUnit * w);
            dRect.h = (int)(hUnit * h);

            SDL.SDL_RenderCopy(Program.renderer, tex, IntPtr.Zero, ref dRect);
        }
        public void handleEvents(SDL.SDL_Event e)
        {
            int xI, yI;
            float wUnit = Program.getWUnit();
            float hUnit = Program.getHUnit();
            SDL.SDL_GetMouseState(out xI, out yI);
            if (xI >= x* wUnit && xI <= x * wUnit + w * wUnit && yI >= y * hUnit && yI <= y * hUnit + h * hUnit)
            {
                switch (type)
                {
                    case buttonType.CANCEL:
                        if (Program.getGame().waitingForTarget != null && Program.getGame().waitingForTarget.canCancell)
                            Program.getGame().waitingForTarget = null;
                        break;
                    case buttonType.PHASE:
                            Program.getGame().nextPhase();
                        break;
                    case buttonType.RIGHT_ON:
                        if (type == buttonType.RIGHT_ON && Program.getGame().player.handIndex + 7 < Program.getGame().player.cardsInHand)
                            Program.getGame().player.handIndex++;
                        break;
                    case buttonType.LEFT_ON:
                        if (type == buttonType.LEFT_ON && Program.getGame().player.handIndex != 0)
                            Program.getGame().player.handIndex--;
                        break;
                }
            }
        }
    }
}
