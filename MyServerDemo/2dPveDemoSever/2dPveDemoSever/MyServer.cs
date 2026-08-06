using System;
using System.Collections.Generic;
using System.IO;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net.Config;
using Photon.SocketServer;

namespace _2dPveDemoSever
{
    public class MyServer : ApplicationBase
    { 
        public static ILogger log = LogManager.GetCurrentClassLogger();
        public static List<MyClient> clients = new List<MyClient>();

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            log.Info("有一个客户端连接了服务器");
            MyClient client = new MyClient(initRequest);
            clients.Add(client);
            log.Info("当前在线人数: " + clients.Count);
            return client;
        }

        protected override void Setup()
        {
            // 初始化日志系统
            InitLog();
            log.Info("服务器启动成功。");
            log.Warn("打印警告类型的信息日志");
            log.Error("打印错误类型的日志信息");
        }

        protected override void TearDown()
        {
            log.Info("服务器关闭成功。");
        }
        private void InitLog()
        {
            //指定日志文件输出位置 - 日志在bin_Win64/log目录下
            log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(Path.Combine(ApplicationRootPath, "bin_Win64"), "log");
            //写入日志
            FileInfo fileInfo = new FileInfo(Path.Combine(this.BinaryPath, "log4net.config"));
            if (fileInfo.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(fileInfo);
            }
        }
 
    }
}