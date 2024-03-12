using System;
using System.Windows;

//OpenCV
using OpenCvSharp;

//Timer
using System.Windows.Threading;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Media;



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
            if(client.ConnectingServer("10.10.20.98", 6101))
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
            Mat represent = frame.Clone();
            //화면에 사각형 구역 그리기
            if (productName.Text == "product 3")
            {
                Cv2.Rectangle(represent, new OpenCvSharp.Point(20, 90), new OpenCvSharp.Point(190, 360), new Scalar(121, 255, 255), 2);
                Cv2.Rectangle(represent, new OpenCvSharp.Point(230, 90), new OpenCvSharp.Point(400, 360), new Scalar(121, 255, 255), 2);
                Cv2.Rectangle(represent, new OpenCvSharp.Point(440, 90), new OpenCvSharp.Point(600, 360), new Scalar(121, 255, 255), 2);
            }
            else
            {
                Cv2.Rectangle(represent, new OpenCvSharp.Point(40, 60), new OpenCvSharp.Point(170, 450), new Scalar(121, 255, 255), 2);
                Cv2.Rectangle(represent, new OpenCvSharp.Point(260, 60), new OpenCvSharp.Point(380, 450), new Scalar(121, 255, 255), 2);
                Cv2.Rectangle(represent, new OpenCvSharp.Point(480, 60), new OpenCvSharp.Point(600, 450), new Scalar(121, 255, 255), 2);
            }
            // 읽어온 Mat 데이터를 Bitmap 데이터로 변경 후 컨트롤에 그려줌
            screen.Source = OpenCvSharp.WpfExtensions.WriteableBitmapConverter.ToWriteableBitmap(represent);
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
            capture.IsEnabled = false; //결과 수신시까지 버튼 비활성화
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
            resultLBL.Content = result;
            if(result == "PASS")
            {
                MessageBox.Show("PASS");
                resultLBL.Background = System.Windows.Media.Brushes.Green;
            }
            else if(result=="NG")
            {
                MessageBox.Show("NG");
                resultLBL.Background = System.Windows.Media.Brushes.Red;
            }
            capture.IsEnabled = true;
        }
    }
}
