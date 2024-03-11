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
// OpenCV 사용을 위한 using
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

// Timer 사용을 위한 using
using System.Windows.Threading;
using System.Security.Cryptography;
using System.Windows.Interop;
using Point = OpenCvSharp.Point;
using System.Windows.Media.Media3D;
using Rect = OpenCvSharp.Rect;
using Size = OpenCvSharp.Size;
using System.Xml.Linq;
using System.Diagnostics;
using Daneung;

namespace ttest
{
    public class B_Check_2
    {
        public static void Check_2(Mat r_frame, Mat t_frame)
        {
            //MainWindow MW = mw;

            //Mat t_frame = MW.frame.Clone(); //원본 복사하고

            Mat[] subImages = new Mat[3];
            Mat[] aaa = new Mat[3];

            for (int i = 0; i < 3; i++)
            {
                int width = t_frame.Width; //test 넓이 크기
                int height = t_frame.Height; //높이
                int subWidth = width / 3; //넒이 나누기 3
                int subHeight = height;

                int startX = i * subWidth;
                int endX = (i + 1) * subWidth;
                int startY = 0;
                int endY = height - subHeight; //좌표계산

                subImages[i] = t_frame.SubMat(new Rect(startX, startY, subWidth, subHeight)); //자르기

                aaa[i] = subImages[i].Clone(); //원본 복사하고

                int roiWidth = 200;
                int roiHeight = 100;
                int x = 0; //x 좌표 시작 위치
                int y = 0; //y 좌표 시작 위치

                ////// 첫 번째 사각형 그리기 //aaa는 원본 복사한거
                ////관심구역 그저 표현한거

                //첫번째 영역
                Mat M_roi_1 = aaa[i].Clone(); //원본 복사하고
                Mat roi_1 = new Mat(M_roi_1, new Rect(x + 100, y + 50, x + roiWidth - 120, y + roiHeight - 10)); //관심 구역 지정
                Cv2.CvtColor(roi_1, roi_1, ColorConversionCodes.BGR2HSV); //관심 부분만 HSV화 시킴
                Mat mask1 = new Mat();
                Cv2.InRange(roi_1, new Scalar(20, 50, 50), new Scalar(30, 255, 255), mask1); //노랑
                Cv2.FindContours(mask1, out var contours1, out var hierarchy1, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                int pix = 1000;
                foreach (var c in contours1)
                {
                    var area = Cv2.ContourArea(c);
                    if (area > 1000) //픽셀단위  숫자 이상만
                    {
                        string shape = GetShape(c);
                        string name = "Yellow " + shape;
                        var M = Cv2.Moments(c);
                        var cx = (int)(M.M10 / M.M00) + 20;
                        var cy = (int)(M.M01 / M.M00) + 30; //이 3놈 중앙 찾음
                        Cv2.DrawContours(roi_1, contours1, -1, Scalar.Red, 2); //윤곽그리고
                        Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
                        if (shape == "circle")
                        {
                            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);
                        }
                    }

                }

                //2번째 영역
                Mat M_roi_2 = aaa[i].Clone();
                Mat roi_2 = new Mat(M_roi_2, new Rect(x + 120, y + 130, x + roiWidth - 160, y + roiHeight)); //관심 구역 지정
                Cv2.CvtColor(roi_2, roi_2, ColorConversionCodes.BGR2GRAY); //흑백으로 만들고
                Cv2.Threshold(roi_2, roi_2, 127, 255, ThresholdTypes.Binary); //
                Cv2.FindContours(roi_2, out var contours2, out var hierarchy2, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);

                if (contours2.Length > 1)
                {
                    string name = "B-Checker";
                    Cv2.PutText(aaa[i], name, new Point(150, 200), HersheyFonts.HersheySimplex, 0.5, Scalar.Black, 2);
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
                                                             //Cv2.DrawContours(aaa, contours3, -1, Scalar.Red, 2); //윤곽그리고
                        if (shape == "triangle")
                        {
                            Cv2.PutText(aaa[i], name, new Point(cx, cy), HersheyFonts.HersheySimplex, 0.5, Scalar.Red, 2);
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
                        }
                    }
                }
               
                Cv2.Rectangle(aaa[i], new Point(x + 20, y + 30), new Point(x + roiWidth, y + roiHeight + 20), new Scalar(0, 0, 0), 1);
                Cv2.Rectangle(aaa[i], new Point(x + 20, y + 120), new Point(x + roiWidth, y + roiHeight + 120), new Scalar(0, 0, 0), 1);
                Cv2.Rectangle(aaa[i], new Point(x + 20, y + 220), new Point(x + roiWidth, y + roiHeight + 350), new Scalar(0, 0, 0), 1);

                Cv2.Rectangle(aaa[i], new Point(x + 110, y + 60), new Point(x + roiWidth - 30, y + roiHeight + 20), new Scalar(100, 255, 255), 1);
                Cv2.Rectangle(aaa[i], new Point(x + 120, y + 130), new Point(x + roiWidth - 40, y + roiHeight + 130), new Scalar(100, 255, 255), 1);

                MatchingIMG.CalculateRateIMG(r_frame, subImages[i]);
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    Cv2.ImShow($"SubImage{(i + 1)}_3", aaa[i]); // show subImages_3
                }
            });


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


