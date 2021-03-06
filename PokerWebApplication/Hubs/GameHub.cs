﻿using Microsoft.AspNetCore.SignalR;
using PokerWebApp.DAL.Manager;
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
                await SendScoreInfo(Tables[tableMostPlayerIndex].id);
                
                if (Tables[tableMostPlayerIndex].Players.Count == NumberOfSeats)
                {
                    Tables[tableMostPlayerIndex].Full = true;
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
            table.NewRound();
          
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
                    CardOneHighlight = p.CardOneHighlight,
                    CardTwoHighlight = p.CardTwoHighlight,
                    Action = p.Action,
                    PotMoney = p.PotMoney


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
            
            TableInfo tableInfo = new TableInfo() { TableHand= GetTablebyId(tableId).Game.TableHand , TableHandHighlight = GetTablebyId(tableId).Game.TableHandHighlight,
                Pot = GetTablebyId(tableId).Game.Pot, VictoryText= GetTablebyId(tableId).Game.VictoryText
            };
       

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
                    Tables[i].NumberOfPlayers() != NumberOfSeats && Tables[i].Full == false)
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

        public async Task PlayerAction(int tableID, string action, string name)
        {

            if(GetTablebyId(tableID).Game.CurrentPlayer.Equals(name) && GetTablebyId(tableID).Game.GameEnded == false)
            {
                GetTablebyId(tableID).Game.ProcessPlayerAction(action);
                await NewGameLogMessage(GetTablebyId(tableID).Game.CurrentPlayer + " " + action);
                await SendPlayerInfo(tableID);
                if (GetTablebyId(tableID).Game.ReadyForNextRound)
                {
                    string roundResult = GetTablebyId(tableID).Game.FinishRound();
                    await SendPlayerInfo(tableID);
                    await SendTableInfo(tableID);
                    await NewGameLogMessage(roundResult);

                    foreach (Player player in GetTablebyId(tableID).Players)
                    {
                        await ShowPlayerCards(player.ConnectionId, player.Hand);
                    }
                    Thread.Sleep(15000);

                    await NewRound(GetTablebyId(tableID));

                }
                else
                {

                    if ((GetTablebyId(tableID).Game.ActivePlayers.Count - GetTablebyId(tableID).Game.AllInPlayers.Count < 2 && GetTablebyId(tableID).Game.ReadyForNextPhase)
                        || GetTablebyId(tableID).Game.ActivePlayers.Count == 1)
                    {
                        if (GetTablebyId(tableID).Game.AllInPlayers.Count > 1 || 
                            (GetTablebyId(tableID).Game.AllInPlayers.Count == 1 && GetTablebyId(tableID).Game.ActivePlayers.Count > 1))
                        {
                            for (int i = GetTablebyId(tableID).Game.GamePhase; i < 4; i++)
                            {
                                
                                GetTablebyId(tableID).Game.StartNextPhase();
                                await NewGameLogMessage(GetTablebyId(tableID).Game.GamePhaseName + " started");
                                //asztal lapok firssítése
                                Thread.Sleep(1000);
                                await SendTableInfo(tableID);
                                GetTablebyId(tableID).Game.GamePhase += 1;

                            }

                            string roundResult = GetTablebyId(tableID).Game.FinishRound();
                            await SendPlayerInfo(tableID);
                           
                            await NewGameLogMessage(roundResult);

                            foreach (Player player in GetTablebyId(tableID).Players)
                            {
                                await ShowPlayerCards(player.ConnectionId, player.Hand);
                            }

                            if (GetTablebyId(tableID).Game.GameEnded == false)
                            {
                                await SendTableInfo(tableID);
                                Thread.Sleep(15000);
                                await NewRound(GetTablebyId(tableID));

                            }
                            else
                            {
                                await SendTableInfo(tableID);
                                PokerAppDBManager manager = new PokerAppDBManager();
                                await manager.IncrementNumberOfGamesAsync(GetTablebyId(tableID).Game.WinnerPlayer);
                                await manager.IncrementNumberOfWinsAsync(GetTablebyId(tableID).Game.WinnerPlayer);
                                await manager.AddMoneyAsync(GetTablebyId(tableID).Game.WinnerPlayer, 500);
                                GetTablebyId(tableID).Reset();

                            }


                        }
                        else
                        {
                            string roundResult = GetTablebyId(tableID).Game.FinishRound();
                            await NewGameLogMessage(roundResult);


                            if (GetTablebyId(tableID).Game.GameEnded == false)
                            {
                               
                                await SendTableInfo(tableID);
                                Thread.Sleep(7000);
                                await NewRound(GetTablebyId(tableID));
                            }
                            else
                            {
                                await SendTableInfo(tableID);
                                PokerAppDBManager manager = new PokerAppDBManager();
                                await manager.IncrementNumberOfGamesAsync(GetTablebyId(tableID).Game.WinnerPlayer);
                                await manager.IncrementNumberOfWinsAsync(GetTablebyId(tableID).Game.WinnerPlayer);
                                await manager.AddMoneyAsync(GetTablebyId(tableID).Game.WinnerPlayer, 500);
                                GetTablebyId(tableID).Reset();
                            
                            }

                        }

                    }
                    else
                    {
                        if (GetTablebyId(tableID).Game.ReadyForNextPhase)
                        {
                            GetTablebyId(tableID).Game.StartNextPhase();
                            await NewGameLogMessage(GetTablebyId(tableID).Game.GamePhaseName + " started");
                            //asztal lapok firssítése
                            await SendTableInfo(tableID);
                        }

                        GetTablebyId(tableID).Game.GetCurrentPlayerAction();
                        int currentPlayerIndex = GetTablebyId(tableID).Game.CurrentPlayerPos;

                        if (GetTablebyId(tableID).Game.ActivePlayers[currentPlayerIndex].Action.Equals("giveup"))
                        {
                            await PlayerAction(tableID, "giveup", GetTablebyId(tableID).Game.ActivePlayers[currentPlayerIndex].Name);

                        }
                        else
                        {
                            await NewGameLogMessage(GetTablebyId(tableID).Game.CurrentPlayer + " 's turn");
                            await SendPlayerInfo(tableID);
                            await SendTableInfo(tableID);


                            int callValue = GetTablebyId(tableID).Game.GetCurrentPlayerCallValue();
                            await SendCallValue(GetTablebyId(tableID).Game.ActivePlayers[currentPlayerIndex].ConnectionId, callValue);

                        }


                    }


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
            if(GetTablebyId(tableID).NumberOfPlayers() != 0)
            {
                if (GetTablebyId(tableID).Game.PlayerKonckedOut.Contains(name))
                {
                    //player leaves the table after they have been knocked out
                    GetTablebyId(tableID).RemovePlayer(name);
                }
                else
                {
                    //player leaves the table while they can still play 
                    GetTablebyId(tableID).Players.Find(p => p.Name == name).Action = "giveup";
                    if (GetTablebyId(tableID).Players.Find(p => p.Name == name).PlayersTurn)
                    {
                        await PlayerAction(tableID, "giveup", name);
                    }

                }

               
                PokerAppDBManager manager = new PokerAppDBManager();
                await manager.IncrementNumberOfGamesAsync(name);
                
                await NewGameLogMessage(name + " has left the game");
                

            }
           

            await Clients.Caller.SendAsync("LeaveActionProcessed");

        }


    }
}
