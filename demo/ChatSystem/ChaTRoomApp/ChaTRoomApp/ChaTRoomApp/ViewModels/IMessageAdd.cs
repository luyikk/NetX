using System;
using ChaTRoomApp.Models;

namespace ChaTRoomApp.ViewModels
{
    public interface IMessageAdd
    {
        User My { get; }
        event EventHandler<MessageTable> MessageAdd;
    }
}