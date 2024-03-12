using System.Windows;
// OpenCV 사용을 위한 using
using OpenCvSharp;

// Timer 사용을 위한 using
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;

//이미지 유사도 검사를 위한 using
using Daneung;

namespace ttest
{
    public class B_Check_1
    { //1번 제품을 검사하기 위한 로직
        public static bool Check_1(Mat r_frame,Mat t_frame)
        { //정상제품 이미지와 클라이언트 화면 배열을 함께 받아와야한다.
            Mat[] subImages = new Mat[3]; //분할한 화면을 저장할 Mat
            Mat[] aaa = new Mat[3]; //원본을 저장해둘 Mat
            //여기서 정상제품 이미지 크기 조정해야함 --비교할 이미지에 맞춰 크기 조정
            //r_frame.Resize(new OpenCvSharp.Size(t_frame.Width/3,t_frame.Height));

            for (int i = 0; i < 3; i++) //이제 분할된 화면들에 대해 검사 시작
            {
                int width = t_frame.Width; //test 넓이 크기 --검사할 화면의 너비 저장
                int height = t_frame.Height; //높이 --검사할 화면의 높이
                int subWidth = width / 3; //넒이 나누기 3 --분할 이미지의 너비
                int subHeight = height; //분할 이미지 높이

                int startX = i * subWidth; //시작 좌표 x
                int endX = (i + 1) * subWidth; //끝 좌표 x
                int startY = 0; //시작 좌표 y
                int endY = height - subHeight; //좌표계산 --끝 좌표 y

                subImages[i] = t_frame.SubMat(new Rect(startX, startY, subWidth, subHeight)); //자르기 --화면 분할

                aaa[i] = subImages[i].Clone(); //원본 복사하고

                int roiWidth = 200; //사각형 구역 너비
                int roiHeight = 100; //사각형 구역 높이
                int x = 0; //x 좌표 시작 위치
                int y = 0; //y 좌표 시작 위치

                //첫번째 영역
                Mat M_roi_1 = aaa[i].Clone(); //원본 복사하고
                Mat roi_1 = new Mat(M_roi_1, new Rect(x + 100, y + 50, x + roiWidth - 120, y + roiHeight - 10)); //관심 구역 지정
                Cv2.CvtColor(roi_1, roi_1, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴
                Mat mask1 = new Mat();
                Cv2.InRange(roi_1, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask1); //노랑 -- 색상 추출
                Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple); //외곽선?

                int pix = 1000; //픽셀 기준
                foreach (var c in contours1)
                {
                    var area = Cv2.ContourArea(c);
                    if (area > pix) //픽셀단위  숫자 이상만
                    {
                        string shape = GetShape(c); //도형 추출
                        string name = "Yellow " + shape;
                        var M = Cv2.Moments(c);
                        var cx = (int)(M.M10 / M.M00) + 20;
                        var cy = (int)(M.M01 / M.M00) + 30; //이 3놈 중앙 찾음
                        if (shape == "circle")
                        {
                            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
                            /*여기서 제품 바디의 상단부분 색상, 도형 검사 결과 저장해야함*/

                        }
                    }
                }

                //2번째 영역
                Mat M_roi_2 = aaa[i].Clone();
                Mat roi_2 = new Mat(M_roi_2, new Rect(x + 120, y + 130, x + roiWidth - 160, y + roiHeight + 20)); //관심 구역 지정

                Cv2.CvtColor(roi_2, roi_2, ColorConversionCodes.BGR2GRAY); //흑백으로 만들고
                Cv2.Threshold(roi_2, roi_2, 127, 255, ThresholdTypes.Binary); //
                Cv2.FindContours(roi_2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                if (contours2.Length > 1)
                {
                    string name = "B-Checker";
                    Cv2.PutText(aaa[i], name, new Point(150, 200), HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 2);
                    /*제품의 중간부 공백이 아닌지 검사한 결과 저장*/

                }

                //3번째 영역
                Mat M_roi_3 = aaa[i].Clone(); //원본 복사하고
                Mat roi_3 = new Mat(M_roi_3, new Rect(x + 20, y + 220, roiWidth - 20, roiHeight + 130));

                Cv2.CvtColor(roi_3, roi_3, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴

                Mat mask3 = new Mat();
                Cv2.InRange(roi_3, new Scalar(0, 50, 120), new Scalar(10, 255, 255), mask3); //빨강
                Mat mask4 = new Mat();
                Cv2.InRange(roi_3, new Scalar(90, 60, 0), new Scalar(121, 255, 255), mask4); //파랑

                Cv2.FindContours(mask3, out var contours3, out var hierarchy3, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);
                Cv2.FindContours(mask4, out var contours4, out var hierarchy4, RetrievalModes.CComp, ContourApproximationModes.ApproxSimple);

                foreach (var c in contours3)
                {
                    var area = Cv2.ContourArea(c);
                    if (area > pix)
                    {
                        string shape = GetShape(c); //*형상구분
                        string name = "Red " + shape;
                        var M = Cv2.Moments(c);
                        var cx = (int)(M.M10 / M.M00);
                        var cy = (int)(M.M01 / M.M00) + 220; //이 3놈 중앙 찾음
                        if (shape == "square")
                        {
                            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 2);
                            /*제품의 빨간 사각형의 존재 여부 저장*/

                        }
                    }
                }

                foreach (var c in contours4)
                {
                    var area = Cv2.ContourArea(c);
                    if (area > pix)
                    {
                        string shape = GetShape(c); //*형상구분
                        string name = "Blue " + shape;
                        var M = Cv2.Moments(c);
                        var cx = (int)(M.M10 / M.M00);
                        var cy = (int)(M.M01 / M.M00) + 220; //이 3놈 중앙 찾음
                        if (shape == "triangle")
                        {
                            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Blue, 2);
                            /*제품의 파란 삼각형 존재 여부 저장*/
                        }
                    }
                }
                Cv2.Rectangle(aaa[i], new Point(x + 20, y + 30), new Point(x + roiWidth, y + roiHeight + 20), new Scalar(0, 0, 0), 1);
                Cv2.Rectangle(aaa[i], new Point(x + 20, y + 120), new Point(x + roiWidth, y + roiHeight + 120), new Scalar(0, 0, 0), 1);
                Cv2.Rectangle(aaa[i], new Point(x + 20, y + 220), new Point(x + roiWidth, y + roiHeight + 350), new Scalar(0, 0, 0), 1);

                Cv2.Rectangle(aaa[i], new Point(x + 110, y + 60), new Point(x + roiWidth - 30, y + roiHeight + 20), new Scalar(100, 255, 255), 1);
                Cv2.Rectangle(aaa[i], new Point(x + 120, y + 130), new Point(x + roiWidth - 40, y + roiHeight + 130), new Scalar(100, 255, 255), 1);
                //여기서 각 이미지 유사도 계산도 해야함
                MatchingIMG.CalculateRateIMG(r_frame, subImages[i]);
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    Cv2.ImShow($"SubImage{(i + 1)}_3", aaa[i]); // show subImages_3
                }
            });

            return false; //임시
        }
        public static string GetShape(Point[] c)
        {
            string shape = "unidentified";
            double peri = Cv2.ArcLength(c, true);
            Point[] approx = Cv2.ApproxPolyDP(c, 0.03 * peri, true);

            if (approx.Length == 3) //if the shape is a triangle, it will have 3 vertices
            {
                shape = "triangle";
            }
            else if (approx.Length == 4 || approx.Length == 5)    //if the shape has 4 vertices, it is either a square or a rectangle
            {
                shape = "square";
            }
            else  //각이 없으면
            {
                shape = "circle";
            }
            return shape;
        }

    }
}


