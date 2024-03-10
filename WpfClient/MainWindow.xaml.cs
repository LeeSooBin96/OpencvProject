using System;
using System.Windows;

//OpenCV
using OpenCvSharp;

//Timer
using System.Windows.Threading;



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
        HandlingClient client =null; //통신을 위한 클래스

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
            notice.Visibility = 0;
            MessageBox.Show("업무가 시작됩니다.");

            if(client==null) client = new HandlingClient();
            if(client.ConnectingServer("127.0.0.1", 6101))
            { //연결 성공
                client.SendData("Product@" + productName.Text);
            }
            else
            { //연결 실패
                MessageBox.Show(client.errMSG);
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
            //서버와 연결 종료
            client.ClosingConnect();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        { 
            
        }
    }
}
