using System;
using System.Text;

//TCP
using System.Net;
using System.Net.Sockets;

namespace WpfServer
{
    internal class HandlingServer
    {
        //필요한 객체
        public TcpListener serv = null; //서버 객체
        //외부에서 조회 가능
        public string errMSG = null;
        
        //서버 오픈
        public bool ServerOpen(string IP, int Port) //서버 정보 초기화 및 오픈
        {
            try
            {
                IPEndPoint localAddr = new IPEndPoint(IPAddress.Parse(IP), Port); //서버 주소 정보 초기화

                serv = new TcpListener(localAddr); //서버 객체 초기화
                serv.Start(); //서버 오픈
                return true;
            }
            catch(SocketException err)
            {
                errMSG = err.ToString();
                return false;
            }
        }
        public void SendData(NetworkStream stream,string msg)
        {
            byte[] msgBytes = Encoding.Default.GetBytes(msg); //메시지 바이트 배열로 저장
            byte[] len = BitConverter.GetBytes(msgBytes.Length); //메시지 바이트 배열 크기 

            stream.Write(len, 0, 4);
            stream.Write(msgBytes, 0, msgBytes.Length); //데이터는 크기와 함께 한번에 보내기
            //서버가 끊어 읽을 것
        }
        public byte[] RecvData(NetworkStream stream)
        {
            //사용할 버퍼
            //const int BUF_SIZE = 10000;

            /*클라이언트와 통신 규약 : 메시지 길이 수신 그 후 약속된코드@내용 수신*/
            byte[] len = new byte[4];
            int readLen = stream.Read(len, 0, 4); //메시지 길이 수신
            if (readLen == 0) return null; //소켓 종료 시
            int strLen = BitConverter.ToInt32(len, 0); //메시지 길이 저장

            //문득 궁금..메시지 길이만큼 한번에 받으면 다 받아질까? --받아지는듯? 로컬이라 그럴지도
            byte[] buffer = new byte[strLen]; //받아야할 바이트 배열 크기만큼 바이트 배열 생성
            stream.Read(buffer, 0, buffer.Length); //메시지 수신
            return buffer;
            //if (strLen > BUF_SIZE)
            //{ //메시지 길이가 더 크면 버퍼 사이즈대로 나눠서 받아야함
            //    byte[] buffer = new byte[BUF_SIZE];
            //    List<byte> bufArr = new List<byte>();
            //    for (int i = 0; i < strLen / BUF_SIZE; i++)
            //    { //버퍼사이즈 만큼 읽어 들이기
            //        stream.Read(buffer, 0, BUF_SIZE);
            //        bufArr.AddRange(buffer);
            //    }
            //    stream.Read(buffer, 0, strLen % BUF_SIZE); //나머지 읽기
            //    MessageBox.Show((strLen % BUF_SIZE).ToString());
            //    bufArr.AddRange(buffer);
            //    MessageBox.Show("수신된 데이터 크기:" + bufArr.Count.ToString());
            //    return bufArr.ToArray();
            //}
            //else
            //{ //버퍼 사이즈가 메시지 길이보다 크면 메시지 길이만큼
            //    byte[] buffer = new byte[strLen];
            //    stream.Read(buffer, 0, strLen);
            //    return buffer;
            //}
        }
    }
}
