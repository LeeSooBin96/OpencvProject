using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

//TCP
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WpfServer
{
    public partial class MainWindow : Window
    {
        private TcpListener server = null;
        private Thread hTClnt = null; //클라이언트 접속 처리할 스레드
        public MainWindow()
        {
            InitializeComponent();
            //일단은 여기가 종료되어야 창이 나오니까
            //여기서 비동기로 이 생성자를 완료(반환)시키는 것과 서버 오픈하는것을 구현해야한다.
            Thread thr = new Thread(new ThreadStart(ServerInit));
            thr.Start();
            thr.Join();
        }
        //서버는 한번만 오픈하면 되니까
        private void ServerInit() //서버 정보 초기화 및 오픈
        {
            string bindIP = "127.0.0.1";
            const int bindPort = 6101;

            
            try
            {
                IPEndPoint localAdr = new IPEndPoint(IPAddress.Parse(bindIP), bindPort); //주소 정보 설정

                server = new TcpListener(localAdr); //리스너(서버) 객체 생성
                server.Start(); //서버 오픈
                //계속해서 클라이언트 받기 위한 스레드
                hTClnt = new Thread(new ThreadStart(ClientHandle));
                hTClnt.Start(); //스레드 시작
            }
            catch(SocketException err)
            {
                MessageBox.Show(err.ToString());
            }
            finally
            {
                //server.Stop(); //서버는 종료되면 안돼
                MessageBox.Show("서버 오픈 완료");
            }
        }
        
        private void ClientHandle()
        {
            while (true)
            {
                MessageBox.Show("클라이언트 연결 대기중.");
                TcpClient clnt = server.AcceptTcpClient();
                MessageBox.Show("클라이언트 접속");
            }
        }
    }
}
