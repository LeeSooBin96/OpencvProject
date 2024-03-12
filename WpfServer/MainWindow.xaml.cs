using System;
using System.Text;
using System.Windows;
using System.Collections.Generic;

//스레드
using System.Threading;

//OpenCV
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Net;

namespace WpfServer
{
    public partial class MainWindow : System.Windows.Window
    {
        HandlingServer server = null; //서버 오픈 및 데이터 송수신을 위한 클래스
        public MainWindow()
        {
            InitializeComponent();

            ServerInit(); //서버 정보 초기화 및 오픈
        }
        private void ServerInit() //서버 정보 초기화 및 오픈
        {
            server = new HandlingServer();
            if (server.ServerOpen("10.10.20.98", 6101))
            {
                ClientHandle(); //비동기로 실행될 클라이언트 연결 받는 메소드
                MessageBox.Show("서버 오픈 완료");
            }
            else
            {
                MessageBox.Show(server.errMSG);
            }
        }
        
        private async void ClientHandle() //async한정자 --await을 만나는 순간 비동기로 실행됨
        {
            await Task.Run( () => 
            {
                while (true) //계속해서 클라이언트 접속 받아들이기 위한 while문
                {
                    TcpClient clnt = server.serv.AcceptTcpClient();
                    //접속 정보 출력
                    FactoryData.GetInstance().Add(new FactoryData() { 
                        ip = ((IPEndPoint)clnt.Client.RemoteEndPoint).Address.ToString(), product = "", startTime =DateTime.Now, normal=0,defect=0,total=0});
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        factoryListView.ItemsSource = FactoryData.GetInstance();
                    });
                    RecvMSG(clnt.GetStream()); //비동기로 실행되는 수신 메소드
                }
            }); // 안의 함수는 별도로 실행된다.
        }

        private void Window_Closed(object sender, EventArgs e)
        { //창 닫을때 서버 종료
            server.serv.Stop();
            MessageBox.Show("서버가 종료되었습니다.");
        }

        private async void RecvMSG(NetworkStream stream)
        {
            await Task.Run(() =>
            {
                CheckingImage CKimg = null; //이미지 처리를 위한 객체
                while (true) //접속 끊어질 때 까지 메시지 수신 대기상태 유지
                {
                    byte[] buffer = server.RecvData(stream); //데이터 수신
                                                             //MessageBox.Show("버퍼"+buffer.Length.ToString());
                    if (buffer == null)
                    {
                        MessageBox.Show("클라이언트 연결 종료");
                        stream.Close();
                        break; //클라이언트 연결 종료 시
                    }
                    string code = Encoding.Default.GetString(buffer).Split('@')[0]; //코드 저장

                    switch (code)
                    {
                        case "Product":
                            //제품 이미지 로드
                            string msg = Encoding.Default.GetString(buffer).Split('@')[1]; //메시지 저장
                            CKimg = new CheckingImage(msg, this); //정상제품 이미지 로드
                            break;
                        case "Screen":
                            //파일 바이트에  @ 이게 들어가서 스플릿 될수 있으니까 앞에 code지운 바이트 배열을 넘기자
                            List<byte> arr = new List<byte>();
                            arr.AddRange(buffer);
                            arr.RemoveRange(0, Encoding.Default.GetByteCount(code + '@'));
                            //이제 체크해야함
                            if(CKimg.CompareWith(arr.ToArray(),this)) //PASS
                            {
                                server.SendData(stream,"PASS");
                            }
                            else //NG
                            {
                                server.SendData(stream, "NG");
                            }
                            /*체크 결과 클라이언트에게 송신해야하는 데?
                             NG면 NG여부와 전송할 추출 사진 크기, 사진 파일 전송
                            PASS면 PASS만*/
                            break;

                    }
                }
            });
        }

      
    }
    class FactoryData //리스트뷰에 바인딩할 데이터 클래스
    {
        public string ip { get; set; } //접속한 공장 아이피
        public string product { get; set; } //작업 제품
        public DateTime startTime { get; set; } //작업 시작시간
        public uint normal { get; set; } //정상제품수
        public uint defect { get; set; } //불량제품수
        public uint total { get; set; } //총 작업량

        private static List<FactoryData> instance;
        public static List<FactoryData> GetInstance()
        {
            if (instance == null)
                instance = new List<FactoryData>();
            return instance;
        }
    }
}
