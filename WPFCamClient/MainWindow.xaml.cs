using System;
using System.Collections.Generic;
using System.Diagnostics; //TCP
using System.Linq;
using System.Net; //TCP
using System.Net.Sockets; //TCP
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

// OpenCV 사용을 위한 using
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

// Timer 사용을 위한 using
using System.Windows.Threading;
using System.Xml.Schema;

namespace WPF
{
    // OpenCvSharp 설치 시 Window를 명시적으로 사용해 주어야 함 (window -> System.Windows.Window)
    public partial class MainWindow : System.Windows.Window
    {
        // 필요한 변수 선언
        VideoCapture cam;
        Mat frame;
        DispatcherTimer timer;
        bool is_initCam, is_initTimer;
        // TCP 통신에 필요
        TcpClient client; //TCP 통신에 사용할 객체
        NetworkStream stream; //데이터 송수신에 사용할 객체

        public MainWindow()
        {
            InitializeComponent(); //구성요소들 초기화

            //클라이언트 코드 작성
            string bindIP = "127.0.0.1"; //클라이언트 아이피 정보
            int bindPort = 0; //클라이언트 포트 정보 --자동할당(0)
            string serverIP = "127.0.0.1"; //서버 아이피
            const int serverPort = 6101; //서버 포트 번호

            try
            {
                IPEndPoint clntAddress = new IPEndPoint(IPAddress.Parse(bindIP), bindPort); //본인 주소 정보 설정
                IPEndPoint servAddress = new IPEndPoint(IPAddress.Parse(serverIP), serverPort); //서버 주소 정보 설정

                client = new TcpClient(clntAddress); //클라이언트 객체 생성
                client.Connect(servAddress); //서버에 연결
                stream = client.GetStream(); //데이터 송수신에 사용할 스트림 생성
            }
            catch(SocketException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void windows_loaded(object sender, RoutedEventArgs e)
        {
            // 카메라, 타이머(0.01ms 간격) 초기화
            is_initCam = init_camera();
            is_initTimer = init_Timer(0.01);

            // 초기화 완료면 타이머 실행
            if (is_initTimer && is_initCam) timer.Start();
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
                cam.FrameHeight = (int)Cam_1.Height;
                cam.FrameWidth = (int)Cam_1.Width;

                // 카메라 영상을 담을 Mat 변수 생성
                frame = new Mat();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Mat screenShot = new Mat();
            cam.Read(screenShot);
            //너는 한번에 보내도 되니까 길이보내고 데이터 한번에 전송
            byte[] size = new byte[4];
            size = BitConverter.GetBytes(screenShot.ToBytes().Length);
            stream.Write(size, 0, 4); //데이터 크기 먼저 보내기
            MessageBox.Show("송신데이터 크기: "+BitConverter.ToInt32(size, 0).ToString());

            byte[] bytes = new byte[screenShot.ToBytes().Length]; //바이트 배열 생성
            bytes = screenShot.ToBytes();
            stream.Write(bytes, 0, bytes.Length); //데이터 한번에 보내기
        }

        private void timer_tick(object sender, EventArgs e)
        {
            // 0번 장비로 생성된 VideoCapture 객체에서 frame을 읽어옴
            cam.Read(frame);
            // 읽어온 Mat 데이터를 Bitmap 데이터로 변경 후 컨트롤에 그려줌
            Cam_1.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);
            //여기서 frame을 서버에 보내줘야한다.
            //byte[] bytes = new byte[frame.ToBytes().Length]; //바이트 배열 생성
            //bytes = frame.ToBytes();
            //stream.Write(bytes, 0, bytes.Length);
        }
    }
}