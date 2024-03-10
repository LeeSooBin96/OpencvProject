using System;
using System.Text;
using System.Windows;

//스레드
using System.Threading;

//OpenCV
using System.Net.Sockets;

namespace WpfServer
{
    public partial class MainWindow : System.Windows.Window
    {
        HandlingServer server = null; //서버 오픈 및 데이터 송수신을 위한 클래스
        private Thread hTClnt = null; //클라이언트 접속 처리할 스레드
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
                //다중 클라이언트 연결 요청 수락을 위한 스레드 생성
                hTClnt = new Thread(new ThreadStart(ClientHandle));
                hTClnt.Start(); //스레드 시작
                MessageBox.Show("서버 오픈 완료");
            }
            else
            {
                MessageBox.Show(server.errMSG);
            }
        }
        
        private void ClientHandle()
        {
            while (true)
            {
                TcpClient clnt = server.serv.AcceptTcpClient();
                //클라이언트 접속하면 스레드로 보내기
                Thread hclnt = new Thread(new ThreadStart(()=> { RecvMSG(clnt.GetStream()); }));
                hclnt.Start();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        { //창 닫을때 서버 종료
            server.serv.Stop();
            MessageBox.Show("서버가 종료되었습니다.");
        }

        private void RecvMSG(NetworkStream stream)
        {
            while (true)
            {
                CheckingImage CKimg = null; //이미지 처리를 위한 객체
                byte[] buffer = server.RecvData(stream); //데이터 수신
                if (buffer == null) break; //클라이언트 연결 종료 시
                string code = Encoding.Default.GetString(buffer).Split('@')[0]; //코드 저장
                string msg = Encoding.Default.GetString(buffer).Split('@')[1]; //메시지 저장

                switch (code)
                {
                    case "Product":
                        //제품 이미지 로드
                        CKimg = new CheckingImage(msg);
                        break;
                    case "Screen":
                        MessageBox.Show(msg); //화면 수신 확인
                        break;
                }
            }
        }

      
    }
}
