﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common;
using Common.ActionInfo;
using Common.Interfaces;
using GameMaster.ActionHandlers;
using GameMaster.Configuration;
using Messaging.InitializationMessages;
using NLog;

namespace GameMaster
{
    public class GameMaster : IGameMaster
    {
        private readonly IErrorsMessagesFactory _errorsMessagesFactory;
        private readonly GameConfiguration _gameConfiguration;
        private readonly string _gameName;
        private readonly MessagingHandler _messagingHandler;
        private ActionHandlerDispatcher _actionHandler;
        private GameMasterBoardGenerator _boardGenerator;

        private int _gameId;
        private Dictionary<int, bool> _hasPlayerBeenInformedAboutGameResult = new Dictionary<int, bool>();
        private PieceGenerator _pieceGenerator;
        private Dictionary<Guid, int> _playerGuidToId;
        private List<(TeamColor team, PlayerType role)> _playersSlots;
        private Timer checkIfFullTeamTimer;

        public GameMaster(GameConfiguration gameConfiguration, ICommunicationClient communicationCommunicationClient,
            string gameName, IErrorsMessagesFactory errorsMessagesFactory, LoggingMode loggingMode)
        {
            _gameConfiguration = gameConfiguration;
            _gameName = gameName;

            checkIfFullTeamTimer = new Timer(CheckIfGameFullCallback, null,
                (int) Constants.GameFullCheckStartDelay.TotalMilliseconds,
                (int) Constants.GameFullCheckInterval.TotalMilliseconds);

            _errorsMessagesFactory = errorsMessagesFactory;

            _messagingHandler = new MessagingHandler(gameConfiguration, communicationCommunicationClient, HostNewGame);
            _messagingHandler.MessageReceived += (sender, args) => MessageHandler(args);

            VerboseLogger = new VerboseLogger(LogManager.GetCurrentClassLogger(), loggingMode);
            KnowledgeExchangeManager = new KnowledgeExchangeManager();
            HostNewGame();
        }

        /// <summary>
        ///     Only for tests
        /// </summary>
        public GameMaster(GameMasterBoard board, Dictionary<Guid, int> playerGuidToId)
        {
            Board = board;

            _playerGuidToId = playerGuidToId;
        }

        public VerboseLogger VerboseLogger { get; }

        public GameMasterBoard Board { get; private set; }

        public bool IsSlotAvailable()
        {
            return _playersSlots.Count > 0;
        }

        public (int gameId, Guid playerGuid, PlayerBase playerInfo) AssignPlayerToAvailableSlotWithPrefered(
            int playerId, TeamColor preferedTeam, PlayerType preferedRole)
        {
            (TeamColor team, PlayerType role) assignedValue;
            if (_playersSlots.Contains((preferedTeam, preferedRole)))
                assignedValue = (preferedTeam, preferedRole);
            else if (_playersSlots.Contains((preferedTeam, PlayerType.Member)))
                assignedValue = (preferedTeam, PlayerType.Member);
            else
                assignedValue = _playersSlots.First();

            _playersSlots.Remove(assignedValue);

            var playerInfo = new PlayerInfo(playerId, assignedValue.team, assignedValue.role);
            Board.Players.Add(playerId, playerInfo);

            var playerGuid = Guid.NewGuid();
            _playerGuidToId.Add(playerGuid, playerId);

            return (_gameId, playerGuid, playerInfo);
        }

        public void HandlePlayerDisconnection(int playerId)
        {
            VerboseLogger.Log($"Player {playerId} disconnected from game");

            if (!_playerGuidToId.ContainsValue(playerId))
                return;
            var disconnectedPlayerPair = _playerGuidToId.Single(x => x.Value == playerId);
            _playerGuidToId.Remove(disconnectedPlayerPair.Key);
        }

        public void RegisterGame()
        {
            _messagingHandler.CommunicationClient.Send(new RegisterGameMessage(new GameInfo(_gameName,
                _gameConfiguration.GameDefinition.NumberOfPlayersPerTeam,
                _gameConfiguration.GameDefinition.NumberOfPlayersPerTeam)));
        }

        public IKnowledgeExchangeManager KnowledgeExchangeManager { get; private set; }

        public int? Authorize(Guid playerGuid)
        {
            if (_playerGuidToId.ContainsKey(playerGuid))
                return _playerGuidToId[playerGuid];
            return null;
        }

        public void SendDataToInitiator(int initiatorId, IMessage message)
        {
            _messagingHandler.CommunicationClient.Send(message);
        }

        public bool PlayerIdExists(int playerId)
        {
            return _playerGuidToId.ContainsValue(playerId);
        }

        public void SetGameId(int gameId)
        {
            _gameId = gameId;
        }

        public (BoardData data, bool isGameFinished) EvaluateAction(ActionInfo actionInfo)
        {
            var playerId = _playerGuidToId[actionInfo.PlayerGuid];
            var action = _actionHandler.Resolve((dynamic) actionInfo, playerId);
            var hasGameEnded = Board.IsGameFinished();
            if (hasGameEnded)
            {
                GameFinished?.Invoke(this, new GameFinishedEventArgs(Board.CheckWinner()));
                _messagingHandler.FinishGame();
            }

            return (data: action.Respond(), isGameFinished: hasGameEnded);
        }

        public void MessageHandler(IMessage message)
        {
            // TODO Log all messages
            if (message is IRequestMessage request)
                PutActionLog(request);

            IMessage response;
            lock (Board.Lock)
            {
                response = message.Process(this);

                if (response != null)
                    _messagingHandler.CommunicationClient.Send(response);
            }
        }

        private void CheckIfGameFullCallback(object obj)
        {
            if (_playersSlots.Count > 0) return;

            var boardInfo = new BoardInfo(Board.Width, Board.TaskAreaSize, Board.GoalAreaSize);

            _messagingHandler.StartListeningToRequests(_playerGuidToId.Keys);

            _boardGenerator.SpawnGameObjects(_gameConfiguration.GameDefinition);

            _pieceGenerator = new PieceGenerator(Board, _gameConfiguration.GameDefinition.ShamProbability,
                _gameConfiguration.GameDefinition.PlacingNewPiecesFrequency);

            KnowledgeExchangeManager = new KnowledgeExchangeManager();
            _actionHandler = new ActionHandlerDispatcher(Board, KnowledgeExchangeManager);

            foreach (var i in _playerGuidToId)
            {
                var playerLocation = Board.Players.Values.Single(x => x.Id == i.Value).Location;
                var gameStartMessage = new GameMessage(i.Value, Board.Players.Values, playerLocation, boardInfo);
                _messagingHandler.CommunicationClient.Send(gameStartMessage);
            }
        }

        private void HostNewGame()
        {
            _boardGenerator = new GameMasterBoardGenerator();
            Board = _boardGenerator.InitializeBoard(_gameConfiguration.GameDefinition);
            _playersSlots =
                _boardGenerator.GeneratePlayerSlots(_gameConfiguration.GameDefinition.NumberOfPlayersPerTeam);

            _playerGuidToId = new Dictionary<Guid, int>();
            foreach (var player in Board.Players) _playerGuidToId.Add(Guid.NewGuid(), player.Key);

            RegisterGame();
        }

        public virtual event EventHandler<GameFinishedEventArgs> GameFinished;

        public void PutActionLog(IRequestMessage record)
        {
            var playerId = _playerGuidToId[record.PlayerGuid];
            var playerInfo = Board.Players[playerId];
            var actionLog = new RequestLog(record, playerId, playerInfo.Team, playerInfo.Role);
            VerboseLogger.Log(actionLog.ToLog());
        }

        public void LogGameResults(TeamColor winners)
        {
            foreach (var player in Board.Players.Values)
            {
                var result = GameResult.Defeat;
                var playerGuid = _playerGuidToId.FirstOrDefault(p => p.Value == player.Id).Key;

                if (player.Team == winners)
                    result = GameResult.Victory;

                VerboseLogger.Log(
                    $"{result}, {DateTime.Now}, {_gameId}, {player.Id}, {playerGuid},{player.Team}, {player.Role}");
            }
        }
    }

    public class GameFinishedEventArgs : EventArgs
    {
        public GameFinishedEventArgs(TeamColor winners)
        {
            Winners = winners;
        }

        public TeamColor Winners { get; set; }
    }
}