using System;
using System.Windows;

//OpenCV
using OpenCvSharp;

//Timer
using System.Windows.Threading;
using System.Text;
using System.Collections.Generic;



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
            notice.Content = "";
            MessageBox.Show("업무가 시작됩니다.");

            if(client==null) client = new HandlingClient();
            if(client.ConnectingServer("10.10.20.107", 6101))
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
            //화면에 사각형 구역 그리기
            Cv2.Rectangle(frame, new OpenCvSharp.Point(30, 50), new OpenCvSharp.Point(200, 450), new Scalar(0, 0, 0), 2);
            Cv2.Rectangle(frame, new OpenCvSharp.Point(230, 50), new OpenCvSharp.Point(400, 450), new Scalar(0, 0, 0), 2);
            Cv2.Rectangle(frame, new OpenCvSharp.Point(430, 50), new OpenCvSharp.Point(600, 450), new Scalar(0, 0, 0), 2);
            // 읽어온 Mat 데이터를 Bitmap 데이터로 변경 후 컨트롤에 그려줌
            screen.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(frame);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //서버와 연결 종료
            client.ClosingConnect();
        }

        private void WorkStop(object sender, RoutedEventArgs e)
        {
            notice.Content = "제품을 선택해주세요!";
        }

        private void CaptureScreen(object sender, RoutedEventArgs e)
        {
            /*NG나 PASS팝업창이 떠있는 경우 닫아야함*/
            if (client == null) return; //서버 연결 안되어있으면 동작 안함
            Mat shot = new Mat(); //객체 생성
            shot = frame; //현재 화면 저장 --이부분 안하고 바로 frame으로 보낼까
            //바이트 배열로 바꿔서 보내면 더 좋을듯 --안되나 보오
            //MessageBox.Show("클"+shot.ToBytes().Length.ToString());
            List<byte> arr = new List<byte>();
            arr.AddRange(Encoding.Default.GetBytes("Screen@"));
            arr.AddRange(shot.ToBytes());
            //MessageBox.Show("클" + arr.ToArray().Length.ToString());
            client.SendData(arr.ToArray()); //서버에 화면 전송
            /*검사 결과 수신 받아야한다.
             NG면 NG팝업창(이랑 NG부분 체크된 사진) 띄우기
            PASS면 PASS팝업창 띄우기.*/
            string result = Encoding.Default.GetString(client.RecvData());
            if(result =="PASS")
            {
                MessageBox.Show("PASS");
            }
            else if(result=="NG")
            {
                MessageBox.Show("NG");
            }
        }
    }
}
