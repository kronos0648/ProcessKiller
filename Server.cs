using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ProcessKiller
{
    public class Server
    {
        private IPEndPoint serverIP;
        private IPEndPoint clientIP;
        private Socket serverSocket;
        private Socket clientSocket;
        bool Run { get; set; }
        public Server()
        {
            Run = true;
            serverIP = new IPEndPoint(IPAddress.Any, 8000);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(serverIP);
            ConnectClient();
        }

        public void ConnectClient()
        {
            Console.WriteLine("클라이언트 대기중");
            serverSocket.Listen(20);
            clientSocket = serverSocket.Accept();
            clientIP = (IPEndPoint)clientSocket.RemoteEndPoint;
            Console.WriteLine("연결 완료");
            KillProcess();
        }

        public string RecvData()
        {
            byte[] _data = new byte[1024];
            clientSocket.Receive(_data);
            byte[] realData=null;
            for(int i=0;i<_data.Length;i++)
            {
                if(_data[i]==0)
                {
                    realData = new byte[i];
                    for(int j=0;j<i;j++)
                    {
                        realData[j] = _data[j];
                    }
                    break;
                }
            }
            string data = Encoding.Default.GetString(realData);
            Console.WriteLine("받은 데이터 : " + data);
            return data;
        }

        public void KillProcess()
        {
            try
            {
                while (Run)
                {
                    Console.WriteLine("받아봄");
                    string offer = RecvData();
                    bool isSuccess=false;
                    
                    if(offer=="END")
                    {
                        Console.WriteLine("강제종료함");
                        System.Environment.Exit(1);
                    }

                    else
                    {
                        Console.WriteLine("시도해봄");
                        Process[] arrP = Process.GetProcessesByName(offer);
                        if (arrP.GetLength(0) > 0)
                        {
                            Console.WriteLine("발견");
                            foreach (Process p in arrP)
                            {

                                p.Kill();
                            }
                            isSuccess = true;
                        }
                    }
     
                    SendACK(isSuccess);
                }
                

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ConnectClient();
            }
        }

        public void SendACK(bool isSuccess)
        {
            string ack = null;
            switch(isSuccess)
            {
                case true:
                    ack = (char)2 + "1,0 OK" + (char)13;
                    break;
                case false:
                    ack = (char)2 + "0,1 OK" + (char)13;
                    break;
            }

            Byte[] _data = Encoding.Default.GetBytes(ack);
            clientSocket.Send(_data);
        }

    }
}
