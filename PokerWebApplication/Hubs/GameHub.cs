using Microsoft.AspNetCore.SignalR;
using PokerWebApplication.Game;
using PokerWebApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PokerWebApplication.Hubs
{
    public class GameHub:Hub
    {
        public static List<Table> Tables = new List<Table>();
        private const int NumberOfSeats = 3;

        public static List<string> Connections = new List<string>();

        public static Dictionary<Guid, UserInGameInfo> usersInGame = new Dictionary<Guid, UserInGameInfo>();


        public async Task AddPlayer(string username)
        {
            if (Tables.Count == 0)
            {
                Tables.Add(new Table { id = 1 });
            }


            int tableMostPlayerIndex = FindTableWithMostPlayers();
            
            string message; 
           
            if(tableMostPlayerIndex != -1)
            {
                int tablePosition = Tables[tableMostPlayerIndex].AddNewPlayer(username, Context.ConnectionId);
               
               

                Guid gameId = Guid.NewGuid();
                usersInGame.Add(gameId, new UserInGameInfo() { TableId= Tables[tableMostPlayerIndex].id,
                    TablePosition= tablePosition});

                message = Tables[tableMostPlayerIndex].id + "@" + tablePosition+"@"+ gameId.ToString();

            } else
            {
                message = "full";
            }
            await Clients.Caller.SendAsync("queueResult", message);

            if(tableMostPlayerIndex != -1)
            {
                await SendPlayerInfo(Tables[tableMostPlayerIndex].id);
                if (Tables[tableMostPlayerIndex].Players.Count == NumberOfSeats)
                {

                    await StartGameAsync(Tables[tableMostPlayerIndex]);
                    
                }
            }
            
        }

        public async Task StartGameAsync(Table table)
        {

            table.InitGame();

            await NewRound(table);
 

        }

        public async Task NewRound(Table table)
        {
            table.Game.StartNextRound();
          
            ResetClientCardPictures();

            table.Game.GetBlinds();

            table.Game.GiveCards();

            await SendPlayerInfo(table.id);

            foreach (Player player in table.Game.activePlayers)
            {
                await ShowPlayerCards(player.ConnectionId, player.Hand);
            }

            table.Game.GetCurrentPlayerAction();
            await NewGameLogMessage("New round started");
            await NewGameLogMessage(table.Game.CurrentPlayer + " 's turn");

            await SendPlayerInfo(table.id);


        }
 

        public async Task SendPlayerInfo(int tableId )
        {
           
             await Clients.All.SendAsync("getInfo", GetPlayerInfo(tableId));
            
            
        }

        public List<PlayerInfo> GetPlayerInfo(int tableId)
        {
            List<Player> players = GetTablebyId(tableId).GetPlayers();


            List<PlayerInfo> playerInfo = new List<PlayerInfo>();

            foreach (Player p in players)
            {
                playerInfo.Add(new PlayerInfo()
                {
                    Name = p.Name,
                    Chips = p.Chips,
                    BigBlind = p.BigBlind,
                    SmallBlind = p.SmallBlind,
                    Dealer = p.Dealer,
                    PlayersTurn = p.PlayersTurn,
                    TablePosition = p.TablePosition,
                    CardOne = p.PublicHand[0],
                    CardTwo = p.PublicHand[1],
                    Action = p.Action


                });
            }

            return playerInfo;


        }



        public async Task GetTableCardInfo(int tableId)
        {
            
            await Clients.All.SendAsync("GetTableCardInfo", GetTablebyId(tableId).Game.TableHand);

        }


            public async Task NewGameLogMessage(string message)
        {
           
            await Clients.All.SendAsync("messageReceived",  message);
        }

        private Table GetTablebyId(int tableID)
        {
            return Tables.SingleOrDefault(t => t.id == tableID);
        }


        private int FindTableWithMostPlayers()
        {
            int tableMostPlayerIndex = -1;
            int tableHighestPlayers = -1;
            for (int i = 0; i < Tables.Count; i++)
            {
                if (Tables[i].NumberOfPlayers() > tableHighestPlayers &&
                    Tables[i].NumberOfPlayers() != NumberOfSeats)
                {
                    tableHighestPlayers = Tables[i].NumberOfPlayers();
                    tableMostPlayerIndex = i;
                }

            }

            return tableMostPlayerIndex;

        }

      

        public async Task ShowPlayerCards(string connectionId, List<Card> hand)
        {
            await Clients.Client(connectionId).SendAsync("ShowPlayerCards", hand);
                
        }

        public async Task PlayerAction(int playerPosition, int tableID, string action)
        {


            GetTablebyId(tableID).Game.ProcessPlayerAction(action);
            await NewGameLogMessage(GetTablebyId(tableID).Game.CurrentPlayer +" "+ action);
            await SendPlayerInfo(tableID);
            if (GetTablebyId(tableID).Game.ReadyForNextRound)
            {
                GetTablebyId(tableID).Game.FinishRound();
                await SendPlayerInfo(tableID);

                foreach (Player player in GetTablebyId(tableID).Game.activePlayers)
                {
                    await ShowPlayerCards(player.ConnectionId, player.Hand);
                }
                Thread.Sleep(15000);

                await NewRound(GetTablebyId(tableID));
            } else
            {
                if(GetTablebyId(tableID).Game.ReadyForNextPhase)
                {
                    GetTablebyId(tableID).Game.StartNextPhase();
                    await NewGameLogMessage(GetTablebyId(tableID).Game.GamePhaseName+ " started");
                    //asztal lapok firssítése
                    await GetTableCardInfo(tableID);
                } 
                
                GetTablebyId(tableID).Game.GetCurrentPlayerAction();

                await NewGameLogMessage(GetTablebyId(tableID).Game.CurrentPlayer + " 's turn");
                await SendPlayerInfo(tableID);


            }

          

        }

        public void ResetClientCardPictures()
        {
            Clients.All.SendAsync("ResetClientCardPictures");
        }


        public async Task RefreshGame(string gameId)
        {
            Guid gameid = Guid.Parse(gameId);
            UserInGameInfo info;
            usersInGame.TryGetValue(gameid, out info);

            GetTablebyId(info.TableId).Players[info.TablePosition].ConnectionId = Context.ConnectionId;

            await Clients.Caller.SendAsync("RefreshGameUserInfo", info.TableId, info.TablePosition);
            await Clients.Caller.SendAsync("getInfo", GetPlayerInfo(info.TableId));
            await ShowPlayerCards(Context.ConnectionId, GetTablebyId(info.TableId).Players[info.TablePosition].Hand);
            await Clients.All.SendAsync("GetTableCardInfo", GetTablebyId(info.TableId).Game.TableHand);
        }


    }
}
