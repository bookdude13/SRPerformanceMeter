using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using MelonLoader;
using Newtonsoft.Json;
using PerformanceMeter.Messages;
using PerformanceMeter.Events;

namespace PerformanceMeter
{
    // Events coming from SynthRiders-Websockets-Mod
    class SynthRidersEventsManager
    {
        private readonly MelonLogger.Instance logger;
        private readonly WebSocket socket;
        private bool isConnected = false;
        private ISynthRidersEventHandler eventHandler;

        public SynthRidersEventsManager(MelonLogger.Instance logger, string connectionString, ISynthRidersEventHandler eventHandler)
        {
            this.logger = logger;
            this.eventHandler = eventHandler;
            
            // Set up, but don't open yet
            socket = new WebSocket(connectionString);
            socket.OnOpen += (sender, e) => HandleOpen(sender);
            socket.OnMessage += HandleMessage;
            socket.OnError += HandleError;
            socket.OnClose += HandleClose;
        }

        public void StartAsync()
        {
            try
            {
                if (isConnected)
                {
                    logger.Msg("Closing open websocket...");
                    socket.Close();
                }

                logger.Msg("Connecting to websocket at " + socket.Url);
                socket.ConnectAsync();
            }
            catch (Exception e)
            {
                logger.Msg("Failed to start websocket client! " + e.Message);
            }
        }

        private void HandleOpen(object sender)
        {
            logger.Msg(string.Format(
                "Opened websocket client with sender {0}",
                sender
            ));
            isConnected = true;
        }

        private void HandleMessage(object sender, MessageEventArgs messageArgs)
        {
            try
            {
                // Parse top layer as generic to get type, then parse the specific message data if needed.
                var genericEvent = JsonConvert.DeserializeObject<SynthRidersEvent<object>>(messageArgs.Data);
                if (genericEvent.eventType == "SongStart")
                {
                    var songStart = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSongStart>>(messageArgs.Data);
                    eventHandler.OnSongStart(songStart.data);
                }
                else if (genericEvent.eventType == "SongEnd")
                {
                    var songEnd = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSongEnd>>(messageArgs.Data);
                    eventHandler.OnSongEnd(songEnd.data);
                }
                else if (genericEvent.eventType == "PlayTime")
                {
                    var playTime = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataPlayTime>>(messageArgs.Data);
                    eventHandler.OnPlayTime(playTime.data);
                }
                else if (genericEvent.eventType == "NoteHit")
                {
                    var noteHit = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataNoteHit>>(messageArgs.Data);
                    eventHandler.OnNoteHit(noteHit.data);
                }
                else if (genericEvent.eventType == "NoteMiss")
                {
                    var noteMiss = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataNoteMiss>>(messageArgs.Data);
                    eventHandler.OnNoteMiss(noteMiss.data);
                }
                else if (genericEvent.eventType == "SceneChange")
                {
                    var sceneChange = JsonConvert.DeserializeObject<SynthRidersEvent<EventDataSceneChange>>(messageArgs.Data);
                    eventHandler.OnSceneChange(sceneChange.data);
                }
                else if (genericEvent.eventType == "ReturnToMenu")
                {
                    eventHandler.OnReturnToMenu();
                }
            }
            catch (Exception e)
            {
                logger.Msg("Failed to parse message " + messageArgs.Data + ": " + e.Message);
            }
        }

        private void HandleError(object sender, ErrorEventArgs errorArgs)
        {
            logger.Msg(string.Format(
                "Error in websocket handling: {0}.\n{1}",
                errorArgs.Message,
                errorArgs.Exception
            ));
        }

        private void HandleClose(object sender, CloseEventArgs closeArgs)
        {
            logger.Msg(string.Format(
                "Closing websocket client with code {0} ({1})",
                closeArgs.Code,
                closeArgs.Reason
            ));

            isConnected = false;
        }

        public void Shutdown()
        {
            if (isConnected)
            {
                socket.Close();
            }
        }
    }
}
