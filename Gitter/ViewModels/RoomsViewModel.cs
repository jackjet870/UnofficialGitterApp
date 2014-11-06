﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using ReactiveUI;
using Splat;

namespace Gitter.ViewModels
{
    public class RoomsViewModel : ReactiveObject, IRoutableViewModel
    {
        private RoomViewModel selectedRoom;

        public RoomsViewModel(IGitterApi api = null, IScreen hostScreen = null)
        {
            this.HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.Rooms = new ReactiveList<RoomViewModel>();

            this.LoadRooms = ReactiveCommand.CreateAsyncObservable(_ => LoadRoomsImpl(api ?? GitterApi.UserInitiated));
            this.LoadRooms.Subscribe(x =>
            {
                using (this.Rooms.SuppressChangeNotifications())
                {
                    this.Rooms.Clear();
                    this.Rooms.AddRange(x);
                }
            });

            this.WhenAnyValue(x => x.SelectedRoom)
                .Where(x => x != null)
                .Subscribe(x => this.HostScreen.Router.Navigate.Execute(new MessagesViewModel(x.Room)));
        }

        public IScreen HostScreen { get; private set; }

        public ReactiveCommand<IEnumerable<RoomViewModel>> LoadRooms { get; private set; }

        public IReactiveList<RoomViewModel> Rooms { get; private set; }

        public RoomViewModel SelectedRoom
        {
            get { return this.selectedRoom; }
            set { this.RaiseAndSetIfChanged(ref this.selectedRoom, value); }
        }

        public string UrlPathSegment
        {
            get { return "Rooms"; }
        }

        private static IObservable<IEnumerable<RoomViewModel>> LoadRoomsImpl(IGitterApi api)
        {
            return GitterApi.GetAccessToken()
                .SelectMany(api.GetRooms)
                .Select(rooms => rooms.Select(room => new RoomViewModel(room)));
        }
    }
}