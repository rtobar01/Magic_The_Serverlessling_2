using Amazon.Lambda.Core;


[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MagicTheServerlessling
{
    public class Player
    {
        int life;
        public uint cardsInHand = 0;
        public List<Card> hand;
        public List<Card> battlefield;
        public int landDrops = 1;
        public int landsPlayed = 0;
        playerMana manaPool;
        public uint playerId;
        public int handIndex = 0;
        public int landIndex = 0;
        public int permanentIndex = 0;
        struct playerMana
        {
            public uint totalMana;
            public uint[] mana;
        }
        public Player(uint playerId_)
        {
            life = 20;
            hand = new List<Card>();
            battlefield = new List<Card>();
            manaPool = new playerMana();
            manaPool.mana = new uint[6];
            playerId = playerId_;
        }
        public void setLife(int newLife)
        {
            life = newLife;
        }
        public void render(bool opp)
        {
            uint landCounter = 0;
            uint nonLandCounter = 0;
            float wUnit = Program.getWUnit();
            float hUnit = Program.getHUnit();
            if (!opp)
                for (int i = handIndex; i < hand.Count(); i++)
                    hand[(int)i].render((uint)(wUnit*(1+(i-handIndex)*15)),(uint)(hUnit*80),(uint)(hUnit* 15 / 1.4),(uint)hUnit* 15);
            for (int i = 0; i < battlefield.Count(); i++)
            {
                if (battlefield[(int)i].getCardType() == "LAND")
                {
                        if (!opp)
                            battlefield[(int)i].render((uint)(wUnit * (5 + landCounter * 10)), (uint)(hUnit * 60), (uint)(hUnit * 15 / 1.4), (uint)hUnit * 15);
                        else battlefield[(int)i].render((uint)(wUnit * (5 + landCounter * 10)), (uint)(hUnit * 10), (uint)(hUnit * 15 / 1.4), (uint)hUnit * 15);
                        landCounter++;
                }
                else
                {
                    if (!opp)
                    battlefield[(int)i].render((uint)(wUnit * (5 + nonLandCounter * 10)), (uint)(hUnit * 43), (uint)(hUnit * 15 / 1.4), (uint)hUnit * 15);
                    else battlefield[(int)i].render((uint)(wUnit * (5 + nonLandCounter * 10)), (uint)(hUnit * 26), (uint)(hUnit * 15 / 1.4), (uint)hUnit * 15);
                    nonLandCounter++;
                }
            }
        }
        public int getLife()
        {
            return life;
        }
    }
}
