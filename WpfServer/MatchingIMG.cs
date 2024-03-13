using OpenCvSharp;
using OpenCvSharp.XFeatures2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Daneung
{
    internal class MatchingIMG
    {
        static public void CalculateRateIMG(Mat mat, Mat temp)
        {
            //using (Mat mat = new Mat("real1.png")) // ok이미지 컬러 --이게 정상제품 이미지?
            //using (Mat temp = new Mat("real2.png")) // ng이미지 컬러 --이게 비교할 이미지?
            Cv2.Resize(mat,mat,new OpenCvSharp.Size(temp.Width, temp.Height));
            using (Mat result = new Mat())
            {
                Mat sub = new Mat(); // 일단 선언

                Mat gray_image1 = mat.CvtColor(ColorConversionCodes.BGR2GRAY); // 회색조변환 // ok흑백
                Mat gray_image2 = temp.CvtColor(ColorConversionCodes.BGR2GRAY); // 회색조변환 // ng흑백
                //MessageBox.Show(mat.Height.ToString() + " " + temp.Height.ToString());
                //MessageBox.Show(mat.Width.ToString() + " " + temp.Width.ToString());

                /// 회색변환완료
                Cv2.Absdiff(gray_image1, gray_image2, sub); // 차이고 ==여기서 튕김
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    Cv2.ImShow("변환된이미지", sub); // 차이 이미지가 뜸 굿
                //});

                Mat gray = sub; // 선언
                Mat test2 = gray.Clone(); // 복사
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    Cv2.ImShow("1", gray); // =sub
                //    Cv2.ImShow("2", test2); // =sub, gray
                //});

                OpenCvSharp.Point[][] contours; // point 배열 , 좌표값
                HierarchyIndex[] hierarchy; // 몰라뭔지

                Mat qwer1111 = new Mat(); // 선언일단하고
                Cv2.InRange(sub, new Scalar(0, 100, 100), new Scalar(0, 255, 255), gray); // 흑백으로 처리끝난값
                Cv2.FindContours(gray, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxTC89KCOS);
                Cv2.CvtColor(test2, qwer1111, ColorConversionCodes.GRAY2BGR); // 흑백처리끝난거 -> 컬러로바꿈
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    Cv2.ImShow("tjnksdjnai", qwer1111); // 차이값의 이미지 -> 컬러값의 이미지
                //});                                    //List<Point[]> lInPoints = new List<Point[]>();
                foreach (OpenCvSharp.Point[] p in contours)
                {
                    double length = Cv2.ArcLength(p, true); // 길이?
                    double area = Cv2.ContourArea(p, true); // 구역?

                    if (length < 1 && area < 100 && p.Length < 1) continue; // 출력부분 위치잡기

                    OpenCvSharp.Rect boundingRect = Cv2.BoundingRect(p); // 해당부분 사각형으로 잡기위한 구역

                    Cv2.Rectangle(temp, boundingRect, Scalar.Red, 2, LineTypes.Link8); // 사각형으로 ? 출력함
                                                                                       // 흑백처리 -> 컬러처리이미지 위에 그려 오류를
                                                                                       //lInPoints.Add(p);
                }
                Mat eeee = new Mat();
                Mat ffff = new Mat();
                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    Cv2.ImShow("dst", temp); // 최종출력 컬러로 바꾼상태
                //});                         //Cv2.CvtColor(test2, eeee, ColorConversionCodes.GRAY2BGR); // 흑백 -> 컬러 색상변경
                                            //Cv2.ImShow("변환", eeee);
                                            //Cv2.Rectangle(eeee, , Scalar.Red, 2, LineTypes.AntiAlias);


                //[이미지 비교 유사도]
                ///////////////////////////////////////////////////////////////////////////////////////////

                //Mat aaaaa = new Mat();
                //Mat bbbbb = new Mat();

                //Mat screen = null, find = null, res = null;
                ////Cv2.CvtColor(temp, temp, ColorConversionCodes.GRAY2BGR);

                //Mat test222 = temp.Clone();
                //Cv2.Resize(mat, aaaaa, new OpenCvSharp.Size(158, 463)); // mat은 real1.png
                //Cv2.Resize(test222, bbbbb, new OpenCvSharp.Size(158, 463)); // test222는 real2.png

                //screen = aaaaa; // real1
                //find = bbbbb; // real2

                //res = screen.MatchTemplate(find, TemplateMatchModes.CCoeffNormed);

                //double min, max = 0;

                //Cv2.MinMaxLoc(res, out min, out max);
                // 호모그래피

                //[비교 이미지1]
                var ok_image = mat.Clone(); // ok파일
                //Cv2.WaitKey(1);
                //Mat ok_image = img1.SubMat(new Rect(700, 600, 900, 1000)); // 전체이미지에서 비교할 부분

                //[비교 이미지2]
                //var frame_image = new Mat("real1.png");
                var frame_image = temp.Clone();
                //Cv2.WaitKey(1);
                //Mat frame_image = img2.SubMat(new Rect(700, 600, 900, 1000));

                //키포인트검출?
                var Detector = SURF.Create(hessianThreshold: 10);
                var keypoint1 = Detector.Detect(ok_image);
                var keypoint2 = Detector.Detect(frame_image);

                //[descriptors 계산]
                var extractor = BriefDescriptorExtractor.Create();
                var Descriptors1 = new Mat();
                var Descriptors2 = new Mat();
                extractor.Compute(ok_image, ref keypoint1, Descriptors1);
                extractor.Compute(frame_image, ref keypoint2, Descriptors2);

                //matching descriptor
                var matcher = new BFMatcher();
                var matches = matcher.Match(Descriptors1, Descriptors2);

                //Mat ok_image_gray = ok_image.CvtColor(ColorConversionCodes.BGR2HSV);
                //Mat frame_image_gray = frame_image.CvtColor(ColorConversionCodes.BGR2HSV);
                

                //drawing the results
                var imgMatches = new Mat();
                Cv2.DrawMatches(ok_image, keypoint1, frame_image, keypoint2, matches, imgMatches);
                //Cv2.ImShow("최종화면Matches", imgMatches);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Cv2.ImShow("최종화면Matches", imgMatches);
                });




                    double number_keypoints = 0;
                double number_keypoints2 = 0;

                if (keypoint1.Length <= keypoint2.Length)
                {
                    number_keypoints = keypoint1.Length;
                    number_keypoints2 = keypoint2.Length;
                }
                else
                {
                    number_keypoints = keypoint1.Length;
                    number_keypoints2 = keypoint2.Length;
                }


                //Console.WriteLine("Keypoints 1ST image: " + keypoint1.Length);
                //Console.WriteLine("Keypoints 2ND image: " + keypoint1.Length);
                //Label_dd.Content = ("유사도 : " + (String.Format("{0:P}", (double)number_keypoints / number_keypoints2)));

                //Cv2.WaitKey(1);
                //Cv2.WaitKey(0);
                //Cv2.DestroyAllWindows();
                ok_image.Dispose();
                frame_image.Dispose();

                ////Label_dd.Content = ("유사도 : " + (String.Format("{0:P}", max)));
                MessageBox.Show("유사도 : " + String.Format("{0:P}", (double)number_keypoints / number_keypoints2));
                //screen.Dispose();
                //find.Dispose();
                //res.Dispose();
            }
        }
    }
}
