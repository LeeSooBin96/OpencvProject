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

//OpenCV
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

//TCP
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

//Timer
using System.Windows.Threading;
using System.Xml.Schema;



namespace WpfClient
{
    public partial class MainWindow : System.Windows.Window
    {
        //웹캠 연결
        VideoCapture cam;
        Mat frame;
        DispatcherTimer timer;
        bool is_initCam, is_initTimer;
        //TCP 통신
        TcpClient client;
        NetworkStream stream;
        const int BUF_SIZE = 1024; //버퍼 크기

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WorkStart(object sender, RoutedEventArgs e)
        {
            //검사할 제품 선택 안되었을 때 업무 시작 못함
            if(productName.Text=="")
            {
                MessageBox.Show("검사할 제품을 선택해주세요.");
                return;
            }
            MessageBox.Show("업무가 시작됩니다.");

            //서버에 연결
            string bindIP = "127.0.0.1";
            const int bindPort = 0;//포트번호 자동 할당
            string servIP = "127.0.0.1";
            const int servPort = 6101;

            try
            {
                IPEndPoint clntAddr = new IPEndPoint(IPAddress.Parse(bindIP), bindPort);
                IPEndPoint servAddr = new IPEndPoint(IPAddress.Parse(servIP), servPort);

                client = new TcpClient(clntAddr); //통신에 사용할 객체 생성
                client.Connect(servAddr); //서버에 연결
                stream = client.GetStream(); //데이터 송수신에 사용할 스트림 생성
                MessageBox.Show("서버에 연결되었습니다.");

                //선택된 제품명 보내야한다.
                /*서버와 통신 규약 : 늘 전체 메시지 길이 먼저
                 그 후에 약속된코드@내용 보내기*/

                string msg = "Product@" + productName.Text;
                byte[] msgBytes = Encoding.Default.GetBytes(msg);
                byte[] len = new byte[4];
                len = BitConverter.GetBytes(msgBytes.Length);
                stream.Write(len, 0, 4); //메시지 길이 송신
                stream.Write(msgBytes, 0, msgBytes.Length); //메시지 송신
            }
            catch (SocketException err)
            {
                MessageBox.Show(err.ToString());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //웹캠 카메라, 타이머 초기화
            is_initCam = init_camera();
            is_initTimer = init_Timer(0.01);

            if (is_initTimer && is_initCam) timer.Start(); //웹캠 송출 시작
        }

        private bool init_Timer(double interval_ms)
        {
            try
            {
                timer = new DispatcherTimer();

                timer.Interval = TimeSpan.FromMilliseconds(interval_ms);
                timer.Tick += new EventHandler(timer_tick);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool init_camera()
        {
            try
            {
                // 0번 카메라로 VideoCapture 생성 (카메라가 없으면 안됨)
                cam = new VideoCapture(0);
                cam.FrameHeight = (int)screen.Height;
                cam.FrameWidth = (int)screen.Width;

                // 카메라 영상을 담을 Mat 변수 생성
                frame = new Mat();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void timer_tick(object sender, EventArgs e)
        {
            // 0번 장비로 생성된 VideoCapture 객체에서 frame을 읽어옴
            cam.Read(frame);
            // 읽어온 Mat 데이터를 Bitmap 데이터로 변경 후 컨트롤에 그려줌
            screen.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            client.Close();
            stream.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        { //화면 캡처, 캡처본 서버에 송신
            Mat screenShot = new Mat();
            cam.Read(screenShot);
            //너는 한번에 보내도 되니까 길이보내고 데이터 한번에 전송
            byte[] size = new byte[4];
            size = BitConverter.GetBytes(screenShot.ToBytes().Length);
            stream.Write(size, 0, 4); //데이터 크기 먼저 보내기
            MessageBox.Show("송신데이터 크기: " + BitConverter.ToInt32(size, 0).ToString());

            byte[] bytes = new byte[screenShot.ToBytes().Length]; //바이트 배열 생성
            bytes = screenShot.ToBytes();
            stream.Write(bytes, 0, bytes.Length); //데이터 한번에 보내기
        }
    }
}
