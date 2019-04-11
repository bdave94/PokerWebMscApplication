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
                usersInGame.Add(gameId, new UserInGameInfo() {ConnectionID = Context.ConnectionId, TableId= Tables[tableMostPlayerIndex].id,
                    TablePosition= tablePosition});

                message = Tables[tableMostPlayerIndex].id + "@" + tablePosition+"@"+ gameId.ToString();

            } else
            {
                message = "full";
            }
            await Clients.Caller.SendAsync("queueResult", message);

            if(tableMostPlayerIndex != -1)
            {
                await GetTableInfo(Tables[tableMostPlayerIndex].id);
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
           

            await this.GetTableInfo(table.id);

            table.Game.GiveCards();

            foreach (Player player in table.Game.activePlayers)
            {
                ShowPlayerCards(player.ConnectionId, player.Hand);
            }

            table.Game.GetCurrentPlayerAction();
            await NewGameLogMessage("New round started");
            await NewGameLogMessage(table.Game.CurrentPlayer + " 's turn");

            await this.GetTableInfo(table.id);


        }
 

        public async Task GetTableInfo(int tableId )
        {
            List<Player> players = GetTablebyId(tableId).GetPlayers();

           
            List<PlayerInfo> playerInfo = new List<PlayerInfo>();

                foreach (Player p in players)
                {
                    playerInfo.Add(new PlayerInfo()
                    {
                        Chips = p.Chips,
                        Name = p.Name,
                        TablePosition = p.TablePosition,
                        Dealer = p.Dealer,
                        BigBlind = p.BigBlind,
                        SmallBlind = p.SmallBlind,
                        PlayersTurn = p.PlayersTurn
                    });
                }

                await Clients.All.SendAsync("getInfo", players);
            
            
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

      

        public void ShowPlayerCards(string connectionId, List<Card> hand)
        {
            Clients.Client(connectionId).SendAsync("ShowPlayerCards", hand);
                
        }

        public async Task PlayerAction(int playerPosition, int tableID, string action)
        {


            GetTablebyId(tableID).Game.ProcessPlayerAction(action);

            if (GetTablebyId(tableID).Game.ReadyForNextRound)
            {
                await this.NewRound(GetTablebyId(tableID));
            } else
            {
                GetTablebyId(tableID).Game.GetCurrentPlayerAction();

                await NewGameLogMessage(GetTablebyId(tableID).Game.CurrentPlayer + " 's turn");
                await this.GetTableInfo(tableID);

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
            ShowPlayerCards(Context.ConnectionId, this.GetTablebyId(info.TableId).Players[info.TablePosition].Hand);
            await Clients.Caller.SendAsync("RefreshGameUserInfo", info.TableId, info.TablePosition);
        }


    }
}
