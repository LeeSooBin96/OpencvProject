using System;
using System.Text;

//TCP 통신에 사용할 using
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Windows;

namespace WpfClient
{
    /*클라이언트 서버 연결부터 통신 분리할 클래스*/
    class HandlingClient
    {
        //필요한 객체
        private TcpClient client = null; //서버 연결 객체 저장
        private NetworkStream stream = null; //데이터 송수신 객체 저장
        private bool connectFlag = false; //연결 상태 저장
        //외부에서 건들수 있는 것들
        public string errMSG = null;

        //객체 초기화 및 서버 연결
        public bool ConnectingServer(string servIP,int servPort)
        {
            if (connectFlag)
            {
                errMSG = "이미 서버와 연결되어있습니다.";
                return false;
            }
            string bindIP = "10.10.20.98"; //클라이언트 자체 아이피
            const int bindPort = 0; //포트번호 자동 할당

            try
            {
                IPEndPoint clntAddr = new IPEndPoint(IPAddress.Parse(bindIP), bindPort);
                IPEndPoint servAddr = new IPEndPoint(IPAddress.Parse(servIP), servPort);

                client = new TcpClient(clntAddr); //통신에 사용할 객체 생성
                client.Connect(servAddr); //서버에 연결
                stream = client.GetStream(); //스트림 생성
                connectFlag = true; //연결 상태 변화
                return true;
            }
            catch(SocketException err)
            {
                errMSG = err.ToString();
                return false;
            }
        }
        public void ClosingConnect()
        {
            stream.Close();
            client.Close();
            connectFlag = false; //연결 상태 변화
        }
        public void SendData(string msg)
        {
            /*메시지 송신 규약 : 바이트 배열 크기 먼저 송신, 그 후 약속된코드@메시지 바이트 배열로*/
            byte[] msgBytes = Encoding.Default.GetBytes(msg); //메시지 바이트 배열로 저장
            byte[] len = BitConverter.GetBytes(msgBytes.Length); //메시지 바이트 배열 크기 

            stream.Write(len, 0, 4);
            stream.Write(msgBytes, 0, msgBytes.Length); //데이터는 크기와 함께 한번에 보내기
            //서버가 끊어 읽을 것
        }
        public byte[] RecvData()
        {
            //사용할 버퍼
            const int BUF_SIZE = 10000;
            byte[] buffer = new byte[BUF_SIZE];

            /*클라이언트와 통신 규약 : 메시지 길이 수신 그 후 약속된코드@내용 수신*/

            int readLen = stream.Read(buffer, 0, 4); //메시지 길이 수신
            if (readLen == 0) return null; //소켓 종료 시
            MessageBox.Show(BitConverter.ToInt32(buffer, 0).ToString());
            int strLen = BitConverter.ToInt32(buffer, 0); //메시지 길이 저장

            if (strLen > BUF_SIZE)
            { //메시지 길이가 더 크면 버퍼 사이즈대로 나눠서 받아야함
                List<byte> bufArr = new List<byte>();
                for (int i = 0; i < strLen / BUF_SIZE; i++)
                { //버퍼사이즈 만큼 읽어 들이기
                    stream.Read(buffer, 0, BUF_SIZE);
                    bufArr.AddRange(buffer);
                }
                stream.Read(buffer, 0, strLen % BUF_SIZE); //나머지 읽기
                MessageBox.Show((strLen % BUF_SIZE).ToString());
                bufArr.AddRange(buffer);
                MessageBox.Show("수신된 데이터 크기:" + bufArr.Count.ToString());
                return bufArr.ToArray();
            }
            else
            { //버퍼 사이즈가 메시지 길이보다 크면 메시지 길이만큼
                stream.Read(buffer, 0, strLen);
                return buffer;
            }
        }
    }
}
