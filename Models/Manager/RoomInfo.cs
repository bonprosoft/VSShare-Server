﻿using Microsoft.AspNet.SignalR;
using ProtocolModels.Notifications;
using Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.Manager
{
    public class RoomInfo
    {
        public string RoomId { get; set; }

        public Room Room { get; set; }

        public HashSet<string> Listeners { get; set; } = new HashSet<string>();

        public HashSet<string> Broadcasters { get; set; } = new HashSet<string>();

        public long ViewCount { get; set; } = 0;

        public long VisitorCount { get; set; } = 0;

        public bool IsBroadcasting { get; set; } = false;

        public Dictionary<string, SessionManager> Sessions { get; } = new Dictionary<string, SessionManager>();

        public SessionManager CurrentSession { get; private set; }

        public SessionManager GetSession(string id)
        {
            if (this.Sessions.ContainsKey(id))
            {
                return this.Sessions[id];
            }

            return null;
        }

        public async Task NotifyStartBroadcast()
        {
            var item = new BroadcastEventNotification()
            {
                EventType = BroadcastEventType.StartBroadcast
            };

            var manager = ListenerManager.GetInstance();

            await Task.Run(() =>
            {
                Parallel.ForEach(this.Listeners, c =>
                {
                    var listener = manager.GetConnectionInfo(c);
                    if (listener != null)
                    {
                        lock (listener)
                        {
                            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ListenHub>();
                            hubContext.Clients.Group(this.RoomId).NotifyBroadcastEvent(item);
                        }
                    }
                });
            });
        }

        public async Task NotifyStopBroadcast()
        {
            var item = new BroadcastEventNotification()
            {
                EventType = BroadcastEventType.StopBroadcast
            };
            var manager = ListenerManager.GetInstance();

            await Task.Run(() =>
            {
                Parallel.ForEach(this.Listeners, c =>
                {
                    var listener = manager.GetConnectionInfo(c);
                    if (listener != null)
                    {
                        lock (listener)
                        {
                            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ListenHub>();
                            hubContext.Clients.Group(this.RoomId).NotifyBroadcastEvent(item);
                        }
                    }
                });
            });
        }

        public async Task UpdateBroadcastStatus()
        {
            var item = new UpdateBroadcastStatusNotification()
            {
                VisitorCount = this.VisitorCount,
                ViewCount = this.ViewCount
            };
            var manager = ListenerManager.GetInstance();

            await Task.Run(() =>
            {
                Parallel.ForEach(this.Listeners, c =>
                {
                    var listener = manager.GetConnectionInfo(c);
                    if (listener != null)
                    {
                        lock (listener)
                        {
                            IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ListenHub>();
                            hubContext.Clients.Group(this.RoomId).UpdateBroadcastEvent(item);
                        }
                    }
                });
            });
        }

        public async Task AppendSession(AppendSessionNotification item)
        {
            if (!this.Sessions.ContainsKey(item.SessionId))
            {
                var session = new SessionManager()
                {
                    Id = item.SessionId,
                    ContentType = item.ContentType,
                    FileName = item.FileName
                };

                this.Sessions.Add(item.SessionId, session);

                var manager = ListenerManager.GetInstance();
                await Task.Run(() =>
                {
                    Parallel.ForEach(this.Listeners, c =>
                    {
                        var listener = manager.GetConnectionInfo(c);
                        if (listener != null)
                        {
                            lock (listener)
                            {
                                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ListenHub>();
                                hubContext.Clients.Group(this.RoomId).AppendSession(item);
                            }
                        }
                    });
                });
            }
        }

        public async Task RemoveSession(RemoveSessionNotification item)
        {
            if (this.Sessions.ContainsKey(item.SessionId))
            {
                this.Sessions.Remove(item.SessionId);

                var manager = ListenerManager.GetInstance();
                await Task.Run(() =>
                {
                    Parallel.ForEach(this.Listeners, c =>
                    {
                        var listener = manager.GetConnectionInfo(c);
                        if (listener != null)
                        {
                            lock (listener)
                            {
                                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ListenHub>();
                                hubContext.Clients.Group(this.RoomId).RemoveSession(item);
                            }
                        }
                    });
                });
            }
        }

        public async Task SwitchActiveSession(SwitchActiveSessionNotification item)
        {
            if (this.Sessions.ContainsKey(item.SessionId))
            {
                this.CurrentSession = this.Sessions[item.SessionId];

                var manager = ListenerManager.GetInstance();
                await Task.Run(() =>
                {
                    Parallel.ForEach(this.Listeners, c =>
                    {
                        var listener = manager.GetConnectionInfo(c);
                        if (listener != null)
                        {
                            lock (listener)
                            {
                                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ListenHub>();
                                hubContext.Clients.Group(this.RoomId).SwitchActiveSession(item);
                            }
                        }
                    });
                });
            }
        }

        public async Task UpdateSession(UpdateSessionNotification item)
        {
            if (this.Sessions.ContainsKey(item.SessionId))
            {
                var session = this.Sessions[item.SessionId];
                session.ContentType = item.ContentType;
                session.FileName = item.FileName;

                var manager = ListenerManager.GetInstance();
                await Task.Run(() =>
                {
                    Parallel.ForEach(this.Listeners, c =>
                    {
                        var listener = manager.GetConnectionInfo(c);
                        if (listener != null)
                        {
                            lock (listener)
                            {
                                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ListenHub>();
                                hubContext.Clients.Group(this.RoomId).UpdateSession(item);
                            }
                        }
                    });
                });
            }
        }

        public async Task UpdateSessionContent(UpdateContentNotification item)
        {
            var session = this.GetSession(item.SessionId);
            if (session != null)
            {
                session.UpdateContent(item);

                var manager = ListenerManager.GetInstance();
                await Task.Run(() =>
                {
                    Parallel.ForEach(this.Listeners, c =>
                    {
                        var listener = manager.GetConnectionInfo(c);
                        if (listener != null)
                        {
                            lock (listener)
                            {
                                IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<ListenHub>();
                                hubContext.Clients.Group(this.RoomId).NotifyBroadcastEvent(item);
                            }
                        }
                    });
                });
            }
        }


    }

}
