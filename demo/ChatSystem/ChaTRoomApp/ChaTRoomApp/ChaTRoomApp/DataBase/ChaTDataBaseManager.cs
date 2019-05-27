using ChaTRoomApp.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ChaTRoomApp.DataBase
{
    public  class ChaTDataBaseManager
    {
        readonly SQLiteAsyncConnection _database;

        public ChaTDataBaseManager()
        {
            _database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChaTRoom.db3"));
            _database.CreateTableAsync<SaveUser>().Wait();
            _database.CreateTableAsync<MessageTable>().Wait();
        }

        public Task<SaveUser> GetSaveUser()
        {
            return _database.Table<SaveUser>().FirstOrDefaultAsync();
        }

        public  async Task ClecrUser()
        {
            await _database.Table<SaveUser>().Where(p => p.UserName != null).DeleteAsync();
        }

        public async Task SaveUser(SaveUser user)
        {
            await _database.InsertOrReplaceAsync(user);
        }

        public async Task SaveMessage(MessageTable table)
        {
            if (table.Id == 0)
                await _database.InsertAsync(table);
            else
                await _database.UpdateAsync(table);
        }

        public async Task ClecrMessage()
        {
            await _database.Table<MessageTable>().Where(p=>p.Id!=0).DeleteAsync();
        }

        public async Task<List<MessageTable>> GetMessage(byte type,long fromId=-1)
        {
            if (fromId != -1)
            {
                int count = await _database.Table<MessageTable>().Where(p => p.MsgType == type && (p.FromId == fromId||p.TargetId==fromId)).CountAsync();

                if (count < 60)
                {
                    return await _database.Table<MessageTable>().Where(p => p.MsgType == type && (p.FromId == fromId || p.TargetId == fromId)).ToListAsync();
                }
                else
                {
                    return await _database.Table<MessageTable>().Where(p => p.MsgType == type && (p.FromId == fromId || p.TargetId == fromId)).Skip(count - 60).Take(60).ToListAsync();
                }
            }
            else
            {
                int count = await _database.Table<MessageTable>().Where(p => p.MsgType == type).CountAsync();

                if (count < 60)
                {
                    return await _database.Table<MessageTable>().Where(p => p.MsgType == type).ToListAsync();
                }
                else
                {
                    return await _database.Table<MessageTable>().Where(p => p.MsgType == type).Skip(count - 60).Take(60).ToListAsync();
                }
            }
        }


    }
}
