using OpenCvSharp;
using System.IO;
using System.Windows;
using System;
//취합 코드 네임스페이스 using
using ttest;
//using Daneung;

namespace WpfServer
{ 
    internal class CheckingImage
    {
        byte[] normalP = null; //정상제품 이미지 저장할 바이트 배열
        string num;

        public CheckingImage(string productName) //생성자
        {
            switch(productName) //제품명에 따른 정상 제품 이미지 로드해야함
            {
                case "product 1":
                    num = "1";
                    break;
                case "product 2":
                    num = "2";
                    break;
                case "product 3":
                    num = "3";
                    break;
                default:
                    MessageBox.Show(productName);
                    return;
            }

            //해당하는 이미지 파일 읽어서 바이트 배열에 저장해야함
            Stream rFile = new FileStream("..//Image/P" + num + ".png", FileMode.Open,FileAccess.Read);
            normalP = new byte[rFile.Length]; //파일 사이즈에 맞춰 바이트 배열 생성
            rFile.Read(normalP, 0, normalP.Length); //파일 읽기
            rFile.Close();
            //일단 정상 제품 이미지 바이트 배열에 저장 완료

            //파일 제대로 읽나 테스트 --일단 제대로 읽음.. 이건 확인되는데
            //Stream wFile = new FileStream("..//Image/Pt"+num+".png", FileMode.Create,FileAccess.Write); //저장은 제대로 됨
            //wFile.Write(normalP, 0, normalP.Length); //이렇게 하면 정상적으로 저장되네 저장했다 해야하나
            //wFile.Close();
            //스레드 내에서 UI(WPF)관련 건드릴때 사용하는 구문
            Application.Current.Dispatcher.Invoke(() =>
            {
                Cv2.ImShow(num, Mat.FromImageData(normalP, ImreadModes.AnyColor));
            });
        }
        public void CompareWith(byte[] compBytes)
        { //바이트 배열로 받아올 것
            //캡처본 잘 전달되나 테스트 --완료
            //MessageBox.Show(compBytes.Length.ToString()); //아 다 못간거네 --@에 스플릿 되었던것...
            Application.Current.Dispatcher.Invoke(() =>
            {
                Cv2.ImShow("test"+ compBytes.Length.ToString(), Mat.FromImageData(compBytes, ImreadModes.AnyColor));
            });
            //일단은 검사 로직 완성 --연결만 하면됨. 내가 더 공부하자
            /*Check_num 안에서 원본 이미지 화면 이미지 크기에 맞춰서 유사도 검사도 진행해야함*/
            switch (num)
            {
                case "1":
                    //1번 제품 검사
                    B_Check_1.Check_1(Mat.FromImageData(normalP, ImreadModes.AnyColor),Mat.FromImageData(compBytes, ImreadModes.AnyColor));
                    break;
                case "2":
                    //2번 제품 검사
                    B_Check_2.Check_2(Mat.FromImageData(normalP, ImreadModes.AnyColor), Mat.FromImageData(compBytes, ImreadModes.AnyColor));
                    break;
                case "3":
                    //3번 제품 검사
                    B_Check_3.Check_3(Mat.FromImageData(normalP, ImreadModes.AnyColor), Mat.FromImageData(compBytes, ImreadModes.AnyColor));
                    break;
                default:
                    break;
            }
            /*1차 검사 : 색상과 도형 추출 및 일치 검사 마치고 그 결과를 받아서 저장해 둘 bool값 필요*/
            //double rate = MatchingIMG.CalculateRateIMG(Mat.FromImageData(normalP, ImreadModes.AnyColor), Mat.FromImageData(compBytes, ImreadModes.AnyColor));
            //MessageBox.Show("유사도" + String.Format("{0:P}", rate));
            /*2차 검사 : 정상 제품 이미지와 일치율 비교 기준은...어떻게 하려나 이것도 어느정도 이상이면 true 저장*/
        }
    }
}
