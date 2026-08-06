using _2dPveDemoSever.DOA;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using System;

namespace _2dPveDemoSever.DOA;

public class DataManager
{
    public ISession GetSession()
    {
        var configration = new Configuration();
        // 2. 加载 NHibernate 核心配置文件（hibernate.cfg.xml）
        configration.Configure();
        // 3. 加载指定程序集中的所有 ORM 映射文件（.hbm.xml）
        configration.AddAssembly("2dPveDemoSever");
        Console.WriteLine("数据库映射配置成功");
        return configration.BuildSessionFactory().OpenSession();
    }

    public string GetUserName(int phoneNum)
    {
        using (ISession session = GetSession())
        {
            GameData gamedata = session.CreateCriteria(typeof(GameData))
                .Add(Restrictions.Eq("PhoneNum", phoneNum))
                .UniqueResult<GameData>();
            return gamedata != null ? gamedata.UserName : "";
        }
    }

    public bool VerifyUser(int phoneNum, string password) //比较用户名和密码
    {
        using (ISession session = GetSession())
        {
            GameData gamedata = session.CreateCriteria(typeof(GameData))                                                                    
                .Add(Restrictions.Eq("PhoneNum", phoneNum))
                .Add(Restrictions.Eq("Password", password))
                .UniqueResult<GameData>();
            return gamedata != null;
        }
    }

    public bool Register(int phoneNum, string password, string userName) //添加用户
    {
        GameData gamedata;
        using (ISession session = GetSession())
        {
            gamedata = session.CreateCriteria(typeof(GameData))
                .Add(Restrictions.Eq("PhoneNum", phoneNum))
                .UniqueResult<GameData>();
        }

        if (gamedata != null)
        {
            return false;
        }

        gamedata = new GameData();
        gamedata.PhoneNum = phoneNum;
        gamedata.UserName = userName;
        gamedata.Password = password;
        using (ISession session = GetSession())
        {
            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Save(gamedata);
                transaction.Commit();
            }
        }

        return true;
    }

    public bool ChangePassword(int phoneNum, string oldPwd, string newPwd) //修改密码
    {
        // 先验证旧密码是否正确
        if (!VerifyUser(phoneNum, oldPwd))
        {
            return false;
        }

        using (ISession session = GetSession())
        {
            GameData gamedata = session.CreateCriteria(typeof(GameData))
                .Add(Restrictions.Eq("PhoneNum", phoneNum))
                .UniqueResult<GameData>();

            if (gamedata == null)
            {
                return false;
            }

            using (ITransaction transaction = session.BeginTransaction())
            {
                gamedata.Password = newPwd;
                session.Update(gamedata);
                transaction.Commit();
            }

            return true;
        }
    }

    public bool DeleteUser(int phoneNum, string password) //注销用户
    {
        // 先验证账号密码是否正确
        if (!VerifyUser(phoneNum, password))
        {
            return false;
        }

        using (ISession session = GetSession())
        {
            GameData gamedata = session.CreateCriteria(typeof(GameData))
                .Add(Restrictions.Eq("PhoneNum", phoneNum))
                .UniqueResult<GameData>();

            if (gamedata == null)
            {
                return false;
            }

            using (ITransaction transaction = session.BeginTransaction())
            {
                session.Delete(gamedata);
                transaction.Commit();
            }

            return true;
        }
    }

    public bool UpdateGameResult(int phoneNum, string passResult, string passTime, int passScore)
    {
        using (ISession session = GetSession())
        {
            GameData gamedata = session.CreateCriteria(typeof(GameData))
                .Add(Restrictions.Eq("PhoneNum", phoneNum))
                .UniqueResult<GameData>();

            if (gamedata == null)
                return false;

            using (ITransaction transaction = session.BeginTransaction())
            {
                gamedata.PassResult = passResult;
                gamedata.PassTime = passTime;
                gamedata.PassScore = passScore;
                session.Update(gamedata);
                transaction.Commit();
            }
            return true;
        }
    }
}