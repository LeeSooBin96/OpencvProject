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

//OpenCV
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace WpfServer
{
    public partial class MainWindow : System.Windows.Window
    {
        private TcpListener server = null;
        private Thread hTClnt = null; //클라이언트 접속 처리할 스레드
        public MainWindow()
        {
            InitializeComponent();
            //일단은 여기가 종료되어야 창이 나오니까
            //여기서 비동기로 이 생성자를 완료(반환)시키는 것과 서버 오픈하는것을 구현해야한다.
            //Thread thr = new Thread(new ThreadStart(ServerInit));
            //thr.Start();
            //thr.Join(); //함수 종료 보장만 되면 되기 때문에 여기서 쓰레드는 필요가 없음
            ServerInit();
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
                MessageBox.Show("서버 오픈 완료");
            }
            catch (SocketException err)
            {
                MessageBox.Show(err.ToString());
            }
            finally
            {
                //server.Stop(); //서버는 종료되면 안돼
            }
        }
        
        private void ClientHandle()
        {
            while (true)
            {
                MessageBox.Show("클라이언트 연결 대기중.");
                TcpClient clnt = server.AcceptTcpClient();
                //클라이언트 접속하면 각각의 스레드로 보내기
                Thread hclnt = new Thread(new ThreadStart(()=> { RecvMSG(clnt.GetStream()); }));
                hclnt.Start();
                //그 안에서 데이터 송수신
                //이제 메시지 처리할 준비 
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        { //창 닫을때 서버 종료
            server.Stop();
            MessageBox.Show("서버가 종료되었습니다.");
        }

        private void RecvMSG(NetworkStream stream)
        {
            MessageBox.Show("클라이언트 접속");

            //사용할 버퍼
            const int BUF_SIZE = 10000;
            byte[] buffer = new byte[BUF_SIZE];

            /*클라이언트와 통신 규약 : 메시지 길이 수신
            그 후 약속된코드@내용 수신*/

            while (true)
            {
                stream.Read(buffer, 0, 4); //메시지 길이 수신
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
                    //테스트
                    Mat frame = Mat.FromImageData(bufArr.ToArray(), ImreadModes.AnyColor);
                    //screen.Source = WriteableBitmapConverter.ToWriteableBitmap(frame);
                    Cv2.ImShow("test", frame);
                }
                else
                { //버퍼 사이즈가 메시지 길이보다 크면 메시지 길이만큼
                    stream.Read(buffer, 0, strLen);
                    //MessageBox.Show(Encoding.Default.GetString(buffer)); //문자열로
                }

            }
        }
    }
}
