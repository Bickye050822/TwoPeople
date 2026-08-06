using System;
using System.Data;

//namespace 多人聊天.Sql;
namespace _2dPveDemoSever.DOA
{
    public class GameData
    {
        public virtual int PhoneNum { get; set; }
        public virtual string UserName { get; set; }
        public virtual string Password { get; set; }
        public virtual string PassResult { get; set; }
        public virtual string PassTime { get; set; }
        public virtual int PassScore { get; set; }
    }
}