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

            foreach (Player player in table.Game.ActivePlayers)
            {
                await ShowPlayerCards(player.ConnectionId, player.Hand);
            }



            table.Game.GetCurrentPlayerAction();
            await NewGameLogMessage("New round started");
            await NewGameLogMessage(table.Game.CurrentPlayer + " 's turn");

            await SendPlayerInfo(table.id);

            int currentPlayerIndex = table.Game.CurrentPlayerPos;
            int callValue = table.Game.GetCurrentPlayerCallValue();
            await SendCallValue(table.Game.ActivePlayers[currentPlayerIndex].ConnectionId, callValue);

            await SendScoreInfo(table.id);
            await SendTableInfo(table.id);

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

        public async Task SendTableInfo(int tableId)
        {

            await Clients.All.SendAsync("GetTableInfo", GetTableInfo(tableId));


        }

        public TableInfo GetTableInfo(int tableId)
        {
            List<Player> players = GetTablebyId(tableId).GetPlayers();


            TableInfo tableInfo = new TableInfo() { TableHand= GetTablebyId(tableId).Game.TableHand , Pot = GetTablebyId(tableId).Game.Pot };

            

            return tableInfo;


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
                string roundResult = GetTablebyId(tableID).Game.FinishRound();
                await SendPlayerInfo(tableID);
                await NewGameLogMessage(roundResult);

                foreach (Player player in GetTablebyId(tableID).Players)
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
                    await SendTableInfo(tableID);
                } 

                if(GetTablebyId(tableID).Game.ActivePlayers.Count < 2)
                {
                    if(GetTablebyId(tableID).Game.AllInPlayers.Count > 0)
                    {
                        for(int i = GetTablebyId(tableID).Game.GamePhase+1; i < 4; i++)
                        {
                            GetTablebyId(tableID).Game.GamePhase += 1;                          
                            GetTablebyId(tableID).Game.StartNextPhase();
                            await NewGameLogMessage(GetTablebyId(tableID).Game.GamePhaseName + " started");
                            //asztal lapok firssítése
                            await SendTableInfo(tableID);
                            
                        }

                        string roundResult = GetTablebyId(tableID).Game.FinishRound();
                        await SendPlayerInfo(tableID);
                        await NewGameLogMessage(roundResult);

                        foreach (Player player in GetTablebyId(tableID).Players)
                        {
                            await ShowPlayerCards(player.ConnectionId, player.Hand);
                        }
                        Thread.Sleep(15000);
                        if(GetTablebyId(tableID).Game.GameEnded == false)
                        {
                            await NewRound(GetTablebyId(tableID));
                           
                        }
                       

                    } else
                    {
                        await NewGameLogMessage(GetTablebyId(tableID).Game.ActivePlayers[0].Name + " won the round");
                        Thread.Sleep(15000);

                        await NewRound(GetTablebyId(tableID));
                        
                    }

                } else
                {
                    GetTablebyId(tableID).Game.GetCurrentPlayerAction();
                   
                    await NewGameLogMessage(GetTablebyId(tableID).Game.CurrentPlayer + " 's turn");
                    await SendPlayerInfo(tableID);
                    await SendTableInfo(tableID);

                    int currentPlayerIndex = GetTablebyId(tableID).Game.CurrentPlayerPos;
                    int callValue = GetTablebyId(tableID).Game.GetCurrentPlayerCallValue();
                    await SendCallValue(GetTablebyId(tableID).Game.ActivePlayers[currentPlayerIndex].ConnectionId, callValue);

                }                
            }

            

          

        }

        public void ResetClientCardPictures()
        {
            Clients.All.SendAsync("ResetClientCardPictures");
        }


        public async Task SendCallValue(string connectionId, int callValue)
        { 
            await Clients.Client(connectionId).SendAsync("RefreshCallValueInfo", callValue);
        }

        public async Task SendScoreInfo(int tableId)
        {
            await Clients.All.SendAsync("RefreshScore", GetTablebyId(tableId).Game.ScoreBoard);
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
            await Clients.Caller.SendAsync("GetTableInfo", GetTableInfo(info.TableId));
            await Clients.Caller.SendAsync("RefreshScore", GetTablebyId(info.TableId).Game.ScoreBoard);

            if (GetTablebyId(info.TableId).Players[info.TablePosition].Name.Equals(GetTablebyId(info.TableId).Game.CurrentPlayer) )
            {
                int currentPlayerIndex = GetTablebyId(info.TableId).Game.CurrentPlayerPos;
                int callValue = GetTablebyId(info.TableId).Game.GetCurrentPlayerCallValue();
                await SendCallValue(GetTablebyId(info.TableId).Game.ActivePlayers[currentPlayerIndex].ConnectionId, callValue);

            }

        }

        public async Task LeaveTableAsync(int tableID, string name)
        {
            GetTablebyId(tableID).RemovePlayer(name);
            await NewGameLogMessage(name + "has left the game");
        }


    }
}
