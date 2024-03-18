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
using System.IO;
using System.Net;
using System.Net.Sockets;

// OpenCV 사용을 위한 using
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

// Timer 사용을 위한 using
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

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

        public MainWindow()
        {
            InitializeComponent();
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



        private void timer_tick(object sender, EventArgs e)
        {
            // 0번 장비로 생성된 VideoCapture 객체에서 frame을 읽어옴
            cam.Read(frame);
            // 읽어온 Mat 데이터를 Bitmap 데이터로 변경 후 컨트롤에 그려줌
            Cam_1.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // [*] 영상을 사진으로찍음. 
            Cv2.ImShow("클라?", frame);
            var test_image = frame;
            // frame을 저장해야함.
            Cv2.ImWrite("C:\\test\\TTTTTTTTTTTTTTTTTTTT.jpg", test_image); // 프레임 스크린샷 저장경로+파일명
            int i = 0;
            // 업로드 할 파일 정보
            FileInfo file = new FileInfo("C:\\test\\TTTTTTTTTTTTTTTTTTTT.jpg"); // 작업할 파일 불러오기.
            //FileInfo file = new FileInfo();
            //stream을 취득?
            using (FileStream stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                // 파일 binary를 연다.
                byte[] data = new byte[file.Length];
                stream.Read(data, 0, data.Length);
                // 소켓을 연다
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP))
                {
                    // 파일 서버로 접속한다. 주소, 포트번호 변경요망
                    socket.Connect(IPAddress.Parse("127.0.0.1"), 9090);
                    // 전송 람다 함수 (C++와 약속한 규약대로 데이터를 송수신한다.
                    Action<byte[]> Send = (b) =>
                    {
                        // 먼저 데이터 사이즈를 보냄
                        socket.Send(BitConverter.GetBytes(b.Length), 4, SocketFlags.None);
                        // 데이터를 보낸다.
                        socket.Send(b, b.Length, SocketFlags.None);
                    };
                    // 먼저 파일명을 전송? 한? 다? 영상보낼건데?  일단 파일송수신테스트
                    // unicode가 아닌 utf8형식으로 전송한다.
                    Send(Encoding.UTF8.GetBytes("Download.jpg\0")); // 서버에서 받은 이미지
                    // 파일 바이너리 데이터를 보낸다.
                    Send(data);
                    // 서버로 부터 byte=1 데이터가 오면 클라를 종료한다.
                    byte[] ret = new byte[1];
                    socket.Receive(ret, 1, SocketFlags.None);
                    if (ret[0] == 1)
                    {
                        Console.WriteLine("Completed");
                    }
                }
            }


        }
    }
}