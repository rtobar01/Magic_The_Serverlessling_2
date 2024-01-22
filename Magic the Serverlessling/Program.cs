using SDL2;
using Amazon.S3;
using Amazon.S3.Model;
using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;
using System;

namespace MagicTheServerlessling
{
    [Assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

    public class Cambios
    {
        public int idCambios { get; set; }
        public int[] cardsDrawnPlayer0 { get; set; }
        public int[] cardsDrawnPlayer1 { get; set; }
        public int[] cardsPlayedPlayer0 { get; set; }
        public int[] cardsPlayedPlayer1 { get; set; }
        public int[] cardsTappedPlayer0 { get; set; }
        public int[] cardsTappedPlayer1 { get; set; }
        public int[] cardsUntappedPlayer0 { get; set; }
        public int[] cardsUntappedPlayer1 { get; set; }
        public int[] cardsAttackingPlayer0 { get; set; }
        public int[] cardsAttackingPlayer1 { get; set; }
        public int[] cardsBlockingPlayer0 { get; set; }
        public int[] cardsBlockingPlayer1 { get; set; }
        public int[] cardsBlockedPlayer0 { get; set; }
        public int[] cardsBlockedPlayer1 { get; set; }
        public int[] cardsDestroyedPlayer0 { get; set; }
        public int[] cardsDestroyedPlayer1 { get; set; }
        public int[] cardsDiscardedPlayer0 { get; set; }
        public int[] cardsDiscardedPlayer1 { get; set; }
        public int[] temporalStatsPlayer0 { get; set; }
        public int[] temporalStatsPlayer1 { get; set; }
        public int victoryPlayer0 { get; set; }
        public int victoryPlayer1 { get; set; }
        public int livesPlayer0 { get; set; }
        public int livesPlayer1 { get; set; }
        public int newPhase { get; set; }
        public int newActivePlayer { get; set; }
    }
    public class Program
    {
        static public IntPtr renderer;
        static public Game game;
        static private IntPtr window;
        static public bool exit = false;
        static private bool start;
        static public AmazonS3Client s3;
        static public uint localPlayerId; // Id del jugador
        static public int msgId = 0;
        static public bool[] readMsg = new bool[100000];
        static public string gameId;
        static public Cambios cambios;
        static private int victory;
        static void Main(string[] args)
        {
            s3 = new AmazonS3Client(Amazon.RegionEndpoint.USEast1);
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            //SDL_ttf.TTF_Init();
            window = IntPtr.Zero;
            window = SDL.SDL_CreateWindow("Magic The Serverlessling", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 640, 480, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            SDL.SDL_RenderClear(renderer);
            
            //Reseteador de input.txt
            using (StreamWriter writer = new StreamWriter("input.txt"))
            {
                writer.WriteLine("");
                writer.Close();
            }
            //Reseteador de output.txt
            using (StreamWriter writer = new StreamWriter("output.txt"))
            {
                writer.WriteLine("");
                writer.Close();
            }

            start = true;
            //onlineConnection().Wait(); antigua version
            Task.Run(() => initialConnection()).Wait();
            game = new Game();
            receiveInfo();
            game.lastFocusedCard = game.player.hand[0];
            int contador = 0;
            while (!exit)
            {
                UInt64 start = SDL.SDL_GetPerformanceCounter();
                contador++;
                SDL.SDL_RenderClear(renderer);
                if (contador%30 == 0)
                    checkForChanges();
                handleEvents();
                render();
                SDL.SDL_RenderPresent(Program.renderer);
                UInt64 end = SDL.SDL_GetPerformanceCounter();
                float elapsedMS = (end - start) / (float)SDL.SDL_GetPerformanceFrequency() * 1000.0f;
                uint alpha = (uint)(Math.Floor(16.666f - elapsedMS));
                // Cap to 60 FPS
                if (alpha > 100000) alpha = 16;
                SDL.SDL_Delay(alpha);
            }
            if (victory == 1)
                Console.WriteLine("Felicidades, ¡has ganado!");
            else Console.WriteLine("Has perdido");
            Task.Delay(5000).Wait();
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
        static void initialConnection()
        {
            bool success = false;
            int deckId = -1;
            Console.Write("Escribe 0 para jugar el mazo negro o 1 para jugar el mazo verde: ");
            success = int.TryParse(Console.ReadLine(), out deckId);
            while (!success || (deckId != 0 && deckId != 1))
            {
                Console.Write("Opción no válida. Por favor, escribe 0 para jugar el mazo negro o 1 para jugar el mazo verde: ");
                success = int.TryParse(Console.ReadLine(), out deckId);
            }
            int opt = -1;
            Console.Write("Escribe 0 para crear una partida o 1 para unirte a una: ");
            success = int.TryParse(Console.ReadLine(), out opt);
            while (!success || (opt != 0 && opt != 1))
            {
                Console.Write("Opción no válida. Por favor, escribe 0 para crear una partida o 1 para unirte a una: ");
                success = int.TryParse(Console.ReadLine(), out opt);
            }
            if (opt == 0)
            {
                start = true;
                localPlayerId = 0;
                using (var client = new HttpClient())
                {
                    var endpoint = new Uri("https://f7s6nbu4ak.execute-api.us-east-1.amazonaws.com/Stage/resource?command=NEW_GAME&deckId=" + deckId);
                    var result = client.GetAsync(endpoint).Result;
                    gameId = result.Content.ReadAsStringAsync().Result;
                }
                Console.WriteLine("Envía este código a tu oponente: " + gameId);
            }
            else if (opt == 1)
            {
                start = false;
                HttpResponseMessage result = null;
                do
                {
                    Console.Write("Escribe el id de la partida a la que te quieres unir: ");
                    gameId = Console.ReadLine();
                    localPlayerId = 1;
                    using (var client = new HttpClient())
                    {
                        var endpoint = new Uri("https://f7s6nbu4ak.execute-api.us-east-1.amazonaws.com/Stage/resource?command=JOIN_GAME&id=" + gameId + "&deckId=" + deckId);
                        result = client.GetAsync(endpoint).Result;
                    }
                    if (result.StatusCode != System.Net.HttpStatusCode.OK)
                        Console.WriteLine(result.ReasonPhrase.ToString() + result.Content.ReadAsStringAsync().Result);
                }
                while (result.StatusCode != System.Net.HttpStatusCode.OK);
            }
            return;
        }
        static void receiveInfo()
        {
            string cambiosKey = "games/" + gameId + "/cambios.json";
            var cambiosRequest = new GetObjectRequest
            {
                BucketName = "magic-the-serverlessling",
                Key = cambiosKey,
                RequestPayer = RequestPayer.Requester
            };
            string cambiosRealizados = "";
            using (GetObjectResponse response = s3.GetObjectAsync(cambiosRequest).Result) // Obtenemos el archivo del bucket
            using (Stream responseStream = response.ResponseStream) // Cogemos la respuesta y la pasamos por un flujo de lectura
            using (StreamReader reader = new StreamReader(responseStream))
            {
                cambiosRealizados = reader.ReadToEnd();
            }
            cambios = JsonSerializer.Deserialize<Cambios>(cambiosRealizados);
            // Control de mensajes para evitar procesar duplicados
            if (readMsg[cambios.idCambios]) return;
            readMsg[cambios.idCambios] = true;

            // Cambios a aplicar si el cliente es el jugador 0
            if (localPlayerId == 0)
            {
                if (cambios.idCambios != 1)
                {
                    // Robo de cartas del jugador
                    for (int i = 0; i < cambios.cardsDrawnPlayer0.Length; i++)
                    {
                        string key = "games/" + gameId + "/Usuario0/hand/" + cambios.cardsDrawnPlayer0[i] + ".json";
                        var request = new GetObjectRequest
                        {
                            BucketName = "magic-the-serverlessling",
                            Key = key,
                            RequestPayer = RequestPayer.Requester
                        };
                        using (GetObjectResponse response = s3.GetObjectAsync(request).Result) // Obtenemos el archivo del bucket
                        using (Stream responseStream = response.ResponseStream) // Cogemos la respuesta y la pasamos por un flujo de lectura
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            cambiosRealizados = reader.ReadToEnd();
                        }
                        Card carta = JsonSerializer.Deserialize<Card>(cambiosRealizados);
                        carta.cardId = cambios.cardsDrawnPlayer0[i];
                        carta.owner = game.player;
                        game.player.hand.Add(carta);
                        game.player.cardsInHand++;
                    }
                    // Cartas jugadas por el jugador
                    int idHand = -1;
                    int idBattlefield = -1;
                    for (int i = 0; i < cambios.cardsPlayedPlayer0.Length; i++)
                    {
                        for (int j = 0; j < game.player.hand.Count; j++)
                        {
                            if (game.player.hand[j].cardId == cambios.cardsPlayedPlayer0[i])
                                idHand = j;
                        }
                        if (game.player.hand[idHand].cardType == "SORCERY" || game.player.hand[idHand].cardType == "INSTANT")
                        {
                            game.player.hand[idHand].cardZone = zone.GRAVEYARD;
                            game.lastSpellCasted = game.player.hand[idHand];
                        }
                        else
                        {
                            game.player.hand[idHand].cardZone = zone.BATTLEFIELD;
                            game.player.battlefield.Add(game.player.hand[idHand]);
                        }
                        game.player.hand.Remove(game.player.hand[idHand]);
                        game.player.cardsInHand--;
                    }
                    // Cartas giradas por el jugador
                    for (int i = 0; i < cambios.cardsTappedPlayer0.Length; i++)
                    {
                        for (int j = 0; j < game.player.battlefield.Count; j++)
                        {
                            if (game.player.battlefield[j].cardId == cambios.cardsTappedPlayer0[i])
                                idBattlefield = j;
                        }
                        game.player.battlefield[idBattlefield].tapped = 1;
                    }
                    // Cartas enderezadas por el jugador
                    for (int i = 0; i < cambios.cardsUntappedPlayer0.Length; i++)
                    {
                        for (int j = 0; j < game.player.battlefield.Count; j++)
                        {
                            if (game.player.battlefield[j].id == cambios.cardsUntappedPlayer0[i])
                                idBattlefield = j;
                        }
                        game.player.battlefield[idBattlefield].tapped = 0;
                    }
                    // Cartas atacando por el jugador
                    for (int i = 0; i < cambios.cardsAttackingPlayer0.Length; i++)
                    {
                        for (int j = 0; j < game.player.battlefield.Count; j++)
                        {
                            if (game.player.battlefield[j].cardId == cambios.cardsAttackingPlayer0[i])
                                idBattlefield = j;
                        }
                        game.player.battlefield[idBattlefield].attacking = 1;
                    }
                    // Cartas bloqueando por el jugador
                    for (int i = 0; i < cambios.cardsBlockingPlayer0.Length; i++)
                    {
                        for (int j = 0; j < game.player.battlefield.Count; j++)
                        {
                            if (game.player.battlefield[j].cardId == cambios.cardsBlockingPlayer0[i])
                                idBattlefield = j;
                        }
                        if (game.player.battlefield[idBattlefield].blocking == 0)
                        {
                            game.player.battlefield[idBattlefield].blocking = 1;
                            game.currentlyBlocking = game.player.battlefield[idBattlefield];
                        }
                        else
                        {
                            if (game.player.battlefield[idBattlefield].attackerBlocked != null)
                            {
                                game.player.battlefield[idBattlefield].attackerBlocked.blocker = null;
                                game.player.battlefield[idBattlefield].attackerBlocked = null;
                            }
                            game.currentlyBlocking = game.player.battlefield[idBattlefield];
                        }
                    }
                    // Cartas bloqueadas por el jugador
                    for (int i = 0; i < cambios.cardsBlockedPlayer0.Length; i++)
                    {
                        for (int j = 0; j < game.opponent.battlefield.Count; j++)
                        {
                            if (game.opponent.battlefield[j].cardId == cambios.cardsBlockedPlayer0[i])
                                idBattlefield = j;
                        }
                        if (game.opponent.battlefield[idBattlefield].blocker != null)
                        {
                            game.opponent.battlefield[idBattlefield].blocker.blocking = 0;
                            game.opponent.battlefield[idBattlefield].blocker.attackerBlocked = null;
                        }
                        game.currentlyBlocking.attackerBlocked = game.opponent.battlefield[idBattlefield];
                        game.opponent.battlefield[idBattlefield].blocker = game.currentlyBlocking;
                        game.currentlyBlocking = null;
                    }
                    
                    // Cartas destruidas del jugador
                    for (int i = 0; i < cambios.cardsDestroyedPlayer0.Length; i++)
                    {
                        for (int j = 0; j < game.player.battlefield.Count; j++)
                        {
                            if (game.player.battlefield[j].cardId == cambios.cardsDestroyedPlayer0[i])
                                idBattlefield = j;
                        }
                        game.player.battlefield[idBattlefield].destroy();
                    }
                    // Cartas descartadas por el jugador
                    int idDiscard = -1;
                    for (int i = 0; i < cambios.cardsDiscardedPlayer0.Length; i++)
                    {
                        for (int j = 0; j < game.player.hand.Count; j++)
                        {
                            if (game.player.hand[j].id == cambios.cardsDiscardedPlayer0[i])
                                idDiscard = j;
                        }
                        game.player.hand[idDiscard].discard();
                    }
                    // Cartas del jugador que han aumentado sus stats temporalmente 
                    for (int i = 0; i < cambios.temporalStatsPlayer0.Length; i+=2)
                    {
                        for (int j = 0; j < game.player.battlefield.Count; j++)
                        {
                            if (game.player.battlefield[j].cardId == cambios.temporalStatsPlayer0[i])
                                idBattlefield = j;
                        }
                        game.player.battlefield[idBattlefield].temporalPower = cambios.temporalStatsPlayer0[i+1];
                        game.player.battlefield[idBattlefield].temporalToughness = cambios.temporalStatsPlayer0[i+1];
                    }
                    if (cambios.victoryPlayer0 == 1)
                    {
                        exit = true;
                        victory = 1;
                    }
                    // Cambios en las vidas del jugador
                    if (cambios.livesPlayer0 != -1)
                        game.player.setLife(cambios.livesPlayer0);
                }
                // Robo de cartas del oponente
                for (int i = 0; i < cambios.cardsDrawnPlayer1.Length; i++)
                {
                    string key = "games/" + gameId + "/Usuario1/hand/" + cambios.cardsDrawnPlayer1[i] + ".json";
                    var request = new GetObjectRequest
                    {
                        BucketName = "magic-the-serverlessling",
                        Key = key,
                        RequestPayer = RequestPayer.Requester
                    };
                    using (GetObjectResponse response = s3.GetObjectAsync(request).Result) // Obtenemos el archivo del bucket
                    using (Stream responseStream = response.ResponseStream) // Cogemos la respuesta y la pasamos por un flujo de lectura
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        cambiosRealizados = reader.ReadToEnd();
                    }
                    Card carta = JsonSerializer.Deserialize<Card>(cambiosRealizados);
                    carta.cardId = cambios.cardsDrawnPlayer1[i];
                    carta.owner = game.opponent;
                    game.opponent.hand.Add(carta);
                    game.opponent.cardsInHand++;
                }
                // Cartas jugadas por el oponente
                int idHandOp = -1;
                int idBattlefieldOp = -1;
                for (int i = 0; i < cambios.cardsPlayedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.opponent.hand.Count; j++)
                    {
                        if (game.opponent.hand[j].cardId == cambios.cardsPlayedPlayer1[i])
                            idHandOp = j;
                    }
                    if (game.opponent.hand[idHandOp].cardType == "SORCERY" || game.opponent.hand[idHandOp].cardType == "INSTANT")
                    {
                        game.lastSpellCastedopp = game.opponent.hand[idHandOp];
                        game.opponent.hand[idHandOp].cardZone = zone.GRAVEYARD;
                    }
                    else
                    {
                        game.opponent.hand[idHandOp].cardZone = zone.BATTLEFIELD;
                        game.opponent.battlefield.Add(game.opponent.hand[idHandOp]);
                    }
                    game.opponent.hand.Remove(game.opponent.hand[idHandOp]);
                    game.opponent.cardsInHand--;
                }
                // Cartas giradas por el oponente
                for (int i = 0; i < cambios.cardsTappedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsTappedPlayer1[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].tapped = 1;
                }
                // Cartas enderezadas por el oponente
                for (int i = 0; i < cambios.cardsUntappedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].id == cambios.cardsUntappedPlayer1[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].tapped = 0;
                }
                // Cartas atacando por el oponente
                for (int i = 0; i < cambios.cardsAttackingPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsAttackingPlayer1[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].attacking = 1;
                }
                // Cartas bloqueando por el oponente
                for (int i = 0; i < cambios.cardsBlockingPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsBlockingPlayer1[i])
                            idBattlefieldOp = j;
                    }
                    if (game.opponent.battlefield[idBattlefieldOp].blocking == 0)
                    {
                        game.opponent.battlefield[idBattlefieldOp].blocking = 1;
                        game.currentlyBlocking = game.opponent.battlefield[idBattlefieldOp];
                    }
                    else
                    {
                        if (game.opponent.battlefield[idBattlefieldOp].attackerBlocked != null)
                        {
                            game.opponent.battlefield[idBattlefieldOp].attackerBlocked.blocker = null;
                            game.opponent.battlefield[idBattlefieldOp].attackerBlocked = null;
                        }
                        game.currentlyBlocking = game.opponent.battlefield[idBattlefieldOp];
                    }
                }
                // Cartas bloqueadas por el oponente
                for (int i = 0; i < cambios.cardsBlockedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.player.battlefield.Count; j++)
                    {
                        if (game.player.battlefield[j].cardId == cambios.cardsBlockedPlayer1[i])
                            idBattlefieldOp = j;
                    }
                    if (game.player.battlefield[idBattlefieldOp].blocker != null)
                    {
                        game.player.battlefield[idBattlefieldOp].blocker.blocking = 0;
                        game.player.battlefield[idBattlefieldOp].blocker.attackerBlocked = null;
                    }
                    game.currentlyBlocking.attackerBlocked = game.player.battlefield[idBattlefieldOp];
                    game.player.battlefield[idBattlefieldOp].blocker = game.currentlyBlocking;
                    game.currentlyBlocking = null;
                }
                // Cartas destruidas del oponente
                for (int i = 0; i < cambios.cardsDestroyedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsDestroyedPlayer1[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].destroy();
                }
                // Cartas descartadas por el oponente
                int idHandDis = -1;
                for (int i = 0; i < cambios.cardsDiscardedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.opponent.hand.Count; j++)
                    {
                        if (game.opponent.hand[j].id == cambios.cardsDiscardedPlayer1[i])
                            idHandDis = j;
                    }
                    game.opponent.hand[idHandDis].discard();
                }
                // Cartas del oponente que han aumentado sus stats temporalmente 
                for (int i = 0; i < cambios.temporalStatsPlayer1.Length; i += 2)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.temporalStatsPlayer1[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].temporalPower = cambios.temporalStatsPlayer1[i + 1];
                    game.opponent.battlefield[idBattlefieldOp].temporalToughness = cambios.temporalStatsPlayer1[i + 1];
                }
                if (cambios.victoryPlayer1 == 1)
                {
                    exit = true;
                    victory = 0;
                }
                // Cambios en las vidas del oponente
                if (cambios.livesPlayer1 != -1)
                    game.opponent.setLife(cambios.livesPlayer1);

                //Cambios generales
                if (cambios.newActivePlayer != -1) game.changeActivePlayer(cambios.newActivePlayer);
                if (cambios.newPhase != -1) game.updatePhase(cambios.newPhase);
            }

            // Cambios a aplicar si el cliente es el jugador 1
            else
            {
                // Robo de cartas
                for (int i = 0; i < cambios.cardsDrawnPlayer1.Length; i++)
                {
                    string key = "games/" + gameId + "/Usuario1/hand/" + cambios.cardsDrawnPlayer1[i] + ".json";
                    var request = new GetObjectRequest
                    {
                        BucketName = "magic-the-serverlessling",
                        Key = key,
                        RequestPayer = RequestPayer.Requester
                    };
                    using (GetObjectResponse response = s3.GetObjectAsync(request).Result) // Obtenemos el archivo del bucket
                    using (Stream responseStream = response.ResponseStream) // Cogemos la respuesta y la pasamos por un flujo de lectura
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        cambiosRealizados = reader.ReadToEnd();
                    }
                    Card carta = JsonSerializer.Deserialize<Card>(cambiosRealizados);
                    carta.cardId = cambios.cardsDrawnPlayer1[i];
                    carta.owner = game.player;
                    game.player.hand.Add(carta);
                    game.player.cardsInHand++;
                }
                // Cartas jugadas
                int idHand = -1;
                int idBattlefield = -1;
                for (int i = 0; i < cambios.cardsPlayedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.player.hand.Count; j++)
                    {
                        if (game.player.hand[j].cardId == cambios.cardsPlayedPlayer1[i])
                            idHand = j;
                    }
                    if (game.player.hand[idHand].cardType == "SORCERY" || game.player.hand[idHand].cardType == "INSTANT")
                    {
                        game.player.hand[idHand].cardZone = zone.GRAVEYARD;
                        game.lastSpellCasted = game.player.hand[idHand];
                    }
                    else
                    {
                        game.player.hand[idHand].cardZone = zone.BATTLEFIELD;
                        game.player.battlefield.Add(game.player.hand[idHand]);
                    }
                    game.player.hand.Remove(game.player.hand[idHand]);
                    game.player.cardsInHand--;
                }
                // Cartas giradas
                for (int i = 0; i < cambios.cardsTappedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.player.battlefield.Count; j++)
                    {
                        if (game.player.battlefield[j].cardId == cambios.cardsTappedPlayer1[i])
                            idBattlefield = j;
                    }
                    game.player.battlefield[idBattlefield].tapped = 1;
                }
                // Cartas enderezadas
                for (int i = 0; i < cambios.cardsUntappedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.player.battlefield.Count; j++)
                    {
                        if (game.player.battlefield[j].id == cambios.cardsUntappedPlayer1[i])
                            idBattlefield = j;
                    }
                    game.player.battlefield[idBattlefield].tapped = 0;
                }
                // Cartas atacando por el jugador
                for (int i = 0; i < cambios.cardsAttackingPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.player.battlefield.Count; j++)
                    {
                        if (game.player.battlefield[j].cardId == cambios.cardsAttackingPlayer1[i])
                            idBattlefield = j;
                    }
                    game.player.battlefield[idBattlefield].attacking = 1;
                }
                // Cartas bloqueando por el jugador
                for (int i = 0; i < cambios.cardsBlockingPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.player.battlefield.Count; j++)
                    {
                        if (game.player.battlefield[j].cardId == cambios.cardsBlockingPlayer1[i])
                            idBattlefield = j;
                    }
                    if (game.player.battlefield[idBattlefield].blocking == 0)
                    {
                        game.player.battlefield[idBattlefield].blocking = 1;
                        game.currentlyBlocking = game.player.battlefield[idBattlefield];
                    }
                    else
                    {
                        if (game.player.battlefield[idBattlefield].attackerBlocked != null)
                        {
                            game.player.battlefield[idBattlefield].attackerBlocked.blocker = null;
                            game.player.battlefield[idBattlefield].attackerBlocked = null;
                        }
                        game.currentlyBlocking = game.player.battlefield[idBattlefield];
                    }
                }
                // Cartas bloqueadas por el jugador
                for (int i = 0; i < cambios.cardsBlockedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsBlockedPlayer1[i])
                            idBattlefield = j;
                    }
                    if (game.opponent.battlefield[idBattlefield].blocker != null)
                    {
                        game.opponent.battlefield[idBattlefield].blocker.blocking = 0;
                        game.opponent.battlefield[idBattlefield].blocker.attackerBlocked = null;
                    }
                    game.currentlyBlocking.attackerBlocked = game.opponent.battlefield[idBattlefield];
                    game.opponent.battlefield[idBattlefield].blocker = game.currentlyBlocking;
                    game.currentlyBlocking = null;
                }
                // Cartas destruidas del jugador
                for (int i = 0; i < cambios.cardsDestroyedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.player.battlefield.Count; j++)
                    {
                        if (game.player.battlefield[j].cardId == cambios.cardsDestroyedPlayer1[i])
                            idBattlefield = j;
                    }
                    game.player.battlefield[idBattlefield].destroy();
                }
                // Cartas descartadas por el jugador
                int idHandDis = -1;
                for (int i = 0; i < cambios.cardsDiscardedPlayer1.Length; i++)
                {
                    for (int j = 0; j < game.player.hand.Count; j++)
                    {
                        if (game.player.hand[j].id == cambios.cardsDiscardedPlayer1[i])
                            idHandDis = j;
                    }
                    game.player.hand[idHandDis].discard();
                }
                // Cartas del jugador que han aumentado sus stats temporalmente 
                for (int i = 0; i < cambios.temporalStatsPlayer1.Length; i += 2)
                {
                    for (int j = 0; j < game.player.battlefield.Count; j++)
                    {
                        if (game.player.battlefield[j].cardId == cambios.temporalStatsPlayer1[i])
                            idBattlefield = j;
                    }
                    game.player.battlefield[idBattlefield].temporalPower = cambios.temporalStatsPlayer1[i + 1];
                    game.player.battlefield[idBattlefield].temporalToughness = cambios.temporalStatsPlayer1[i + 1];
                }
                if (cambios.victoryPlayer0 == 1)
                {
                    exit = true;
                    victory = 0;
                }
                // Cambios en las vidas del jugador
                if (cambios.livesPlayer1 != -1)
                    game.player.setLife(cambios.livesPlayer1);

                // Robo de cartas del oponente
                for (int i = 0; i < cambios.cardsDrawnPlayer0.Length; i++)
                {
                    string key = "games/" + gameId + "/Usuario0/hand/" + cambios.cardsDrawnPlayer0[i] + ".json";
                    var request = new GetObjectRequest
                    {
                        BucketName = "magic-the-serverlessling",
                        Key = key,
                        RequestPayer = RequestPayer.Requester
                    };
                    using (GetObjectResponse response = s3.GetObjectAsync(request).Result) // Obtenemos el archivo del bucket
                    using (Stream responseStream = response.ResponseStream) // Cogemos la respuesta y la pasamos por un flujo de lectura
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        cambiosRealizados = reader.ReadToEnd();
                    }
                    Card carta = JsonSerializer.Deserialize<Card>(cambiosRealizados);
                    carta.cardId = cambios.cardsDrawnPlayer0[i];
                    carta.owner = game.opponent;
                    game.opponent.hand.Add(carta);
                    game.opponent.cardsInHand++;
                }
                // Cartas jugadas por el oponente
                int idHandOp = -1;
                int idBattlefieldOp = -1;
                for (int i = 0; i < cambios.cardsPlayedPlayer0.Length; i++)
                {
                    for (int j = 0; j < game.opponent.hand.Count; j++)
                    {
                        if (game.opponent.hand[j].cardId == cambios.cardsPlayedPlayer0[i])
                            idHandOp = j;
                    }
                    if (game.opponent.hand[idHandOp].cardType == "SORCERY" || game.opponent.hand[idHandOp].cardType == "INSTANT")
                    {
                        game.lastSpellCastedopp = game.opponent.hand[idHandOp];
                        game.opponent.hand[idHandOp].cardZone = zone.GRAVEYARD;
                    }
                    else
                    {
                        game.opponent.hand[idHandOp].cardZone = zone.BATTLEFIELD;
                        game.opponent.battlefield.Add(game.opponent.hand[idHandOp]);
                    }
                    game.opponent.hand.Remove(game.opponent.hand[idHandOp]);
                    game.opponent.cardsInHand--;
                }
                // Cartas giradas por el oponente
                for (int i = 0; i < cambios.cardsTappedPlayer0.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsTappedPlayer0[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].tapped = 1;
                }
                // Cartas enderezadas por el oponente
                for (int i = 0; i < cambios.cardsUntappedPlayer0.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].id == cambios.cardsUntappedPlayer0[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].tapped = 0;
                }
                // Cartas atacando por el oponente
                for (int i = 0; i < cambios.cardsAttackingPlayer0.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsAttackingPlayer0[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].attacking = 1;
                }
                // Cartas bloqueando por el oponente
                for (int i = 0; i < cambios.cardsBlockingPlayer0.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsBlockingPlayer0[i])
                            idBattlefield = j;
                    }
                    if (game.opponent.battlefield[idBattlefield].blocking == 0)
                    {
                        game.opponent.battlefield[idBattlefield].blocking = 1;
                        game.currentlyBlocking = game.opponent.battlefield[idBattlefield];
                    }
                    else
                    {
                        if (game.opponent.battlefield[idBattlefield].attackerBlocked != null)
                        {
                            game.opponent.battlefield[idBattlefield].attackerBlocked.blocker = null;
                            game.opponent.battlefield[idBattlefield].attackerBlocked = null;
                        }
                        game.currentlyBlocking = game.opponent.battlefield[idBattlefield];
                    }
                }
                // Cartas bloqueadas por el oponente
                for (int i = 0; i < cambios.cardsBlockedPlayer0.Length; i++)
                {
                    for (int j = 0; j < game.player.battlefield.Count; j++)
                    {
                        if (game.player.battlefield[j].cardId == cambios.cardsBlockedPlayer0[i])
                            idBattlefield = j;
                    }
                    if (game.player.battlefield[idBattlefield].blocker != null)
                    {
                        game.player.battlefield[idBattlefield].blocker.blocking = 0;
                        game.player.battlefield[idBattlefield].blocker.attackerBlocked = null;
                    }
                    game.currentlyBlocking.attackerBlocked = game.player.battlefield[idBattlefield];
                    game.player.battlefield[idBattlefield].blocker = game.currentlyBlocking;
                    game.currentlyBlocking = null;
                }
                // Cartas destruidas del oponente
                for (int i = 0; i < cambios.cardsDestroyedPlayer0.Length; i++)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.cardsDestroyedPlayer0[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].destroy();
                }
                // Cartas descartadas por el oponente
                for (int i = 0; i < cambios.cardsDiscardedPlayer0.Length; i++)
                {
                    for (int j = 0; j < game.opponent.hand.Count; j++)
                    {
                        if (game.opponent.hand[j].id == cambios.cardsDiscardedPlayer0[i])
                            idHandDis = j;
                    }
                    game.opponent.hand[idHandDis].discard();
                }
                // Cartas del oponente que han aumentado sus stats temporalmente 
                for (int i = 0; i < cambios.temporalStatsPlayer0.Length; i += 2)
                {
                    for (int j = 0; j < game.opponent.battlefield.Count; j++)
                    {
                        if (game.opponent.battlefield[j].cardId == cambios.temporalStatsPlayer0[i])
                            idBattlefieldOp = j;
                    }
                    game.opponent.battlefield[idBattlefieldOp].temporalPower = cambios.temporalStatsPlayer0[i + 1];
                    game.opponent.battlefield[idBattlefieldOp].temporalToughness = cambios.temporalStatsPlayer0[i + 1];
                }
                if (cambios.victoryPlayer1 == 1)
                {
                    exit = true;
                    victory = 1;
                }
                // Cambios en las vidas del oponente
                if (cambios.livesPlayer0 != -1)
                    game.opponent.setLife(cambios.livesPlayer0);

                if (cambios.newActivePlayer != -1) game.changeActivePlayer(cambios.newActivePlayer);
                if (cambios.newPhase != -1) game.updatePhase(cambios.newPhase);
            }
        }
        
        static void render()
        {
            game.render();
        }
        static void handleEvents()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
            {
                switch (e.type)
                {
                    case SDL.SDL_EventType.SDL_QUIT:
                        exit = true;
                        break;
                    case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        game.handleEvents(e);
                        break;

                }
            }
        }
        public static float getWUnit()
        {
            int w, h;
            SDL.SDL_GetWindowSize(window, out w, out h);
            return (float)(w) / 100;
        }
        public static float getHUnit()
        {
            int w, h;
            SDL.SDL_GetWindowSize(window, out w, out h);
            return (float)(h) / 100;
        }
        public static Game getGame()
        {
            return game;
        }

        public static async Task<bool> UploadFileAsync(string filePath,string key)
        {
            var request = new PutObjectRequest
            {
                BucketName = "inputmts",
                Key = key,
                FilePath = filePath,
            };

            var response = await s3.PutObjectAsync(request); // subimos el objeto al bucket
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"Successfully uploaded {request.Key} to {request.BucketName}.");
                return true;
            }
            else
            {
                Console.WriteLine($"Could not upload {request.Key} to {request.BucketName}.");
                return false;
            }
            
        }
        public static void checkForChanges()
        {
                receiveInfo();
        }
    }
}
